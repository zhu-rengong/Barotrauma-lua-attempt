using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Barotrauma.Networking;
using System.Threading.Tasks;
using Barotrauma.Items.Components;
using System.IO;
using System.Net;
using System.Linq;
using System.Xml.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis;

namespace Barotrauma
{
    public static partial class LuaForSumneko
    {
        private static int FielNo = 0;

        public class ClassMetadata
        {
            public readonly static HashSet<ClassMetadata> Caches = new HashSet<ClassMetadata>();
            public Type OriginalType;
            public Type NullableType;
            public Type ResolvedType => NullableType != null ? NullableType : OriginalType;
            public bool IsArrayIndexer = false;
            public bool IsValueIndexer = false;
            public bool IsKeyValueIndexer = false;
            public bool IsIndexer => IsArrayIndexer || IsValueIndexer || IsKeyValueIndexer;
            public Type ArrayElementType;
            public Type ValueType;
            public (Type Key, Type Value)? KeyValueType;
            public bool IsDelegate = false;
            public MethodInfo DelegateMehtod;

            public static ClassMetadata QuickRetrieve(Type type)
            {
                try
                {
                    return Caches.First(metadata => metadata.OriginalType == type);
                }
                catch
                {
                    return null;
                }
            }

            public ClassMetadata(Type originalType)
            {
                if (Caches.Any(metadata => metadata.OriginalType == originalType)) { return; }
                OriginalType = originalType;
                Caches.Add(this);
            }

            public static ClassMetadata Obtain(Type originalType)
            {
                var metadata = QuickRetrieve(originalType);
                if (metadata == null)
                {
                    {
                        if (ResolveNullableArgumentType(originalType, out Type argumentType))
                        {
                            return new ClassMetadata(originalType)
                            {
                                NullableType = argumentType
                            };
                        }
                    }

                    {
                        if (ResolveArrayElementType(originalType, out Type elementType))
                        {
                            return new ClassMetadata(originalType)
                            {
                                IsArrayIndexer = true,
                                ArrayElementType = elementType
                            };
                        }
                    }

                    {
                        if (ResolveValueIndexerArgumentType(originalType, out Type argumentType))
                        {
                            return new ClassMetadata(originalType)
                            {
                                IsValueIndexer = true,
                                ValueType = argumentType
                            };
                        }
                    }

                    {
                        if (ResolveKeyValueIndexerArgumentType(originalType, out (Type, Type)? argumentTypes))
                        {
                            return new ClassMetadata(originalType)
                            {
                                IsKeyValueIndexer = true,
                                KeyValueType = argumentTypes
                            };
                        }
                    }

                    {
                        if (originalType.IsSubclassOf(typeof(Delegate)))
                        {
                            var delegateMethod = originalType.GetMethod("Invoke");
                            if (delegateMethod != null)
                            {
                                return new ClassMetadata(originalType)
                                {
                                    IsDelegate = true,
                                    DelegateMehtod = delegateMethod
                                };
                            }
                        }
                    }

                    return new ClassMetadata(originalType);
                }
                return metadata;
            }

            private string _luaClrName;
            public string LuaClrName
            {
                get
                {
                    if (_luaClrName != null) { return _luaClrName; }
                    return _luaClrName = GetLuaClrName(ResolvedType);
                }
            }

            private string _luaClrNameWithoutNamespace;
            public string LuaClrNameWithoutNamespace
            {
                get
                {
                    if (_luaClrNameWithoutNamespace != null) { return _luaClrNameWithoutNamespace; }
                    return _luaClrNameWithoutNamespace = GetLuaClrName(ResolvedType, containNamespace: false);
                }
            }

            private string _luaScriptName;
            public string LuaScriptName
            {
                get
                {
                    if (_luaScriptName != null) { return _luaScriptName; }
                    if (IsIndexer)
                    {
                        if (IsArrayIndexer) { return _luaScriptName = Obtain(ArrayElementType).LuaScriptName + @"[]"; }
                        if (IsValueIndexer) { return _luaScriptName = Obtain(ValueType).LuaScriptName + @"[]"; }
                        if (IsKeyValueIndexer)
                        {
                            var key = Obtain(KeyValueType.Value.Key).LuaScriptName;
                            var value = Obtain(KeyValueType.Value.Value).LuaScriptName;
                            return _luaScriptName = $@"table<{key}, {value}>";
                        }
                    }

                    return _luaScriptName = LuaClrName;
                }
            }

            public static string GloballyTable(string[] parts) => parts.Aggregate("_G", (p1, p2) => p1 + $@"['{p2}']");

            private string _defaultTable;
            public string DefaultTable
            {
                get
                {
                    if (_defaultTable != null) { return _defaultTable; }
                    var typeInfo = ResolvedType.GetTypeInfo();
                    string[] parts = typeInfo.Namespace.Split('.');
                    if (parts[0] == "Barotrauma") { parts = LuaClrNameWithoutNamespace.Split('.'); }
                    else { parts = parts.Concat(LuaClrNameWithoutNamespace.Split('.')).ToArray(); }
                    return _defaultTable = GloballyTable(parts);
                }
            }

            private static bool ResolveNullableArgumentType(Type type, out Type argumentType)
            {
                argumentType = null;

                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    var genericArguments = type.GetGenericArguments();
                    argumentType = genericArguments[0];
                    return true;
                }

                return false;
            }

            private static bool ResolveArrayElementType(Type type, out Type elementType)
            {
                elementType = null;

                if (type.IsArray)
                {
                    elementType = type.GetElementType();
                    if (ResolveNullableArgumentType(elementType, out Type argumentType))
                    { elementType = argumentType; }
                    return true;
                }

                return false;
            }

            private static bool ResolveValueIndexerArgumentType(Type type, out Type argumentType)
            {
                argumentType = null;

                if (type.IsGenericType)
                {
                    var genericTypeDefinition = type.GetGenericTypeDefinition();
                    bool ImplAtLeastOneInterface(params Type[] types) => types.Any(t => genericTypeDefinition == t);
                    if (ImplAtLeastOneInterface(
                        typeof(IEnumerable<>),
                        typeof(IList<>),
                        typeof(IReadOnlyList<>),
                        typeof(List<>),
                        typeof(HashSet<>),
                        typeof(IImmutableList<>)))
                    {
                        var genericArguments = type.GetGenericArguments();
                        argumentType = genericArguments[0];
                        if (ResolveNullableArgumentType(argumentType, out Type nullableArgumentType))
                        { argumentType = nullableArgumentType; }
                        return true;
                    }
                }

                return false;
            }

            private static bool ResolveKeyValueIndexerArgumentType(Type type, out (Type, Type)? argumentTypes)
            {
                argumentTypes = null;

                if (type.IsGenericType)
                {
                    var genericTypeDefinition = type.GetGenericTypeDefinition();
                    bool ImplAtLeastOneInterface(params Type[] types) => types.Any(t => genericTypeDefinition == t);
                    if (ImplAtLeastOneInterface(
                        typeof(IDictionary<,>),
                        typeof(IReadOnlyDictionary<,>),
                        typeof(Dictionary<,>),
                        typeof(IImmutableDictionary<,>)))
                    {
                        var genericArguments = type.GetGenericArguments();
                        for (int i = 0; i < 2; i++)
                        {
                            if (ResolveNullableArgumentType(genericArguments[i], out Type nullableArgumentType))
                            {
                                genericArguments[i] = nullableArgumentType;
                            }
                        }
                        argumentTypes = (genericArguments[0], genericArguments[1]);
                        return true;
                    }
                }

                return false;
            }
        }

        public static string GetLuaClrName(Type type, bool isGenericArgument = false, bool containNamespace = true, bool includeDeclaringType = true, int depth = 1)
        {
            var name = new StringBuilder();
            var typeInfo = type.GetTypeInfo();
            // Use '.' to represent trutly name structure of the table
            // Formatter: [Namespace<.|*>][Outer.]<Nested>
            // Solves the confliction with get_DefaultTable
            char nsSplit = isGenericArgument ? '*' : '.';
            if (containNamespace) { name.Append(typeInfo.Namespace.Replace('.', nsSplit) + nsSplit); }

            if (includeDeclaringType)
            {
                Type declaringType = typeInfo.DeclaringType;
                var declaringTypes = new Stack<Type>();
                while (declaringType != null)
                {
                    declaringTypes.Push(declaringType);
                    declaringType = declaringType.DeclaringType;
                }
                bool first = true;
                while (declaringTypes.Count > 0)
                {
                    declaringType = declaringTypes.Pop();
                    name.Append((first ? string.Empty : nsSplit) + GetLuaClrName(declaringType, containNamespace: false, includeDeclaringType: false));
                    first = false;
                }
                if (!first) { name.Append(nsSplit); }
            }

            int subLen = typeInfo.Name.IndexOf('`'); // removes all trivils from the begining of the generic type symbol
            name.Append(typeInfo.Name.Substring(0, (subLen > -1) ? subLen : typeInfo.Name.Length));
            if (typeInfo.GenericTypeArguments.Length > 0)
            {
                foreach (var genericTypeArgument in typeInfo.GenericTypeArguments)
                {
                    name.Append($"*{depth}");
                    name.Append(GetLuaClrName(genericTypeArgument, isGenericArgument: true, depth: depth + 1));
                }
            }
            return name.ToString();
        }

        public static void Lualy<T>(string[] majorTable = null, string[][] minorTables = null)
        {
            var luaDocBuilder = new StringBuilder();
            var type = typeof(T);
            var typeInfo = type.GetTypeInfo();
            var metadata = ClassMetadata.Obtain(type);
            string luaClrName = metadata.LuaClrName;
            string table = (majorTable == null) ? metadata.DefaultTable : ClassMetadata.GloballyTable(majorTable);

            luaDocBuilder.AppendLine($"---@meta");
            luaDocBuilder.Append($"---@class {luaClrName}");
            if (typeInfo.BaseType != null) { luaDocBuilder.Append($" : {ClassMetadata.Obtain(typeInfo.BaseType).LuaClrName}"); }
            luaDocBuilder.AppendLine();

            foreach (var field in (
                from field in typeInfo.DeclaredFields
                where !field.Name.Contains(">k__BackingField")
                select field).ToList())
            {
                luaDocBuilder.AppendLine($"---@field {field.Name} {ClassMetadata.Obtain(field.FieldType).LuaScriptName}");
            }

            luaDocBuilder.AppendLine($"{table} = {{}}");

            File.WriteAllText(Path.Combine(DocumentationRelativePath, $"{++FielNo}.lua"), luaDocBuilder.ToString());
        }
    }
}
