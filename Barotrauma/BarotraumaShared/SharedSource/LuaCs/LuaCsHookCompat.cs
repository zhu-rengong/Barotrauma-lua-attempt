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
    partial class LuaCsHook
    {
		private Dictionary<long, HashSet<(string, LuaCsPatch, ACsMod)>> compatHookPrefixMethods;
		private Dictionary<long, HashSet<(string, LuaCsPatch, ACsMod)>> compatHookPostfixMethods;

		private static void _hookLuaCsPatch(MethodBase __originalMethod, object[] __args, object __instance, out object result, HookMethodType hookMethodType)
		{
			result = null;

			try
			{
				var funcAddr = ((long)__originalMethod.MethodHandle.GetFunctionPointer());
				HashSet<(string, LuaCsPatch, ACsMod)> methodSet = null;
				switch (hookMethodType)
				{
					case HookMethodType.Before:
						instance.compatHookPrefixMethods.TryGetValue(funcAddr, out methodSet);
						break;
					case HookMethodType.After:
						instance.compatHookPostfixMethods.TryGetValue(funcAddr, out methodSet);
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
						{
							outOfSocpe.Add(tuple);
						}
						else
						{
							var _result = tuple.Item2(__instance, args);
							if (_result != null)
							{
								if (_result is LuaResult res)
								{
									if (!res.IsNull())
									{
										if (__originalMethod is MethodInfo mi && mi.ReturnType != typeof(void))
										{
											result = res.DynValue().ToObject(mi.ReturnType);
										}
										else
										{
											result = res.DynValue().ToObject();
										}
									}
								}
								else
								{
									result = _result;
								}
							}
						}
					}
					foreach (var tuple in outOfSocpe) { methodSet.Remove(tuple); }
				}
			}
			catch (Exception ex)
			{
				GameMain.LuaCs.HandleException(ex, $"Error in {__originalMethod.Name}:", exceptionType: LuaCsSetup.ExceptionType.Both);
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
				string parameterNamesStr = parameterNames == null ? "" : string.Join(", ", parameterNames);
				GameMain.LuaCs.HandleException(new Exception($"Method '{methodName}' with parameters '{parameterNamesStr}' not found in class '{className}'"));
			}

			return methodInfo;
		}

		public void HookMethod(string identifier, MethodInfo method, LuaCsPatch patch, HookMethodType hookType = HookMethodType.Before, ACsMod owner = null)
		{
			if (identifier == null || method == null || patch == null)
			{
				GameMain.LuaCs.HandleException(new ArgumentNullException("Identifier, Method and Patch arguments must not be null."), exceptionType: ExceptionType.Both);
				return;
			}
			if (prohibitedHooks.Any(h => method.DeclaringType.FullName.StartsWith(h)))
			{
				GameMain.LuaCs.HandleException(new ArgumentException("Hooks into Modding Environment are prohibited."), exceptionType: ExceptionType.Both);
				return;
			}

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

				if (compatHookPrefixMethods.TryGetValue(funcAddr, out HashSet<(string, LuaCsPatch, ACsMod)> methodSet))
				{
					methodSet.RemoveWhere(tuple => tuple.Item1 == identifier);
					methodSet.Add((identifier, patch, owner));
				}
				else if (patch != null)
				{
					compatHookPrefixMethods.Add(funcAddr, new HashSet<(string, LuaCsPatch, ACsMod)>() { (identifier, patch, owner) });
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

				if (compatHookPostfixMethods.TryGetValue(funcAddr, out HashSet<(string, LuaCsPatch, ACsMod)> methodSet))
				{
					methodSet.RemoveWhere(tuple => tuple.Item1 == identifier);
					methodSet.Add((identifier, patch, owner));
				}
				else if (patch != null)
				{
					compatHookPostfixMethods.Add(funcAddr, new HashSet<(string, LuaCsPatch, ACsMod)>() { (identifier, patch, owner) });
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
			if (hookType == HookMethodType.Before) methods = compatHookPrefixMethods;
			else if (hookType == HookMethodType.After) methods = compatHookPostfixMethods;
			else throw null;

			if (methods.ContainsKey(funcAddr)) methods[funcAddr]?.RemoveWhere(t => t.Item1 == identifier);
		}
		protected void UnhookMethod(string identifier, string className, string methodName, string[] parameterNames, HookMethodType hookType = HookMethodType.Before)
		{
			MethodInfo methodInfo = ResolveMethod("UnhookMathod", className, methodName, parameterNames);
			if (methodInfo == null) return;
			UnhookMethod(identifier, methodInfo, hookType);
		}
	}
}