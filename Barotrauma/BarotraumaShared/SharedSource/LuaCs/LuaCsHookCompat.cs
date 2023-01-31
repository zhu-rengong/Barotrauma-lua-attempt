﻿using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using LuaCsCompatPatchFunc = Barotrauma.LuaCsPatch;

namespace Barotrauma
{
    // XXX: this can't be renamed because of backward compatibility with C# mods
    public delegate object LuaCsPatch(object self, Dictionary<string, object> args);

    partial class LuaCsHook
    {
        private Dictionary<long, HashSet<(string, LuaCsCompatPatchFunc, ACsMod)>> compatHookPrefixMethods = new Dictionary<long, HashSet<(string, LuaCsCompatPatchFunc, ACsMod)>>();
        private Dictionary<long, HashSet<(string, LuaCsCompatPatchFunc, ACsMod)>> compatHookPostfixMethods = new Dictionary<long, HashSet<(string, LuaCsCompatPatchFunc, ACsMod)>>();

        private static void _hookLuaCsPatch(MethodBase __originalMethod, object[] __args, object __instance, out object result, HookMethodType hookType)
        {
            result = null;

            try
            {
                var funcAddr = ((long)__originalMethod.MethodHandle.GetFunctionPointer());
                HashSet<(string, LuaCsCompatPatchFunc, ACsMod)> methodSet = null;
                switch (hookType)
                {
                    case HookMethodType.Before:
                        instance.compatHookPrefixMethods.TryGetValue(funcAddr, out methodSet);
                        break;
                    case HookMethodType.After:
                        instance.compatHookPostfixMethods.TryGetValue(funcAddr, out methodSet);
                        break;
                    default:
                        throw new ArgumentException($"Invalid {nameof(HookMethodType)} enum value.", nameof(hookType));
                }

                if (methodSet != null)
                {
                    var @params = __originalMethod.GetParameters();
                    var args = new Dictionary<string, object>();
                    for (int i = 0; i < @params.Length; i++)
                    {
                        args.Add(@params[i].Name, __args[i]);
                    }

                    var outOfSocpe = new HashSet<(string, LuaCsCompatPatchFunc, ACsMod)>();
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
                                if (_result is DynValue res)
                                {
                                    if (!res.IsNil())
                                    {
                                        if (__originalMethod is MethodInfo mi && mi.ReturnType != typeof(void))
                                        {
                                            result = res.ToObject(mi.ReturnType);
                                        }
                                        else
                                        {
                                            result = res.ToObject();
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
                LuaCsLogger.LogError($"Error in {__originalMethod.Name}:", LuaCsMessageOrigin.Unknown);
                LuaCsLogger.HandleException(ex, LuaCsMessageOrigin.Unknown);
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

        // TODO: deprecate this
        public void HookMethod(string identifier, MethodBase method, LuaCsCompatPatchFunc patch, HookMethodType hookType = HookMethodType.Before, ACsMod owner = null)
        {
            if (identifier == null || method == null || patch == null)
            {
                LuaCsLogger.HandleException(new ArgumentNullException("Identifier, Method and Patch arguments must not be null."), LuaCsMessageOrigin.Unknown);
                return;
            }
            ValidatePatchTarget(method);

            var funcAddr = ((long)method.MethodHandle.GetFunctionPointer());
            var patches = Harmony.GetPatchInfo(method);

            if (hookType == HookMethodType.Before)
            {
                if (method is MethodInfo mi && mi.ReturnType != typeof(void))
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

                if (compatHookPrefixMethods.TryGetValue(funcAddr, out HashSet<(string, LuaCsCompatPatchFunc, ACsMod)> methodSet))
                {
                    if (identifier != "")
                    {
                        methodSet.RemoveWhere(tuple => tuple.Item1 == identifier);
                    }

                    methodSet.Add((identifier, patch, owner));
                }
                else if (patch != null)
                {
                    compatHookPrefixMethods.Add(funcAddr, new HashSet<(string, LuaCsCompatPatchFunc, ACsMod)>() { (identifier, patch, owner) });
                }

            }
            else if (hookType == HookMethodType.After)
            {
                if (method is MethodInfo mi && mi.ReturnType != typeof(void))
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

                if (compatHookPostfixMethods.TryGetValue(funcAddr, out HashSet<(string, LuaCsCompatPatchFunc, ACsMod)> methodSet))
                {
                    if (identifier != "")
                    {
                        methodSet.RemoveWhere(tuple => tuple.Item1 == identifier);
                    }

                    methodSet.Add((identifier, patch, owner));
                }
                else if (patch != null)
                {
                    compatHookPostfixMethods.Add(funcAddr, new HashSet<(string, LuaCsCompatPatchFunc, ACsMod)>() { (identifier, patch, owner) });
                }
            }
        }
        protected void HookMethod(string identifier, string className, string methodName, string[] parameterNames, LuaCsCompatPatchFunc patch, HookMethodType hookMethodType = HookMethodType.Before)
        {
            var method = ResolveMethod(className, methodName, parameterNames);
            if (method == null) return;
            if (method.GetParameters().Any(x => x.ParameterType.IsByRef))
            {
                throw new InvalidOperationException($"{nameof(HookMethod)} doesn't support ByRef parameters; use {nameof(Patch)} instead.");
            }
            HookMethod(identifier, method, patch, hookMethodType);
        }
        protected void HookMethod(string identifier, string className, string methodName, LuaCsCompatPatchFunc patch, HookMethodType hookMethodType = HookMethodType.Before) =>
            HookMethod(identifier, className, methodName, null, patch, hookMethodType);
        protected void HookMethod(string className, string methodName, LuaCsCompatPatchFunc patch, HookMethodType hookMethodType = HookMethodType.Before) =>
            HookMethod("", className, methodName, null, patch, hookMethodType);
        protected void HookMethod(string className, string methodName, string[] parameterNames, LuaCsCompatPatchFunc patch, HookMethodType hookMethodType = HookMethodType.Before) =>
            HookMethod("", className, methodName, parameterNames, patch, hookMethodType);


        public void UnhookMethod(string identifier, MethodBase method, HookMethodType hookType = HookMethodType.Before)
        {
            var funcAddr = (long)method.MethodHandle.GetFunctionPointer();

            Dictionary<long, HashSet<(string, LuaCsCompatPatchFunc, ACsMod)>> methods;
            if (hookType == HookMethodType.Before) methods = compatHookPrefixMethods;
            else if (hookType == HookMethodType.After) methods = compatHookPostfixMethods;
            else throw null;

            if (methods.ContainsKey(funcAddr)) methods[funcAddr]?.RemoveWhere(t => t.Item1 == identifier);
        }
        protected void UnhookMethod(string identifier, string className, string methodName, string[] parameterNames, HookMethodType hookType = HookMethodType.Before)
        {
            var method = ResolveMethod(className, methodName, parameterNames);
            if (method == null) return;
            UnhookMethod(identifier, method, hookType);
        }
    }
}
