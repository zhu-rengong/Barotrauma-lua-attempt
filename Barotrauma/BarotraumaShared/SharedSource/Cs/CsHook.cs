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

            //public enum class HookMethodTypeProxy
            //{
            //    Before = Barotrauma.HookMethodType.Before;
            //    After = Barotrauma.HookMethodType.After;

            //    public Barotrauma.HookMethodType type;

            //    public HookMethodTypeProxy(int i) => type = (Barotrauma.HookMethodType)i;
            //    public HookMethodTypeProxy(Barotrauma.HookMethodType t) => type = t;

            //    public static implicit operator Barotrauma.HookMethodType(HookMethodTypeProxy t) => t.type;
            //    public static implicit operator int(HookMethodTypeProxy t) => (int)t.type;

            //    public static implicit operator HookMethodTypeProxy(Barotrauma.HookMethodType t) => new HookMethodTypeProxy(t);
            //    public static implicit operator HookMethodTypeProxy(int i) => new HookMethodTypeProxy(i);                
            //}
            //public readonly HookMethodTypeProxy HookMethodType = new HookMethodTypeProxy(Barotrauma.HookMethodType.Before);

            public void HookMethod(string identifier, MethodInfo method, CsPatchDelegate hook, HookMethodType hookType = HookMethodType.Before, ACsMod owner = null) =>
                _hook.HookCsMethod(identifier, method, hook, hookType, owner);

            public void UnhookMethod(string identifier, MethodInfo method, HookMethodType hookType = HookMethodType.Before) =>
                _hook.RemovePatch(identifier, method, hookType);

            public void Add(string name, string hookName, CsHookDelegate hook, ACsMod owner = null) =>
                _hook.AddCsHook(name, hookName, hook, owner);

        }
    }

}