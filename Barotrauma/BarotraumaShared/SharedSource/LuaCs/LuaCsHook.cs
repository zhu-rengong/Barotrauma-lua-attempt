using System;
using System.Linq;
using System.Reflection;
using MoonSharp.Interpreter;
using HarmonyLib;
using System.Collections.Generic;
using System.Text;

namespace Barotrauma
{
	public enum HookMethodType
	{
		Before, After
	}

	public delegate object CsHookDelegate(params object[] args);
	public delegate object CsPatchDelegate(object self, params object[] args);

	public abstract class LuaCsHookWrapper {

		protected LuaCsHook _hook;

		public LuaCsHookWrapper(LuaCsHook hook)
		{
			_hook = hook;
		}

		public void Remove(string name, string hookName) =>
			_hook.RemoveHook(name, hookName);

		public void Update() =>
			_hook.Update();

		public object Call(string name, params object[] args) =>
			_hook.Call(name, args);

	}

	public class LuaCsHook
	{
		private class LuaHookFunction
		{
			public string name;
			public string hookName;
			public object function;

			public LuaHookFunction(string n, string hn, object func)
			{
				name = n;
				hookName = hn;
				function = func;
			}
		}

		private Dictionary<string, Dictionary<string, LuaHookFunction>> luaHookFunctions;
		private Dictionary<string, Dictionary<string, (CsHookDelegate, ACsMod)>> csHookFunctions;

		private Dictionary<long, HashSet<(string, object)>> luaHookPrefixMethods;
		private Dictionary<long, HashSet<(string, object)>> luaHookPostfixMethods;
		private Dictionary<long, HashSet<(string, CsPatchDelegate, ACsMod)>> csHookPrefixMethods;
		private Dictionary<long, HashSet<(string, CsPatchDelegate, ACsMod)>> csHookPostfixMethods;

		private Queue<Tuple<float, object, object[]>> queuedFunctionCalls;

		private LuaCsHook() {
			luaHookFunctions = new Dictionary<string, Dictionary<string, LuaHookFunction>>();
			csHookFunctions = new Dictionary<string, Dictionary<string, (CsHookDelegate, ACsMod)>>();

			luaHookPrefixMethods = new Dictionary<long, HashSet<(string, object)>>();
			luaHookPostfixMethods = new Dictionary<long, HashSet<(string, object)>>();
			csHookPrefixMethods = new Dictionary<long, HashSet<(string, CsPatchDelegate, ACsMod)>>();
			csHookPostfixMethods = new Dictionary<long, HashSet<(string, CsPatchDelegate, ACsMod)>>();

			queuedFunctionCalls = new Queue<Tuple<float, object, object[]>>();
		}

		private static LuaCsHook _inst;
		static LuaCsHook() => _inst = new LuaCsHook();
		public static LuaCsHook Instance { get => _inst; }


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
				HashSet<(string, object)> methodSet = null;
				switch (hookMethodType)
				{
					case HookMethodType.Before:
						_inst.luaHookPrefixMethods.TryGetValue(funcAddr, out methodSet);
						break;
					case HookMethodType.After:
						_inst.luaHookPostfixMethods.TryGetValue(funcAddr, out methodSet);
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
						var luaResult = new LuaResult(GameMain.LuaCs.lua.Call(tuple.Item2, __instance, ptable));
						if (!luaResult.IsNull()) result = luaResult;
					}
				}
			}
			catch (Exception ex)
			{
				GameMain.LuaCs.HandleException(ex);
			}
		}

		static void _hookCsPatch(MethodBase __originalMethod, object[] __args, object __instance, ref object result, HookMethodType hookMethodType)
		{
#if CLIENT
		if (GameMain.GameSession?.IsRunning == false && GameMain.IsSingleplayer)
			return;
#endif
			try
			{
				var funcAddr = ((long)__originalMethod.MethodHandle.GetFunctionPointer());
				HashSet<(string, CsPatchDelegate, ACsMod)> methodSet = null;
				switch (hookMethodType)
				{
					case HookMethodType.Before:
						_inst.csHookPrefixMethods.TryGetValue(funcAddr, out methodSet);
						break;
					case HookMethodType.After:
						_inst.csHookPostfixMethods.TryGetValue(funcAddr, out methodSet);
						break;
					default:
						break;
				}

				if (methodSet != null)
				{
					var @params = __originalMethod.GetParameters();
					var args = new Dictionary<string, object>();
					for (int i = 0; i < @params.Length; i++)
					{
						args.Add(@params[i].Name, __args[i]);
					}

					var outOfSocpe = new HashSet<(string, CsPatchDelegate, ACsMod)>();
					foreach (var tuple in methodSet)
					{
						if (tuple.Item3 != null && tuple.Item3.IsDisposed)
							outOfSocpe.Add(tuple);
						else
							result = tuple.Item2(__instance, args) ?? result;
					}
					foreach (var tuple in outOfSocpe) methodSet.Remove(tuple);
				}
			}
			catch (Exception ex)
			{
				GameMain.LuaCs.HandleException(ex, exceptionType: LuaCsSetup.ExceptionType.CSharp);
			}
		}


		private static bool HookLuaPatchPrefix(MethodBase __originalMethod, object[] __args, object __instance)
		{
			_hookLuaPatch(__originalMethod, __args, __instance, out LuaResult result, HookMethodType.Before);

			return result.IsNull();
		}

		private static bool HookLuaPatchRetPrefix(MethodBase __originalMethod, object[] __args, ref object __result, object __instance)
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

		private static void HookLuaPatchPostfix(MethodBase __originalMethod, object[] __args, object __instance)
		{
			_hookLuaPatch(__originalMethod, __args, __instance, out LuaResult result, HookMethodType.After);
		}
		private static void HookLuaPatchRetPostfix(MethodBase __originalMethod, object[] __args, ref object __result, object __instance)
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

		private static bool HookCsPatchPrefix(MethodBase __originalMethod, object[] __args, ref object __result, object __instance)
		{
			_hookCsPatch(__originalMethod, __args, __instance, ref __result, HookMethodType.Before);

			if (__result != null) return false;
			else return true;
		}
		private static void HookCsPatchPostfix(MethodBase __originalMethod, object[] __args, ref object __result, object __instance) =>
			_hookCsPatch(__originalMethod, __args, __instance, ref __result, HookMethodType.After);


		private const BindingFlags DefaultBindingFlags = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
		private static MethodInfo _miHookLuaPatchPrefix = typeof(LuaCsHook).GetMethod("HookLuaPatchPrefix", BindingFlags.NonPublic | BindingFlags.Static);
		private static MethodInfo _miHookLuaPatchRetPrefix = typeof(LuaCsHook).GetMethod("HookLuaPatchRetPrefix", BindingFlags.NonPublic | BindingFlags.Static);
		private static MethodInfo _miHookLuaPatchPostfix = typeof(LuaCsHook).GetMethod("HookLuaPatchPostfix", BindingFlags.NonPublic | BindingFlags.Static);
		private static MethodInfo _miHookLuaPatchRetPostfix = typeof(LuaCsHook).GetMethod("HookLuaPatchRetPostfix", BindingFlags.NonPublic | BindingFlags.Static);

		private static MethodInfo _miHookCsPatchRetPrefix = typeof(LuaCsHook).GetMethod("HookCsPatchPrefix", BindingFlags.NonPublic | BindingFlags.Static);
		private static MethodInfo _miHookCsPatchRetPostfix = typeof(LuaCsHook).GetMethod("HookCsPatchPostfix", BindingFlags.NonPublic | BindingFlags.Static);


		private static MethodInfo ResolveMethod(string where, string className, string methodName, string[] parameterNames)
        {
			var classType = LuaUserData.GetType(className);

			if (classType == null)
			{
				GameMain.LuaCs.HandleException(new Exception($"Tried to use {where} with an invalid class name '{className}'."));
				return null;
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
				GameMain.LuaCs.HandleException(new Exception($"Method '{methodName}' with parameters '{parameterNamesStr}' not found in class '{className}'"));
			}

			return methodInfo;
		}

		public void HookLuaMethod(string identifier, string className, string methodName, string[] parameterNames, object hookMethod, HookMethodType hookMethodType = HookMethodType.Before)
		{

			MethodInfo methodInfo = ResolveMethod("HookMethod", className, methodName, parameterNames);
			if (methodInfo == null) return;

			identifier = identifier.ToLower();
			var funcAddr = ((long)methodInfo.MethodHandle.GetFunctionPointer());
			var patches = Harmony.GetPatchInfo(methodInfo);

			if (hookMethodType == HookMethodType.Before)
			{
				if (methodInfo.ReturnType == typeof(void))
				{
					if (patches == null || patches.Prefixes == null || patches.Prefixes.Find(patch => patch.PatchMethod == _miHookLuaPatchPrefix) == null)
					{
						GameMain.LuaCs.harmony.Patch(methodInfo, prefix: new HarmonyMethod(_miHookLuaPatchPrefix));
					}
				}
				else
				{
					if (patches == null || patches.Prefixes == null || patches.Prefixes.Find(patch => patch.PatchMethod == _miHookLuaPatchRetPrefix) == null)
					{
						GameMain.LuaCs.harmony.Patch(methodInfo, prefix: new HarmonyMethod(_miHookLuaPatchRetPrefix));
					}
				}

				if (luaHookPrefixMethods.TryGetValue(funcAddr, out HashSet<(string, object)> methodSet))
				{
					if (identifier != "")
					{
						methodSet.RemoveWhere(tuple => tuple.Item1 == identifier);
					}
					if (hookMethod != null)
					{
						methodSet.Add((identifier, hookMethod));
					}
				}
				else if (hookMethod != null)
				{
					luaHookPrefixMethods.Add(funcAddr, new HashSet<(string, object)>() { (identifier, hookMethod) });
				}

			}
			else if (hookMethodType == HookMethodType.After)
			{
				if (methodInfo.ReturnType == typeof(void))
				{
					if (patches == null || patches.Postfixes == null || patches.Postfixes.Find(patch => patch.PatchMethod == _miHookLuaPatchPostfix) == null)
					{
						GameMain.LuaCs.harmony.Patch(methodInfo, postfix: new HarmonyMethod(_miHookLuaPatchPostfix));
					}
				}
				else
				{
					if (patches == null || patches.Postfixes == null || patches.Postfixes.Find(patch => patch.PatchMethod == _miHookLuaPatchRetPostfix) == null)
					{
						GameMain.LuaCs.harmony.Patch(methodInfo, postfix: new HarmonyMethod(_miHookLuaPatchRetPostfix));
					}
				}

				if (luaHookPostfixMethods.TryGetValue(funcAddr, out HashSet<(string, object)> methodSet))
				{
					if (identifier != "")
					{
						methodSet.RemoveWhere(tuple => tuple.Item1 == identifier);
					}
					if (hookMethod != null)
					{
						methodSet.Add((identifier, hookMethod));
					}
				}
				else if (hookMethod != null)
				{
					luaHookPostfixMethods.Add(funcAddr, new HashSet<(string, object)>() { (identifier, hookMethod) });
				}

			}

		}

		public void HookCsMethod(string identifier, MethodInfo method, CsPatchDelegate hook, HookMethodType hookType = HookMethodType.Before, ACsMod owner = null)
		{
			if (identifier == null || method == null || hook == null) throw new ArgumentNullException("Identifier, Method and Hook arguments must not be null.");

			var funcAddr = ((long)method.MethodHandle.GetFunctionPointer());
			var patches = Harmony.GetPatchInfo(method);

			if (hookType == HookMethodType.Before)
			{
				if (patches == null || patches.Prefixes == null || patches.Prefixes.Find(patch => patch.PatchMethod == _miHookCsPatchRetPrefix) == null)
				{
					GameMain.LuaCs.harmony.Patch(method, prefix: new HarmonyMethod(_miHookCsPatchRetPrefix));
				}

				if (csHookPrefixMethods.TryGetValue(funcAddr, out HashSet<(string, CsPatchDelegate, ACsMod)> methodSet))
				{
					methodSet.RemoveWhere(tuple => tuple.Item1 == identifier);
					methodSet.Add((identifier, hook, owner));
				}
				else if (hook != null)
				{
					csHookPrefixMethods.Add(funcAddr, new HashSet<(string, CsPatchDelegate, ACsMod)>() { (identifier, hook, owner) });
				}

			}
			else if (hookType == HookMethodType.After)
			{
				if (patches == null || patches.Postfixes == null || patches.Postfixes.Find(patch => patch.PatchMethod == _miHookCsPatchRetPrefix) == null)
				{
					GameMain.LuaCs.harmony.Patch(method, postfix: new HarmonyMethod(_miHookCsPatchRetPostfix));
				}

				if (csHookPostfixMethods.TryGetValue(funcAddr, out HashSet<(string, CsPatchDelegate, ACsMod)> methodSet))
				{
					methodSet.RemoveWhere(tuple => tuple.Item1 == identifier);
					methodSet.Add((identifier, hook, owner));
				}
				else if (hook != null)
				{
					csHookPostfixMethods.Add(funcAddr, new HashSet<(string, CsPatchDelegate, ACsMod)>() { (identifier, hook, owner) });
				}

			}

		}

		public void RemoveLuaPatch(string identifier, string className, string methodName, string[] parameterNames, HookMethodType hookType = HookMethodType.Before)
        {
			MethodInfo methodInfo = ResolveMethod("UnhookMathod", className, methodName, parameterNames);
			if (methodInfo == null) return;
			RemovePatch(identifier, methodInfo, hookType);
		}
		public void RemovePatch(string identifier, MethodInfo method, HookMethodType hookType = HookMethodType.Before)
		{
			var funcAddr = ((long)method.MethodHandle.GetFunctionPointer());

			Dictionary<long, HashSet<(string, object)>> luaMethods;
			Dictionary<long, HashSet<(string, CsPatchDelegate, ACsMod)>> csMethods;
			if (hookType == HookMethodType.Before)
			{
				luaMethods = luaHookPrefixMethods;
				csMethods = csHookPrefixMethods;
			}
			else if (hookType == HookMethodType.After)
			{
				luaMethods = luaHookPostfixMethods;
				csMethods = csHookPostfixMethods;
			}
			else throw null;

			if (luaMethods.ContainsKey(funcAddr)) luaMethods[funcAddr]?.RemoveWhere(t => t.Item1 == identifier);
			if (csMethods.ContainsKey(funcAddr)) csMethods[funcAddr]?.RemoveWhere(t => t.Item1 == identifier);
		}


		public void EnqueueLuaFunction(object function, params object[] args)
		{
			queuedFunctionCalls.Enqueue(new Tuple<float, object, object[]>(0, function, args));
		}

		public void EnqueueTimedLuaFunction(float time, object function, params object[] args)
		{
			queuedFunctionCalls.Enqueue(new Tuple<float, object, object[]>(time, function, args));
		}


		public void AddLuaHook(string name, string hookName, object function)
		{
			if (name == null || hookName == null || function == null) return;

			name = name.ToLower();

			if (!luaHookFunctions.ContainsKey(name))
				luaHookFunctions.Add(name, new Dictionary<string, LuaHookFunction>());

			luaHookFunctions[name][hookName] = new LuaHookFunction(name, hookName, function);
		}

		public void AddCsHook(string name, string hookName, CsHookDelegate hook, ACsMod owner = null)
		{
			if (name == null || hookName == null || hook == null) throw new ArgumentNullException("Names and Hook must not be null");

			if (!csHookFunctions.ContainsKey(name))
				csHookFunctions.Add(name, new Dictionary<string, (CsHookDelegate, ACsMod)>());

			csHookFunctions[name][hookName] = (hook, owner);
		}

		public void RemoveHook(string name, string hookName)
		{
			if (name == null || hookName == null) return;

			name = name.ToLower();

			if (luaHookFunctions.ContainsKey(name) && luaHookFunctions[name].ContainsKey(hookName))
				luaHookFunctions[name].Remove(hookName);
			if (csHookFunctions.ContainsKey(name) && csHookFunctions[name].ContainsKey(hookName))
				csHookFunctions[name].Remove(hookName);
		}

		public void Clear()
        {
			luaHookFunctions.Clear();
			csHookFunctions.Clear();

			luaHookPrefixMethods.Clear();
			luaHookPostfixMethods.Clear();
			csHookPrefixMethods.Clear();
			csHookPostfixMethods.Clear();

			queuedFunctionCalls.Clear();
		}


		public void Update()
		{
			try
			{
				if (queuedFunctionCalls.TryPeek(out Tuple<float, object, object[]> result))
				{
					if (Timing.TotalTime >= result.Item1)
					{
						GameMain.LuaCs.CallLuaFunction(result.Item2, result.Item3);

						queuedFunctionCalls.Dequeue();
					}
				}
			}
			catch (Exception ex)
			{
				GameMain.LuaCs.HandleException(ex, $"queuedFunctionCalls was {queuedFunctionCalls}");
			}
		}

		public object Call(string name, params object[] args)
		{
#if CLIENT
			if (GameMain.GameSession?.IsRunning == false && GameMain.IsSingleplayer)
				return null;
#endif
			if (GameMain.LuaCs == null) return null;
			if (name == null) return null;
			if (args == null) { args = new object[] { }; }

			name = name.ToLower();

			if (!luaHookFunctions.ContainsKey(name))
				return null;

			object lastResult = null;

			if (csHookFunctions.ContainsKey(name))
			{
				var outOfScope = new List<string>();
				foreach ((var key, var tuple) in csHookFunctions[name])
				{
					if (tuple.Item2 != null && tuple.Item2.IsDisposed)
						outOfScope.Add(key);
					else
					{
						try
						{
							var result = tuple.Item1(args);
							if (result != null) lastResult = result;
						}
						catch (Exception e)
						{
							StringBuilder argsSb = new StringBuilder();
							foreach (var arg in args) argsSb.Append(arg + " ");
							GameMain.LuaCs.HandleException(
								e, $"Error in Hook '{name}'->'{key}', with args '{argsSb}':\n{e}",
								LuaCsSetup.ExceptionType.CSharp);
						}
					}
				}
				foreach (var key in outOfScope) csHookFunctions[name].Remove(key);
			}

			if (luaHookFunctions.ContainsKey(name))
			{
				foreach (LuaHookFunction hf in luaHookFunctions[name].Values)
				{
					try
					{
						if (hf.function is Closure)
						{
							var result = GameMain.LuaCs.lua.Call(hf.function, args);
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

						GameMain.LuaCs.HandleException(e, $"Error in Hook '{name}'->'{hf.hookName}', with args '{argsSb}'");
					}
				}
			}

			return lastResult;
		}
	}
}