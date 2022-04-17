using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;

namespace Barotrauma {
    class CsScriptFilter
    {
        private static readonly string[] typesPermitted = new string[] {
            // Basics
            "System.Runtime.CompilerServices.CompilationRelaxationsAttribute",
            "System.Runtime.CompilerServices.RuntimeCompatibilityAttribute",
            "System.Diagnostics.DebuggableAttribute",
            "System.Object",
            "System.String",
            "System.Collections",
            "System",
            // Some roslyn magic
            ".DebuggingModes",
            // Barotrauma
            "Barotrauma",
            // Lua
            "MoonSharp.Interpreter"
        };
        private static readonly string[] typesProhibited = new string[] {
            //"System.Reflection",
            "System.IO",
            "Moonsharp.Interpreter.UserData"
        };
        public static bool IsTypeAllowed(string name)
        {
            var longestPemited = typesPermitted.Where(s => s.StartsWith(name)).Max(s => s.Length);
            var longestProhibitted = typesProhibited.Where(s => s.StartsWith(name)).Max(s => s.Length);
            if (longestPemited == 0 || longestPemited < longestProhibitted) return false;
            else return true;
        }

        public static string FilterSyntaxTree(CSharpSyntaxTree tree)
        {
            if (tree == null) throw new ArgumentNullException("Syntax tree must not be null.");

            { // Disallow top-level statements
                var nodeCheck = tree.GetRoot().DescendantNodes();

                var tlStatements = nodeCheck.Where(n => n is GlobalStatementSyntax).ToList();
                if (tlStatements.Count > 0)
                {
                    string errStr = "Compilation Error:";
                    foreach (var tls in tlStatements) tls.GetDiagnostics().ToList().ForEach(d => errStr += $"\n  {d.ToString()}");
                    return errStr;
                }
            }

            return null; 
        }

        public static string FilterMetadata(MetadataReader reader)
        {
            if (reader == null) throw new ArgumentNullException("Metadata Reader must not be null.");

            var conflictingTypes = new List<string>();
            reader.TypeReferences.ToList().ForEach(t =>
            {
                var tRef = reader.GetTypeReference(t);
                var typeName = $"{reader.GetString(tRef.Namespace)}.{reader.GetString(tRef.Name)}";
                if (!IsTypeAllowed(typeName)) conflictingTypes.Add(typeName);
            });

            if (conflictingTypes.Count > 0)
            {
                string errStr = "Metadata Error:";
                conflictingTypes.ForEach(t => errStr += $"\n  Usage of type '{t}' in mods is prohibited.");
                return errStr;
            }

            return null;
        }
    }
}