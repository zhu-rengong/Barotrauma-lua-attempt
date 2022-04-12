using System;
using System.Collections.Generic;
using System.Reflection;

namespace Barotrauma
{
    public abstract class ACsMod : IDisposable
    {
        private static List<ACsMod> mods = new List<ACsMod>();
        public static List<ACsMod> LoadedMods { get => mods; }

        //public static ACsMod CreateInstance(Type type)
        //{
        //    if (!type.IsSubclassOf(typeof(ACsMod))) throw new Exception("Type argument is not the subclass of ACsMod.");
        //    return type.GetConstructor(new Type[] { }).Invoke(new object[] { }) as ACsMod;
        //}

        public bool IsDisposed { get; private set; }

        public ACsMod()
        {
            IsDisposed = false;
            LoadedMods.Add(this);
            Start();
        }

        public void Dispose() {
            Stop();
            LoadedMods.Remove(this);
            IsDisposed = true;
        }

        // TODO: some hooks

        /// Mod initialization
        public abstract void Start();
        /// Error or client exit
        public abstract void Stop();
        public virtual void Update() { }
    }
}