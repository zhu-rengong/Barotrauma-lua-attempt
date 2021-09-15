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
		}

		partial class LuaPlayer
		{

			public static List<Character> GetAllCharacters()
			{
				return Character.CharacterList;
			}

			public static List<Client> GetAllClients()
			{
				return GameMain.Server.ConnectedClients;
			}

			public static CharacterInfo CreateCharacterInfo(string speciesName, string name = "", JobPrefab jobPrefab = null, string ragdollFileName = null, int variant = 0, Rand.RandSync randSync = Rand.RandSync.Unsynced, string npcIdentifier = "")
			{
				return new CharacterInfo(speciesName, name, name, jobPrefab, ragdollFileName, variant, randSync, npcIdentifier);
			}

			public static void SetClientCharacter(Client client, Character character)
			{
				GameMain.Server.SetClientCharacter(client, character);
			}

			public static void SetCharacterTeam(Character character, int team)
			{
				character.TeamID = (CharacterTeamType)team;
			}

			public static void SetClientTeam(Client character, int team)
			{
				character.TeamID = (CharacterTeamType)team;
			}

			public static void Kick(Client client, string reason = "")
			{
				GameMain.Server.KickClient(client.Connection, reason);
			}

			public static void Ban(Client client, string reason = "", bool range = false, float seconds = -1)
			{
				if (seconds == -1)
				{
					GameMain.Server.BanClient(client, reason, range, null);
				}
				else
				{
					GameMain.Server.BanClient(client, reason, range, TimeSpan.FromSeconds(seconds));
				}
			}

			public static void UnbanPlayer(string player, string endpoint)
			{
				GameMain.Server.UnbanPlayer(player, endpoint);

			}

			public static void SetSpectatorPos(Client client, Vector2 pos)
			{

			}

			public static void SetRadioRange(Character character, float range)
			{
				if (character.Inventory == null) { return; }

				foreach (Item item in character.Inventory.AllItems)
				{
					if (item == null) { continue; }

					if (item.Name == "Headset")
					{
						item.GetComponent<Items.Components.WifiComponent>().Range = range;
					}
				}
			}

			public static bool CheckPermission(Client client, ClientPermissions permissions)
			{
				return client.Permissions.HasFlag(permissions);
			}
		}
	}
}
