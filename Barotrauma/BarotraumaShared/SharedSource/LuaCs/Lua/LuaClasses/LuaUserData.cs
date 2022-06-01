using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace Barotrauma
{
	partial class LuaUserData
	{
		public static Type GetType(string typeName)
		{
			var type = Type.GetType(typeName);
			if (type != null) return type;
			foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
			{
				if (CsScriptFilter.LoadedAssemblyName.Contains(a.GetName().Name))
                {
					var attrs = a.GetCustomAttributes<AssemblyMetadataAttribute>();
					var revision = attrs.FirstOrDefault(attr => attr.Key == "Revision")?.Value;
					if (revision != null && int.Parse(revision) != (int)CsScriptBase.Revision[a.GetName().Name]) { continue; }
                }
				type = a.GetType(typeName);
				if (type != null)
					return type;
			}
			return null;
		}

		public static IUserDataDescriptor RegisterType(string typeName)
		{
			Type type = GetType(typeName);

			if (type == null)
			{
				throw new ScriptRuntimeException($"Tried to register a type that doesn't exist: {typeName}.");
			}

			return UserData.RegisterType(type);
		}

		public static void UnregisterType(string typeName)
		{
			Type type = GetType(typeName);

			if (type == null)
			{
				throw new ScriptRuntimeException($"Tried to unregister a type that doesn't exist: {typeName}.");
			}

			UserData.UnregisterType(type);
		}
		public static IUserDataDescriptor RegisterGenericType(string typeName, params string[] typeNameArguements)
		{
			Type type = GetType(typeName);
			Type[] typeArguements = typeNameArguements.Select(x => GetType(x)).ToArray();
			Type genericType = type.MakeGenericType(typeArguements);
			return UserData.RegisterType(genericType);
		}

		public static void UnregisterGenericType(string typeName, params string[] typeNameArguements)
		{
			Type type = GetType(typeName);
			Type[] typeArguements = typeNameArguements.Select(x => GetType(x)).ToArray();
			Type genericType = type.MakeGenericType(typeArguements);
			UserData.UnregisterType(genericType);
		}

		private static bool IsType<T>(object obj) { return obj is T; }

		public static bool IsTargetType(object obj, string typeName)
		{
			var type = GetType(typeName);
			MethodInfo method = typeof(LuaUserData).GetMethod(nameof(IsType), BindingFlags.NonPublic | BindingFlags.Static);
			MethodInfo generic = method.MakeGenericMethod(type);
			return (bool)generic.Invoke(null, new object[] { obj });
		}

		public static object CreateStatic(string typeName)
		{
			Type type = GetType(typeName);

			if (type == null)
			{
				throw new ScriptRuntimeException($"Tried to create a static userdata of a type that doesn't exist: {typeName}.");
			}

			MethodInfo method = typeof(UserData).GetMethod(nameof(UserData.CreateStatic), 1, new Type[0]);
			MethodInfo generic = method.MakeGenericMethod(type);
			return generic.Invoke(null, null);
		}

		public static object CreateEnumTable(string typeName)
        {
			Type type = GetType(typeName);

			if (type == null)
			{
				throw new ScriptRuntimeException($"Tried to create an enum table with a type that doesn't exist:: {typeName}.");
			}

			Dictionary<string, object> result = new Dictionary<string, object>();

			foreach (var value in Enum.GetValues(type))
            {
				string name = Enum.GetName(type, value);

				result[name] = value;
            }

			return result;
		}

		private static FieldInfo FindFieldRecursively(Type type, string fieldName)
        {
			var field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

			if (field == null && type.BaseType != null)
            {
				return FindFieldRecursively(type.BaseType, fieldName);
            }

			return field;
		}

		public static void MakeFieldAccessible(IUserDataDescriptor IUUD, string fieldName)
		{
			if (IUUD == null)
			{
				throw new ScriptRuntimeException($"Tried to use a UserDataDescriptor that is null to make {fieldName} accessible.");
			}

			var descriptor = (StandardUserDataDescriptor)IUUD;
			var field = IUUD.Type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

			if (field == null)
            {
				field = FindFieldRecursively(IUUD.Type, fieldName);
            }

			if (field == null)
			{
				throw new ScriptRuntimeException($"Tried to make field '{fieldName}' accessible, but the field doesn't exist.");
			}

			descriptor.RemoveMember(fieldName);
			descriptor.AddMember(fieldName, new FieldMemberDescriptor(field, InteropAccessMode.Default));
		}

		private static MethodInfo FindMethodRecursively(Type type, string methodName)
		{
			var method = type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

			if (method == null && type.BaseType != null)
			{
				return FindMethodRecursively(type.BaseType, methodName);
			}

			return method;
		}

		public static void MakeMethodAccessible(IUserDataDescriptor IUUD, string methodName)
		{
			if (IUUD == null)
			{
				throw new ScriptRuntimeException($"Tried to use a UserDataDescriptor that is null to make {methodName} accessible.");
			}

			var descriptor = (StandardUserDataDescriptor)IUUD;
			var method = IUUD.Type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

			if (method == null)
			{
				method = FindMethodRecursively(IUUD.Type, methodName);
			}

			if (method == null)
			{
				throw new ScriptRuntimeException($"Tried to make method '{methodName}' accessible, but the method doesn't exist.");
			}

			descriptor.RemoveMember(methodName);
			descriptor.AddMember(methodName, new MethodMemberDescriptor(method, InteropAccessMode.Default));
		}

		public static void AddMethod(IUserDataDescriptor IUUD, string methodName, object function)
		{
			if (IUUD == null)
			{
				throw new ScriptRuntimeException($"Tried to use a UserDataDescriptor that is null to add method {methodName}.");
			}

			var descriptor = (StandardUserDataDescriptor)IUUD;
			descriptor.RemoveMember(methodName);
			descriptor.AddMember(methodName, new ObjectCallbackMemberDescriptor(methodName, (object arg1, ScriptExecutionContext arg2, CallbackArguments arg3) =>
			{
				if (GameMain.LuaCs != null)
					return GameMain.LuaCs.CallLuaFunction(function, arg3.GetArray());
				return null;
			}));
		}

		public static void AddField(IUserDataDescriptor IUUD, string fieldName, DynValue value)
		{
			if (IUUD == null)
			{
				throw new ScriptRuntimeException($"Tried to use a UserDataDescriptor that is null to add field {fieldName}.");
			}

			var descriptor = (StandardUserDataDescriptor)IUUD;
			descriptor.RemoveMember(fieldName);
			descriptor.AddMember(fieldName, new DynValueMemberDescriptor(fieldName, value));
		}

		public static void RemoveMember(IUserDataDescriptor IUUD, string memberName)
		{
			if (IUUD == null)
			{
				throw new ScriptRuntimeException($"Tried to use a UserDataDescriptor that is null to remove the member {memberName}.");
			}

			var descriptor = (StandardUserDataDescriptor)IUUD;
			descriptor.RemoveMember(memberName);
		}

		/// <summary>
		/// Converts a Lua value to a CLR object of a desired type and wraps it in a userdata.
		/// Example: a Lua script needs to pass a List`1 to a CLR method expecting System.Object, MoonSharp gets
		/// in the way by converting the List`1 to a MoonSharp.Interpreter.Table and breaking everything.
		/// Wrapping the value in a userdata preserves the original type during conversions.
		/// </summary>
		/// <param name="scriptObject">Lua value to convert and wrap in a userdata.</param>
		/// <param name="desiredType">The CLR type of the object to convert the Lua value to. Uses MoonSharp ScriptToClr converters. Lua scripts can obtain Types from descriptors.</param>
		/// <returns>A userdata that wraps the Lua value converted to an object of the desired type.</returns>
		public static DynValue CreateUserDataOfType(DynValue scriptObject, Type desiredType)
		{
			return UserData.Create(scriptObject.ToObject(desiredType));
		}
	}
}