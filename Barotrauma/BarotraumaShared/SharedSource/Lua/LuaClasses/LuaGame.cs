using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using MoonSharp.Interpreter;

namespace Barotrauma
{
	partial class LuaGame
	{
		public bool IsSingleplayer => GameMain.IsSingleplayer;
		public bool IsMultiplayer => GameMain.IsMultiplayer;

#if CLIENT
		public bool Paused => GameMain.Instance?.Paused == true;

		public byte MyID => GameMain.Client.ID;


		public ChatBox ChatBox
		{
			get
			{
				if (GameMain.IsSingleplayer)
					return GameMain.GameSession.CrewManager.ChatBox;
				else
					return GameMain.Client.ChatBox;
			}
		}
#else

		public bool IsDedicated
		{
			get
			{
				return GameMain.Server.ServerPeer is LidgrenServerPeer;
			}
		}

		public ServerSettings ServerSettings => GameMain.Server.ServerSettings;
#endif

		public DynValue Settings;


		public bool allowWifiChat = false;
		public bool overrideTraitors = false;
		public bool overrideRespawnSub = false;
		public bool overrideSignalRadio = false;
		public bool disableSpamFilter = false;
		public bool disableDisconnectCharacter = false;
		public bool enableControlHusk = false;

		public int mapEntityUpdateInterval
		{
			get { return MapEntity.MapEntityUpdateInterval; }
			set { MapEntity.MapEntityUpdateInterval = value; }
		}

		public int gapUpdateInterval
		{
			get { return MapEntity.GapUpdateInterval; }
			set { MapEntity.GapUpdateInterval = value; }
		}

		public int characterUpdateInterval
		{
			get { return Character.CharacterUpdateInterval; }
			set { Character.CharacterUpdateInterval = value; }
		}

		public HashSet<Item> updatePriorityItems = new HashSet<Item>();
		public HashSet<Character> updatePriorityCharacters = new HashSet<Character>();

		public void AddPriorityItem(Item item)
		{
			updatePriorityItems.Add(item);
		}

		public void RemovePriorityItem(Item item)
		{
			updatePriorityItems.Remove(item);
		}

		public void ClearPriorityItem()
		{
			updatePriorityItems.Clear();
		}

		public void AddPriorityCharacter(Character character)
		{
			updatePriorityCharacters.Add(character);
		}

		public void RemovePriorityCharacter(Character character)
		{
			updatePriorityCharacters.Remove(character);
		}

		public void ClearPriorityCharacter()
		{
			updatePriorityCharacters.Clear();
		}

		public bool RoundStarted
		{

			get
			{
#if SERVER
				return GameMain.Server.GameStarted;
#else
				return GameMain.Client.GameStarted;
#endif
			}
		}

		public GameSession GameSession
		{
			get
			{
				return GameMain.GameSession;
			}
		}

		public NetLobbyScreen NetLobbyScreen
		{
			get
			{
				return GameMain.NetLobbyScreen;
			}
		}

		public GameScreen GameScreen
		{
			get
			{
				return GameMain.GameScreen;
			}
		}

		public World World
		{
			get
			{
				return GameMain.World;
			}
		}

#if SERVER
			public ServerPeer Peer
			{
				get
				{
					return GameMain.Server.ServerPeer;
				}
			}
#else
		public ClientPeer Peer
		{
			get
			{
				return GameMain.Client.ClientPeer;
			}
		}
#endif

		public LuaGame()
        {
			LuaUserData.MakeFieldAccessible(UserData.RegisterType(typeof(GameSettings)), "currentConfig");
			Settings = UserData.CreateStatic(typeof(GameSettings));
        }

		public void OverrideTraitors(bool o)
		{
			overrideTraitors = o;
		}

		public void OverrideRespawnSub(bool o)
		{
			overrideRespawnSub = o;
		}

		public void AllowWifiChat(bool o)
		{
			allowWifiChat = o;
		}

		public void OverrideSignalRadio(bool o)
		{
			overrideSignalRadio = o;
		}

		public void DisableSpamFilter(bool o)
		{
			disableSpamFilter = o;
		}

		public void DisableDisconnectCharacter(bool o)
		{
			disableDisconnectCharacter = o;
		}


		public void EnableControlHusk(bool o)
		{
			enableControlHusk = o;
		}

		public static void Explode(Vector2 pos, float range = 100, float force = 30, float damage = 30, float structureDamage = 30, float itemDamage = 30, float empStrength = 0, float ballastFloraStrength = 0)
		{
			new Explosion(range, force, damage, structureDamage, itemDamage, empStrength, ballastFloraStrength).Explode(pos, null);
		}

		public static string SpawnItem(string name, Vector2 pos, bool inventory = false, Character character = null)
		{
			string error;
			DebugConsole.SpawnItem(new string[] { name, inventory ? "inventory" : "cursor" }, pos, character, out error);
			return error;
		}

		public static ContentPackage[] GetEnabledContentPackages()
        {
			return ContentPackageManager.EnabledPackages.All.ToArray();
        }

		public static ItemPrefab GetItemPrefab(string itemNameOrId)
		{
			ItemPrefab itemPrefab =
			(MapEntityPrefab.Find(itemNameOrId, identifier: null, showErrorMessages: false) ??
			MapEntityPrefab.Find(null, identifier: itemNameOrId, showErrorMessages: false)) as ItemPrefab;

			return itemPrefab;
		}

		public static Submarine GetRespawnSub()
		{
#if SERVER
				if (GameMain.Server.RespawnManager == null)
					return null;
				return GameMain.Server.RespawnManager.RespawnShuttle;
#else
			if (GameMain.Client.RespawnManager == null)
				return null;
			return GameMain.Client.RespawnManager.RespawnShuttle;
#endif
		}

		public static Items.Components.Steering GetSubmarineSteering(Submarine sub)
		{
			foreach (Item item in Item.ItemList)
			{
				if (item.Submarine != sub) continue;

				var steering = item.GetComponent<Items.Components.Steering>();
				if (steering != null)
				{
					return steering;
				}
			}

			return null;
		}

		public static WifiComponent GetWifiComponent(Item item)
		{
			if (item == null) return null;
			return item.GetComponent<WifiComponent>();
		}

		public static LightComponent GetLightComponent(Item item)
		{
			if (item == null) return null;
			return item.GetComponent<LightComponent>();
		}

		public static CustomInterface GetCustomInterface(Item item)
		{
			if (item == null) return null;
			return item.GetComponent<CustomInterface>();
		}

		public static Fabricator GetFabricatorComponent(Item item)
		{
			if (item == null) return null;
			return item.GetComponent<Fabricator>();
		}

		public static Holdable GetHoldableComponent(Item item)
		{
			if (item == null) return null;
			return item.GetComponent<Holdable>();
		}

		public static void ExecuteCommand(string command)
		{
			DebugConsole.ExecuteCommand(command);
		}

		public static Signal CreateSignal(string value, int stepsTaken = 1, Character sender = null, Item source = null, float power = 0, float strength = 1)
		{
			return new Signal(value, stepsTaken, sender, source, power, strength);
		}

		private List<DebugConsole.Command> luaAddedCommand = new List<DebugConsole.Command>();

		public void RemoveCommand(string name)
		{
			for (var i = 0; i < DebugConsole.Commands.Count; i++)
			{
				foreach (var cmdname in DebugConsole.Commands[i].names)
				{
					if (cmdname == name)
					{
						luaAddedCommand.Remove(DebugConsole.Commands[i]);
						DebugConsole.Commands.RemoveAt(i);
						continue;
					}
				}
			}
		}

		public void AddCommand(string name, string help, object onExecute, object getValidArgs = null, bool isCheat = false)
		{
			var cmd = new DebugConsole.Command(name, help, (string[] arg1) => { GameMain.Lua.CallFunction(onExecute, new object[] { arg1 }); },
				() =>
				{
					if (getValidArgs == null) return null;
					var result = new LuaResult(GameMain.Lua.CallFunction(getValidArgs, new object[] { }));
					var obj = result.Object();
					if (obj is string[][]) return (string[][])obj;
					return null;
				}, isCheat);

			luaAddedCommand.Add(cmd);
			DebugConsole.Commands.Add(cmd);

#if SERVER
				foreach (var client in GameMain.Server.ConnectedClients) {
					var index = client.PermittedConsoleCommands.FindIndex((pc) => pc.names[0] == cmd.names[0]);
					if (index > -1) {
						client.PermittedConsoleCommands[index] = cmd;
					}
				}
				foreach (var permissions in GameMain.Server.ServerSettings.ClientPermissions) {
					var index = permissions.PermittedCommands.FindIndex((pc) => pc.names[0] == cmd.names[0]);
					if (index > -1) {
						permissions.PermittedCommands[index] = cmd;
					}
				}
#endif
		}

		public List<DebugConsole.Command> Commands => DebugConsole.Commands;

		public void AssignOnExecute(string names, object onExecute) => DebugConsole.AssignOnExecute(names, (string[] a) => { GameMain.Lua.CallFunction(onExecute, new object[] { a }); });


#if SERVER

		public static void SendMessage(string msg, ChatMessageType? messageType = null, Client sender = null, Character character = null)
		{
			GameMain.Server.SendChatMessage(msg, messageType, sender, character);
		}

		public static void SendMessage(string msg, int messageType, Client sender = null, Character character = null)
		{
			GameMain.Server.SendChatMessage(msg, (ChatMessageType)messageType, sender, character);
		}

		public static void SendTraitorMessage(Client client, string msg, Identifier missionid, TraitorMessageType type)
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

		public static void EndGame()
		{
			GameMain.Server.EndGame();
		}

		public void AssignOnClientRequestExecute(string names, object onExecute) => DebugConsole.AssignOnClientRequestExecute(names, (Client a, Vector2 b, string[] c) => { GameMain.Lua.CallFunction(onExecute, new object[] { a, b, c }); });

#endif

		public void Stop()
		{
			mapEntityUpdateInterval = 1;
			gapUpdateInterval = 4;
			characterUpdateInterval = 1;

			foreach (var cmd in luaAddedCommand)
			{
				DebugConsole.Commands.Remove(cmd);
			}
		}
	}

}