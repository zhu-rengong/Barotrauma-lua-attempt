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
using System.Runtime.CompilerServices;
using System.Linq;
using System.Reflection;

[assembly: InternalsVisibleTo("NetScriptAssembly", AllInternalsVisible = true)]
[assembly: InternalsVisibleTo("NetOneTimeScriptAssembly", AllInternalsVisible = true)]
namespace Barotrauma
{
	class LuaCsSetupConfig
    {
		public bool FirstTimeCsWaring = true;

		public LuaCsSetupConfig() { }
	}

	partial class LuaCsSetup
	{
		public const string LUASETUP_FILE = "Lua/LuaSetup.lua";
		public const string VERSION_FILE = "luacsversion.txt";

		private const string configFileName = "LuaCsSetupConfig.xml";

#if SERVER
		public const bool IsServer = true;
		public const bool IsClient = false;
#else
		public const bool IsServer = false;
		public const bool IsClient = true;
#endif

		private Script lua;
		public CsScriptRunner CsScript { get; private set; }
		public LuaGame Game { get; private set; }
		public LuaScriptLoader LuaScriptLoader { get; private set; }

		public LuaCsHook Hook { get; private set; }
		public LuaCsNetworking Networking { get; private set; }
		public LuaCsModStore ModStore { get; private set; }

		public CsScriptLoader CsScriptLoader { get; private set; }
		public CsLua Lua { get; private set; }

		public LuaCsSetupConfig Config { get; private set; }

		public LuaCsSetup()
		{
			Hook = new LuaCsHook();
			ModStore = new LuaCsModStore();

			Game = new LuaGame();
			Networking = new LuaCsNetworking();
		}

		public void UpdateConfig()
        {
			FileStream file;
			if (!File.Exists(configFileName)) file = File.Create(configFileName);
			else file = File.Open(configFileName, FileMode.Truncate, FileAccess.Write);
			LuaCsConfig.Save(file, Config);
			file.Close();
		}


		public static ContentPackage GetPackage(Identifier name)
		{
			foreach (ContentPackage package in ContentPackageManager.LocalPackages)
			{
				if (package.NameMatches(name))
				{
					return package;
				}
			}

			foreach (ContentPackage package in ContentPackageManager.AllPackages)
			{
				if (package.NameMatches(name))
				{
					return package;
				}
			}

			return null;
		}

		public enum ExceptionType
        {
			Lua,
			CSharp,
			Both
        }
		public void HandleException(Exception ex, string extra = "", ExceptionType exceptionType = ExceptionType.Lua)
		{
			if (!string.IsNullOrWhiteSpace(extra))
				if (exceptionType == ExceptionType.Lua) PrintError(extra);
				else if (exceptionType == ExceptionType.CSharp) PrintCsError(extra);
				else PrintBothError(extra);

			if (ex is InterpreterException)
			{
				if (((InterpreterException)ex).DecoratedMessage == null)
					PrintError(((InterpreterException)ex).Message);
				else
					PrintError(((InterpreterException)ex).DecoratedMessage);
			}
			else
			{
				if (exceptionType == ExceptionType.Lua) PrintError(ex);
				else if (exceptionType == ExceptionType.CSharp) PrintCsError(ex);
				else PrintBothError(ex);
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
		public void PrintError(object message) => PrintErrorBase("[SV LUA ERROR] ", message, "nil");
		public static void PrintCsError(object message) => PrintErrorBase("[SV CS ERROR] ", message, "Null");
		public static void PrintBothError(object message) => PrintErrorBase("[SV ERROR] ", message, "Null");
#else
		private void PrintError(object message) => PrintErrorBase("[CL LUA ERROR] ", message, "nil");
		public static void PrintCsError(object message) => PrintErrorBase("[CL CS ERROR] ", message, "Null");
		public static void PrintBothError(object message) => PrintErrorBase("[CL ERROR] ", message, "Null");
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
		public static void PrintLogMessage(object message) => PrintMessageBase("[LuaCs LOG] ", message, "Null");

		private DynValue DoString(string code, Table globalContext = null, string codeStringFriendly = null)
		{
			try
			{
				return lua.DoString(code, globalContext, codeStringFriendly);
			}
			catch (Exception e)
			{
				HandleException(e);
			}

			return null;
		}

		private DynValue DoFile(string file, Table globalContext = null, string codeStringFriendly = null)
		{
			if (!LuaCsFile.IsPathAllowedLuaException(file, false)) return null;
			if (!LuaCsFile.Exists(file))
			{
				HandleException(new Exception($"dofile: File {file} not found."));
				return null;
			}

			try
			{
				return lua.DoFile(file, globalContext, codeStringFriendly);

			}
			catch (Exception e)
			{
				HandleException(e);
			}

			return null;
		}


		private DynValue LoadString(string file, Table globalContext = null, string codeStringFriendly = null)
		{
			try
			{
				return lua.LoadString(file, globalContext, codeStringFriendly);

			}
			catch (Exception e)
			{
				HandleException(e);
			}

			return null;
		}

		private DynValue LoadFile(string file, Table globalContext = null, string codeStringFriendly = null)
		{
			if (!LuaCsFile.IsPathAllowedLuaException(file, false)) return null;
			if (!LuaCsFile.Exists(file))
			{
				HandleException(new Exception($"loadfile: File {file} not found."));
				return null;
			}

			try
			{
				return lua.LoadFile(file, globalContext, codeStringFriendly);

			}
			catch (Exception e)
			{
				HandleException(e);
			}

			return null;
		}

		private DynValue Require(string modname, Table globalContext)
		{
			try
			{
				return lua.Call(lua.RequireModule(modname, globalContext));

			}
			catch (Exception e)
			{
				HandleException(e);
			}

			return null;
		}

		public object CallLuaFunction(object function, params object[] arguments)
		{
			try
			{
				return lua.Call(function, arguments);
			}
			catch (Exception e)
			{
				HandleException(e);
			}

			return null;
		}

		private void SetModulePaths(string[] str)
		{
			LuaScriptLoader.ModulePaths = str;
		}

		public void Update()
		{
			Hook?.Update();
		}

		public void Stop()
		{
			foreach (var type in AppDomain.CurrentDomain.GetAssemblies().Where(a => a.GetName().Name == "NetScriptAssembly").SelectMany(assembly => assembly.GetTypes()))
			{
				UserData.UnregisterType(type);
			}
			foreach (var mod in ACsMod.LoadedMods.ToArray()) mod.Dispose();
			ACsMod.LoadedMods.Clear();
			Hook?.Call("stop");

			Game?.Stop();

			Hook.Clear();
			ModStore.Clear();
			Game = new LuaGame();
			Networking = new LuaCsNetworking();
			LuaScriptLoader = null;
			lua = null;
			Lua = null;
			CsScript = null;
			Config = null;

            if (CsScriptLoader != null)
			{
				CsScriptLoader.Clear();
				CsScriptLoader.Unload();
				CsScriptLoader = null;
				GC.Collect();
				GC.WaitForPendingFinalizers();
			}
		}

		public void Initialize()
		{
			Stop();

			PrintMessage("Lua! Version " + AssemblyInfo.GitRevision);


			if (File.Exists(configFileName))
			{
				using (var file = File.Open(configFileName, FileMode.Open, FileAccess.Read))
					Config = LuaCsConfig.Load<LuaCsSetupConfig>(file);
			}
			else Config = new LuaCsSetupConfig();


			LuaScriptLoader = new LuaScriptLoader();
			LuaScriptLoader.ModulePaths = new string[] { };

			LuaCustomConverters.RegisterAll();

			lua = new Script(CoreModules.Preset_SoftSandbox | CoreModules.Debug);
			lua.Options.DebugPrint = PrintMessage;
			lua.Options.ScriptLoader = LuaScriptLoader;
			Lua = new CsLua(this);
			CsScript = new CsScriptRunner(this);

			Game = new LuaGame();
			Networking = new LuaCsNetworking();
			Hook.Initialize();
			ModStore.Initialize();

			UserData.RegisterType<LuaCsConfig>();
			UserData.RegisterType<LuaCsAction>();
			UserData.RegisterType<LuaCsFile>();
			UserData.RegisterType<LuaCsPatch>();
			UserData.RegisterType<LuaCsConfig>();
			UserData.RegisterType<CsScriptRunner>();
			UserData.RegisterType<LuaGame>();
			UserData.RegisterType<LuaCsTimer>();
			UserData.RegisterType<LuaCsFile>();
			UserData.RegisterType<LuaCsNetworking>();
			UserData.RegisterType<LuaUserData>();
			UserData.RegisterType<IUserDataDescriptor>();

			lua.Globals["printerror"] = (Action<object>)PrintError;

			lua.Globals["setmodulepaths"] = (Action<string[]>)SetModulePaths;

			lua.Globals["dofile"] = (Func<string, Table, string, DynValue>)DoFile;
			lua.Globals["loadfile"] = (Func<string, Table, string, DynValue>)LoadFile;
			lua.Globals["require"] = (Func<string, Table, DynValue>)Require;

			lua.Globals["dostring"] = (Func<string, Table, string, DynValue>)DoString;
			lua.Globals["load"] = (Func<string, Table, string, DynValue>)LoadString;

			lua.Globals["CsScript"] = CsScript;
			lua.Globals["LuaUserData"] = UserData.CreateStatic<LuaUserData>();
			lua.Globals["Game"] = Game;
			lua.Globals["Hook"] = Hook;
			lua.Globals["ModStore"] = ModStore;
			lua.Globals["Timer"] = new LuaCsTimer();
			lua.Globals["File"] = UserData.CreateStatic<LuaCsFile>();
			lua.Globals["Networking"] = Networking;

			lua.Globals["SERVER"] = IsServer;
			lua.Globals["CLIENT"] = IsClient;

			CsScriptLoader = new CsScriptLoader(this);
			CsScriptLoader.SearchFolders();
			if (CsScriptLoader.HasSources)
			{
				if (Config.FirstTimeCsWaring)
				{
					Config.FirstTimeCsWaring = false;
					UpdateConfig();

					LuaCsTimer.Wait((args) => PrintCsError(@"
  ----====    ====----

        WARNING!
  --  --  --  --  --  --
  !Use of Cs Mods detected!

    Cs Mods are questionably
sandboxed, as they have
access to reflection, due to
modding needs.

    USE ON YOUR OWN RISK!

  ----====    ====----
"), 200);
				}

				try
				{
					var modTypes = CsScriptLoader.Compile();
					modTypes.ForEach(t => {
						UserData.RegisterType(t);
						t.GetConstructor(new Type[] { })?.Invoke(null);
					});
				}
				catch (Exception ex)
				{
					HandleException(ex, exceptionType: ExceptionType.CSharp);
				}

				PrintMessage("Cs! Version " + AssemblyInfo.GitRevision);
			}


			ContentPackage luaPackage = GetPackage("LuaForBarotraumaUnstable");

			if (File.Exists(LUASETUP_FILE))
			{
				try
				{
					lua.Call(lua.LoadFile(LUASETUP_FILE), Path.GetDirectoryName(Path.GetFullPath(LUASETUP_FILE)));
				}
				catch (Exception e)
				{
					HandleException(e);
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
					HandleException(e);
				}
			}
			else
			{
				PrintError("LuaSetup.lua not found! Lua/LuaSetup.lua, no Lua scripts will be executed or work.");
			}
		}

	}



}

