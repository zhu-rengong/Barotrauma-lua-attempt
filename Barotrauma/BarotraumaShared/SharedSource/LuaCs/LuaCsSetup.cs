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
using static Barotrauma.LuaCsSetup;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("NetScriptAssembly", AllInternalsVisible = true)]
namespace Barotrauma
{
	partial class LuaCsSetup
	{
		public const string LUASETUP_FILE = "Lua/LuaSetup.lua";
		public const string VERSION_FILE = "luacsversion.txt";

		public Script lua;

		public LuaCsHook hook;
		public LuaGame game;
		public LuaNetworking networking;
		public Harmony harmony;

		public LuaScriptLoader luaScriptLoader;
		public NetScriptLoader netScriptLoader;

		public static ContentPackage GetPackage()
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

		private static void PrintErrorBase(string prefix, object message, string empty)
        {
			if (message == null) { message = empty; }
			string str = message.ToString();

			for (int i = 0; i < str.Length; i += 1024)
			{
				string subStr = str.Substring(i, Math.Min(1024, str.Length - i));

				string errorMsg = subStr;
				if (i == 0) errorMsg = prefix + errorMsg;

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

#if SERVER
		private void PrintError(object message) => PrintErrorBase("[SV LUA ERROR] ", message, "nil");
		public static void PrintCsError(object message) => PrintErrorBase("[SV CS ERROR] ", message, "Null");
#else
		private void PrintError(object message) => PrintErrorBase("[CL LUA ERROR] ", message, "nil");
		public static void PrintCsError(object message) => PrintErrorBase("[CL CS ERROR] ", message, "Null");
#endif

		private static void PrintMessageBase(string prefix, object message, string empty)
        {
			if (message == null) { message = empty; }
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

					GameServer.Log(prefix + subStr, ServerLog.MessageType.ServerMessage);
				}
#endif
			}

#if SERVER
			DebugConsole.NewMessage(message.ToString(), Color.MediumPurple);
#else
			DebugConsole.NewMessage(message.ToString(), Color.Purple);
#endif
		}
		private void PrintMessage(object message) => PrintMessageBase("[LUA] ", message, "nil");
		public static void PrintCsMessage(object message) => PrintMessageBase("[CS] ", message, "Null");

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
			ANetMod.LoadedMods.ForEach(m => m.Dispose());
			ANetMod.LoadedMods.Clear();
			hook?.Call("stop");

			game?.Stop();
			harmony?.UnpatchAll();

			hook = new LuaCsHook();
			game = new LuaGame();
			networking = new LuaNetworking();
			luaScriptLoader = null;
		}

		private void InitCs()
        {
			netScriptLoader = new NetScriptLoader(this);
			netScriptLoader.SearchFolders();
			if (netScriptLoader == null) throw new Exception("LuaCsSetup was not properly initialized.");
			try
			{
				var modTypes = netScriptLoader.Compile();
				modTypes.ForEach(t => t.GetConstructor(new Type[] { }).Invoke(null));
			}
			catch (Exception ex)
			{
				PrintMessage(ex);
			}
		}

		public void Initialize()
		{
			Stop();

			PrintMessage("LuaCs! Version " + AssemblyInfo.GitRevision);

			luaScriptLoader = new LuaScriptLoader();
			luaScriptLoader.ModulePaths = new string[] { };
			InitCs();

			LuaCustomConverters.RegisterAll();

			lua = new Script(CoreModules.Preset_SoftSandbox | CoreModules.Debug);
			lua.Options.DebugPrint = PrintMessage;
			lua.Options.ScriptLoader = luaScriptLoader;

			harmony = new Harmony("com.LuaForBarotrauma");
			harmony.UnpatchAll();

			hook = new LuaCsHook();
			game = new LuaGame();
			networking = new LuaNetworking();

			UserData.RegisterType<LuaCsHook>();
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

			ContentPackage luaPackage = GetPackage();

			if (File.Exists(LUASETUP_FILE))
			{
				try
				{
					lua.Call(lua.LoadFile(LUASETUP_FILE), Path.GetDirectoryName(Path.GetFullPath(LUASETUP_FILE)));
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
					string luaPath = Path.Combine(path, "Binary/LuaCs/LuaCsSetup.lua");
					lua.Call(lua.LoadFile(luaPath), Path.GetDirectoryName(luaPath));
				}
				catch (Exception e)
				{
					HandleLuaException(e);
				}
			}
			else
			{
				PrintError("LuaCs loader not found! LuaCs/LuaCsSetup.lua, no LuaCs scripts will be executed or work.");
			}
		}

		public LuaCsSetup()
		{
			hook = new LuaCsHook();
			game = new LuaGame();
			networking = new LuaNetworking();
		}

	}



}

