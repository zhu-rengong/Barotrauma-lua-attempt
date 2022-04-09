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

        public abstract void Dispose();

        // TODO: some hooks
    }
}