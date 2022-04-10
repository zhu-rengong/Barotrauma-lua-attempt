using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Barotrauma.Networking;
using MoonSharp.Interpreter;
using Microsoft.Xna.Framework;
using MoonSharp.Interpreter.Interop;
using System.IO.Compression;
using HarmonyLib;

namespace Barotrauma
{
	partial class LuaSetup
	{
		public const string LUA_PATH = "Lua/LuaSetup.lua";

		public Script lua;

		public LuaHook hook;
		public LuaGame game;
		public LuaNetworking networking;
		public Harmony harmony;

		public LuaScriptLoader luaScriptLoader;

		public static ContentPackage GetLuaPackage()
		{
			foreach (ContentPackage package in ContentPackageManager.LocalPackages)
			{
				if (package.NameMatches(new Identifier("LuaForBarotraumaUnstable")))
				{
					return package;
				}
			}

			foreach (ContentPackage package in ContentPackageManager.AllPackages)
			{
				if (package.NameMatches(new Identifier("LuaForBarotraumaUnstable")))
				{
					return package;
				}
			}

			return null;
		}

		public void HandleLuaException(Exception ex, string extra = "")
		{
			if (!string.IsNullOrWhiteSpace(extra))
				PrintError(extra);

			if (ex is InterpreterException)
			{
				if (((InterpreterException)ex).DecoratedMessage == null)
					PrintError(((InterpreterException)ex).Message);
				else
					PrintError(((InterpreterException)ex).DecoratedMessage);
			}
			else
			{
				PrintError(ex.ToString());
			}
		}

		public void PrintError(object message)
		{
			if (message == null) { message = "nil"; }
			string str = message.ToString();

			for (int i = 0; i < str.Length; i += 1024)
			{
				string subStr = str.Substring(i, Math.Min(1024, str.Length - i));

				string errorMsg = subStr;
				if (i == 0)
#if SERVER
					errorMsg = "[SV LUA ERROR] " + errorMsg;
#else
					errorMsg = "[CL LUA ERROR] " + errorMsg;
#endif

				DebugConsole.ThrowError(errorMsg);

#if SERVER
				if (GameMain.Server != null)
				{
					foreach (var c in GameMain.Server.ConnectedClients)
					{
						GameMain.Server.SendDirectChatMessage(ChatMessage.Create("", errorMsg, ChatMessageType.Console, null, textColor: Color.Red), c);
					}

					GameServer.Log(errorMsg, ServerLog.MessageType.Error);
				}
#endif
			}
		}

		public void PrintMessage(object message)
		{
			if (message == null) { message = "nil"; }
			string str = message.ToString();

			for (int i = 0; i < str.Length; i += 1024)
			{
				string subStr = str.Substring(i, Math.Min(1024, str.Length - i));


#if SERVER
				if (GameMain.Server != null)
				{
					foreach (var c in GameMain.Server.ConnectedClients)
					{
						GameMain.Server.SendDirectChatMessage(ChatMessage.Create("", subStr, ChatMessageType.Console, null, textColor: Color.MediumPurple), c);
					}

					GameServer.Log("[LUA] " + subStr, ServerLog.MessageType.ServerMessage);
				}
#endif
			}

#if SERVER
			DebugConsole.NewMessage(message.ToString(), Color.MediumPurple);
#else
			DebugConsole.NewMessage(message.ToString(), Color.Purple);
#endif
		}

		public DynValue DoString(string code, Table globalContext = null, string codeStringFriendly = null)
		{
			try
			{
				return lua.DoString(code, globalContext, codeStringFriendly);
			}
			catch (Exception e)
			{
				HandleLuaException(e);
			}

			return null;
		}

		public DynValue DoFile(string file, Table globalContext = null, string codeStringFriendly = null)
		{
			if (!LuaFile.IsPathAllowedLuaException(file, false)) return null;
			if (!LuaFile.Exists(file))
			{
				HandleLuaException(new Exception($"dofile: File {file} not found."));
				return null;
			}

			try
			{
				return lua.DoFile(file, globalContext, codeStringFriendly);

			}
			catch (Exception e)
			{
				HandleLuaException(e);
			}

			return null;
		}


		public DynValue LoadString(string file, Table globalContext = null, string codeStringFriendly = null)
		{
			try
			{
				return lua.LoadString(file, globalContext, codeStringFriendly);

			}
			catch (Exception e)
			{
				HandleLuaException(e);
			}

			return null;
		}

		public DynValue LoadFile(string file, Table globalContext = null, string codeStringFriendly = null)
		{
			if (!LuaFile.IsPathAllowedLuaException(file, false)) return null;
			if (!LuaFile.Exists(file))
			{
				HandleLuaException(new Exception($"loadfile: File {file} not found."));
				return null;
			}

			try
			{
				return lua.LoadFile(file, globalContext, codeStringFriendly);

			}
			catch (Exception e)
			{
				HandleLuaException(e);
			}

			return null;
		}

		public DynValue Require(string modname, Table globalContext)
		{
			try
			{
				return lua.Call(lua.RequireModule(modname, globalContext));

			}
			catch (Exception e)
			{
				HandleLuaException(e);
			}

			return null;
		}

		public object CallFunction(object function, params object[] arguments)
		{
			try
			{
				return lua.Call(function, arguments);
			}
			catch (Exception e)
			{
				HandleLuaException(e);
			}

			return null;
		}

		public void SetModulePaths(string[] str)
		{
			luaScriptLoader.ModulePaths = str;
		}

		public void Update()
		{
			hook?.Update();
		}

		public void Stop()
		{
			harmony?.UnpatchAll();

			game?.Stop();
			hook?.Call("stop", new object[] { });

			hook = new LuaHook();
			game = new LuaGame();
			networking = new LuaNetworking();
			luaScriptLoader = null;
		}

		public void Initialize()
		{
			Stop();

			PrintMessage("Lua! Version " + AssemblyInfo.GitRevision);

			luaScriptLoader = new LuaScriptLoader();
			luaScriptLoader.ModulePaths = new string[] { };

			LuaCustomConverters.RegisterAll();

			lua = new Script(CoreModules.Preset_SoftSandbox | CoreModules.Debug);
			lua.Options.DebugPrint = PrintMessage;
			lua.Options.ScriptLoader = luaScriptLoader;

			harmony = new Harmony("com.LuaForBarotrauma");
			harmony.UnpatchAll();

			hook = new LuaHook();
			game = new LuaGame();
			networking = new LuaNetworking();

			UserData.RegisterType<LuaHook>();
			UserData.RegisterType<LuaGame>();
			UserData.RegisterType<LuaTimer>();
			UserData.RegisterType<LuaFile>();
			UserData.RegisterType<LuaNetworking>();
			UserData.RegisterType<LuaUserData>();
			UserData.RegisterType<IUserDataDescriptor>();

			lua.Globals["printerror"] = (Action<object>)PrintError;
			
			lua.Globals["setmodulepaths"] = (Action<string[]>)SetModulePaths;

			lua.Globals["dofile"] = (Func<string, Table, string, DynValue>)DoFile;
			lua.Globals["loadfile"] = (Func<string, Table, string, DynValue>)LoadFile;
			lua.Globals["require"] = (Func<string, Table, DynValue>)Require;

			lua.Globals["dostring"] = (Func<string, Table, string, DynValue>)DoString;
			lua.Globals["load"] = (Func<string, Table, string, DynValue>)LoadString;

			lua.Globals["LuaUserData"] = UserData.CreateStatic<LuaUserData>();
			lua.Globals["Game"] = game;
			lua.Globals["Hook"] = hook;
			lua.Globals["Timer"] = new LuaTimer();
			lua.Globals["File"] = UserData.CreateStatic<LuaFile>();
			lua.Globals["Networking"] = networking;

			bool isServer = true;

#if SERVER
			isServer = true;
#else
			isServer = false;
#endif

			lua.Globals["SERVER"] = isServer;
			lua.Globals["CLIENT"] = !isServer;

			// LuaDocs.GenerateDocsAll();

			ContentPackage luaPackage = GetLuaPackage();

			if (File.Exists(LUA_PATH))
			{
				try
				{
					lua.Call(lua.LoadFile(LUA_PATH), Path.GetDirectoryName(Path.GetFullPath(LUA_PATH)));
				}
				catch (Exception e)
				{
					HandleLuaException(e);
				}
			}
			else if (luaPackage != null)
			{
				string path = Path.GetDirectoryName(luaPackage.Path);

				try
				{
					string luaPath = Path.Combine(path, "Binary/Lua/LuaSetup.lua");
					lua.Call(lua.LoadFile(luaPath), Path.GetDirectoryName(luaPath));
				}
				catch (Exception e)
				{
					HandleLuaException(e);
				}
			}
			else
			{
				PrintError("Lua loader not found! Lua/LuaSetup.lua, no Lua scripts will be executed or work.");
			}
		}

		public LuaSetup()
		{
			hook = new LuaHook();
			game = new LuaGame();
			networking = new LuaNetworking();
		}

	}



}

