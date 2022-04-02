using System;
using System.Linq;
using System.Reflection;
using MoonSharp.Interpreter;
using HarmonyLib;
using System.Collections.Generic;
using System.Text;

namespace Barotrauma
{
	partial class LuaHook
	{
		public LuaHook()
		{
			_hookPrefixMethods = new Dictionary<long, HashSet<Tuple<string, object>>>();
			_hookPostfixMethods = new Dictionary<long, HashSet<Tuple<string, object>>>();
		}

		public class HookFunction
		{
			public string name;
			public string hookName;
			public object function;

			public HookFunction(string n, string hn, object func)
			{
				name = n;
				hookName = hn;
				function = func;
			}
		}

		private Dictionary<string, Dictionary<string, HookFunction>> hookFunctions = new Dictionary<string, Dictionary<string, HookFunction>>();

		private static Dictionary<long, HashSet<Tuple<string, object>>> _hookPrefixMethods;
		private static Dictionary<long, HashSet<Tuple<string, object>>> _hookPostfixMethods;

		private Queue<Tuple<float, object, object[]>> queuedFunctionCalls = new Queue<Tuple<float, object, object[]>>();

		public enum HookMethodType
		{
			Before, After
		}

		static void _hookLuaPatch(MethodBase __originalMethod, object[] __args, object __instance, out LuaResult result, HookMethodType hookMethodType)
		{
			result = new LuaResult(null);

#if CLIENT
				if (GameMain.GameSession?.IsRunning == false && GameMain.IsSingleplayer)
					return;
#endif

			try
			{
				var funcAddr = ((long)__originalMethod.MethodHandle.GetFunctionPointer());
				HashSet<Tuple<string, object>> methodSet = null;
				switch (hookMethodType)
				{
					case HookMethodType.Before:
						_hookPrefixMethods.TryGetValue(funcAddr, out methodSet);
						break;
					case HookMethodType.After:
						_hookPostfixMethods.TryGetValue(funcAddr, out methodSet);
						break;
					default:
						break;
				}

				if (methodSet != null)
				{
					var @params = __originalMethod.GetParameters();
					var ptable = new Dictionary<string, object>();
					for (int i = 0; i < @params.Length; i++)
					{
						ptable.Add(@params[i].Name, __args[i]);
					}

					foreach (var tuple in methodSet)
					{
						result = new LuaResult(GameMain.Lua.lua.Call(tuple.Item2, __instance, ptable));
					}
				}
			}
			catch (Exception ex)
			{
				GameMain.Lua.HandleLuaException(ex);
			}
		}

		static bool HookLuaPatchPrefix(MethodBase __originalMethod, object[] __args, object __instance)
		{
			_hookLuaPatch(__originalMethod, __args, __instance, out LuaResult result, HookMethodType.Before);

			return result.IsNull();
		}

		static bool HookLuaPatchRetPrefix(MethodBase __originalMethod, object[] __args, ref object __result, object __instance)
		{
			_hookLuaPatch(__originalMethod, __args, __instance, out LuaResult result, HookMethodType.Before);

			if (!result.IsNull())
			{
				
				if (__originalMethod is MethodInfo mi)
				{
					__result = result.DynValue().ToObject(mi.ReturnType);
				}
				else
				{
					__result = result.Object();
				}

				return false;
			}

			return true;
		}

		static void HookLuaPatchPostfix(MethodBase __originalMethod, object[] __args, object __instance)
		{
			_hookLuaPatch(__originalMethod, __args, __instance, out LuaResult result, HookMethodType.After);
		}

		static void HookLuaPatchRetPostfix(MethodBase __originalMethod, object[] __args, ref object __result, object __instance)
		{
			_hookLuaPatch(__originalMethod, __args, __instance, out LuaResult result, HookMethodType.After);

			if (!result.IsNull())
			{
				if (__originalMethod is MethodInfo mi)
				{
					__result = result.DynValue().ToObject(mi.ReturnType);
				}
				else
				{
					__result = result.Object();
				}			
			}
		}

		private const BindingFlags DefaultBindingFlags = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
		private static MethodInfo _miHookLuaPatchPrefix = typeof(LuaHook).GetMethod("HookLuaPatchPrefix", BindingFlags.NonPublic | BindingFlags.Static);
		private static MethodInfo _miHookLuaPatchRetPrefix = typeof(LuaHook).GetMethod("HookLuaPatchRetPrefix", BindingFlags.NonPublic | BindingFlags.Static);
		private static MethodInfo _miHookLuaPatchPostfix = typeof(LuaHook).GetMethod("HookLuaPatchPostfix", BindingFlags.NonPublic | BindingFlags.Static);
		private static MethodInfo _miHookLuaPatchRetPostfix = typeof(LuaHook).GetMethod("HookLuaPatchRetPostfix", BindingFlags.NonPublic | BindingFlags.Static);
		public void HookMethod(string identifier, string className, string methodName, string[] parameterNames, object hookMethod, HookMethodType hookMethodType = HookMethodType.Before)
		{
			var classType = LuaUserData.GetType(className);

			if (classType == null)
            {
				GameMain.Lua.HandleLuaException(new Exception($"Tried to use HookMethod with an invalid class name '{className}'."));
				return;
			}

			MethodInfo methodInfo = null;

			if (parameterNames != null)
			{
				Type[] parameterTypes = parameterNames.Select(x => LuaUserData.GetType(x)).ToArray();
				methodInfo = classType.GetMethod(methodName, DefaultBindingFlags, null, parameterTypes, null);
			}
			else
			{
				methodInfo = classType.GetMethod(methodName, DefaultBindingFlags);
			}

			if (methodInfo == null)
			{
				string parameterNamesStr = parameterNames == null ? "" : string.Join(", ", parameterNames == null);
				GameMain.Lua.HandleLuaException(new Exception($"Method '{methodName}' with parameters '{parameterNamesStr}' not found in class '{className}'"));
				return;
			}

			identifier = identifier.ToLower();
			var funcAddr = ((long)methodInfo.MethodHandle.GetFunctionPointer());
			var patches = Harmony.GetPatchInfo(methodInfo);

			if (hookMethodType == HookMethodType.Before)
			{
				if (methodInfo.ReturnType == typeof(void))
				{
					if (patches == null || patches.Prefixes == null || patches.Prefixes.Find(patch => patch.PatchMethod == _miHookLuaPatchPrefix) == null)
					{
						GameMain.Lua.harmony.Patch(methodInfo, prefix: new HarmonyMethod(_miHookLuaPatchPrefix));
					}
				}
				else
				{
					if (patches == null || patches.Prefixes == null || patches.Prefixes.Find(patch => patch.PatchMethod == _miHookLuaPatchRetPrefix) == null)
					{
						GameMain.Lua.harmony.Patch(methodInfo, prefix: new HarmonyMethod(_miHookLuaPatchRetPrefix));
					}
				}

				if (_hookPrefixMethods.TryGetValue(funcAddr, out HashSet<Tuple<string, object>> methodSet))
				{
					if (identifier != "")
					{
						methodSet.RemoveWhere(tuple => tuple.Item1 == identifier);
					}
                    if (hookMethod != null)
                    {
						methodSet.Add(Tuple.Create(identifier, hookMethod));
                    }
				}
				else if (hookMethod != null)
				{
					_hookPrefixMethods.Add(funcAddr, new HashSet<Tuple<string, object>>() { Tuple.Create(identifier, hookMethod) });
				}

			}
			else if (hookMethodType == HookMethodType.After)
			{
				if (methodInfo.ReturnType == typeof(void))
				{
					if (patches == null || patches.Postfixes == null || patches.Postfixes.Find(patch => patch.PatchMethod == _miHookLuaPatchPostfix) == null)
					{
						GameMain.Lua.harmony.Patch(methodInfo, postfix: new HarmonyMethod(_miHookLuaPatchPostfix));
					}
				}
				else
				{
					if (patches == null || patches.Postfixes == null || patches.Postfixes.Find(patch => patch.PatchMethod == _miHookLuaPatchRetPostfix) == null)
					{
						GameMain.Lua.harmony.Patch(methodInfo, postfix: new HarmonyMethod(_miHookLuaPatchRetPostfix));
					}
				}

				if (_hookPostfixMethods.TryGetValue(funcAddr, out HashSet<Tuple<string, object>> methodSet))
				{
                    if (identifier != "")
                    {
						methodSet.RemoveWhere(tuple => tuple.Item1 == identifier);
                    }
					if (hookMethod != null)
					{
						methodSet.Add(Tuple.Create(identifier, hookMethod));
					}
				}
				else if (hookMethod != null)
				{
					_hookPostfixMethods.Add(funcAddr, new HashSet<Tuple<string, object>>() { Tuple.Create(identifier, hookMethod) });
				}

			}

		}

		public void HookMethod(string className, string methodName, object hookMethod, HookMethodType hookMethodType = HookMethodType.Before)
		{
			HookMethod("", className, methodName, null, hookMethod, hookMethodType);
		}

		public void HookMethod(string className, string methodName, string[] parameterNames, object hookMethod, HookMethodType hookMethodType = HookMethodType.Before)
		{
			HookMethod("", className, methodName, parameterNames, hookMethod, hookMethodType);
		}

		public void HookMethod(string identifier, string className, string methodName, object hookMethod, HookMethodType hookMethodType = HookMethodType.Before)
		{
			HookMethod(identifier, className, methodName, null, hookMethod, hookMethodType);
		}

		public void EnqueueFunction(object function, params object[] args)
		{
			queuedFunctionCalls.Enqueue(new Tuple<float, object, object[]>(0, function, args));
		}

		public void EnqueueTimedFunction(float time, object function, params object[] args)
		{
			queuedFunctionCalls.Enqueue(new Tuple<float, object, object[]>(time, function, args));
		}

		public void Add(string name, string hookName, object function)
		{
			if (name == null && hookName == null && function == null) return;

			name = name.ToLower();

			if (!hookFunctions.ContainsKey(name))
				hookFunctions.Add(name, new Dictionary<string, HookFunction>());

			hookFunctions[name][hookName] = new HookFunction(name, hookName, function);
		}

		public void Remove(string name, string hookName)
		{
			if (name == null && hookName == null) return;

			name = name.ToLower();

			if (!hookFunctions.ContainsKey(name))
				return;

			if (hookFunctions[name].ContainsKey(hookName))
				hookFunctions[name].Remove(hookName);
		}

		public void Update()
		{
			try
			{
				if (queuedFunctionCalls.TryPeek(out Tuple<float, object, object[]> result))
				{
					if (Timing.TotalTime >= result.Item1)
					{
						GameMain.Lua.CallFunction(result.Item2, result.Item3);

						queuedFunctionCalls.Dequeue();
					}
				}
			}
			catch (Exception ex)
			{
				GameMain.Lua.HandleLuaException(ex, $"queuedFunctionCalls was {queuedFunctionCalls}");
			}
		}

		public object Call(string name, params object[] args)
		{
#if CLIENT
				if (GameMain.GameSession?.IsRunning == false && GameMain.IsSingleplayer)
					return null;
#endif
			if (GameMain.Lua == null) return null;
			if (name == null) return null;
			if (args == null) { args = new object[] { }; }

			name = name.ToLower();

			if (!hookFunctions.ContainsKey(name))
				return null;

			object lastResult = null;

			foreach (HookFunction hf in hookFunctions[name].Values)
			{
				try
				{
					if (hf.function is Closure)
					{
						var result = GameMain.Lua.lua.Call(hf.function, args);
						if (!result.IsNil())
							lastResult = result;
					}
				}
				catch (Exception e)
				{
					StringBuilder argsSb = new StringBuilder();
					foreach (var arg in args)
					{
						argsSb.Append(arg + " ");
					}

					GameMain.Lua.HandleLuaException(e, $"Error in Hook '{name}'->'{hf.hookName}', with args '{argsSb}'");
				}
			}

			return lastResult;
		}
	}
}