using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Barotrauma.Networking;
using MoonSharp.Interpreter;
using Microsoft.Xna.Framework;
using System.Threading.Tasks;

namespace Barotrauma
{
	class LuaSetup
	{

		public Script lua;
		public Hook hook;


		public void DoString(string code)
		{
			try
			{
				lua.DoString(code);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
			}
		}

		private class Player
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

			public static void SetClientCharacter(Client client, Character character)
			{
				GameMain.Server.SetClientCharacter(client, character);
			}

			public static void Kick(Client client, string reason="")
			{
				GameMain.Server.KickClient(client.Connection, reason);
			}

			public static void Ban(Client client, string reason = "", bool range = false, float seconds=-1)
			{
				if(seconds == -1)
				{
					GameMain.Server.BanClient(client, reason, range, null);
				}
				else
				{
					GameMain.Server.BanClient(client, reason, range, TimeSpan.FromSeconds(seconds));
				}

			}

			public static void StartGame()
			{
				GameMain.Server.StartGame();
			}
		}
		
		private class Game 
		{
			public static void SendMessage(string msg, int messageType = 0, Client sender = null, Character character = null)
			{
				GameMain.Server.SendChatMessage(msg, (ChatMessageType)messageType, sender, character);
			}

			public static void SendDirectChatMessage(string sendername, string text, Character sender, int messageType = 0, Client client = null)
			{

				ChatMessage cm = ChatMessage.Create(sendername, text, (ChatMessageType)messageType, sender, client);

				GameMain.Server.SendDirectChatMessage(cm, client);

			}

			public static void Log(string message, int type)
			{
				GameServer.Log(message, (ServerLog.MessageType)type);
			}

			public static void Explode(Vector2 pos, float range=100, float force=30, float damage=30, float structureDamage=30, float itemDamage=30, float empStrength=0, float ballastFloraStrength=0)
			{
				new Explosion(range, force, damage, structureDamage, itemDamage, empStrength, ballastFloraStrength).Explode(pos, null);
			}

			public static string Spawn(string name, Vector2 pos)
			{
				string error;
				DebugConsole.SpawnCharacter(new string[] {name, "cursor"}, pos, out error);
				return error;
			}

			public static string SpawnItem(string name, Vector2 pos, bool inventory = false, Character character=null)
			{
				string error;
				DebugConsole.SpawnItem(new string[] { name, inventory ? "inventory" : "cursor" }, pos, character, out error);
				return error;
			}
		}


		private class LuaTimer
		{
			public Script env;

			public LuaTimer(Script e)
			{
				env = e;
			}

			public void Simple(int time, DynValue function)
			{
				Task.Delay(new TimeSpan(0, 0, 0, 0, time)).ContinueWith(o => { env.Call(function); });
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
		}

		// hooks:
		// chatMessage
		// think
		// update
		// clientConnected
		// clientDisconnected
		// roundStart
		// roundEnd

		public class Hook
		{
			public Script env;

			public Hook(Script e)
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
					if(hf.hookName == hookName && hf.name == name)
					{
						hf.function = function;

						return;
					}
				}

				hookFunctions.Add(new HookFunction(name, hookName, function));
			}

			public void Call(string name, DynValue[] args)
			{
				foreach(HookFunction hf in hookFunctions)
				{
					if (hf.name == name)
					{
						try
						{
							env.Call(hf.function, args);
						}catch(Exception e)
						{
							Console.WriteLine(e.ToString());
						}
					}
				}
			}
		}

		public LuaSetup()
		{
			
			Console.WriteLine("Lua!");

			LuaCustomConverters.RegisterAll();

			LuaScriptLoader luaScriptLoader = new LuaScriptLoader();

			//UserData.RegisterAssembly();
			UserData.RegisterType<Character>();
			UserData.RegisterType<Client>();
			UserData.RegisterType<Player>();
			UserData.RegisterType<Hook>();
			UserData.RegisterType<Game>();
			UserData.RegisterType<LuaRandom>();
			UserData.RegisterType<LuaTimer>();
			UserData.RegisterType<Vector2>();
			UserData.RegisterType<Vector3>();
			UserData.RegisterType<Vector4>();

			lua = new Script(CoreModules.Preset_SoftSandbox);

			lua.Options.ScriptLoader = luaScriptLoader;

			hook = new Hook(lua);

			lua.Globals["Player"] = new Player();
			lua.Globals["Game"] = new Game();
			lua.Globals["Hook"] = hook;
			lua.Globals["Random"] = new LuaRandom();
			lua.Globals["Timer"] = new LuaTimer(lua);

			luaScriptLoader.RunFolder("Lua/autorun", lua);

			
		}

	}



}


