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
				GameMain.Lua.HandleLuaException(new Exception($"Tried to register a type that doesn't exist: {typeName}."));
				return null;
			}

			return UserData.RegisterType(type);
		}

		public static void UnregisterType(string typeName)
		{
			Type type = GetType(typeName);

			if (type == null)
			{
				GameMain.Lua.HandleLuaException(new Exception($"Tried to unregister a type that doesn't exist: {typeName}."));
				return;
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
				GameMain.Lua.HandleLuaException(new Exception($"Tried to create a static userdata of a type that doesn't exist: {typeName}."));
				return null;
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
				GameMain.Lua.HandleLuaException(new Exception($"Tried to create an enum table with a type that doesn't exist:: {typeName}."));
				return null;
			}

			Dictionary<string, object> result = new Dictionary<string, object>();

			foreach (var value in Enum.GetValues(type))
            {
				string name = Enum.GetName(type, value);

				result[name] = value;
            }

			return result;
		}

		public static void MakeFieldAccessible(IUserDataDescriptor IUUD, string fieldName)
		{
			if (IUUD == null)
			{
				GameMain.Lua.HandleLuaException(new Exception($"Tried to use a UserDataDescriptor that is null to make {fieldName} accessible."));
				return;
			}

			var descriptor = (StandardUserDataDescriptor)IUUD;
			var field = IUUD.Type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

			if (field == null)
			{
				GameMain.Lua.HandleLuaException(new Exception($"Tried to make field '{fieldName}' accessible, but the field doesn't exist."));
				return;
			}

			descriptor.RemoveMember(fieldName);
			descriptor.AddMember(fieldName, new FieldMemberDescriptor(field, InteropAccessMode.Default));
		}

		public static void MakeMethodAccessible(IUserDataDescriptor IUUD, string methodName)
		{
			if (IUUD == null)
			{
				GameMain.Lua.HandleLuaException(new Exception($"Tried to use a UserDataDescriptor that is null to make {methodName} accessible."));
				return;
			}

			var descriptor = (StandardUserDataDescriptor)IUUD;
			var method = IUUD.Type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

			if (method == null)
			{
				GameMain.Lua.HandleLuaException(new Exception($"Tried to make method '{method}' accessible, but the method doesn't exist."));
				return;
			}

			descriptor.RemoveMember(methodName);
			descriptor.AddMember(methodName, new MethodMemberDescriptor(method, InteropAccessMode.Default));
		}

		public static void AddMethod(IUserDataDescriptor IUUD, string methodName, object function)
		{
			if (IUUD == null)
			{
				GameMain.Lua.HandleLuaException(new Exception($"Tried to use a UserDataDescriptor that is null to add method {methodName}."));
				return;
			}

			var descriptor = (StandardUserDataDescriptor)IUUD;
			descriptor.RemoveMember(methodName);
			descriptor.AddMember(methodName, new ObjectCallbackMemberDescriptor(methodName, (object arg1, ScriptExecutionContext arg2, CallbackArguments arg3) =>
			{
				if (GameMain.Lua != null)
					return GameMain.Lua.CallFunction(function, arg3.GetArray());
				return null;
			}));
		}

		public static void RemoveMember(IUserDataDescriptor IUUD, string memberName)
		{
			if (IUUD == null)
			{
				GameMain.Lua.HandleLuaException(new Exception($"Tried to use a UserDataDescriptor that is null to remove the member {memberName}."));
				return;
			}

			var descriptor = (StandardUserDataDescriptor)IUUD;
			descriptor.RemoveMember(memberName);
		}
	}
}