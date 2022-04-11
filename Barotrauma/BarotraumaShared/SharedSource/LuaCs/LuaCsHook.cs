using System;
using System.Linq;
using System.Reflection;
using MoonSharp.Interpreter;
using HarmonyLib;
using System.Collections.Generic;
using System.Text;

namespace Barotrauma
{
	delegate object CsHookDelegate(params object[] args);
	delegate object CsPatchDelegate(object self, params object[] args);

	partial class LuaCsHook
	{
		public LuaCsHook()
		{
			luaHookPrefixMethods = new Dictionary<long, HashSet<(string, object)>>();
			luaHookPostfixMethods = new Dictionary<long, HashSet<(string, object)>>();
			csHookPrefixMethods = new Dictionary<long, HashSet<(IDisposable, CsPatchDelegate)>>();
			csHookPostfixMethods = new Dictionary<long, HashSet<(IDisposable, CsPatchDelegate)>>();
		}

		public class LuaHookFunction
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

		private Dictionary<string, Dictionary<string, LuaHookFunction>> luaHookFunctions = new Dictionary<string, Dictionary<string, LuaHookFunction>>();
		private Dictionary<string, Dictionary<CsHookDelegate, IDisposable>> csHookFunctions = new Dictionary<string, Dictionary<CsHookDelegate, IDisposable>>();

		private static Dictionary<long, HashSet<(string, object)>> luaHookPrefixMethods;
		private static Dictionary<long, HashSet<(string, object)>> luaHookPostfixMethods;
		private static Dictionary<long, HashSet<(IDisposable, CsPatchDelegate)>> csHookPrefixMethods;
		private static Dictionary<long, HashSet<(IDisposable, CsPatchDelegate)>> csHookPostfixMethods;

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
				HashSet<(string, object)> methodSet = null;
				switch (hookMethodType)
				{
					case HookMethodType.Before:
						luaHookPrefixMethods.TryGetValue(funcAddr, out methodSet);
						break;
					case HookMethodType.After:
						luaHookPostfixMethods.TryGetValue(funcAddr, out methodSet);
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
						result = new LuaResult(GameMain.LuaCs.lua.Call(tuple.Item2, __instance, ptable));
					}
				}
			}
			catch (Exception ex)
			{
				GameMain.LuaCs.HandleLuaException(ex);
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

		private const BindingFlags DefaultBindingFlags = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
		private static MethodInfo _miHookLuaPatchPrefix = typeof(LuaCsHook).GetMethod("HookLuaPatchPrefix", BindingFlags.NonPublic | BindingFlags.Static);
		private static MethodInfo _miHookLuaPatchRetPrefix = typeof(LuaCsHook).GetMethod("HookLuaPatchRetPrefix", BindingFlags.NonPublic | BindingFlags.Static);
		private static MethodInfo _miHookLuaPatchPostfix = typeof(LuaCsHook).GetMethod("HookLuaPatchPostfix", BindingFlags.NonPublic | BindingFlags.Static);
		private static MethodInfo _miHookLuaPatchRetPostfix = typeof(LuaCsHook).GetMethod("HookLuaPatchRetPostfix", BindingFlags.NonPublic | BindingFlags.Static);
		private void HookMethod(string identifier, string className, string methodName, string[] parameterNames, object hookMethod, HookMethodType hookMethodType = HookMethodType.Before)
		{
			var classType = LuaUserData.GetType(className);

			if (classType == null)
			{
				GameMain.LuaCs.HandleLuaException(new Exception($"Tried to use HookMethod with an invalid class name '{className}'."));
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
				GameMain.LuaCs.HandleLuaException(new Exception($"Method '{methodName}' with parameters '{parameterNamesStr}' not found in class '{className}'"));
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

		private void HookMethod(string className, string methodName, object hookMethod, HookMethodType hookMethodType = HookMethodType.Before)
		{
			HookMethod("", className, methodName, null, hookMethod, hookMethodType);
		}

		private void HookMethod(string className, string methodName, string[] parameterNames, object hookMethod, HookMethodType hookMethodType = HookMethodType.Before)
		{
			HookMethod("", className, methodName, parameterNames, hookMethod, hookMethodType);
		}

		private void HookMethod(string identifier, string className, string methodName, object hookMethod, HookMethodType hookMethodType = HookMethodType.Before)
		{
			HookMethod(identifier, className, methodName, null, hookMethod, hookMethodType);
		}

		public void HookMethod(IDisposable owner, MethodInfo methodInfo, CsPatchDelegate hookMethod, HookMethodType hookMethodType = HookMethodType.Before)
		{
			if (owner == null || methodInfo == null || hookMethod == null) throw new ArgumentNullException("All 'HookMethod' arguments must not be null.");

			var funcAddr = ((long)methodInfo.MethodHandle.GetFunctionPointer());
			var patches = Harmony.GetPatchInfo(methodInfo);

			if (hookMethodType == HookMethodType.Before)
			{
				if (patches == null || patches.Prefixes == null || patches.Prefixes.Find(patch => patch.PatchMethod == hookMethod.Method) == null)
				{
					GameMain.LuaCs.harmony.Patch(methodInfo, prefix: new HarmonyMethod(hookMethod.Method));
				}

				if (csHookPrefixMethods.TryGetValue(funcAddr, out HashSet<(IDisposable, CsPatchDelegate)> methodSet))
				{
					methodSet.RemoveWhere(tuple => tuple.Item1 == owner);
					if (hookMethod != null)
					{
						methodSet.Add((owner, hookMethod));
					}
				}
				else if (hookMethod != null)
				{
					csHookPrefixMethods.Add(funcAddr, new HashSet<(IDisposable, CsPatchDelegate)>() { (owner, hookMethod) });
				}

			}
			else if (hookMethodType == HookMethodType.After)
			{
				if (patches == null || patches.Postfixes == null || patches.Postfixes.Find(patch => patch.PatchMethod == hookMethod.Method) == null)
				{
					GameMain.LuaCs.harmony.Patch(methodInfo, postfix: new HarmonyMethod(hookMethod.Method));
				}

				if (csHookPostfixMethods.TryGetValue(funcAddr, out HashSet<(IDisposable, CsPatchDelegate)> methodSet))
				{
					methodSet.RemoveWhere(tuple => tuple.Item1 == owner);
					if (hookMethod != null)
					{
						methodSet.Add((owner, hookMethod));
					}
				}
				else if (hookMethod != null)
				{
					csHookPostfixMethods.Add(funcAddr, new HashSet<(IDisposable, CsPatchDelegate)>() { (owner, hookMethod) });
				}

			}

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
			if (name == null || hookName == null || function == null) return;

			name = name.ToLower();

			if (!luaHookFunctions.ContainsKey(name))
				luaHookFunctions.Add(name, new Dictionary<string, LuaHookFunction>());

			luaHookFunctions[name][hookName] = new LuaHookFunction(name, hookName, function);
		}

		public void Remove(string name, string hookName)
		{
			if (name == null || hookName == null) return;

			name = name.ToLower();

			if (!luaHookFunctions.ContainsKey(name))
				return;

			if (luaHookFunctions[name].ContainsKey(hookName))
				luaHookFunctions[name].Remove(hookName);
		}

		public void Add(string name, CsHookDelegate hook, IDisposable owner = null)
		{
			if (name == null || hook == null) throw new ArgumentNullException("Name and Hook must not be null");

			if (!csHookFunctions.ContainsKey(name))
				csHookFunctions.Add(name, new Dictionary<CsHookDelegate, IDisposable>());

			csHookFunctions[name][hook] = owner;
		}

		public void Remove(string name, CsHookDelegate hook)
		{
			if (name == null || hook == null) throw new ArgumentNullException("All arguments must not be null");

			if (!csHookFunctions.ContainsKey(name))
				return;

			if (csHookFunctions[name].ContainsKey(hook))
				csHookFunctions[name].Remove(hook);
		}


		public void Update()
		{
			try
			{
				if (queuedFunctionCalls.TryPeek(out Tuple<float, object, object[]> result))
				{
					if (Timing.TotalTime >= result.Item1)
					{
						GameMain.LuaCs.CallFunction(result.Item2, result.Item3);

						queuedFunctionCalls.Dequeue();
					}
				}
			}
			catch (Exception ex)
			{
				GameMain.LuaCs.HandleLuaException(ex, $"queuedFunctionCalls was {queuedFunctionCalls}");
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

					GameMain.LuaCs.HandleLuaException(e, $"Error in Hook '{name}'->'{hf.hookName}', with args '{argsSb}'");
				}
			}

			return lastResult;
		}
	}
}