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
        public class ClassMetadata
        {
            public readonly static ClassMetadata Empty = new ClassMetadata();
            public readonly static HashSet<ClassMetadata> Caches = new HashSet<ClassMetadata>();
            public bool IsEmpty = false;
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

            private ClassMetadata() { IsEmpty = true; }

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
                    return _luaClrName = GetLuaClrName(OriginalType);
                }
            }

            private string _luaClrNameWithoutNamespace;
            public string LuaClrNameWithoutNamespace
            {
                get
                {
                    if (_luaClrNameWithoutNamespace != null) { return _luaClrNameWithoutNamespace; }
                    return _luaClrNameWithoutNamespace = GetLuaClrName(OriginalType, containNamespace: false);
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
                    else if (IsDelegate)
                    {
                        var delegateMethodBuilder = new StringBuilder();
                        var parameters = DelegateMehtod.GetParameters();
                        ExplanDelegateMethodStart(delegateMethodBuilder);
                        for (var i = 0; i < parameters.Length; i++)
                        {
                            var parameter = parameters[i];
                            ExplanDelegateMethodParam(delegateMethodBuilder, parameter);
                            if ((i < parameters.Length - 1) && parameters.Length > 1) { delegateMethodBuilder.Append(", "); }
                        }
                        ExplanDelegateMethodEnd(delegateMethodBuilder, DelegateMehtod);
                        return _luaScriptName = $"{LuaClrName}|({delegateMethodBuilder})";
                    }

                    return _luaScriptName = ResolvedType switch
                    {
                        { Namespace: "System", Name: "String" } => $"string|{LuaClrName}",
                        { Namespace: "System", Name: "Boolean" } => $"boolean|{LuaClrName}",
                        { Namespace: "System", Name: "SByte" } => $"integer|{LuaClrName}",
                        { Namespace: "System", Name: "Byte" } => $"integer|{LuaClrName}",
                        { Namespace: "System", Name: "Int16" } => $"integer|{LuaClrName}",
                        { Namespace: "System", Name: "UInt16" } => $"integer|{LuaClrName}",
                        { Namespace: "System", Name: "Int32" } => $"integer|{LuaClrName}",
                        { Namespace: "System", Name: "UInt32" } => $"integer|{LuaClrName}",
                        { Namespace: "System", Name: "Int64" } => $"integer|{LuaClrName}",
                        { Namespace: "System", Name: "UInt64" } => $"integer|{LuaClrName}",
                        { Namespace: "System", Name: "Single" } => $"number|{LuaClrName}",
                        { Namespace: "System", Name: "Double" } => $"number|{LuaClrName}",
                        _ => LuaClrName,
                    };
                }
            }

            public static string GloballyTable(string[] parts) => parts.Aggregate("_G", (p1, p2) => p1 + $@"['{p2}']");

            private string _defaultTable;
            public string DefaultTable
            {
                get
                {
                    if (_defaultTable != null) { return _defaultTable; }
                    var typeInfo = OriginalType.GetTypeInfo();
                    string[] parts = typeInfo.Namespace.Split('.');
                    if (parts[0] == "Barotrauma") { parts = LuaClrNameWithoutNamespace.Split('.'); }
                    else { parts = parts.Concat(LuaClrNameWithoutNamespace.Split('.')).ToArray(); }
                    return _defaultTable = GloballyTable(parts);
                }
            }

            public void CollectSelfToGlobal()
            {
                if (!LuaClrBasePairs.Exists(pair => pair.derivedMetadata == this))
                {
                    if (OriginalType.BaseType != null)
                    {
                        var metadata = Obtain(OriginalType.BaseType);
                        LuaClrBasePairs.Add((this, metadata));
                        metadata.CollectAllToGlobal();
                    }
                    else
                    {
                        LuaClrBasePairs.Add((this, Empty));
                    }
                }
            }

            public void CollectAllToGlobal()
            {
                CollectSelfToGlobal();
                var allTypes = new List<Type>();
                if (IsArrayIndexer) { allTypes.Add(ArrayElementType); }
                if (IsValueIndexer) { allTypes.Add(ValueType); }
                if (IsKeyValueIndexer) { allTypes.AddRange(new Type[] { KeyValueType.Value.Key, KeyValueType.Value.Value }); }
                foreach (var type in allTypes) { Obtain(type).CollectAllToGlobal(); }
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
            // &:ref/out; *:unknown; []:Array
            name.Append(typeInfo.Name.Substring(0, (subLen > -1) ? subLen : typeInfo.Name.Length)
                .Replace("&", "-")
                .Replace("*", "--")
                .Replace("[]", "---"));
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

        public static void Lualy<T>(string[] majorTable = null, params string[][] minorTables)
            => LualyBase(typeof(T), majorTable, minorTables);
        public static void LualyBase(Type type, string[] majorTable = null, params string[][] minorTables)
        {
            if (type.IsArray) { return; } // disallow to lualy array
            if (LuaClrDefinitions.ContainsKey(GetToken(type))) { Console.WriteLine($"{type.Name} has defined! token: {GetToken(type)}"); return; }

            var luaDocBuilder = new StringBuilder();
            var typeInfo = type.GetTypeInfo();
            var metadata = ClassMetadata.Obtain(type);
            metadata.CollectAllToGlobal();
            string luaClrName = metadata.LuaClrName;
            string table = (majorTable == null) ? metadata.DefaultTable : ClassMetadata.GloballyTable(majorTable);

            luaDocBuilder.AppendLine($"---@meta");
            luaDocBuilder.Append($"---@class {luaClrName}");
            if (typeInfo.BaseType != null) { luaDocBuilder.Append($" : {ClassMetadata.Obtain(typeInfo.BaseType).LuaClrName}"); }
            // this[] for indexer
            foreach (var indexer in (
                from property in typeInfo.DeclaredProperties
                where property.GetIndexParameters().Length == 1
                select property).ToList())
            {
                var param = indexer.GetIndexParameters()[0];
                luaDocBuilder.Append($", {{[{ClassMetadata.Obtain(param.ParameterType).LuaScriptName}]:{ClassMetadata.Obtain(indexer.GetMethod.ReturnType).LuaScriptName}}}");
            }
            ExplanNewLine(luaDocBuilder);

            // insert ops here:
            luaDocBuilder.Append("[placeHolder:operators]");
            ExplanNewLine(luaDocBuilder);

            // excludes BackingField, special name
            foreach (var field in (
                from field in typeInfo.DeclaredFields
                where !field.Name.Contains(">k__BackingField")
                    && !field.IsSpecialName
                    && !Regex.IsMatch(field.Name, @"<.*?>")
                    && !Regex.IsMatch(field.Name, @"\.")
                select field).ToList())
            {
                ExplanField(luaDocBuilder, field);
                ExplanNewLine(luaDocBuilder);
            }

            // excludes indexers and special name
            foreach (var property in (
                from property in typeInfo.DeclaredProperties
                where property.GetIndexParameters().Length == 0
                    && !property.IsSpecialName
                    && !Regex.IsMatch(property.Name, @"<.*?>")
                    && !Regex.IsMatch(property.Name, @"\.")
                select property).ToList())
            {
                ExplanProperty(luaDocBuilder, property);
                ExplanNewLine(luaDocBuilder);
            }

            luaDocBuilder.AppendLine($"{table} = {{}}");
            for (int i = 0; i < minorTables.Length; i++)
            {
                string[] minorTable = minorTables[i];
                luaDocBuilder.AppendLine($"{ClassMetadata.GloballyTable(minorTable)} = {table}");
            }
            ExplanNewLine(luaDocBuilder);

            var methods = (
                from method in typeInfo.DeclaredMethods
                where !method.IsSpecialName
                  && !Regex.IsMatch(method.Name, @"<.*?>")
                  && !Regex.IsMatch(method.Name, @"\.")
                select method).ToArray();

            if (methods.Length > 0)
            {
                foreach (var groupMethodByName in (
                        from method in methods
                        group method by method.Name
                    )
                )
                {
                    foreach (var groupMethodByModifers in (
                            from method in groupMethodByName.ToArray()
                            group method by GetMethodModifiers(method)
                        )
                    )
                    {
                        ExplanAnnotationPrefix(luaDocBuilder);
                        ExplanMethodModifiers(luaDocBuilder, groupMethodByModifers.Key);
                        ExplanNewLine(luaDocBuilder);
                        ExplanMethods(luaDocBuilder, luaClrName, table, groupMethodByModifers.ToArray(), groupMethodByName.Key);
                    }
                }
            }

            MethodInfo[] specialMethods = typeInfo.DeclaredMethods.Where(mi => mi.IsSpecialName).ToArray();
            for (int i = 0; i < specialMethods.Length; i++)
            {
                MethodInfo method = specialMethods[i];
                if (!IsOverloadedOperatorMethod(method)) { continue; }

                switch (method.Name)
                {
                    case "op_UnaryNegation": ExplanUnOps(method, type, "unm"); break;
                    case "op_Addition": ExplanBinOps(method, type, "add"); break;
                    case "op_Ssubtraction": ExplanBinOps(method, type, "sub"); break;
                    case "op_Multiply": ExplanBinOps(method, type, "mul"); break;
                    case "op_Division": ExplanBinOps(method, type, "div"); break;
                    default: break;
                }

                void ExplanBinOps(MethodInfo method, Type type, string op)
                {
                    var tgtOps = (method.GetParameters()[0].ParameterType == type)
                        ? ObtainOverloadedOperatorAnnotations(type)
                        : ObtainOverloadedOperatorAnnotations(method.GetParameters()[0].ParameterType);
                    ExplanAnnotationBinOperator(
                        tgtOps,
                        op,
                        ClassMetadata.Obtain(method.GetParameters()[1].ParameterType).LuaScriptName,
                        ClassMetadata.Obtain(method.ReturnType).LuaScriptName
                    );
                    ExplanNewLine(tgtOps);
                }

                void ExplanUnOps(MethodInfo method, Type type, string op)
                {
                    var tgtOps = ObtainOverloadedOperatorAnnotations(type);
                    ExplanAnnotationUnOperator(
                        tgtOps,
                        op,
                        ClassMetadata.Obtain(method.ReturnType).LuaScriptName
                    );
                    ExplanNewLine(tgtOps);
                }
            }

            var constructors = typeInfo.DeclaredConstructors.ToArray();
            if (constructors.Length > 0)
            {
                foreach (var groupConstructorByModifers in (
                            from ctor in constructors
                            group ctor by GetMethodModifiers(ctor)
                        )
                    )
                {
                    ExplanAnnotationPrefix(luaDocBuilder);
                    ExplanMethodModifiers(luaDocBuilder, groupConstructorByModifers.Key, "Constructor");
                    ExplanNewLine(luaDocBuilder);
                    ExplanMethods(luaDocBuilder, luaClrName, table, constructors, null);

                    ExplanAnnotationPrefix(luaDocBuilder);
                    ExplanMethodModifiers(luaDocBuilder, groupConstructorByModifers.Key, "Constructor");
                    ExplanNewLine(luaDocBuilder);
                    ExplanMethods(luaDocBuilder, luaClrName, table, constructors, "__new");
                }
            }

            LuaClrDefinitions.Add(GetToken(type), luaDocBuilder);
            LualyRecorder.Add(metadata);
        }
    }
}
