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
        private static readonly string[] typesPermitted = {
            // Basics
            "System",
            // Barotrauma
            "Barotrauma",
            // Lua
            "MoonSharp.Interpreter.DynValue",
            "MoonSharp.Interpreter.Closure",
            "MoonSharp.Interpreter.Coroutine",
            "MoonSharp.Interpreter.CoroutineState",
            "MoonSharp.Interpreter.Table",
            "MoonSharp.Interpreter.YieldRequest",
            "MoonSharp.Interpreter.TailCallData",
            "MoonSharp.Interpreter.DataType",
        };
        private static readonly string[] typesProhibited = {
            "System.IO",
            "Moonsharp",
            "Barotrauma.IO",
        };
        public static bool IsTypeAllowed(string name)
        {
            var matchPermitted = typesPermitted.Where(s => name.StartsWith(s));
            var longestPemitted = matchPermitted.Count() > 0 ? matchPermitted.Max(s => s.Length) : 0;
            var matchProhibited = typesProhibited.Where(s => name.StartsWith(s));
            var longestProhibited = matchProhibited.Count() > 0 ? matchProhibited.Max(s => s.Length) : 0;
            if (longestPemitted == 0 || longestPemitted < longestProhibited) return false;
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

        private static string ResolveTypeRef(MetadataReader reader, TypeReferenceHandle t)
        {
            var tRef = reader.GetTypeReference(t);

            var typeName = $"{reader.GetString(tRef.Name)}";
            EntityHandle handle = tRef.ResolutionScope;
            TypeReference tr = tRef;
            while (!handle.IsNil && handle.Kind == HandleKind.TypeReference)
            {
                tr = reader.GetTypeReference((TypeReferenceHandle)handle);
                handle = tr.ResolutionScope;
                typeName = $"{reader.GetString(tr.Name)}.{typeName}";
            }
            typeName = $"{reader.GetString(tr.Namespace)}.{typeName}";

            return typeName;
        }

        public static string FilterMetadata(MetadataReader reader)
        {
            if (reader == null) throw new ArgumentNullException("Metadata Reader must not be null.");

            var conflictingTypes = new List<string>();
            reader.TypeReferences.ToList().ForEach(t =>
            {
                var typeName = ResolveTypeRef(reader, t);

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

        private static readonly string[] permitedDefinitions = {
            ".<Module>",
            "NetOneTimeScript.NetOneTimeScriptRunner"
        };
        private static readonly string[] permitedMethods = {
            ".ctor",
            "Run"
        };
        public static string FilterOneTimeMetadata(MetadataReader reader)
        {
            var errStr = FilterMetadata(reader);
            if (errStr != null) return errStr;

            TypeDefinition? runDef = null;
            reader.TypeDefinitions.Select(t => reader.GetTypeDefinition(t)).ToList().ForEach(t =>
            {
                var typeName = $"{reader.GetString(t.Name)}";
                if (typeName == "NetOneTimeScriptRunner") runDef = t;
                while (t.IsNested)
                {
                    t = reader.GetTypeDefinition(t.GetDeclaringType());
                    typeName = $"{reader.GetString(t.Name)}.{typeName}";
                }
                typeName = $"{reader.GetString(t.Namespace)}.{typeName}";
                if (!permitedDefinitions.Contains(typeName)) errStr = "Malformed assembly";
            });
            if (errStr != null) return errStr;

            if (runDef == null) return "runner class not detected";
            else
            {
                var methods = runDef.Value.GetMethods();
                if (methods.Count > 2) return "malformed runner class";

                methods.Select(m => reader.GetMethodDefinition(m)).ToList().ForEach(m => {
                    if (!permitedMethods.Contains(reader.GetString(m.Name))) errStr = "malformed runner class";
                });
                if (errStr != null) return errStr;
            }

            return null;
        }
    }
}