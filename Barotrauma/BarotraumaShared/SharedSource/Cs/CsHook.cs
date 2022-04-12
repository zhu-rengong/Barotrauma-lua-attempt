using System;
using System.Reflection;


namespace Barotrauma
{

    partial class LuaCsSetup
    {
        // CSharp wrapper for LuaCsHook
        public class CsHook : LuaCsHookWrapper
        {
            public CsHook(LuaCsHook hook) : base(hook) { }

            public void HookMethod(string identifier, MethodInfo method, CsPatchDelegate hook, HookMethodType hookType = HookMethodType.Before, ACsMod owner = null) =>
                _hook.HookCsMethod(identifier, method, hook, hookType, owner);

            public void UnhookMethod(string identifier, MethodInfo method, HookMethodType hookType = HookMethodType.Before) =>
                _hook.RemovePatch(identifier, method, hookType);

            public void Add(string name, string hookName, CsHookDelegate hook, ACsMod owner = null) =>
                _hook.AddCsHook(name, hookName, hook, owner);

        }
    }

}