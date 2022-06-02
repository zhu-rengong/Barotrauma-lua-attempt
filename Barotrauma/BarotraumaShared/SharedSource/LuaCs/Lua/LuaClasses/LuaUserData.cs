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
		/// See <see cref="CreateUserDataFromType"/>.
		/// </summary>
		/// <param name="scriptObject">Lua value to convert and wrap in a userdata.</param>
		/// <param name="desiredTypeDescriptor">Descriptor of the type of the object to convert the Lua value to. Uses MoonSharp ScriptToClr converters.</param>
		/// <returns>A userdata that wraps the Lua value converted to an object of the desired type as described by <paramref name="desiredTypeDescriptor"/>.</returns>
		public static DynValue CreateUserDataFromDescriptor(DynValue scriptObject, IUserDataDescriptor desiredTypeDescriptor)
		{
			return UserData.Create(scriptObject.ToObject(desiredTypeDescriptor.Type), desiredTypeDescriptor);
		}

		/// <summary>
		/// Converts a Lua value to a CLR object of a desired type and wraps it in a userdata.
		/// If the type is not registered, then a new <see cref="MoonSharp.Interpreter.Interop.StandardUserDataDescriptor"/> will be created and used.
		/// The goal of this method is to allow Lua scripts to create userdata to wrap certain data without having to register types.
		/// <remarks>Wrapping the value in a userdata preserves the original type during script-to-CLR conversions.</remarks>
		/// <example>A Lua script needs to pass a List`1 to a CLR method expecting System.Object, MoonSharp gets
		/// in the way by converting the List`1 to a MoonSharp.Interpreter.Table and breaking everything.
		/// Registering the List`1 type can break other scripts relying on default converters, so instead
		/// it is better to manually wrap the List`1 object into a userdata.
		/// </example>
		/// </summary>
		/// <param name="scriptObject">Lua value to convert and wrap in a userdata.</param>
		/// <param name="desiredType">Type describing the CLR type of the object to convert the Lua value to.</param>
		/// <returns>A userdata that wraps the Lua value converted to an object of the desired type.</returns>
		public static DynValue CreateUserDataFromType(DynValue scriptObject, Type desiredType)
		{
			IUserDataDescriptor descriptor = UserData.GetDescriptorForType(desiredType, true);
			descriptor ??= new StandardUserDataDescriptor(desiredType, InteropAccessMode.Default);
			return CreateUserDataFromDescriptor(scriptObject, descriptor);
		}
	}
}