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
    partial class NetSetup
    {
        public NetScriptLoader Loader { get; private set; }
        public Harmony harmony;
        public LuaHook hook;
        public NetSetup() => Initialize();

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

            //var baseType = typeof(CSharpSyntaxNode);
            //var typeDict = new Dictionary<Type, object>() { { baseType, new Dictionary<Type, object>() }  };
            //var sytaxTypes = AppDomain.CurrentDomain
            //    .GetAssemblies()
            //    .Where(a => !(a.IsDynamic || string.IsNullOrEmpty(a.Location) || a.Location.Contains("xunit")))
            //    .Select(a => a.GetTypes().Where(t => t.IsSubclassOf(baseType)))
            //    .Aggregate((t1, t2) => t1.Concat(t2))
            //    .ToList();

            //var q = new Queue<(Type, Dictionary<Type, object>)>();
            //q.Enqueue((baseType, typeDict[baseType] as Dictionary<Type, object>));
            //while (q.Count > 0)
            //{
            //    var entry = q.Dequeue();
            //    var type = entry.Item1;
            //    var dict = entry.Item2;

            //    sytaxTypes.Where(t => t.BaseType == type).ToList().ForEach(t =>
            //    {
            //        var newDict = new Dictionary<Type, object>();
            //        dict.Add(t, newDict);
            //        q.Enqueue((t, newDict));
            //    });
            //}

            //Action<int, KeyValuePair<Type, object>> rprint = null;
            //rprint = (indent, kv) =>
            //{
            //    string idnt = "";
            //    for (int i = 0; i < indent - 1; i++) idnt += "│ ";
            //    if (indent > 0) idnt += "├─";
            //    Console.WriteLine(idnt + kv.Key.Name);
            //    foreach (var _kv in kv.Value as Dictionary<Type, object>)
            //        rprint(indent + 1, _kv);
            //};
            //foreach (var kv in typeDict) rprint(0, kv);
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
            harmony?.UnpatchAll();
            hook?.Call("stop", new object[] { });

            hook = null;
            Loader = null;

            ANetMod.LoadedMods.ForEach(m => m.Dispose());
            ANetMod.LoadedMods.Clear();
            Loader.Unload();
        }

        public void Update()
        {
            hook?.Update();
        }

        private static bool wasPublicized = false;

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
    }
}