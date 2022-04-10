using System;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;
using HarmonyLib;
using System.Runtime.CompilerServices;
//using System.Linq;
//using System.Collections.Generic;
//using Microsoft.CodeAnalysis;
//using Microsoft.CodeAnalysis.CSharp;

[assembly: InternalsVisibleTo("NetScriptAssembly", AllInternalsVisible = true)]

namespace Barotrauma
{
    partial class NetSetup : IDisposable
    {
        public NetScriptLoader Loader { get; private set; }
        public Harmony harmony;
        public LuaHook hook;
        public NetSetup() => Initialize();
        public void Dispose() => Stop();

        public void Reload()
        {
            Stop();
            Initialize();
            Execute();
        }
        public void Initialize()
        {
            hook = new LuaHook();
            Loader = new NetScriptLoader(this);
            Loader.SearchFolders(); 
        }
        public void Execute()
        {
            if (Loader == null) throw new Exception("NetSetup was not properly initialized.");
            try
            {
                var modTypes = Loader.Compile();
                modTypes.ForEach(t => t.GetConstructor(new Type[] { }).Invoke(null));
            }
            catch (Exception ex)
            {
                PrintMessage(ex);
            }
        }
        public void Stop()
        {
            ANetMod.LoadedMods.ForEach(m => m.Dispose());
            ANetMod.LoadedMods.Clear();
            Loader.Unload();

            harmony?.UnpatchAll();
            hook?.Call("stop", new object[] { });

            hook = null;
            Loader = null;
        }

        public void Update()
        {
            hook?.Update();
            ANetMod.LoadedMods.ForEach(m => m.Update());
        }

        public static void PrintMessage(object message)
        {
            if (message == null) { message = "null"; }
            string str = message.ToString();

#if SERVER
            if (GameMain.Server != null)
            {
                for (int i = 0; i < str.Length; i += 1024)
                {
                    string subStr = str.Substring(i, Math.Min(1024, str.Length - i));


                    foreach (var c in GameMain.Server.ConnectedClients)
                    {
                        GameMain.Server.SendDirectChatMessage(ChatMessage.Create("", subStr, ChatMessageType.Console, null, textColor: Color.MediumPurple), c);
                    }

                    GameServer.Log("[NET] " + subStr, ServerLog.MessageType.ServerMessage);
                }
            }
            else
            {
                DebugConsole.NewMessage("[NET]" + message.ToString(), Color.MediumPurple);
            }
#else
            DebugConsole.NewMessage(message.ToString(), Color.Purple);
#endif
        }

        public void HandleException(Exception ex, string? info)
        {
            throw new NotImplementedException();
        }
    }
}