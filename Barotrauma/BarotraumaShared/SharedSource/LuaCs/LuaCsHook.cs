using System;
using System.Linq;
using System.Reflection;
using MoonSharp.Interpreter;
using HarmonyLib;
using System.Collections.Generic;
using System.Text;
using MoonSharp.Interpreter.Interop;
using static Barotrauma.LuaCsSetup;
using System.Threading;
using System.Diagnostics;

namespace Barotrauma
{
	public delegate void LuaCsAction(params object[] args);
	public delegate DynValue LuaCsFunc(params object[] args);
	public delegate object LuaCsPatch(object self, Dictionary<string, object> args);

	public partial class LuaCsHook
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

		private const BindingFlags DefaultBindingFlags = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
		private static readonly string[] prohibitedHooks = {
			"Barotrauma.Lua",
			"Barotrauma.Cs",
			"ContentPackageManager",
		};

        private static void ValidatePatchTarget(MethodInfo methodInfo)
        {
            if (prohibitedHooks.Any(h => methodInfo.DeclaringType.FullName.StartsWith(h)))
            {
                throw new ArgumentException("Hooks into the modding environment are prohibited.");
            }
        }

		private Harmony harmony;

		private Dictionary<string, Dictionary<string, (LuaCsHookCallback, ACsMod)>> hookFunctions;
		private Dictionary<long, HashSet<(string, LuaCsFunc, ACsMod)>> hookPrefixMethods;
		private Dictionary<long, HashSet<(string, LuaCsFunc, ACsMod)>> hookPostfixMethods;

		private static LuaCsHook instance;

		public LuaCsHook() {
			instance = this;
			
			hookFunctions = new Dictionary<string, Dictionary<string, (LuaCsHookCallback, ACsMod)>>();

			hookPrefixMethods = new Dictionary<long, HashSet<(string, LuaCsFunc, ACsMod)>>();
			hookPostfixMethods = new Dictionary<long, HashSet<(string, LuaCsFunc, ACsMod)>>();

			compatHookPrefixMethods = new Dictionary<long, HashSet<(string, LuaCsPatch, ACsMod)>>();
			compatHookPostfixMethods = new Dictionary<long, HashSet<(string, LuaCsPatch, ACsMod)>>();
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

		public void Add(string name, string hookName, LuaCsFunc hook, ACsMod owner = null)
		{
			if (name == null || hookName == null || hook == null)
			{
				throw new ScriptRuntimeException("Hook.Add: name, hookName and hook must not be null.");
			}

			name = name.ToLower();

			if (!hookFunctions.ContainsKey(name))
			{
				hookFunctions.Add(name, new Dictionary<string, (LuaCsHookCallback, ACsMod)>());
			}

			hookFunctions[name][hookName] = (new LuaCsHookCallback(name, hookName, hook), owner);
		}

		public void Remove(string name, string hookName)
		{
			if (name == null || hookName == null) { return; }

			name = name.ToLower();

			if (hookFunctions.ContainsKey(name) && hookFunctions[name].ContainsKey(hookName))
			{
				hookFunctions[name].Remove(hookName);
			}
		}

		public void Clear()
        {
			hookFunctions.Clear();

			hookPrefixMethods.Clear();
			hookPostfixMethods.Clear();

			compatHookPrefixMethods.Clear();
			compatHookPostfixMethods.Clear();

			harmony?.UnpatchAll();
		}


		public void Update()
		{

		}

		private Stopwatch performanceMeasurement = new Stopwatch();

		[MoonSharpHidden]
		public T Call<T>(string name, params object[] args)
		{
			if (GameMain.LuaCs == null) return default; // FIXME: should this throw an exception?
			if (name == null) throw new ArgumentNullException(name);
			if (args == null) args = new object[0];

			name = name.ToLower();

			if (!hookFunctions.ContainsKey(name))
			{
				return default;
			}

			T lastResult = default;

			if (!hookFunctions.ContainsKey(name))
			{
				return lastResult;
			}

			var hooksToRemove = new List<string>();
			foreach ((var key, var tuple) in hookFunctions[name])
			{
				if (tuple.Item2 != null && tuple.Item2.IsDisposed)
				{
					hooksToRemove.Add(key);
					continue;
				}

				try
				{
					if (GameMain.LuaCs.PerformanceCounter.EnablePerformanceCounter)
					{
						performanceMeasurement.Start();
					}

					var result = tuple.Item1.func(args);
					if (result != null && !result.IsNil())
					{
						lastResult = result.ToObject<T>();
					}

					if (GameMain.LuaCs.PerformanceCounter.EnablePerformanceCounter)
					{
						performanceMeasurement.Stop();
						GameMain.LuaCs.PerformanceCounter.SetHookElapsedTicks(name, key, performanceMeasurement.ElapsedTicks);
						performanceMeasurement.Reset();
					}
				}
				catch (Exception e)
				{
					StringBuilder argsSb = new StringBuilder();
					foreach (var arg in args) argsSb.Append(arg + " ");
					GameMain.LuaCs.HandleException(e, $"Error in Hook '{name}'->'{key}', with args '{argsSb}':\n{e}", ExceptionType.Both);
				}
			}
			foreach (var key in hooksToRemove) 
			{ 
				hookFunctions[name].Remove(key); 
			}

			return lastResult;
		}

        public object Call(string name, params object[] args)
        {
			if (name == null) throw new ScriptRuntimeException("Hook.Call: name must not be null.");
			return Call<object>(name, args);
        }

        private static bool PatchPrefix(MethodBase __originalMethod, object[] __args, object __instance)
		{
			ExecutePatch(__originalMethod, __args, __instance, out object result, HookMethodType.Before);
			return result == null;
		}
		private static void PatchPostfix(MethodBase __originalMethod, object[] __args, object __instance) =>
			ExecutePatch(__originalMethod, __args, __instance, out object _, HookMethodType.After);

		private static bool PatchPrefixWithReturn(MethodBase __originalMethod, object[] __args, ref object __result, object __instance)
		{
			ExecutePatch(__originalMethod, __args, __instance, out object result, HookMethodType.Before);
			if (result != null)
			{
				__result = result;
				return false;
			}
			else { return true; }
		}
		private static void PatchPostfixWithReturn(MethodBase __originalMethod, object[] __args, ref object __result, object __instance)
		{
			ExecutePatch(__originalMethod, __args, __instance, out object result, HookMethodType.After);
			if (result != null) { __result = result; }
		}


		private static readonly MethodInfo miPatchPrefix = typeof(LuaCsHook).GetMethod("PatchPrefix", BindingFlags.NonPublic | BindingFlags.Static);
		private static readonly MethodInfo miPatchPostfix = typeof(LuaCsHook).GetMethod("PatchPostfix", BindingFlags.NonPublic | BindingFlags.Static);
		private static readonly MethodInfo miPatchPrefixWithReturn = typeof(LuaCsHook).GetMethod("PatchPrefixWithReturn", BindingFlags.NonPublic | BindingFlags.Static);
		private static readonly MethodInfo miPatchPostfixWithReturn = typeof(LuaCsHook).GetMethod("PatchPostfixWithReturn", BindingFlags.NonPublic | BindingFlags.Static);

		private static MethodInfo ResolveMethod(string className, string methodName, string[] parameterNames)
		{
			var classType = LuaUserData.GetType(className);

			if (classType == null)
			{
				throw new ArgumentNullException($"Invalid class name '{className}'.");
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
				string parameterNamesStr = parameterNames == null ? "" : string.Join(", ", parameterNames);
				throw new ArgumentNullException($"Method '{methodName}' with parameters '{parameterNamesStr}' not found in class '{className}'");
			}

			return methodInfo;
		}

		public void Patch(string identifier, MethodInfo method, LuaCsFunc patch, HookMethodType hookType = HookMethodType.Before, ACsMod owner = null)
		{
			if (identifier == null || method == null || patch == null)
			{
				throw new ArgumentNullException("Identifier, Method and Patch arguments must not be null.");
			}
			ValidatePatchTarget(method);

			var funcAddr = (long)method.MethodHandle.GetFunctionPointer();
			var patches = Harmony.GetPatchInfo(method);

			if (hookType == HookMethodType.Before)
			{
				if (method.ReturnType != typeof(void))
				{
					if (patches == null || patches.Prefixes == null || patches.Prefixes.Find(patch => patch.PatchMethod == miPatchPrefixWithReturn) == null)
					{
						harmony.Patch(method, prefix: new HarmonyMethod(miPatchPrefixWithReturn));
					}
				}
				else
				{
					if (patches == null || patches.Prefixes == null || patches.Prefixes.Find(patch => patch.PatchMethod == miPatchPrefix) == null)
					{
						harmony.Patch(method, prefix: new HarmonyMethod(miPatchPrefix));
					}
				}

				if (hookPrefixMethods.TryGetValue(funcAddr, out HashSet<(string, LuaCsFunc, ACsMod)> methodSet))
				{
					if (identifier != "")
					{
						methodSet.RemoveWhere(tuple => tuple.Item1 == identifier);
					}

					methodSet.Add((identifier, patch, owner));
				}
				else if (patch != null)
				{
					hookPrefixMethods.Add(funcAddr, new HashSet<(string, LuaCsFunc, ACsMod)>() { (identifier, patch, owner) });
				}

			}
			else if (hookType == HookMethodType.After)
			{
				if (method.ReturnType != typeof(void))
				{
					if (patches == null || patches.Postfixes == null || patches.Postfixes.Find(patch => patch.PatchMethod == miPatchPostfixWithReturn) == null)
					{
						harmony.Patch(method, postfix: new HarmonyMethod(miPatchPostfixWithReturn));
					}
				}
				else
				{
					if (patches == null || patches.Postfixes == null || patches.Postfixes.Find(patch => patch.PatchMethod == miPatchPostfix) == null)
					{
						harmony.Patch(method, postfix: new HarmonyMethod(miPatchPostfix));
					}
				}

				if (hookPostfixMethods.TryGetValue(funcAddr, out HashSet<(string, LuaCsFunc, ACsMod)> methodSet))
				{
					if (identifier != "")
					{
						methodSet.RemoveWhere(tuple => tuple.Item1 == identifier);
					}

					methodSet.Add((identifier, patch, owner));
				}
				else if (patch != null)
				{
					hookPostfixMethods.Add(funcAddr, new HashSet<(string, LuaCsFunc, ACsMod)>() { (identifier, patch, owner) });
				}

			}

		}

		private static void ExecutePatch(MethodBase __originalMethod, object[] __args, object __instance, out object result, HookMethodType hookMethodType)
		{
			result = null;

			try
			{
				long funcAddr = (long)__originalMethod.MethodHandle.GetFunctionPointer();

				HashSet<(string, LuaCsFunc, ACsMod)> methodSet = null;
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

				if (methodSet == null)
				{
					return;
				}

				var patchesToRemove = new HashSet<(string, LuaCsFunc, ACsMod)>();
				foreach (var tuple in methodSet)
				{
					if (tuple.Item3 != null && tuple.Item3.IsDisposed)
					{
						patchesToRemove.Add(tuple);
						continue;
					}

					var args = Enumerable.Empty<object>()
						.Concat(__args)
						.Prepend(__instance)
						.ToArray();
					var _result = tuple.Item2(args);

					if (_result != null && !_result.IsNil())
					{
						if (__originalMethod is MethodInfo mi && mi.ReturnType != typeof(void))
						{
							result = _result.ToObject(mi.ReturnType);
						}
						else
						{
							result = _result.ToObject();
						}
					}
				}

				foreach (var tuple in patchesToRemove)
				{
					methodSet.Remove(tuple);
				}
			}
			catch (Exception ex)
			{
				GameMain.LuaCs.HandleException(ex, $"Error in {__originalMethod.Name}:", exceptionType: LuaCsSetup.ExceptionType.Both);
			}
		}


		public void Patch(string identifier, string className, string methodName, string[] parameterNames, LuaCsFunc patch, HookMethodType hookMethodType = HookMethodType.Before)
		{
			MethodInfo methodInfo = ResolveMethod(className, methodName, parameterNames);
			if (methodInfo == null) return;
			Patch(identifier, methodInfo, patch, hookMethodType);
		}
		public void Patch(string identifier, string className, string methodName, LuaCsFunc patch, HookMethodType hookMethodType = HookMethodType.Before) =>
			Patch(identifier, className, methodName, null, patch, hookMethodType);
		public void Patch(string className, string methodName, LuaCsFunc patch, HookMethodType hookMethodType = HookMethodType.Before) =>
			Patch("", className, methodName, null, patch, hookMethodType);
		public void Patch(string className, string methodName, string[] parameterNames, LuaCsFunc patch, HookMethodType hookMethodType = HookMethodType.Before) =>
			Patch("", className, methodName, parameterNames, patch, hookMethodType);


		public void RemovePatch(string identifier, MethodInfo method, HookMethodType hookType = HookMethodType.Before)
		{
			var funcAddr = (long)method.MethodHandle.GetFunctionPointer();

			Dictionary<long, HashSet<(string, LuaCsFunc, ACsMod)>> methods;
			if (hookType == HookMethodType.Before) { methods = hookPrefixMethods; }
			else if (hookType == HookMethodType.After) { methods = hookPostfixMethods; }
			else { throw new NotImplementedException(); }

			if (methods.ContainsKey(funcAddr))
			{
				methods[funcAddr]?.RemoveWhere(t => t.Item1 == identifier);
			}
		}

		public void RemovePatch(string identifier, string className, string methodName, string[] parameterNames, HookMethodType hookType = HookMethodType.Before)
		{
			MethodInfo methodInfo = ResolveMethod(className, methodName, parameterNames);

			if (methodInfo == null)
			{
				return;
			}

			RemovePatch(identifier, methodInfo, hookType);
		}
	}
}