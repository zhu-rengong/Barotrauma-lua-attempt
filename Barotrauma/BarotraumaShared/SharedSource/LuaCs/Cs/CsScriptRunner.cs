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
	class CsScriptRunner : AssemblyLoadContext
	{
		public LuaCsSetup setup;
		private List<MetadataReference> defaultReferences;
		private CSharpCompilationOptions compileOptions;

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
			var prefix = @"
public class NetOneTimeScriptRunner {
	public NetOneTimeScriptRunner() { }
	public object Run() {

";
			var postfix = @"
		return null;
	}
}
";
			return prefix + code + postfix;
		}

		public object Run(string code)
		{
			object scriptResilt = null;

			try
			{
				var syntaxTree = SyntaxFactory.ParseSyntaxTree(ToOneTimeScript(code), CSharpParseOptions.Default);
				var compilation = CSharpCompilation.Create("NetOneTimeScriptAssembly", new[] { syntaxTree }, defaultReferences, compileOptions);

				Assembly assembly = null;
				using (var mem = new MemoryStream())
				{
					var result = compilation.Emit(mem);
					if (!result.Success)
					{
						IEnumerable<Diagnostic> failures = result.Diagnostics.Where(d => d.IsWarningAsError || d.Severity == DiagnosticSeverity.Error);

						string errStr = "Script cmopilation errors:";
						foreach (Diagnostic diagnostic in failures)
							errStr = $"\n{diagnostic}";
						LuaCsSetup.PrintCsError(errStr);
					}
					else
					{
						mem.Seek(0, SeekOrigin.Begin);
						var metaReader = new PEReader(mem).GetMetadataReader();
						var errStr = CsScriptFilter.FilterMetadata(metaReader);
						if (errStr == null)
                        {
							foreach (var handle in metaReader.TypeDefinitions)
                            {
								var typeDef = metaReader.GetTypeDefinition(handle);
								var typeName = $"{metaReader.GetString(typeDef.Namespace)}.{metaReader.GetString(typeDef.Name)}";
								if (typeName != ".NetOneTimeScriptRunner")
                                {
									errStr = "Script Error - malformed assembly";
									break;
								}
							}
						}

						if (errStr == null)
						{
							mem.Seek(0, SeekOrigin.Begin);
							assembly = LoadFromStream(mem);
							var runner = assembly.CreateInstance("NetOneTimeScriptRunner");
							if (runner != null)
                            {
								if (runner.GetType().GetMethods().Count() > 1) LuaCsSetup.PrintCsError("Script Error - malformed runner class");
								else
								{
									var method = runner.GetType().GetMethod("Run", BindingFlags.Public | BindingFlags.Instance);
									if (method != null) scriptResilt = method.Invoke(runner, null);
									else LuaCsSetup.PrintCsError("Script Error - no run method detected");
								}
							}
							else LuaCsSetup.PrintCsError("Script Error - no runner class detected");
						}
						else LuaCsSetup.PrintCsError(errStr);
					}
				}
				Unload();
			}
			catch (CompilationErrorException ex)
			{
				string errStr = "Script Cmopilation Error:";
				foreach (var diag in ex.Diagnostics)
				{
					errStr += "\n" + diag.ToString();
				}
				LuaCsSetup.PrintCsError(errStr);
			}
			catch (Exception ex)
			{
				LuaCsSetup.PrintCsError("Error running script:\n" + ex.Message + "\n" + ex.StackTrace);
			}

			return scriptResilt;
		}

	}
}