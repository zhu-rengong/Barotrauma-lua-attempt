using Barotrauma;
using MoonSharp.Interpreter;
using System;
using System.Collections.Concurrent;
using System.Threading;
using Xunit;

namespace TestProject.LuaCs
{
    internal static class HookPatchHelpers
    {
        public class PatchHandle : IDisposable
        {
            private readonly Action disposeAction;

            public PatchHandle(string patchId, Action disposeAction)
            {
                this.disposeAction = disposeAction;
                this.PatchId = patchId;
            }

            public string PatchId { get; }

            public void Dispose() => disposeAction();
        }

        public static PatchHandle AddPrefix<T>(this LuaCsSetup luaCs, string body, string methodName = "Run", string? patchId = null)
        {
            var className = typeof(T).FullName;
            DynValue returnValue;
            if (patchId != null)
            {
                returnValue = luaCs.Lua.DoString(@$"
                    return Hook.Patch('{patchId}', '{className}', '{methodName}', function(instance, ptable)
                    {body}
                    end, Hook.HookMethodType.Before)
                ");
            }
            else
            {
                returnValue = luaCs.Lua.DoString(@$"
                    return Hook.Patch('{className}', '{methodName}', function(instance, ptable)
                    {body}
                    end, Hook.HookMethodType.Before)
                ");
            }

            Assert.Equal(DataType.String, returnValue.Type);
            return new(returnValue.String, () => luaCs.RemovePrefix<T>(returnValue.String, methodName));
        }

        public static PatchHandle AddPostfix<T>(this LuaCsSetup luaCs, string body, string methodName = "Run", string? patchId = null)
        {
            var className = typeof(T).FullName;
            DynValue returnValue;
            if (patchId != null)
            {
                returnValue = luaCs.Lua.DoString(@$"
                    return Hook.Patch('{patchId}', '{className}', '{methodName}', function(instance, ptable)
                    {body}
                    end, Hook.HookMethodType.After)
                ");
            }
            else
            {
                returnValue = luaCs.Lua.DoString(@$"
                    return Hook.Patch('{className}', '{methodName}', function(instance, ptable)
                    {body}
                    end, Hook.HookMethodType.After)
                ");
            }
            return new(returnValue.String, () => luaCs.RemovePostfix<T>(returnValue.String, methodName));
        }

        public static bool RemovePrefix<T>(this LuaCsSetup luaCs, string patchId, string methodName = "Run")
        {
            var className = typeof(T).FullName;
            var returnValue = luaCs.Lua.DoString($@"
                return Hook.RemovePatch('{patchId}', '{className}', '{methodName}', Hook.HookMethodType.Before)
            ");
            Assert.Equal(DataType.Boolean, returnValue.Type);
            return returnValue.Boolean;
        }

        public static bool RemovePostfix<T>(this LuaCsSetup luaCs, string patchId, string methodName = "Run")
        {
            var className = typeof(T).FullName;
            var returnValue = luaCs.Lua.DoString($@"
                return Hook.RemovePatch('{patchId}', '{className}', '{methodName}', Hook.HookMethodType.After)
            ");
            Assert.Equal(DataType.Boolean, returnValue.Type);
            return returnValue.Boolean;
        }

        public class PatchTargetHandle : IDisposable
        {
            private readonly SemaphoreSlim @lock;

            public PatchTargetHandle(SemaphoreSlim @lock)
            {
                this.@lock = @lock;
            }

            public void Dispose() => @lock.Release();
        }

        private static readonly ConcurrentDictionary<Type, SemaphoreSlim> PatchTargetLocks = new();

        public static PatchTargetHandle LockPatchTarget<T>()
        {
            if (!PatchTargetLocks.TryGetValue(typeof(T), out var @lock))
            {
                PatchTargetLocks[typeof(T)] = @lock = new SemaphoreSlim(1);
            }
            @lock.Wait();
            return new(@lock);
        }
    }
}
