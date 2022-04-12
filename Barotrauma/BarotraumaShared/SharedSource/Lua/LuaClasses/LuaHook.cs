using System;

namespace Barotrauma
{

    partial class LuaCsSetup
    {
        // CSharp wrapper for LuaCsHook
        public class LuaHook : LuaCsHookWrapper
        {
			public LuaHook(LuaCsHook hook) : base(hook) { }

			public class HookMethodTypeProxy
			{
				public HookMethodTypeProxy() { }
				public const HookMethodType Before = Barotrauma.HookMethodType.Before;
				public const HookMethodType After = Barotrauma.HookMethodType.After;
			}
			public static readonly HookMethodTypeProxy HookMethodType = new HookMethodTypeProxy();

			public void HookMethod(string identifier, string className, string methodName, string[] parameterNames, object hookMethod, HookMethodType hookMethodType = Barotrauma.HookMethodType.Before) =>
				_hook.HookLuaMethod(identifier, className, methodName, parameterNames, hookMethod, hookMethodType);

			public void HookMethod(string identifier, string className, string methodName, object hookMethod, HookMethodType hookMethodType = Barotrauma.HookMethodType.Before) =>
				_hook.HookLuaMethod(identifier, className, methodName, null, hookMethod, hookMethodType);

			public void HookMethod(string className, string methodName, object hookMethod, HookMethodType hookMethodType = Barotrauma.HookMethodType.Before) =>
				_hook.HookLuaMethod("", className, methodName, null, hookMethod, hookMethodType);

			public void HookMethod(string className, string methodName, string[] parameterNames, object hookMethod, HookMethodType hookMethodType = Barotrauma.HookMethodType.Before) =>
				_hook.HookLuaMethod("", className, methodName, parameterNames, hookMethod, hookMethodType);

			public void Add(string name, string hookName, object function) =>
				_hook.AddLuaHook(name, hookName, function);

			public void EnqueueFunction(object function, params object[] args) =>
				_hook.EnqueueLuaFunction(function, args);

			public void EnqueueTimedFunction(float time, object function, params object[] args) =>
				_hook.EnqueueTimedLuaFunction(time, function, args);
		}
    }

}