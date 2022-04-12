using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;

namespace Barotrauma {
    class CsScriptFilter
    {
        private const bool useWhitelist = false;

        private static string[] typesPermited = new string[] {
            // Basics
            "System.Runtime.CompilerServices.CompilationRelaxationsAttribute",
            "System.Runtime.CompilerServices.RuntimeCompatibilityAttribute",
            "System.Diagnostics.DebuggableAttribute",
            "System.Object",
            "System.String",
            "System.Collections",
            // Some roslyn magic
            ".DebuggingModes",
            // Barotrauma
            "Barotrauma",
        };
        private static string[] typessProhibited = new string[] {
            //"System.Reflection",
            "System.IO.File",
        };
        public static bool IsTypeAllowed(string usingName)
        {
            if (useWhitelist && !typesPermited.Any(u => u.StartsWith(usingName))) return false;
            if (typessProhibited.Any(u => u.StartsWith(usingName))) return false;
            return true;
        }

        public static string FilterSyntaxTree(CSharpSyntaxTree tree)
        {
            if (tree == null) throw new ArgumentNullException("Syntax tree must not be null.");

            { // Disallow top-level statements
                var nodeCheck = tree.GetRoot().DescendantNodes();

                var tlStatements = nodeCheck.Where(n => n is GlobalStatementSyntax).ToList();
                if (tlStatements.Count > 0)
                {
                    string errStr = "Cmopilation Error:";
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