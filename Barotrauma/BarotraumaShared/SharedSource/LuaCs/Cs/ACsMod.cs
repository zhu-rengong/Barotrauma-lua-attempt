using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Barotrauma
{
    public abstract class ACsMod : IDisposable
    {
        private static List<ACsMod> mods = new List<ACsMod>();
        public static List<ACsMod> LoadedMods { get => mods; }

        private const string MOD_STORE = "LocalMods/.modstore";
        public static string GetSoreFolder<T>() where T : ACsMod
        {
            if (!Directory.Exists(MOD_STORE)) Directory.CreateDirectory(MOD_STORE);
            var modFolder = $"{MOD_STORE}/{typeof(T)}";
            if (!Directory.Exists(modFolder)) Directory.CreateDirectory(modFolder);
            return modFolder;
        }


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
    }
}