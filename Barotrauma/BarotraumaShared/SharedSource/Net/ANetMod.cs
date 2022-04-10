using System;
using System.Collections.Generic;

namespace Barotrauma
{
    public abstract class ANetMod : IDisposable
    {
        private static List<ANetMod> mods = new List<ANetMod>();
        public static List<ANetMod> LoadedMods
        {
            get => mods;
        }
        public ANetMod()
        {
            LoadedMods.Add(this);
        }

        /// Error or client exit
        public virtual void Dispose() {
            LoadedMods.Remove(this);
        }

        // TODO: some hooks
        public virtual void Update() { }
    }
}