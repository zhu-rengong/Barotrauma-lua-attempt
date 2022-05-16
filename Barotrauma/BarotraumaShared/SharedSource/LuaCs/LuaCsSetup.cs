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
using System.Threading;

[assembly: InternalsVisibleTo(Barotrauma.CsScriptBase.NET_SCRIPT_ASSEMBLY, AllInternalsVisible = true)]
[assembly: InternalsVisibleTo(Barotrauma.CsScriptBase.NET_ONE_TIME_SCRIPT_ASSEMBLY, AllInternalsVisible = true)]
namespace Barotrauma
{
	class LuaCsSetupConfig
    {
		public bool FirstTimeCsWarning = true;

		public LuaCsSetupConfig() { }
	}

	partial class LuaCsSetup
	{
		public const string LuaSetupFile = "Lua/LuaSetup.lua";
		public const string VersionFile = "luacsversion.txt";

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

		/// <summary>
		/// due to there's a race on the process and the unloaded AssemblyLoadContexts,
		/// should recreate runner after the script runs
		/// </summary>
		public void RecreateCsScript()
        {
			GameMain.LuaCs.CsScript = new CsScriptRunner(GameMain.LuaCs.CsScript.setup);
			lua.Globals["CsScript"] = CsScript;
		}

		public LuaGame Game { get; private set; }
		public LuaScriptLoader LuaScriptLoader { get; private set; }

		public LuaCsHook Hook { get; private set; }

		public LuaCsTimer Timer { get; private set; }

		public LuaCsNetworking Networking { get; private set; }
		public LuaCsModStore ModStore { get; private set; }
		private LuaRequire require { get; set; }

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


		public static ContentPackage GetPackage(Identifier name, bool fallbackToAll = true, bool useBackup = false)
		{
			foreach (ContentPackage package in ContentPackageManager.EnabledPackages.All)
			{
				if (package.NameMatches(name))
				{
					return package;
				}
			}

			if (fallbackToAll)
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
			}

			if (useBackup && ContentPackageManager.EnabledPackages.BackupPackages.Regular != null)
            {
				foreach (ContentPackage package in ContentPackageManager.EnabledPackages.BackupPackages.Regular.Value)
				{
					if (package.NameMatches(name))
					{
						return package;
					}
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

			if (ex is NetRuntimeException netRuntimeException)
			{
				if (netRuntimeException.DecoratedMessage == null)
				{
					PrintError(netRuntimeException);
				}
				else
				{
					PrintError(netRuntimeException.DecoratedMessage + ": " + netRuntimeException.ToString());
				}
			}
			else if (ex is InterpreterException interpreterException)
			{
				if (interpreterException.DecoratedMessage == null)
				{
					PrintError(interpreterException);
				}
				else
				{
					PrintError(interpreterException.DecoratedMessage);
				}
			}
			else
			{
				string msg = ex.StackTrace != null
					? ex.ToString()
					: $"{ex}\n{Environment.StackTrace}";

				if (exceptionType == ExceptionType.Lua) { PrintError(msg); }
				else if (exceptionType == ExceptionType.CSharp) { PrintCsError(msg); }
				else { PrintBothError(msg); }
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

		private DynValue DoFile(string file, Table globalContext = null, string codeStringFriendly = null)
		{
			if (!LuaCsFile.CanReadFromPath(file))
			{
				throw new ScriptRuntimeException($"dofile: File access to {file} not allowed.");
			}

			if (!LuaCsFile.Exists(file))
			{
				throw new ScriptRuntimeException($"dofile: File {file} not found.");
			}

			return lua.DoFile(file, globalContext, codeStringFriendly);
		}

		private DynValue LoadFile(string file, Table globalContext = null, string codeStringFriendly = null)
		{
			if (!LuaCsFile.CanReadFromPath(file))
            {
				throw new ScriptRuntimeException($"loadfile: File access to {file} not allowed.");
			}

			if (!LuaCsFile.Exists(file))
			{
				throw new ScriptRuntimeException($"loadfile: File {file} not found.");
			}

			return lua.LoadFile(file, globalContext, codeStringFriendly);
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
			Timer?.Update();
		}

		public void Stop()
		{
			foreach (var type in AppDomain.CurrentDomain.GetAssemblies().Where(a => a.GetName().Name == CsScriptBase.NET_SCRIPT_ASSEMBLY).SelectMany(assembly => assembly.GetTypes()))
			{
				UserData.UnregisterType(type, true);
			}
			foreach (var mod in ACsMod.LoadedMods.ToArray()) mod.Dispose();
			ACsMod.LoadedMods.Clear();


			if (Thread.CurrentThread == GameMain.MainThread) 
			{
				Hook?.Call("stop");
			}

			Game?.Stop();

			Hook.Clear();
			ModStore.Clear();
			Game = new LuaGame();
			Networking = new LuaCsNetworking();
			Timer = new LuaCsTimer();
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
			lua.Options.CheckThreadAccess = false;
			Lua = new CsLua(this);
			CsScript = new CsScriptRunner(this);

			require = new LuaRequire(lua);

			Game = new LuaGame();
			Networking = new LuaCsNetworking();
			Timer = new LuaCsTimer();
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
			lua.Globals["require"] = (Func<string, Table, DynValue>)require.Require;

			lua.Globals["dostring"] = (Func<string, Table, string, DynValue>)lua.DoString;
			lua.Globals["load"] = (Func<string, Table, string, DynValue>)lua.LoadString;

			lua.Globals["CsScript"] = CsScript;
			lua.Globals["LuaUserData"] = UserData.CreateStatic<LuaUserData>();
			lua.Globals["Game"] = Game;
			lua.Globals["Hook"] = Hook;
			lua.Globals["ModStore"] = ModStore;
			lua.Globals["Timer"] = Timer;
			lua.Globals["File"] = UserData.CreateStatic<LuaCsFile>();
			lua.Globals["Networking"] = Networking;

			lua.Globals["SERVER"] = IsServer;
			lua.Globals["CLIENT"] = IsClient;


			if (GetPackage("CsForBarotrauma", false, true) != null)
			{
				PrintMessage("Cs! Version " + AssemblyInfo.GitRevision);

				if (Config.FirstTimeCsWarning)
				{
					Config.FirstTimeCsWarning = false;
					UpdateConfig();

					Timer.Wait((args) => PrintCsError(@"
  ----====    ====----

        WARNING!
  --  --  --  --  --  --
  !Cs Package Enabled!

    Cs Mods are questionably
sandboxed, as they have
access to reflection, due to
modding needs.

    USE ON YOUR OWN RISK!

  ----====    ====----
"), 200);
				}

				CsScriptLoader = new CsScriptLoader(this);
				CsScriptLoader.SearchFolders();
				if (CsScriptLoader.HasSources)
				{
					try
					{
						var modTypes = CsScriptLoader.Compile();
						modTypes.ForEach(t =>
						{
							//Please register `t` in lua-side
							//UserData.RegisterType(t);
							t.GetConstructor(new Type[] { })?.Invoke(null);
						});
					}
					catch (Exception ex)
					{
						HandleException(ex, exceptionType: ExceptionType.CSharp);
					}
				}

			}


			ContentPackage luaPackage = GetPackage("Lua For Barotrauma");

			if (File.Exists(LuaSetupFile))
			{
				try
				{
					lua.Call(lua.LoadFile(LuaSetupFile), Path.GetDirectoryName(Path.GetFullPath(LuaSetupFile)));
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

