using System;
using System.Collections.Generic;
using System.Text;
using MoonSharp.Interpreter;
using Microsoft.Xna.Framework;
using Barotrauma.Networking;
using System.Threading.Tasks;

namespace Barotrauma
{
	partial class LuaSetup
	{
		private class LuaPlayer
		{

			public static List<DynValue> GetAllCharacters()
			{
				List<DynValue> values = new List<DynValue>();

				foreach (Character ch in Character.CharacterList)
				{
					values.Add(UserData.Create(ch));
				}

				return values;
			}

			public static List<DynValue> GetAllClients()
			{
				List<DynValue> values = new List<DynValue>();

				foreach (Client ch in GameMain.Server.ConnectedClients)
				{
					values.Add(UserData.Create(ch));
				}

				return values;
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

		}

		public class LuaGame
		{
			LuaSetup env;

			public bool allowWifiChat = false;
			public bool overrideTraitors = false;
			public bool overrideRespawnSub = false;


			public LuaGame(LuaSetup e)
			{
				env = e;
			}

			public static void SendMessage(string msg, ChatMessageType messageType = ChatMessageType.Server, Client sender = null, Character character = null)
			{
				GameMain.Server.SendChatMessage(msg, messageType, sender, character);
			}

			public static void SendTraitorMessage(Client client, string msg, string missionid, TraitorMessageType type)
			{
				GameMain.Server.SendTraitorMessage(client, msg, missionid, type);
			}

			public static void SendDirectChatMessage(string sendername, string text, Character sender, ChatMessageType messageType = ChatMessageType.Private, Client client = null)
			{

				ChatMessage cm = ChatMessage.Create(sendername, text, messageType, sender, client);

				GameMain.Server.SendDirectChatMessage(cm, client);

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

			public static Submarine GetRespawnSub()
			{
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
		}


		private class LuaTimer
		{
			public LuaSetup env;

			public LuaTimer(LuaSetup e)
			{
				env = e;
			}

			public void Simple(int time, DynValue function)
			{

				Task.Delay(time).ContinueWith(o => { env.RunFunction(function); });
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

		// hooks:
		// chatMessage
		// think
		// update
		// clientConnected
		// clientDisconnected
		// roundStart
		// roundEnd

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
				public DynValue function;

				public HookFunction(string n, string hn, DynValue func)
				{
					name = n;
					hookName = hn;
					function = func;
				}
			}

			public List<HookFunction> hookFunctions = new List<HookFunction>();

			public void Add(string name, string hookName, DynValue function)
			{
				foreach (HookFunction hf in hookFunctions)
				{
					if (hf.hookName == hookName && hf.name == name)
					{
						hf.function = function;

						return;
					}
				}

				hookFunctions.Add(new HookFunction(name, hookName, function));
			}

			public DynValue Call(string name, DynValue[] args)
			{
				foreach (HookFunction hf in hookFunctions)
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