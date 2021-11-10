using System;
using System.Collections.Generic;
using System.Text;
using MoonSharp.Interpreter;
using Microsoft.Xna.Framework;
using Barotrauma.Networking;
using System.Threading.Tasks;
using Barotrauma.Items.Components;
using System.IO;
using System.Net;
using System.Linq;
using System.Xml.Linq;


namespace Barotrauma
{
	partial class LuaSetup
	{
		partial class LuaGame
		{
			public bool IsDedicated
			{
				get
				{
					return GameMain.Server.ServerPeer is LidgrenServerPeer;
				}
			}

			public ServerSettings ServerSettings => GameMain.Server.ServerSettings;

			public static void SendMessage(string msg, ChatMessageType? messageType = null, Client sender = null, Character character = null)
			{
				GameMain.Server.SendChatMessage(msg, messageType, sender, character);
			}

			public static void SendMessage(string msg, int messageType, Client sender = null, Character character = null)
			{
				GameMain.Server.SendChatMessage(msg, (ChatMessageType)messageType, sender, character);
			}

			public static void SendTraitorMessage(Client client, string msg, string missionid, TraitorMessageType type)
			{
				GameMain.Server.SendTraitorMessage(client, msg, missionid, type);
			}

			public static void SendDirectChatMessage(string sendername, string text, Character sender, ChatMessageType messageType = ChatMessageType.Private, Client client = null, string iconStyle = "")
			{

				ChatMessage cm = ChatMessage.Create(sendername, text, messageType, sender, client);
				cm.IconStyle = iconStyle;

				GameMain.Server.SendDirectChatMessage(cm, client);

			}

			public static void SendDirectChatMessage(ChatMessage chatMessage, Client client)
			{
				GameMain.Server.SendDirectChatMessage(chatMessage, client);
			}

			public static void Log(string message, ServerLog.MessageType type)
			{
				GameServer.Log(message, type);
			}

			public static void DispatchRespawnSub()
			{
				GameMain.Server.RespawnManager.DispatchShuttle();
			}

			public static void StartGame()
			{
				GameMain.Server.StartGame();
			}

			public void AssignOnClientRequestExecute(string names, object onExecute) => DebugConsole.AssignOnClientRequestExecute(names, (Client a, Vector2 b, string[] c) => { env.CallFunction(onExecute, new object[] { a, b, c }); });
		}
	}
}
