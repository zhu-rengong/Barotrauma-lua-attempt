using System;
using System.IO;

namespace Barotrauma
{
    static class LuaCsUpdateChecker
    {
        public static void Check()
        {
            if (!File.Exists(LuaCsSetup.VERSION_FILE)) { return; }

            ContentPackage luaPackage = LuaCsSetup.GetPackage("Lua For Barotrauma");
            string luaCsPath = Path.GetDirectoryName(luaPackage.Path);

            if (luaPackage == null) { return; }

            string clientVersion = File.ReadAllText(LuaCsSetup.VERSION_FILE);
            string workshopVersion = luaPackage.ModVersion;

            if (clientVersion == workshopVersion) { return; }

            string additional = LuaCsSetup.GetPackage("CsForBarotrauma", false) == null ? "" : "Cs";

            var msg = new GUIMessageBox($"Lua{additional} Update", $"Your Lua{additional} client version is different from the version found in the Lua{additional}ForBarotrauma workshop files. Do you want to update?\n\n Client Version: {clientVersion}\n Workshop Version: {workshopVersion}", 
                new LocalizedString[2] { TextManager.Get("Yes"), TextManager.Get("Cancel") });

            msg.Buttons[0].OnClicked = (GUIButton button, object obj) =>
            {
                string[] filesToUpdate = new string[]
                {
                    "Barotrauma.dll", "Barotrauma.deps.json",
                    "0harmony.dll", "Mono.Cecil.dll",
                    "Mono.Cecil.Mdb.dll", "Mono.Cecil.Pdb.dll",
                    "Mono.Cecil.Rocks.dll", "MonoMod.Common.dll",
                    "MoonSharp.Interpreter.dll",
                    "mscordaccore_amd64_amd64_4.700.22.11601.dll",

                    "Microsoft.CodeAnalysis.dll", "Microsoft.CodeAnalysis.CSharp.dll",
                    "Microsoft.CodeAnalysis.CSharp.Scripting.dll", "Microsoft.CodeAnalysis.Scripting.dll",

                    "System.Reflection.Metadata.dll", "System.Collections.Immutable.dll",
                    "System.Runtime.CompilerServices.Unsafe.dll"
                };


                try
                {
                    foreach (string file in filesToUpdate)
                    {
                        File.Move(file, file + ".todelete", true);
                        File.Copy(Path.Combine(luaCsPath, "Binary", file), file, true);
                    }

                    File.WriteAllText(LuaCsSetup.VERSION_FILE, workshopVersion);
                }
                catch (Exception e)
                {
                    new GUIMessageBox("Failed", $"Failed to update, error: {e}");

                    msg.Close();
                    return true;
                }

                new GUIMessageBox("Restart", $"Lua{additional} updated! Restart your game to apply the changes.");

                msg.Close();
                return true;
            };

            msg.Buttons[1].OnClicked = (GUIButton button, object obj) =>
            {
                msg.Close();
                return true;
            };
            
        }
    }
}