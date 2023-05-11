﻿using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis.Scripting;
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;
using Microsoft.CodeAnalysis;
using System.Runtime.Loader;
using System.Reflection.PortableExecutable;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Barotrauma
{
    class CsScriptLoader : CsScriptBase
    {
        private List<MetadataReference> defaultReferences;

        private Dictionary<string, List<string>> sources;
        public Assembly Assembly { get; private set; }

        public CsScriptLoader()
        {
            defaultReferences = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !(a.IsDynamic || string.IsNullOrEmpty(a.Location) || a.Location.Contains("xunit")))
                .Select(a => MetadataReference.CreateFromFile(a.Location) as MetadataReference)
                .ToList();

            sources = new Dictionary<string, List<string>>();
            Assembly = null;
        }

        private enum RunType { Standard, Forced, None };
        private bool ShouldRun(ContentPackage cp, string path)
        {
            if (!Directory.Exists(path + "CSharp"))
            {
                return false;
            }

            var isEnabled = ContentPackageManager.EnabledPackages.All.Contains(cp);
            if (File.Exists(path + "CSharp/RunConfig.xml"))
            {
                Stream stream = File.Open(path + "CSharp/RunConfig.xml", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                var doc = XDocument.Load(stream);
                var elems = doc.Root.Elements().ToArray();
                var elem = elems.FirstOrDefault(e => e.Name.LocalName.Equals(LuaCsSetup.IsServer ? "Server" : (LuaCsSetup.IsClient ? "Client" : "None"), StringComparison.OrdinalIgnoreCase));

                if (elem != null && Enum.TryParse(elem.Value, true, out RunType rtValue))
                {
                    if (rtValue == RunType.Standard && isEnabled)
                    {
                        LuaCsLogger.LogMessage($"Added {cp.Name} {cp.ModVersion} to Cs compilation. (Standard)");
                        return true;
                    }
                    else if (rtValue == RunType.Forced && (isEnabled || !GameMain.LuaCs.Config.TreatForcedModsAsNormal))
                    {
                        LuaCsLogger.LogMessage($"Added {cp.Name} {cp.ModVersion} to Cs compilation. (Forced)");
                        return true;
                    }
                    else if (rtValue == RunType.None)
                    {
                        return false;
                    }
                }

                stream.Close();
            }

            if (isEnabled)
            {
                LuaCsLogger.LogMessage($"Added {cp.Name} {cp.ModVersion} to Cs compilation. (Assumed)");
                return true;
            }
            else
            {
                return false;
            }
        }

        public void SearchFolders()
        {
            var packagesAdded = new HashSet<ContentPackage>();
            var paths = new Dictionary<string, string>();
            foreach (var cp in ContentPackageManager.AllPackages.Concat(ContentPackageManager.EnabledPackages.All))
            {
                if (packagesAdded.Contains(cp)) { continue; }
                var path = $"{Path.GetFullPath(Path.GetDirectoryName(cp.Path)).Replace('\\', '/')}/";
                if (ShouldRun(cp, path))
                {
                    if (paths.ContainsKey(cp.Name))
                    {
                        if (ContentPackageManager.EnabledPackages.All.Contains(cp))
                        {
                            paths[cp.Name] = path;
                        }
                    }
                    else
                    {
                        paths.Add(cp.Name, path);
                    }
                    packagesAdded.Add(cp);
                }
            }

            foreach ((var _, var path) in paths)
            {
                RunFolder(path);
            }
        }

        public bool HasSources { get => sources.Count > 0; }

        private void AddSources(string folder)
        {
            foreach (var str in DirSearch(folder))
            {
                string s = str.Replace("\\", "/");

                if (sources.ContainsKey(folder))
                {
                    sources[folder].Add(s);
                }
                else
                {
                    sources.Add(folder, new List<string> { s });
                }
            }
        }

        private void RunFolder(string folder)
        {

            AddSources(folder + "/CSharp/Shared");

#if SERVER
            AddSources(folder + "/CSharp/Server");
#else
            AddSources(folder + "/CSharp/Client");
#endif
        }

        private IEnumerable<SyntaxTree> ParseSources() {
            var syntaxTrees = new List<SyntaxTree>();

            if (sources.Count <= 0) throw new Exception("No Cs sources detected");
            syntaxTrees.Add(AssemblyInfoSyntaxTree(CsScriptAssembly));
            foreach ((var folder, var src) in sources)
            {
                try
                {
                    foreach (var file in src)
                    {
                        var tree = SyntaxFactory.ParseSyntaxTree(File.ReadAllText(file), ParseOptions, file);

                        syntaxTrees.Add(tree);
                    }
                }
                catch (Exception ex)
                {
                    LuaCsLogger.LogError("Error loading '" + folder + "':\n" + ex.Message + "\n" + ex.StackTrace, LuaCsMessageOrigin.CSharpMod);
                }
            }

            return syntaxTrees;
        }

        private ContentPackage FindSourcePackage(Diagnostic diagnostic)
        {
            if (diagnostic.Location.SourceTree == null)
            {
                return null;
            }

            string path = diagnostic.Location.SourceTree.FilePath;
            foreach (var package in ContentPackageManager.AllPackages)
            {
                if (Path.GetFullPath(path).StartsWith(Path.GetFullPath(package.Dir)))
                {
                    return package;
                }
            }

            return null;
        }

        public List<Type> Compile() 
        {
            IEnumerable<SyntaxTree> syntaxTrees = ParseSources();

            var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithMetadataImportOptions(MetadataImportOptions.All)
                .WithOptimizationLevel(OptimizationLevel.Release)
                .WithAllowUnsafe(true);

            var topLevelBinderFlagsProperty = typeof(CSharpCompilationOptions).GetProperty("TopLevelBinderFlags", BindingFlags.Instance | BindingFlags.NonPublic);
            topLevelBinderFlagsProperty.SetValue(options, (uint)1 << 22);

            var compilation = CSharpCompilation.Create(CsScriptAssembly, syntaxTrees, defaultReferences, options);

            using (var mem = new MemoryStream())
            {
                var result = compilation.Emit(mem);
                if (!result.Success)
                {
                    IEnumerable<Diagnostic> failures = result.Diagnostics.Where(d => d.IsWarningAsError || d.Severity == DiagnosticSeverity.Error);

                    string errStr = "CS MODS NOT LOADED | Compilation errors:";
                    foreach (Diagnostic diagnostic in failures)
                    {
                        errStr += $"\n{diagnostic}";
#if CLIENT
                        ContentPackage package = FindSourcePackage(diagnostic);
                        if (package != null)
                        {
                            LuaCsLogger.ShowErrorOverlay($"{package.Name} {package.ModVersion} is causing compilation errors. Check debug console for more details.", 7f, 7f);
                        }
#endif
                    }
                    LuaCsLogger.LogError(errStr, LuaCsMessageOrigin.CSharpMod);
                }
                else
                {
                    mem.Seek(0, SeekOrigin.Begin);
                    Assembly = LoadFromStream(mem);
                }
            }

            if (Assembly != null)
            {
                RegisterAssemblyWithNativeGame(Assembly);
                try
                {
                    return Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(ACsMod))).ToList();
                }
                catch (ReflectionTypeLoadException re)
                {
                    LuaCsLogger.LogError($"Unable to load CsMod Types. {re.Message}", LuaCsMessageOrigin.CSharpMod);
                    throw re;
                }
            }
            else
            {
                throw new Exception("Unable to create cs mods assembly.");
            }
        }

        /// <summary>
        /// This function should be used whenever a new assembly is created. Wrapper to allow more complicated setup later if need be.
        /// </summary>
        private static void RegisterAssemblyWithNativeGame(Assembly assembly)
        {
            Barotrauma.ReflectionUtils.AddNonAbstractAssemblyTypes(assembly);
        }

        /// <summary>
        /// This function should be used whenever a new assembly is about to be destroyed/unloaded. Wrapper to allow more complicated setup later if need be.
        /// </summary>
        /// <param name="assembly">Assembly to remove</param>
        private static void UnregisterAssemblyFromNativeGame(Assembly assembly)
        {
            Barotrauma.ReflectionUtils.RemoveAssemblyFromCache(assembly);
        }

        private static string[] DirSearch(string sDir)
        {
            if (!Directory.Exists(sDir))
            {
                return new string[] {};
            }

            return Directory.GetFiles(sDir, "*.cs", SearchOption.AllDirectories);
        }

        public void Clear()
        {
            if (Assembly != null)
            {
                UnregisterAssemblyFromNativeGame(Assembly);
                Assembly = null;
            }
        }
    }
}
