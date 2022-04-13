using System;
using System.IO;

namespace Barotrauma
{
    static class LuaCsUpdateChecker
    {
        public static void Check()
        {
            if (!File.Exists(LuaSetup.VERSION_FILE)) { return; }

            ContentPackage luaCsPackage = LuaSetup.GetPackage();
            string luaCsPath = Path.GetDirectoryName(luaCsPackage.Path);

            if (luaCsPackage == null) { return; }

            string clientVersion = File.ReadAllText(LuaSetup.VERSION_FILE);
            string serverVersion = luaCsPackage.ModVersion;

            if (clientVersion == serverVersion) { return; }

            var msg = new GUIMessageBox("LuaCs Update", $"Your LuaCs client version is different from the version found in the LuaCsForBarotrauma workshop files. Do you want to update?\n\n Client Version: {clientVersion}\n Server Version: {serverVersion}", 
                new LocalizedString[2] { TextManager.Get("Yes"), TextManager.Get("Cancel") });

            msg.Buttons[0].OnClicked = (GUIButton button, object obj) =>
            {
                string[] filesToUpdate = new string[]
                {
                    "Barotrauma.dll", "Barotrauma.deps.json",
                    "0harmony.dll", "MoonSharp.Interpreter.dll",
                };


                try
                {
                    foreach (string file in filesToUpdate)
                    {
                        File.Move(file, file + ".todelete", true);
                        File.Copy(Path.Combine(luaCsPath, "Binary", file), file, true);
                    }

                    File.WriteAllText(LuaSetup.VERSION_FILE, serverVersion);
                }
                catch (Exception e)
                {
                    new GUIMessageBox("Failed", $"Failed to update, error: {e}");

                    msg.Close();
                    return true;
                }

                new GUIMessageBox("Restart", "LuaCs updated! Restart your game to apply the changes.");

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