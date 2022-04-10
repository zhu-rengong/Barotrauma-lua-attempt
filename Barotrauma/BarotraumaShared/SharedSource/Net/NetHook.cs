using System;
using System.Linq;
using System.Reflection;
using MoonSharp.Interpreter;
using HarmonyLib;
using System.Collections.Generic;
using System.Text;

namespace Barotrauma
{
	using NetHookMethod = Func<object[], object>;
	using HookMethod = Func<object, Dictionary<string, object>, object>;

	partial class NetHook
	{
		public NetHook()
		{
			_hookPrefixMethods = new Dictionary<long, HashSet<Tuple<string, HookMethod>>>();
			_hookPostfixMethods = new Dictionary<long, HashSet<Tuple<string, HookMethod>>>();
		}

		private Dictionary<string, List<NetHookMethod>> hookFunctions = new Dictionary<string, List<NetHookMethod>>();

		private static Dictionary<long, HashSet<Tuple<string, HookMethod>>> _hookPrefixMethods;
		private static Dictionary<long, HashSet<Tuple<string, HookMethod>>> _hookPostfixMethods;

		private Queue<Tuple<float, NetHookMethod, object[]>> queuedFunctionCalls = new Queue<Tuple<float, NetHookMethod, object[]>>();

		public enum HookMethodType
		{
			Before, After
		}

		static void _hookNetPatch(MethodBase __originalMethod, object[] __args, object __instance, out object result, HookMethodType hookMethodType)
		{
			result = null;

#if CLIENT
				if (GameMain.GameSession?.IsRunning == false && GameMain.IsSingleplayer)
					return;
#endif

			try
			{
				var funcAddr = ((long)__originalMethod.MethodHandle.GetFunctionPointer());
				HashSet<Tuple<string, HookMethod>> methodSet = null;
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
						result = tuple.Item2(__instance, ptable);
					}
				}
			}
			catch (Exception ex)
			{
				GameMain.Net.HandleException(ex, null);
			}
		}

		private const BindingFlags DefaultBindingFlags = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
		public void HookMethod(string identifier, MethodInfo methodInfo, HookMethod hookMethod, HookMethodType hookMethodType = HookMethodType.Before)
		{
			if (identifier == null || methodInfo == null || methodInfo == null) throw new ArgumentNullException("All 'HookMethod' arguments must not be null.");

			identifier = identifier.ToLower();
			var funcAddr = ((long)methodInfo.MethodHandle.GetFunctionPointer());
			var patches = Harmony.GetPatchInfo(methodInfo);

			if (hookMethodType == HookMethodType.Before)
			{
				if (methodInfo.ReturnType == typeof(void))
				{
					if (patches == null || patches.Prefixes == null || patches.Prefixes.Find(patch => patch.PatchMethod == hookMethod.Method) == null)
					{
						GameMain.Net.harmony.Patch(methodInfo, prefix: new HarmonyMethod(hookMethod.Method));
					}
				}
				else
				{
					if (patches == null || patches.Prefixes == null || patches.Prefixes.Find(patch => patch.PatchMethod == hookMethod.Method) == null)
					{
						GameMain.Net.harmony.Patch(methodInfo, prefix: new HarmonyMethod(hookMethod.Method));
					}
				}

				if (_hookPrefixMethods.TryGetValue(funcAddr, out HashSet<Tuple<string, HookMethod>> methodSet))
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
					_hookPrefixMethods.Add(funcAddr, new HashSet<Tuple<string, HookMethod>>() { Tuple.Create(identifier, hookMethod) });
				}

			}
			else if (hookMethodType == HookMethodType.After)
			{
				if (methodInfo.ReturnType == typeof(void))
				{
					if (patches == null || patches.Postfixes == null || patches.Postfixes.Find(patch => patch.PatchMethod == hookMethod.Method) == null)
					{
						GameMain.Net.harmony.Patch(methodInfo, postfix: new HarmonyMethod(hookMethod.Method));
					}
				}
				else
				{
					if (patches == null || patches.Postfixes == null || patches.Postfixes.Find(patch => patch.PatchMethod == hookMethod.Method) == null)
					{
						GameMain.Net.harmony.Patch(methodInfo, postfix: new HarmonyMethod(hookMethod.Method));
					}
				}

				if (_hookPostfixMethods.TryGetValue(funcAddr, out HashSet<Tuple<string, HookMethod>> methodSet))
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
					_hookPostfixMethods.Add(funcAddr, new HashSet<Tuple<string, HookMethod>>() { Tuple.Create(identifier, hookMethod) });
				}

			}

		}

		public void EnqueueFunction(NetHookMethod function, params object[] args)
		{
			queuedFunctionCalls.Enqueue(new Tuple<float, NetHookMethod, object[]>(0, function, args));
		}

		public void EnqueueTimedFunction(float time, NetHookMethod function, params object[] args)
		{
			queuedFunctionCalls.Enqueue(new Tuple<float, NetHookMethod, object[]>(time, function, args));
		}

		public void Add(string name, NetHookMethod hook)
		{
			if (name == null || hook == null) throw new ArgumentNullException("Name and Action cannot be null");

			name = name.ToLower();

			if (!hookFunctions.ContainsKey(name))
				hookFunctions.Add(name, new List<NetHookMethod>());

			hookFunctions[name].Add(hook);
		}

		public void Remove(string name, NetHookMethod hook)
		{
			if (name == null || hook == null) throw new ArgumentNullException("Name and Action cannot be null");

			name = name.ToLower();

			if (!hookFunctions.ContainsKey(name))
				return;

			if (hookFunctions[name].Contains(hook))
				hookFunctions[name].Remove(hook);
		}

		public void Update()
		{
			try
			{
				if (queuedFunctionCalls.TryPeek(out Tuple<float, NetHookMethod, object[]> result))
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
				GameMain.Net.HandleException(ex, $"queuedFunctionCalls was {queuedFunctionCalls}");
			}
		}

		public object Call(string name, params object[] args)
		{
#if CLIENT
				if (GameMain.GameSession?.IsRunning == false && GameMain.IsSingleplayer)
					return null;
#endif
			if (GameMain.Net == null) return null;
			if (name == null) return null;
			if (args == null) { args = new object[] { }; }

			name = name.ToLower();

			if (!hookFunctions.ContainsKey(name))
				return null;

			object lastResult = null;

			foreach (var hook in hookFunctions[name])
			{
				try
				{
					var result = hook(args);
					if (!(result == null))
						lastResult = result;
				}
				catch (Exception e)
				{
					StringBuilder argsSb = new StringBuilder();
					foreach (var arg in args)
					{
						argsSb.Append(arg + " ");
					}

					GameMain.Net.HandleException(e, $"Error in Hook '{name}'->'{hook}', with args '{argsSb}'\n{Environment.StackTrace}");
				}
			}

			return lastResult;
		}
	}
}