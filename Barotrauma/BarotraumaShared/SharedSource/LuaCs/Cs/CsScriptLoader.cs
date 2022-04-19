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

namespace Barotrauma
{
	class CsScriptLoader : AssemblyLoadContext
	{
		public LuaCsSetup setup;
		private List<MetadataReference> defaultReferences;

		private Dictionary<string, List<string>> sources;
		public Assembly Assembly { get; private set; }

		public CsScriptLoader(LuaCsSetup setup) : base(isCollectible: true)
		{
			this.setup = setup;

			defaultReferences = AppDomain.CurrentDomain.GetAssemblies()
				.Where(a => !(a.IsDynamic || string.IsNullOrEmpty(a.Location) || a.Location.Contains("xunit")))
				.Select(a => MetadataReference.CreateFromFile(a.Location) as MetadataReference)
				.ToList();

			sources = new Dictionary<string, List<string>>();
			Assembly = null;
		}

		public void SearchFolders()
        {
			foreach(ContentPackage cp in ContentPackageManager.EnabledPackages.All)
            {
				var path = Path.GetDirectoryName(cp.Path);
				RunFolder(path);
            }
		}

		public bool HasSources { get => sources.Count > 0; }

		private void RunFolder(string folder)
		{
			foreach (var str in DirSearch(folder))
			{
				var s = str.Replace("\\", "/");

				if (s.EndsWith(".cs") && LuaCsFile.IsPathAllowedCsException(s))
				{
					if (sources.ContainsKey(folder)) sources[folder].Add(s);
					else sources.Add(folder, new List<string> { s });
				}
			}
		}

		private IEnumerable<SyntaxTree> ParseSources() {
			var syntaxTrees = new List<SyntaxTree>();

			if (sources.Count <= 0) throw new Exception("No Cs sources detected");
			foreach ((var folder, var src) in sources)
            {
				try
				{
					foreach (var file in src)
					{
						var tree = SyntaxFactory.ParseSyntaxTree(File.ReadAllText(file), CSharpParseOptions.Default, file);
						var error = CsScriptFilter.FilterSyntaxTree(tree as CSharpSyntaxTree); // Check file content for prohibited stuff
						if (error != null) throw new Exception(error);

						syntaxTrees.Add(tree);
					}
				}
				catch (Exception ex)
				{
					LuaCsSetup.PrintCsError("Error loading '" + folder + "':\n" + ex.Message + "\n" + ex.StackTrace);
				}
			}

			return syntaxTrees;
		}

        public List<Type> Compile()
        {
			IEnumerable<SyntaxTree> syntaxTrees = ParseSources();

			var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
				.WithMetadataImportOptions(MetadataImportOptions.All)
				.WithOptimizationLevel(OptimizationLevel.Release)
				.WithAllowUnsafe(false);
			var compilation = CSharpCompilation.Create("NetScriptAssembly",syntaxTrees, defaultReferences, options);

			using (var mem = new MemoryStream())
			{
				var result = compilation.Emit(mem);
				if (!result.Success)
				{
					IEnumerable<Diagnostic> failures = result.Diagnostics.Where(d => d.IsWarningAsError || d.Severity == DiagnosticSeverity.Error);

					string errStr = "NET MODS NOT LOADED | Mod compilation errors:";
					foreach (Diagnostic diagnostic in failures)
						errStr += $"\n{diagnostic}";
					LuaCsSetup.PrintCsError(errStr);
				}
				else
				{
					mem.Seek(0, SeekOrigin.Begin);
					var errStr = CsScriptFilter.FilterMetadata(new PEReader(mem).GetMetadataReader());
					if (errStr == null)
                    {
						mem.Seek(0, SeekOrigin.Begin);
						Assembly = LoadFromStream(mem);
					}
					else LuaCsSetup.PrintCsError(errStr);
				}
			}

			if (Assembly != null)
				return Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(ACsMod))).ToList();
			else
				throw new Exception("Unable to create net mods assembly.");
		}

		private static string[] DirSearch(string sDir)
		{
			List<string> files = new List<string>();

			try
			{
				foreach (string f in Directory.GetFiles(sDir))
				{
					files.Add(f);
				}

				foreach (string d in Directory.GetDirectories(sDir))
				{
					files.AddRange(DirSearch(d));
				}
			}
			catch (System.Exception excpt)
			{
				Console.WriteLine(excpt.Message);
			}

			return files.ToArray();
		}

		public void Clear()
        {
			Assembly = null;
        }

		~CsScriptLoader()
		{
			
		}
	}
}