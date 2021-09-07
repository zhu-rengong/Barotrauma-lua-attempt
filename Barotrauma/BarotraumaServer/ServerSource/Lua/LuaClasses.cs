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

namespace Barotrauma
{
	partial class LuaSetup
	{
		private static Vector2 CreateVector2(float x, float y)
		{
			return new Vector2(x, y);
		}

		private static Vector3 CreateVector3(float x, float y, float z)
		{
			return new Vector3(x, y, z);
		}

		private static Vector4 CreateVector4(float x, float y, float z, float w)
		{
			return new Vector4(x, y, z, w);
		}

		private class LuaPlayer
		{

			public static List<Character> GetAllCharacters()
			{
				return Character.CharacterList;
			}

			public static List<Client> GetAllClients()
			{

				return GameMain.Server.ConnectedClients;
			}

			public static CharacterInfo CreateCharacterInfo(string speciesName, string name = "", JobPrefab jobPrefab = null, string ragdollFileName = null, int variant = 0, Rand.RandSync randSync = Rand.RandSync.Unsynced)
			{
				return new CharacterInfo(speciesName, name, name, jobPrefab, ragdollFileName, variant, randSync);
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
				client.SpectatePos = pos;
			}

			public static void SetRadioRange(Character character, float range)
			{
				if(character.Inventory == null) { return; }

				foreach(Item item in character.Inventory.AllItems)
				{
					if(item == null) { continue; }

					if(item.Name == "Headset")
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

		public class LuaGame
		{
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

			public bool RoundStarted
			{
				get
				{
					return GameMain.Server.GameStarted;
				}
			}


			public bool IsDedicated
			{
				get
				{
					return GameMain.Server.ServerPeer is LidgrenServerPeer;
				}
			}
			
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

			public static void Log(string message, ServerLog.MessageType type)
			{
				GameServer.Log(message, type);
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
				if (GameMain.Server.RespawnManager == null)
					return null;
				return GameMain.Server.RespawnManager.RespawnShuttle;
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

			public static void DispatchRespawnSub()
			{
				GameMain.Server.RespawnManager.DispatchShuttle();
			}

			public static void SetRespawnSubTeam(int team)
			{
				GameMain.Server.RespawnManager.RespawnShuttle.TeamID = (CharacterTeamType)team;
			}

			public static void ExecuteCommand(string command)
			{
				DebugConsole.ExecuteCommand(command);
			}


			public static void StartGame()
			{
				GameMain.Server.StartGame();
			}

			public static Signal CreateSignal(string value, int stepsTaken = 1, Character sender = null, Item source = null, float power = 0, float strength = 1)
			{
				return new Signal(value, stepsTaken, sender, source, power, strength);
			}

			public static ContentPackage[] GetEnabledContentPackages()
			{
				return GameMain.Config.AllEnabledPackages.ToArray();
			}
			 
		}


		private class LuaTimer
		{
			public LuaSetup env;

			public LuaTimer(LuaSetup e)
			{
				env = e;
			}

			public static double GetTime()
			{
				return Timing.TotalTime;
			}


		}

		private class LuaRandom
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

		private class LuaFile
		{
			// TODO: SANDBOXING

			public static string Read(string path)
			{
				return File.ReadAllText(path);
			}

			public static void Write(string path, string text)
			{
				File.WriteAllText(path, text);
			}

			public static bool Exists(string path)
			{
				return File.Exists(path);
			}

			public static bool DirectoryExists(string path)
			{
				return Directory.Exists(path);
			}

			public static string[] GetDirectories(string path)
			{
				return Directory.GetDirectories(path);
			}

			public static string[] DirSearch(string sDir)
			{
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

		private class LuaNetworking
		{
			public LuaSetup env;

			public LuaNetworking(LuaSetup e)
			{
				env = e;
			}

			public string RequestPostHTTP(string url, string data, string contentType = "application/json")
			{
				try
				{
					var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
					httpWebRequest.ContentType = contentType;
					httpWebRequest.Method = "POST";

					using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
						streamWriter.Write(data);

					var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
					using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
						return streamReader.ReadToEnd();
				}catch(Exception e)
				{
					return e.ToString();
				}
			}

			public string RequestGetHTTP(string url)
			{
				try
				{
					var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);

					var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
					using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
						return streamReader.ReadToEnd();
				}catch(Exception e)
				{
					return e.ToString();
				}
			}
		}

		public class LuaHook
		{
			public LuaSetup env;

			public LuaHook(LuaSetup e)
			{
				env = e;
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

			public Dictionary<string, Dictionary<string, HookFunction>> hookFunctions = new Dictionary<string, Dictionary<string, HookFunction>>();

			public void Add(string name, string hookName, object function)
			{
				if (name == null && hookName == null && function == null) return;

				if (!hookFunctions.ContainsKey(name))
					hookFunctions.Add(name, new Dictionary<string, HookFunction>());


				hookFunctions[name][hookName] = new HookFunction(name, hookName, function);
			}

			public void Remove(string name, string hookName)
			{
				if (name == null && hookName == null) return;

				if (!hookFunctions.ContainsKey(name))
					return;

				if(hookFunctions[name].ContainsKey(hookName))
					hookFunctions[name].Remove(hookName);
			}

			public object Call(string name, object[] args)
			{
				if (name == null) return null;

				if (!hookFunctions.ContainsKey(name))
					return null;

				foreach (HookFunction hf in hookFunctions[name].Values)
				{
					if (hf.name == name)
					{
						try
						{
							var result = env.lua.Call(hf.function, args);
							if (result.IsNil() == false)
							{
								return result;
							}
						}
						catch (Exception e)
						{
							env.HandleLuaException(e);
						}
					}
				}

				return null;
			}
		}
	}
}