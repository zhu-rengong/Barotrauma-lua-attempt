
using System;
using System.IO;
using System.Linq;

namespace Barotrauma
{
    static partial class LuaCsInstaller
    {
        private static void CreateMissingDirectory()
        {
            Directory.CreateDirectory("Temp/Original");
            Directory.CreateDirectory("Temp/ToDelete");
            Directory.CreateDirectory("Temp/Old");
        }

    }
}
