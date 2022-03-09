using System;
using System.Collections.Generic;
using System.Text;
using MoonSharp.Interpreter;
using Microsoft.Xna.Framework;
using Barotrauma.Networking;
using Barotrauma.Items.Components;
using System.IO;
using System.Net;
using System.Linq;
using System.Xml.Linq;
using FarseerPhysics.Dynamics;
using System.Reflection;
using HarmonyLib;
using MoonSharp.Interpreter.Interop;
using System.Diagnostics;

namespace Barotrauma
{
	partial class LuaSetup
	{
		partial class LuaUserData
		{
			public static Type GetType(string typeName)
			{
				var type = Type.GetType(typeName);
				if (type != null) return type;
				foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
				{
					type = a.GetType(typeName);
					if (type != null)
						return type;
				}
				return null;
			}

			public static IUserDataDescriptor RegisterType(string typeName)
			{
				Type type = GetType(typeName);

				if (type == null)
				{
					GameMain.Lua.HandleLuaException(new Exception($"Tried to register a type that doesn't exist: {typeName}."));
					return null;
				}

				MethodInfo method = typeof(UserData).GetMethod(nameof(UserData.RegisterType), new Type[2] { typeof(InteropAccessMode), typeof(string) });
				MethodInfo generic = method.MakeGenericMethod(type);
				return (IUserDataDescriptor)generic.Invoke(null, new object[] { null, null });
			}

			public static void UnregisterType(string typeName)
			{
				Type type = GetType(typeName);

				if (type == null)
				{
					GameMain.Lua.HandleLuaException(new Exception($"Tried to unregister a type that doesn't exist: {typeName}."));
					return;
				}

				MethodInfo method = typeof(UserData).GetMethod(nameof(UserData.UnregisterType), new Type[2] { typeof(InteropAccessMode), typeof(string) });
				MethodInfo generic = method.MakeGenericMethod(type);
				generic.Invoke(null, new object[] { null, null });
			}
			public static IUserDataDescriptor RegisterGenericType(string typeName, params string[] typeNameArguements)
			{
				Type type = GetType(typeName);
				Type[] typeArguements = typeNameArguements.Select(x => GetType(x)).ToArray();
				Type genericType = type.MakeGenericType(typeArguements);
				return UserData.RegisterType(genericType);
			}

			public static void UnregisterGenericType(string typeName, params string[] typeNameArguements)
			{
				Type type = GetType(typeName);
				Type[] typeArguements = typeNameArguements.Select(x => GetType(x)).ToArray();
				Type genericType = type.MakeGenericType(typeArguements);
				UserData.UnregisterType(genericType);
			}

			private static bool IsType<T>(object obj) { return obj is T; }

			public static bool IsTargetType(object obj, string typeName)
			{
				var type = GetType(typeName);
				MethodInfo method = typeof(LuaUserData).GetMethod(nameof(IsType), BindingFlags.NonPublic | BindingFlags.Static);
				MethodInfo generic = method.MakeGenericMethod(type);
				return (bool)generic.Invoke(null, new object[] { obj });
			}

			public static object CreateStatic(string typeName)
			{
				Type type = GetType(typeName);

				if (type == null)
				{
					GameMain.Lua.HandleLuaException(new Exception($"Tried to create a static userdata of a type that doesn't exist: {typeName}."));
					return null;
				}

				MethodInfo method = typeof(UserData).GetMethod(nameof(UserData.CreateStatic), 1, new Type[0]);
				MethodInfo generic = method.MakeGenericMethod(type);
				return generic.Invoke(null, null);
			}

			public static void MakeFieldAccessible(IUserDataDescriptor IUUD, string fieldName)
			{
				var descriptor = (StandardUserDataDescriptor)IUUD;
				var field = IUUD.Type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
				descriptor.RemoveMember(fieldName);
				descriptor.AddMember(fieldName, new FieldMemberDescriptor(field, InteropAccessMode.Default));
			}

			public static void MakeMethodAccessible(IUserDataDescriptor IUUD, string methodName)
			{
				var descriptor = (StandardUserDataDescriptor)IUUD;
				var method = IUUD.Type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
				descriptor.RemoveMember(methodName);
				descriptor.AddMember(methodName, new MethodMemberDescriptor(method, InteropAccessMode.Default));
			}

			public static void AddMethod(IUserDataDescriptor IUUD, string methodName, object function)
			{
				var descriptor = (StandardUserDataDescriptor)IUUD;
				descriptor.RemoveMember(methodName);
				descriptor.AddMember(methodName, new ObjectCallbackMemberDescriptor(methodName, (object arg1, ScriptExecutionContext arg2, CallbackArguments arg3) => 
				{
					if (luaSetup != null)
						return luaSetup.CallFunction(function, arg3.GetArray());
					return null;
				}));
			}

			public static void RemoveMember(IUserDataDescriptor IUUD, string memberName)
			{
				var descriptor = (StandardUserDataDescriptor)IUUD;
				descriptor.RemoveMember(memberName);
			}
		}

		public partial class LuaGame
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
#endif

			LuaSetup env;

			public LuaGame(LuaSetup e)
			{
				env = e;
			}

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
					if (spawned?.Type == DataType.Function) env.lua.Call(spawned, UserData.Create(item));
				});
			}

			public void AddItemPrefabToSpawnQueue(ItemPrefab itemPrefab, Inventory inventory, DynValue spawned = null)
			{
				EntitySpawner.Spawner.AddToSpawnQueue(itemPrefab, inventory, onSpawned: (Item item) => {
					if (spawned?.Type == DataType.Function) env.lua.Call(spawned, UserData.Create(item));
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
					if (!bool.TryParse(subElement.GetAttributeString("enabled", "false"), out bool enabled) || !enabled)	{ continue; }

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
				var cmd = new DebugConsole.Command(name, help, (string[] arg1) => { env.CallFunction(onExecute, new object[] { arg1 }); },
					() =>
					{
						if (getValidArgs == null) return null;
						var result = new LuaResult(env.CallFunction(getValidArgs, new object[] { }));
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

			public void AssignOnExecute(string names, object onExecute) => DebugConsole.AssignOnExecute(names, (string[] a) => { env.CallFunction(onExecute, new object[] { a }); });


			public void Stop()
			{
				mapEntityUpdateInterval = 1;
				gapUpdateInterval = 4;

				foreach(var cmd in luaAddedCommand)
				{
					DebugConsole.Commands.Remove(cmd);
				}
			}
		}


		public partial class LuaTimer
		{
			public static long LastUpdateTime = 0;

			public LuaSetup env;

			public LuaTimer(LuaSetup e)
			{
				env = e;
			}

			public static double Time
			{
				get
				{
					return GetTime();
				}
			}

			public void Wait(object function, int millisecondDelay)
			{
				env.hook.EnqueueTimedFunction((float)Timing.TotalTime + (millisecondDelay / 1000f), function);
			}

			public static double GetTime()
			{
				return Timing.TotalTime;
			}

			public static float GetUsageMemory()
			{
				Process proc = Process.GetCurrentProcess();
				float memory = MathF.Round(proc.PrivateMemorySize64 / (1024 * 1024), 2);
				proc.Dispose();

				return memory;
			}
		}

		private partial class LuaRandom
		{
			Random random;

			public LuaRandom()
			{
				random = new Random();
			}

			public int Range(int min, int max)
			{
				return random.Next(min, max);
			}

			public float RangeFloat(float min, float max)
			{
				double range = (double)max - (double)min;
				double sample = random.NextDouble();
				double scaled = (sample * range) + min;
				float f = (float)scaled;

				return f;
			}

		}

		private partial class LuaFile
		{
			// TODO: SANDBOXING

			public static bool IsPathAllowed(string path)
			{
				path = Path.GetFullPath(path).CleanUpPath();

				if (path.StartsWith(Path.GetFullPath("Mods").CleanUpPath()))
					return true;

				if (path.StartsWith(Path.GetFullPath("Submarines").CleanUpPath()))
					return true;

				if (path.StartsWith(Path.GetFullPath("Data").CleanUpPath()))
					return true;

				if (path.StartsWith(Path.GetFullPath("Lua").CleanUpPath()))
					return true;

				if (path.StartsWith(Path.GetFullPath("Content").CleanUpPath()))
					return true;

				return false;
			}

			public static bool IsPathAllowedLuaException(string path)
			{
				if (IsPathAllowed(path))
					return true;
				else
					luaSetup.HandleLuaException(new Exception("File access to \"" + path + "\" not allowed."));

				return false;
			}

			public static string Read(string path)
			{
				if (!IsPathAllowedLuaException(path))
					return "";

				return File.ReadAllText(path);
			}

			public static void Write(string path, string text)
			{
				if (!IsPathAllowedLuaException(path))
					return;

				File.WriteAllText(path, text);
			}

			public static bool Exists(string path)
			{
				if (!IsPathAllowedLuaException(path))
					return false;

				return File.Exists(path);
			}

			public static bool CreateDirectory(string path)
			{
				if (!IsPathAllowedLuaException(path))
					return false;

				Directory.CreateDirectory(path);

				return true;
			}

			public static bool DirectoryExists(string path)
			{
				if (!IsPathAllowedLuaException(path))
					return false;

				return Directory.Exists(path);
			}

			public static string[] GetFiles(string path)
			{
				return Directory.GetFiles(path);
			}

			public static string[] GetDirectories(string path)
			{
				if (!IsPathAllowedLuaException(path))
					return new string[] { };

				return Directory.GetDirectories(path);
			}

			public static string[] DirSearch(string sDir)
			{
				if (!IsPathAllowedLuaException(sDir))
					return new string[] { };

				List<string> files = new List<string>();

				try
				{
					foreach (string f in Directory.GetFiles(sDir))
					{
						files.Add(f);
					}

					foreach (string d in Directory.GetDirectories(sDir))
					{
						foreach (string f in Directory.GetFiles(d))
						{
							files.Add(f);
						}
						DirSearch(d);
					}
				}
				catch (System.Exception excpt)
				{
					Console.WriteLine(excpt.Message);
				}

				return files.ToArray();
			}
		}

		public partial class LuaNetworking
		{
			public LuaSetup env;

			public bool restrictMessageSize = true;

			public LuaNetworking(LuaSetup e)
			{
				env = e;
			}

			public Dictionary<string, object> LuaNetReceives = new Dictionary<string, object>();

#if SERVER
			[MoonSharpHidden]
			public void NetMessageReceived(IReadMessage netMessage, ClientPacketHeader header, Client client = null)
			{
				if (header == ClientPacketHeader.LUA_NET_MESSAGE)
				{
					string netMessageName = netMessage.ReadString();
					if (LuaNetReceives[netMessageName] is Closure)
						env.lua.Call(LuaNetReceives[netMessageName], new object[] { netMessage, client });
				}
				else
				{
					env.hook.Call("netMessageReceived", netMessage, header, client);
				}
			}

#else
			[MoonSharpHidden]
			public void NetMessageReceived(IReadMessage netMessage, ServerPacketHeader header, Client client = null)
			{
				if (header == ServerPacketHeader.LUA_NET_MESSAGE)
				{
					string netMessageName = netMessage.ReadString();
					if (LuaNetReceives[netMessageName] is Closure)
						env.lua.Call(LuaNetReceives[netMessageName], new object[] { netMessage, client });
				}
				else
				{
					env.hook.Call("netMessageReceived", netMessage, header, client);
				}
			}
#endif

			public void Receive(string netMessageName, object callback)
			{
				LuaNetReceives[netMessageName] = callback;
			}

			public IWriteMessage Start(string netMessageName)
			{
				var message = new WriteOnlyMessage();
#if SERVER
				message.Write((byte)ServerPacketHeader.LUA_NET_MESSAGE);
#else
				message.Write((byte)ClientPacketHeader.LUA_NET_MESSAGE);
#endif
				message.Write(netMessageName);
				return ((IWriteMessage)message);
			}

			public IWriteMessage Start()
			{
				return new WriteOnlyMessage();
			}

#if SERVER
			public void ClientWriteLobby(Client client) => GameMain.Server.ClientWriteLobby(client);

			public void Send(IWriteMessage netMessage, NetworkConnection connection = null, DeliveryMethod deliveryMethod = DeliveryMethod.Reliable)
			{
				if (connection == null)
				{
					foreach (NetworkConnection conn in Client.ClientList.Select(c => c.Connection))
					{
						GameMain.Server.ServerPeer.Send(netMessage, conn, deliveryMethod);
					}
				}
				else
				{
					GameMain.Server.ServerPeer.Send(netMessage, connection, deliveryMethod);
				}
			}
#else
			public void Send(IWriteMessage netMessage, DeliveryMethod deliveryMethod = DeliveryMethod.Reliable)
			{
				GameMain.Client.ClientPeer.Send(netMessage, deliveryMethod);
			}
#endif

			public void RequestPostHTTP(string url, object callback, string data, string contentType = "application/json")
			{
				try
				{
					var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
					httpWebRequest.ContentType = contentType;
					httpWebRequest.Method = "POST";

					using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
						streamWriter.Write(data);

					httpWebRequest.BeginGetResponse(new AsyncCallback((IAsyncResult result) => 
					{
						try 
						{ 
							var httpResponse = httpWebRequest.EndGetResponse(result);
							using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
								env.hook.EnqueueFunction(callback, streamReader.ReadToEnd() );
						}catch(Exception e)
						{
							env.hook.EnqueueFunction(callback, e.ToString());
						}
					}), null);

				}catch(Exception e)
				{
					env.hook.EnqueueFunction(callback, e.ToString());
				}
			}

			public void RequestGetHTTP(string url, object callback)
			{
				try
				{
					var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);

					httpWebRequest.BeginGetResponse(new AsyncCallback((IAsyncResult result) =>
					{
						try 
						{ 
							var httpResponse = httpWebRequest.EndGetResponse(result);
							using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
								env.hook.EnqueueFunction(callback, streamReader.ReadToEnd());
						}catch (Exception e)
						{
							env.hook.EnqueueFunction(callback, e.ToString());
						}
					}), null);
				}
				catch(Exception e)
				{
					env.hook.EnqueueFunction(callback, e.ToString());
				}
			}

			public void CreateEntityEvent(INetSerializable entity, object[] extraData)
			{
				GameMain.NetworkMember.CreateEntityEvent(entity, extraData);
			}

#if SERVER
			public void UpdateClientPermissions(Client client)
			{
				GameMain.Server.UpdateClientPermissions(client);
			}

			public void RemovePendingClient(ServerPeer.PendingClient pendingClient, DisconnectReason reason, string msg)
			{
				GameMain.Server.ServerPeer.RemovePendingClient(pendingClient, reason, msg);
			}
#endif


			public ushort LastClientListUpdateID
			{
				get { return GameMain.NetworkMember.LastClientListUpdateID; }
				set { GameMain.NetworkMember.LastClientListUpdateID = value; }
			}
		}

		public partial class LuaHook
		{
			public LuaSetup env;

			public LuaHook(LuaSetup e)
			{
				env = e;
				_hookPrefixMethods = new Dictionary<long, HashSet<object>>();
				_hookPostfixMethods = new Dictionary<long, HashSet<object>>();
			}

			public class HookFunction
			{
				public string name;
				public string hookName;
				public object function;

				public HookFunction(string n, string hn, object func)
				{
					name = n;
					hookName = hn;
					function = func;
				}
			}

			private Dictionary<string, Dictionary<string, HookFunction>> hookFunctions = new Dictionary<string, Dictionary<string, HookFunction>>();

			private static Dictionary<long, HashSet<object>> _hookPrefixMethods;
			private static Dictionary<long, HashSet<object>> _hookPostfixMethods;

			private Queue<Tuple<float, object, object[]>> queuedFunctionCalls = new Queue<Tuple<float, object, object[]>>();

			public enum HookMethodType
			{
				Before, After
			}

			static void _hookLuaPatch(MethodBase __originalMethod, object[] __args, object __instance, out LuaResult result, HookMethodType hookMethodType)
			{
				result = new LuaResult(null);

#if CLIENT
				if (GameMain.GameSession?.IsRunning == false && GameMain.IsSingleplayer)
					return;
#endif

				try
				{
					var @params = __originalMethod.GetParameters();
					var ptable = new Dictionary<string, object>();
					for (int i = 0; i < @params.Length; i++)
                    {
						ptable.Add(@params[i].Name, __args[i]);
					}

					var funcAddr = ((long)__originalMethod.MethodHandle.GetFunctionPointer());
					HashSet<object> methodSet = null;
					switch (hookMethodType)
                    {
                        case HookMethodType.Before:
							methodSet = _hookPrefixMethods[funcAddr];
							break;
                        case HookMethodType.After:
							methodSet = _hookPostfixMethods[funcAddr];
							break;
                        default:
                            break;
                    }
                    if (methodSet != null)
					{
						foreach (var hookMethod in methodSet)
                        {
							result = new LuaResult(luaSetup.lua.Call(hookMethod, __instance, ptable));
                        }
					}

				}
				catch (Exception ex)
				{
					luaSetup.HandleLuaException(ex, nameof(_hookLuaPatch));
				}
			}

			static bool HookLuaPatchPrefix(MethodBase __originalMethod, object[] __args, object __instance)
			{
				_hookLuaPatch(__originalMethod, __args, __instance, out LuaResult result, HookMethodType.Before);
				
				return result.IsNull();
			}

			static bool HookLuaPatchRetPrefix(MethodBase __originalMethod, object[] __args, ref object __result, object __instance)
			{
				_hookLuaPatch(__originalMethod, __args, __instance, out LuaResult result, HookMethodType.Before);

				if (!result.IsNull())
				{
					__result = result.Object();
					return false;
				}

				return true;
			}

			static void HookLuaPatchPostfix(MethodBase __originalMethod, object[] __args, object __instance)
			{
				_hookLuaPatch(__originalMethod, __args, __instance, out LuaResult result, HookMethodType.After);
			}

			static void HookLuaPatchRetPostfix(MethodBase __originalMethod, object[] __args, ref object __result, object __instance)
			{
				_hookLuaPatch(__originalMethod, __args, __instance, out LuaResult result, HookMethodType.After);

				if (!result.IsNull())
					__result = result.Object();
			}

			private const BindingFlags DefaultBindingFlags = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			private static MethodInfo _miHookLuaPatchPrefix = typeof(LuaHook).GetMethod("HookLuaPatchPrefix", BindingFlags.NonPublic | BindingFlags.Static);
			private static MethodInfo _miHookLuaPatchRetPrefix = typeof(LuaHook).GetMethod("HookLuaPatchRetPrefix", BindingFlags.NonPublic | BindingFlags.Static);
			private static MethodInfo _miHookLuaPatchPostfix = typeof(LuaHook).GetMethod("HookLuaPatchPostfix", BindingFlags.NonPublic | BindingFlags.Static);
			private static MethodInfo _miHookLuaPatchRetPostfix = typeof(LuaHook).GetMethod("HookLuaPatchRetPostfix", BindingFlags.NonPublic | BindingFlags.Static);
			public void HookMethod(string className, string methodName, string[] parameterNames, object hookMethod, HookMethodType hookMethodType = HookMethodType.Before)
			{
                if (hookMethod == null)
                {
					env.PrintError("hookMethod cannot be null");
					return;
				}

				var classType = Type.GetType(className);
				MethodInfo methodInfo = null;

				if (parameterNames.Length > 0)
                {
					Type[] parameterTypes = parameterNames.Select(x => AccessTools.TypeByName(x)).ToArray();
					methodInfo = classType.GetMethod(methodName, DefaultBindingFlags, null, parameterTypes, null);
				}
                else
                {
					methodInfo = classType.GetMethod(methodName, DefaultBindingFlags);
				}

                if (methodInfo == null)
                {
					env.PrintError($"can't find method({className}.{methodName}) with these parameters' types({string.Join(", ", parameterNames)})");
					return;
				}

				var funcAddr = ((long)methodInfo.MethodHandle.GetFunctionPointer());
				var patches = Harmony.GetPatchInfo(methodInfo);

				if (hookMethodType == HookMethodType.Before)
				{
					if (methodInfo.ReturnType == typeof(void))
                    {
                        if (patches == null || patches.Prefixes == null || patches.Prefixes.Find(patch => patch.PatchMethod == _miHookLuaPatchPrefix) == null)
                        {
							env.harmony.Patch(methodInfo, prefix: new HarmonyMethod(_miHookLuaPatchPrefix));
						}
					}
					else
                    {
						if (patches == null || patches.Prefixes == null || patches.Prefixes.Find(patch => patch.PatchMethod == _miHookLuaPatchRetPrefix) == null)
						{
							env.harmony.Patch(methodInfo, prefix: new HarmonyMethod(_miHookLuaPatchRetPrefix));
						}
					}

                    if (_hookPrefixMethods.TryGetValue(funcAddr, out HashSet<object> methodSet))
                    {
						methodSet.Add(hookMethod);
                    }
                    else
                    {
						_hookPrefixMethods.Add(funcAddr, new HashSet<object>() { hookMethod });
					}

				}
				else if (hookMethodType == HookMethodType.After)
				{					
					if (methodInfo.ReturnType == typeof(void))
					{
						if (patches == null || patches.Postfixes == null || patches.Postfixes.Find(patch => patch.PatchMethod == _miHookLuaPatchPostfix) == null)
						{
							env.harmony.Patch(methodInfo, postfix: new HarmonyMethod(_miHookLuaPatchPostfix));
						}
					}
					else
					{
						if (patches == null || patches.Postfixes == null || patches.Postfixes.Find(patch => patch.PatchMethod == _miHookLuaPatchRetPostfix) == null)
						{
							env.harmony.Patch(methodInfo, postfix: new HarmonyMethod(_miHookLuaPatchRetPostfix));
						}
					}

					if (_hookPostfixMethods.TryGetValue(funcAddr, out HashSet<object> methodSet))
					{
						methodSet.Add(hookMethod);
					}
					else
					{
						_hookPostfixMethods.Add(funcAddr, new HashSet<object>() { hookMethod });
					}

				}

			}

			public void HookMethod(string className, string methodName, object hookMethod, HookMethodType hookMethodType = HookMethodType.Before)
			{
				HookMethod(className, methodName, new string[] {}, hookMethod, hookMethodType);
			}


			public void EnqueueFunction(object function, params object[] args)
			{
				queuedFunctionCalls.Enqueue(new Tuple<float, object, object[]>(0, function, args));
			}

			public void EnqueueTimedFunction(float time, object function, params object[] args)
			{
				queuedFunctionCalls.Enqueue(new Tuple<float, object, object[]>(time, function, args));
			}

			public void Add(string name, string hookName, object function)
			{
				if (name == null && hookName == null && function == null) return;

				name = name.ToLower();

				if (!hookFunctions.ContainsKey(name))
					hookFunctions.Add(name, new Dictionary<string, HookFunction>());

				hookFunctions[name][hookName] = new HookFunction(name, hookName, function);
			}

			public void Remove(string name, string hookName)
			{
				if (name == null && hookName == null) return;

				name = name.ToLower();

				if (!hookFunctions.ContainsKey(name))
					return;

				if(hookFunctions[name].ContainsKey(hookName))
					hookFunctions[name].Remove(hookName);
			}

			public void Update()
			{
				try
				{
					if (queuedFunctionCalls.TryPeek(out Tuple<float, object, object[]> result))
					{
						if (Timing.TotalTime >= result.Item1)
						{
							env.CallFunction(result.Item2, result.Item3);

							queuedFunctionCalls.Dequeue();
						}
					}
				}
				catch (Exception ex)
				{
					env.HandleLuaException(ex, $"queuedFunctionCalls was {queuedFunctionCalls}");
				}
			}

			public object Call(string name, params object[] args)
			{
#if CLIENT
				if (GameMain.GameSession?.IsRunning == false && GameMain.IsSingleplayer)
					return null;
#endif
				if (env == null) return null;
				if (name == null) return null;
				if (args == null) { args = new object[] { }; }

				name = name.ToLower();

				if (!hookFunctions.ContainsKey(name))
					return null;

				object lastResult = null;

				foreach (HookFunction hf in hookFunctions[name].Values)
				{
					try
					{
						if (hf.function is Closure)
						{
							var result = env.lua.Call(hf.function, args);
							if (!result.IsNil())
								lastResult = result;
						}
					}
					catch (Exception e)
					{
						StringBuilder argsSb = new StringBuilder();
						foreach(var arg in args)
						{
							argsSb.Append(arg + " ");
						}
						
						env.HandleLuaException(e, $"Error in Hook '{name}'->'{hf.hookName}', with args '{argsSb}'");
					}
				}

				return lastResult;
			}
		}

	}

	public class LuaResult
	{
		object result;
		public LuaResult(object arg)
		{
			result = arg;
		}

		public bool IsNull()
		{
			if (result == null)
				return true;

			if (result is DynValue dynValue)
				return dynValue.IsNil();

			return false;
		}

		public bool Bool()
		{
			if (result is DynValue dynValue)
			{
				return dynValue.CastToBool();
			}

			return false;
		}

		public float Float()
		{
			if (result is DynValue dynValue)
			{
				var num = dynValue.CastToNumber();
				if (num == null) { return 0f; }
				return (float)num.Value;
			}

			return 0f;
		}

		public double Double()
		{
			if (result is DynValue dynValue)
			{
				var num = dynValue.CastToNumber();
				if (num == null) { return 0f; }
				return num.Value;
			}

			return 0f;
		}

		public string String()
		{
			if (result is DynValue dynValue)
			{
				var str = dynValue.CastToString();
				if (str == null) { return ""; }
				return str;
			}

			return "";
		}

		public object Object()
		{
			if(result is DynValue dynValue)
			{
				return dynValue.ToObject();
			}

			return null;
		}
	}
}