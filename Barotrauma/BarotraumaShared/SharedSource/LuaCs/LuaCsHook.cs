using System;
using System.Linq;
using System.Reflection;
using MoonSharp.Interpreter;
using HarmonyLib;
using System.Collections.Generic;
using System.Text;
using MoonSharp.Interpreter.Interop;

namespace Barotrauma
{
	public delegate void LuaCsAction(params object[] args);
	public delegate object LuaCsFunc(params object[] args);
	public delegate object LuaCsPatch(object self, object args);

	public class LuaCsHook
	{
		public enum HookMethodType
		{
			Before, After
		}

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
		private class LuaCsHookCallback
		{
			public string name;
			public string hookName;
			public LuaCsFunc func;

			public LuaCsHookCallback(string name, string hookName, LuaCsFunc func)
			{
				this.name = name;
				this.hookName = hookName;
				this.func = func;
			}
		}

		private Harmony harmony;

		private Dictionary<string, Dictionary<string, (LuaCsHookCallback, ACsMod)>> hookFunctions;

		private Dictionary<long, HashSet<(string, LuaCsPatch, ACsMod)>> hookPrefixMethods;
		private Dictionary<long, HashSet<(string, LuaCsPatch, ACsMod)>> hookPostfixMethods;

		private Queue<(float, LuaCsAction, object[])> queuedFunctionCalls;

		private static LuaCsHook instance;

		public LuaCsHook() {
			instance = this;
			
			hookFunctions = new Dictionary<string, Dictionary<string, (LuaCsHookCallback, ACsMod)>>();

			hookPrefixMethods = new Dictionary<long, HashSet<(string, LuaCsPatch, ACsMod)>>();
			hookPostfixMethods = new Dictionary<long, HashSet<(string, LuaCsPatch, ACsMod)>>();

			queuedFunctionCalls = new Queue<(float, LuaCsAction, object[])>();
		}

		public void Initialize()
        {
			harmony = new Harmony("LuaCsForBarotrauma");

			var hookType = UserData.RegisterType<LuaCsHook>();
			var hookDesc = (StandardUserDataDescriptor)hookType;
			typeof(LuaCsHook).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).ToList().ForEach(m => {
				if (
					m.Name.Contains("HookMethod") ||
					m.Name.Contains("UnhookMethod") ||
					m.Name.Contains("EnqueueFunction") ||
					m.Name.Contains("EnqueueTimedFunction")
				)
				{
					hookDesc.AddMember(m.Name, new MethodMemberDescriptor(m, InteropAccessMode.Default));
				}
			});
		}
		
		private static void _hookLuaCsPatch(MethodBase __originalMethod, object[] __args, object __instance, out object result, HookMethodType hookMethodType)
		{
			result = null;
#if CLIENT
		if (GameMain.GameSession?.IsRunning == false && GameMain.IsSingleplayer)
			return;
#endif
			try
			{
				var funcAddr = ((long)__originalMethod.MethodHandle.GetFunctionPointer());
				HashSet<(string, LuaCsPatch, ACsMod)> methodSet = null;
				switch (hookMethodType)
				{
					case HookMethodType.Before:
						instance.hookPrefixMethods.TryGetValue(funcAddr, out methodSet);
						break;
					case HookMethodType.After:
						instance.hookPostfixMethods.TryGetValue(funcAddr, out methodSet);
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

					var outOfSocpe = new HashSet<(string, LuaCsPatch, ACsMod)>();
					foreach (var tuple in methodSet)
					{
						if (tuple.Item3 != null && tuple.Item3.IsDisposed)
							outOfSocpe.Add(tuple);
						else
						{
							var _result = tuple.Item2(__instance, args);
							if (_result != null)
							{
								if (_result is LuaResult res)
								{
									if (!res.IsNull())
									{
										if (__originalMethod is MethodInfo mi) result = res.DynValue().ToObject(mi.ReturnType);
										else result = res.DynValue().ToObject();
									}
								}
								else result = _result;
							}
						}
					}
					foreach (var tuple in outOfSocpe) methodSet.Remove(tuple);
				}
			}
			catch (Exception ex)
			{
				GameMain.LuaCs.HandleException(ex, exceptionType: LuaCsSetup.ExceptionType.Both);
			}
		}


		private static bool HookLuaCsPatchPrefix(MethodBase __originalMethod, object[] __args, object __instance)
		{
			_hookLuaCsPatch(__originalMethod, __args, __instance, out object result, HookMethodType.Before);
			return result == null;
		}
		private static void HookLuaCsPatchPostfix(MethodBase __originalMethod, object[] __args, object __instance) =>
			_hookLuaCsPatch(__originalMethod, __args, __instance, out object _, HookMethodType.After);

		private static bool HookLuaCsPatchRetPrefix(MethodBase __originalMethod, object[] __args, ref object __result, object __instance)
		{
			_hookLuaCsPatch(__originalMethod, __args, __instance, out object result, HookMethodType.Before);
			if (result != null)
			{
				__result = result;
				return false;
			}
			else return true;
		}
		private static void HookLuaCsPatchRetPostfix(MethodBase __originalMethod, object[] __args, ref object __result, object __instance)
		{
			_hookLuaCsPatch(__originalMethod, __args, __instance, out object result, HookMethodType.After);
			if (result != null) __result = result;
		}


		private const BindingFlags DefaultBindingFlags = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

		private static MethodInfo _miHookLuaCsPatchPrefix = typeof(LuaCsHook).GetMethod("HookLuaCsPatchPrefix", BindingFlags.NonPublic | BindingFlags.Static);
		private static MethodInfo _miHookLuaCsPatchPostfix = typeof(LuaCsHook).GetMethod("HookLuaCsPatchPostfix", BindingFlags.NonPublic | BindingFlags.Static);
		private static MethodInfo _miHookLuaCsPatchRetPrefix = typeof(LuaCsHook).GetMethod("HookLuaCsPatchRetPrefix", BindingFlags.NonPublic | BindingFlags.Static);
		private static MethodInfo _miHookLuaCsPatchRetPostfix = typeof(LuaCsHook).GetMethod("HookLuaCsPatchRetPostfix", BindingFlags.NonPublic | BindingFlags.Static);

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

		private static readonly string[] prohibitedHooks = {
			"Barotrauma.Lua",
			"Barotrauma.Cs"
		};
		public void HookMethod(string identifier, MethodInfo method, LuaCsPatch patch, HookMethodType hookType = HookMethodType.Before, ACsMod owner = null)
		{
			if (identifier == null || method == null || patch == null) throw new ArgumentNullException("Identifier, Method and Patch arguments must not be null.");
			if (prohibitedHooks.Any(h => method.DeclaringType.FullName.StartsWith(h))) throw new ArgumentException("Hooks into Modding Environment are prohibited.");

			var funcAddr = ((long)method.MethodHandle.GetFunctionPointer());
			var patches = Harmony.GetPatchInfo(method);

			if (hookType == HookMethodType.Before)
			{
				if (method.ReturnType != typeof(void))
                {
					if (patches == null || patches.Prefixes == null || patches.Prefixes.Find(patch => patch.PatchMethod == _miHookLuaCsPatchRetPrefix) == null)
					{
						harmony.Patch(method, prefix: new HarmonyMethod(_miHookLuaCsPatchRetPrefix));
					}
				}
				else
                {
					if (patches == null || patches.Prefixes == null || patches.Prefixes.Find(patch => patch.PatchMethod == _miHookLuaCsPatchPrefix) == null)
					{
						harmony.Patch(method, prefix: new HarmonyMethod(_miHookLuaCsPatchPrefix));
					}
				}

				if (hookPrefixMethods.TryGetValue(funcAddr, out HashSet<(string, LuaCsPatch, ACsMod)> methodSet))
				{
					methodSet.RemoveWhere(tuple => tuple.Item1 == identifier);
					methodSet.Add((identifier, patch, owner));
				}
				else if (patch != null)
				{
					hookPrefixMethods.Add(funcAddr, new HashSet<(string, LuaCsPatch, ACsMod)>() { (identifier, patch, owner) });
				}

			}
			else if (hookType == HookMethodType.After)
			{
				if (method.ReturnType != typeof(void))
				{
					if (patches == null || patches.Postfixes == null || patches.Postfixes.Find(patch => patch.PatchMethod == _miHookLuaCsPatchRetPostfix) == null)
					{
						harmony.Patch(method, postfix: new HarmonyMethod(_miHookLuaCsPatchRetPostfix));
					}
				}
				else
                {
					if (patches == null || patches.Postfixes == null || patches.Postfixes.Find(patch => patch.PatchMethod == _miHookLuaCsPatchPostfix) == null)
					{
						harmony.Patch(method, postfix: new HarmonyMethod(_miHookLuaCsPatchPostfix));
					}
				}

				if (hookPostfixMethods.TryGetValue(funcAddr, out HashSet<(string, LuaCsPatch, ACsMod)> methodSet))
				{
					methodSet.RemoveWhere(tuple => tuple.Item1 == identifier);
					methodSet.Add((identifier, patch, owner));
				}
				else if (patch != null)
				{
					hookPostfixMethods.Add(funcAddr, new HashSet<(string, LuaCsPatch, ACsMod)>() { (identifier, patch, owner) });
				}

			}

		}

		protected void HookMethod(string identifier, string className, string methodName, string[] parameterNames, LuaCsPatch patch, HookMethodType hookMethodType = HookMethodType.Before)
		{

			MethodInfo methodInfo = ResolveMethod("HookMethod", className, methodName, parameterNames);
			if (methodInfo == null) return;
			HookMethod(identifier, methodInfo, patch, hookMethodType);
		}
		protected void HookMethod(string identifier, string className, string methodName, LuaCsPatch patch, HookMethodType hookMethodType = HookMethodType.Before) =>
			HookMethod(identifier, className, methodName, null, patch, hookMethodType);
		protected void HookMethod(string className, string methodName, LuaCsPatch patch, HookMethodType hookMethodType = HookMethodType.Before) =>
			HookMethod("", className, methodName, null, patch, hookMethodType);
		protected void HookMethod(string className, string methodName, string[] parameterNames, LuaCsPatch patch, HookMethodType hookMethodType = HookMethodType.Before) =>
			HookMethod("", className, methodName, parameterNames, patch, hookMethodType);


		public void UnhookMethod(string identifier, MethodInfo method, HookMethodType hookType = HookMethodType.Before)
		{
			var funcAddr = ((long)method.MethodHandle.GetFunctionPointer());

			Dictionary<long, HashSet<(string, LuaCsPatch, ACsMod)>> methods;
			if (hookType == HookMethodType.Before) methods = hookPrefixMethods;
			else if (hookType == HookMethodType.After) methods = hookPostfixMethods;
			else throw null;

			if (methods.ContainsKey(funcAddr)) methods[funcAddr]?.RemoveWhere(t => t.Item1 == identifier);
		}
		protected void UnhookMethod(string identifier, string className, string methodName, string[] parameterNames, HookMethodType hookType = HookMethodType.Before)
		{
			MethodInfo methodInfo = ResolveMethod("UnhookMathod", className, methodName, parameterNames);
			if (methodInfo == null) return;
			UnhookMethod(identifier, methodInfo, hookType);
		}


		public void Enqueue(LuaCsAction action, params object[] args)
		{
			queuedFunctionCalls.Enqueue((0, action, args));
		}
		public void EnqueueTimed(float time, LuaCsAction action, params object[] args)
		{
			queuedFunctionCalls.Enqueue((time, action, args));
		}

		protected void EnqueueFunction(LuaCsAction function, params object[] args) => Enqueue(function, args);
		protected void EnqueueTimedFunction(float time, LuaCsAction function, params object[] args) => EnqueueTimed(time, function, args);


		public void Add(string name, string hookName, LuaCsFunc hook, ACsMod owner = null)
		{
			name = name.ToLower();

			if (name == null || hookName == null || hook == null) throw new ArgumentNullException("Names and Hook must not be null");

			if (!hookFunctions.ContainsKey(name))
				hookFunctions.Add(name, new Dictionary<string, (LuaCsHookCallback, ACsMod)>());

			hookFunctions[name][hookName] = (new LuaCsHookCallback(name, hookName, hook), owner);
		}

		public void Remove(string name, string hookName)
		{
			if (name == null || hookName == null) return;

			name = name.ToLower();

			if (hookFunctions.ContainsKey(name) && hookFunctions[name].ContainsKey(hookName))
				hookFunctions[name].Remove(hookName);
		}

		public void Clear()
        {
			hookFunctions.Clear();

			hookPrefixMethods.Clear();
			hookPostfixMethods.Clear();

			queuedFunctionCalls.Clear();

			harmony?.UnpatchAll();
		}


		public void Update()
		{
			try
			{
				if (queuedFunctionCalls.TryPeek(out (float, LuaCsAction, object[]) result))
				{
					if (Timing.TotalTime >= result.Item1)
					{
						result.Item2(result.Item3);
						queuedFunctionCalls.Dequeue();
					}
				}
			}
			catch (Exception ex)
			{
				GameMain.LuaCs.HandleException(ex, $"queuedFunctionCalls was {queuedFunctionCalls}", LuaCsSetup.ExceptionType.Both);
			}
		}

		public T Call<T>(string name, params object[] args)
		{
#if CLIENT
			if (GameMain.GameSession?.IsRunning == false && GameMain.IsSingleplayer)
				return default(T);
#endif
			if (GameMain.LuaCs == null) return default(T);
			if (name == null) return default(T);
			if (args == null) { args = new object[] { }; }

			name = name.ToLower();

			if (!hookFunctions.ContainsKey(name))
				return default(T);

			T lastResult = default(T);

			if (hookFunctions.ContainsKey(name))
			{
				var outOfScope = new List<string>();
				foreach ((var key, var tuple) in hookFunctions[name])
				{
					if (tuple.Item2 != null && tuple.Item2.IsDisposed)
						outOfScope.Add(key);
					else
					{
						try
						{
							var result = tuple.Item1.func(args);
							if (result != null)
                            {
								if (typeof(object) != typeof(T))
								{
									if (result is LuaResult lRes)
									{
										if (!lRes.IsNull()) lastResult = lRes.DynValue().ToObject<T>();
									}
									else if (result is T cRes && cRes != null) lastResult = cRes;
								}
								else if (result is T res && res != null) lastResult = res;
							}
						}
						catch (Exception e)
						{
							StringBuilder argsSb = new StringBuilder();
							foreach (var arg in args) argsSb.Append(arg + " ");
							GameMain.LuaCs.HandleException(
								e, $"Error in Hook '{name}'->'{key}', with args '{argsSb}':\n{e}",
								LuaCsSetup.ExceptionType.Both);
						}
					}
				}
				foreach (var key in outOfScope) hookFunctions[name].Remove(key);
			}

			return lastResult;
		}
		public object Call(string name, params object[] args) => Call<object>(name, args);
	}
}