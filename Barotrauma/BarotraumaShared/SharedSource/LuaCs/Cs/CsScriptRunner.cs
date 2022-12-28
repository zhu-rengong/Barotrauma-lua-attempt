using System;
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
using MoonSharp.Interpreter;

namespace Barotrauma
{
    class CsScriptRunner : CsScriptBase
    {
        public LuaCsSetup setup;
        private List<MetadataReference> defaultReferences;
        private CSharpCompilationOptions compileOptions;
        private static readonly string[] usings = {
            "System",
            "Barotrauma",
            "System.Collections.Generic",
            "System.Linq"
        };

        public CsScriptRunner(LuaCsSetup setup)
        {
            this.setup = setup;

            defaultReferences = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !(a.IsDynamic || string.IsNullOrEmpty(a.Location) || a.Location.Contains("xunit")))
                .Select(a => MetadataReference.CreateFromFile(a.Location) as MetadataReference)
                .ToList();
            compileOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithMetadataImportOptions(MetadataImportOptions.All)
                .WithOptimizationLevel(OptimizationLevel.Release)
                .WithAllowUnsafe(false);
        }

        private static string ToOneTimeScript(string code)
        {
            var prefix = "";
            foreach (var u in usings) prefix += $"using {u}; ";
            prefix += "namespace NetOneTimeScript { public class NetOneTimeScriptRunner { public NetOneTimeScriptRunner() { } public object Run() {\n";
            var postfix = "\nreturn null; } } }";
            return prefix + code + postfix;
        }

        public object Run(string code)
        {
            object scriptResilt = null;

            try
            {
                code = ToOneTimeScript(code);
                var syntaxTree = SyntaxFactory.ParseSyntaxTree(code, ParseOptions);
                var compilation = CSharpCompilation.Create(CsOneTimeScriptAssembly, new[] { AssemblyInfoSyntaxTree(CsOneTimeScriptAssembly), syntaxTree }, defaultReferences, compileOptions);

                Assembly assembly = null;
                using (var mem = new MemoryStream())
                {
                    var result = compilation.Emit(mem);
                    if (!result.Success)
                    {
                        IEnumerable<Diagnostic> failures = result.Diagnostics.Where(d => d.IsWarningAsError || d.Severity == DiagnosticSeverity.Error);

                        string errStr = "Script compilation errors:";
                        var lineErr = new SortedDictionary<int, (string, string)>();
                        foreach (Diagnostic diagnostic in failures)
                        {
                            var line = syntaxTree.GetLineSpan(diagnostic.Location.SourceSpan).StartLinePosition.Line;
                            lineErr[line] = (diagnostic.Id, diagnostic.ToString());
                        }
                        var lines = code.Split('\n');
                        for (var i = 1; i < lines.Length - 1; i++)
                        {
                            errStr += $"\n{i} >>  {lines[i]}";
                            if (lineErr.ContainsKey(i)) errStr += $"        <===  {lineErr[i].Item1}";
                        }
                        errStr += "\n";
                        foreach ((var idx, (var id, var err)) in lineErr)
                        {
                            errStr += $"\n{idx}:  {err}";
                        }
                        LuaCsLogger.LogError(errStr, LuaCsMessageOrigin.CSharpMod);
                    }
                    else
                    {
                        mem.Seek(0, SeekOrigin.Begin);
                        assembly = LoadFromStream(mem);
                        var runner = assembly.CreateInstance("NetOneTimeScript.NetOneTimeScriptRunner");
                        if (runner != null)
                        {
                            var method = runner.GetType().GetMethod("Run", BindingFlags.Public | BindingFlags.Instance);
                            if (method != null)
                            {
                                scriptResilt = method.Invoke(runner, null);
                                foreach (var type in assembly.GetTypes())
                                {
                                    UserData.UnregisterType(type, true);
                                }
                            }
                            else { LuaCsLogger.LogError("Script Error - no run method detected", LuaCsMessageOrigin.CSharpMod); }
                        }
                        else { LuaCsLogger.LogError("Script Error - no runner class detected", LuaCsMessageOrigin.CSharpMod); }
                    }
                }
                Unload();
            }
            catch (Exception ex)
            {
                LuaCsLogger.LogError("Error running script:\n" + ex.Message + "\n" + ex.StackTrace, LuaCsMessageOrigin.CSharpMod);
            }

            return scriptResilt;
        }

    }
}
