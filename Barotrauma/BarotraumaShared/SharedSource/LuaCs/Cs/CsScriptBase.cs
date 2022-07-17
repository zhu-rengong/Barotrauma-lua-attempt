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
using System.Text;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	class CsScriptBase : AssemblyLoadContext
	{

		public const string NET_ONE_TIME_SCRIPT_ASSEMBLY = "NetOneTimeScriptAssembly";
		public const string NET_SCRIPT_ASSEMBLY = "NetScriptAssembly";

		public static readonly string[] LoadedAssemblyName = {
			CsScriptBase.NET_SCRIPT_ASSEMBLY,
			CsScriptBase.NET_ONE_TIME_SCRIPT_ASSEMBLY
		};

		public static Dictionary<string, object> Revision = new Dictionary<string, object>()
        {
			{ NET_SCRIPT_ASSEMBLY, 0},
			{ NET_ONE_TIME_SCRIPT_ASSEMBLY, 0}
        };

		public CSharpParseOptions ParseOptions { get; protected set; }

		public CsScriptBase() : base(isCollectible: true) {
			ParseOptions = CSharpParseOptions.Default
				.WithPreprocessorSymbols(new[] { LuaCsSetup.IsServer ? "SERVER" : (LuaCsSetup.IsClient ? "CLIENT" : "UNDEFINED") });
		}

		public static SyntaxTree AssemblyInfoSyntaxTree(string asmName = null)
        {
			Revision[asmName] = (int)Revision[asmName] + 1;
			var asmInfo = new StringBuilder();
			asmInfo.AppendLine("using System.Reflection;");
			asmInfo.AppendLine($"[assembly: AssemblyMetadata(\"Revision\", \"{Revision[asmName]}\")]");
			asmInfo.AppendLine($"[assembly: AssemblyVersion(\"0.0.0.{Revision[asmName]}\")]");
			return CSharpSyntaxTree.ParseText(asmInfo.ToString(), CSharpParseOptions.Default);
		}

		~CsScriptBase() { }
	}
}