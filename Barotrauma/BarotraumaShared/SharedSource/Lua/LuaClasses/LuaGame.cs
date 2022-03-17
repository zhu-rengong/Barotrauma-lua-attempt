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
		public bool IsSinglePlayer => GameMain.IsSingleplayer;
		public bool IsMultiplayer => GameMain.IsMultiplayer;
		public GameSettings GameSettings => GameMain.Config;

#if CLIENT
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

		public HashSet<Item> updatePriorityItems = new HashSet<Item>();

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

		public static Character Spawn(string name, Vector2 worldPos)
		{
			Character spawnedCharacter = null;
			Vector2 spawnPosition = worldPos;

			string characterLowerCase = name.ToLowerInvariant();
			JobPrefab job = null;
			if (!JobPrefab.Prefabs.ContainsKey(characterLowerCase))
			{
				job = JobPrefab.Prefabs.Find(jp => jp.Name != null && jp.Name.Equals(characterLowerCase, StringComparison.OrdinalIgnoreCase));
			}
			else
			{
				job = JobPrefab.Prefabs[characterLowerCase];
			}
			bool human = job != null || characterLowerCase == CharacterPrefab.HumanSpeciesName;


			if (string.IsNullOrWhiteSpace(name)) { return null; }

			if (human)
			{
				var variant = job != null ? Rand.Range(0, job.Variants, Rand.RandSync.Server) : 0;
				CharacterInfo characterInfo = new CharacterInfo(CharacterPrefab.HumanSpeciesName, jobPrefab: job, variant: variant);
				spawnedCharacter = Character.Create(characterInfo, spawnPosition, ToolBox.RandomSeed(8));
				if (GameMain.GameSession != null)
				{
					//TODO: a way to select which team to spawn to?
					spawnedCharacter.TeamID = Character.Controlled != null ? Character.Controlled.TeamID : CharacterTeamType.Team1;
#if CLIENT
					GameMain.GameSession.CrewManager.AddCharacter(spawnedCharacter);
#endif
				}
				spawnedCharacter.GiveJobItems(null);
				spawnedCharacter.Info.StartItemsGiven = true;
			}
			else
			{
				if (CharacterPrefab.FindBySpeciesName(name) != null)
				{
					spawnedCharacter = Character.Create(name, spawnPosition, ToolBox.RandomSeed(8));
				}
			}

			return spawnedCharacter;
		}

		public static string SpawnItem(string name, Vector2 pos, bool inventory = false, Character character = null)
		{
			string error;
			DebugConsole.SpawnItem(new string[] { name, inventory ? "inventory" : "cursor" }, pos, character, out error);
			return error;
		}

		public static void RemoveItem(Item item)
		{
			EntitySpawner.Spawner.AddToRemoveQueue(item);
		}

		public static ItemPrefab GetItemPrefab(string itemNameOrId)
		{
			ItemPrefab itemPrefab =
			(MapEntityPrefab.Find(itemNameOrId, identifier: null, showErrorMessages: false) ??
			MapEntityPrefab.Find(null, identifier: itemNameOrId, showErrorMessages: false)) as ItemPrefab;

			return itemPrefab;
		}

		public void AddItemPrefabToSpawnQueue(ItemPrefab itemPrefab, Vector2 position, DynValue spawned = null)
		{
			EntitySpawner.Spawner.AddToSpawnQueue(itemPrefab, position, onSpawned: (Item item) => {
				if (spawned?.Type == DataType.Function) GameMain.Lua.CallFunction(spawned, UserData.Create(item));
			});
		}

		public void AddItemPrefabToSpawnQueue(ItemPrefab itemPrefab, Inventory inventory, DynValue spawned = null)
		{
			EntitySpawner.Spawner.AddToSpawnQueue(itemPrefab, inventory, onSpawned: (Item item) => {
				if (spawned?.Type == DataType.Function) GameMain.Lua.CallFunction(spawned, UserData.Create(item));
			});
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

		public static ContentPackage[] GetEnabledContentPackages()
		{
			return GameMain.Config.AllEnabledPackages.ToArray();
		}

		public static List<string> GetEnabledPackagesDirectlyFromFile()
		{
			List<string> enabledPackages = new List<string>();

			XDocument doc = XMLExtensions.LoadXml("config_player.xml");
			var contentPackagesElement = doc.Root.Element("contentpackages");

			string coreName = contentPackagesElement.Element("core")?.GetAttributeString("name", "");
			enabledPackages.Add(coreName);

			XElement regularElement = contentPackagesElement.Element("regular");
			List<XElement> subElements = regularElement?.Elements()?.ToList();

			foreach (var subElement in subElements)
			{
				if (!bool.TryParse(subElement.GetAttributeString("enabled", "false"), out bool enabled) || !enabled) { continue; }

				string name = subElement.GetAttributeString("name", null);
				enabledPackages.Add(name);
			}
			return enabledPackages;
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

			foreach (var cmd in luaAddedCommand)
			{
				DebugConsole.Commands.Remove(cmd);
			}
		}
	}

}