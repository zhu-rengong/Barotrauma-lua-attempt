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
	public partial class LuaCsTimer
	{
		public static long LastUpdateTime = 0;

		public static double Time
		{
			get
			{
				return GetTime();
			}
		}

		public void Wait(LuaCsAction action, int millisecondDelay)
		{
			GameMain.LuaCs.Hook.EnqueueTimed((float)Timing.TotalTime + (millisecondDelay / 1000f), action);
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

	partial class LuaCsFile
	{
		public static bool CanReadFromPath(string path)
		{
			string getFullPath(string p) => System.IO.Path.GetFullPath(p).CleanUpPath();

			path = getFullPath(path);

			bool pathStartsWith(string prefix) => path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);

			string localModsDir = getFullPath(ContentPackage.LocalModsDir);
			string workshopModsDir = getFullPath(ContentPackage.WorkshopModsDir);
#if CLIENT
            string tempDownloadDir = getFullPath(ModReceiver.DownloadFolder);
#endif


			if (pathStartsWith(localModsDir))
				return true;

			if (pathStartsWith(workshopModsDir))
				return true;

#if CLIENT
			if (pathStartsWith(tempDownloadDir))
				return true;
#endif

			if (pathStartsWith(getFullPath(".")))
				return true;

			return false;
		}

		public static bool CanWriteToPath(string path)
		{
			string getFullPath(string p) => System.IO.Path.GetFullPath(p).CleanUpPath();

			path = getFullPath(path);

			bool pathStartsWith(string prefix) => path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);


			if (pathStartsWith(getFullPath(ContentPackage.LocalModsDir + "LuaForBarotraumaUnstable")))
				return false;

			if (pathStartsWith(getFullPath(ContentPackage.WorkshopModsDir + "LuaForBarotraumaUnstable")))
				return false;
#if CLIENT
			if (pathStartsWith(getFullPath(ModReceiver.DownloadFolder + "LuaForBarotraumaUnstable")))
				return false;
#endif

			if (pathStartsWith(getFullPath(ContentPackage.LocalModsDir)))
				return true;

			if (pathStartsWith(getFullPath(ContentPackage.WorkshopModsDir)))
				return true;
#if CLIENT
			if (pathStartsWith(getFullPath(ModReceiver.DownloadFolder)))
				return true;
#endif

			return false;
		}

		public static bool IsPathAllowedException(string path, bool write = true, LuaCsSetup.ExceptionType exceptionType = LuaCsSetup.ExceptionType.Both)
		{
			if (write)
			{
				if (CanWriteToPath(path))
					return true;
				else
					GameMain.LuaCs.HandleException(new Exception("File access to \"" + path + "\" not allowed."));
			}
			else
			{
				if (CanReadFromPath(path))
					return true;
				else
					GameMain.LuaCs.HandleException(new Exception("File access to \"" + path + "\" not allowed."));
			}

			return false;
		}
		public static bool IsPathAllowedLuaException(string path, bool write = true) =>
			IsPathAllowedException(path, write, LuaCsSetup.ExceptionType.Lua);
		public static bool IsPathAllowedCsException(string path, bool write = true) =>
			IsPathAllowedException(path, write, LuaCsSetup.ExceptionType.CSharp);

		public static string Read(string path)
		{
			if (!IsPathAllowedException(path, false))
				return "";

			return File.ReadAllText(path);
		}

		public static void Write(string path, string text)
		{
			if (!IsPathAllowedException(path))
				return;

			File.WriteAllText(path, text);
		}

		public static bool Exists(string path)
		{
			if (!IsPathAllowedException(path, false))
				return false;

			return File.Exists(path);
		}

		public static bool CreateDirectory(string path)
		{
			if (!IsPathAllowedException(path))
				return false;

			Directory.CreateDirectory(path);

			return true;
		}

		public static bool DirectoryExists(string path)
		{
			if (!IsPathAllowedException(path, false))
				return false;

			return Directory.Exists(path);
		}

		public static string[] GetFiles(string path)
		{
			if (!IsPathAllowedException(path, false))
				return null;

			return Directory.GetFiles(path);
		}

		public static string[] GetDirectories(string path)
		{
			if (!IsPathAllowedException(path, false))
				return new string[] { };

			return Directory.GetDirectories(path);
		}

		public static string[] DirSearch(string sDir)
		{
			if (!IsPathAllowedException(sDir, false))
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

	partial class LuaCsNetworking
	{
		public bool restrictMessageSize = true;
		public Dictionary<string, LuaCsAction> LuaCsNetReceives = new Dictionary<string, LuaCsAction>();

#if SERVER
		[MoonSharpHidden]
		public void NetMessageReceived(IReadMessage netMessage, ClientPacketHeader header, Client client = null)
		{
			if (header == ClientPacketHeader.LUA_NET_MESSAGE)
			{
				string netMessageName = netMessage.ReadString();
				if (LuaCsNetReceives.ContainsKey(netMessageName)) LuaCsNetReceives[netMessageName](netMessage, client);
			}
			else
			{
				GameMain.LuaCs.Hook.Call("netMessageReceived", netMessage, header, client);
			}
		}

#else
		[MoonSharpHidden]
		public void NetMessageReceived(IReadMessage netMessage, ServerPacketHeader header, Client client = null)
		{
			if (header == ServerPacketHeader.LUA_NET_MESSAGE)
			{
				string netMessageName = netMessage.ReadString();
				if (LuaCsNetReceives.ContainsKey(netMessageName)) LuaCsNetReceives[netMessageName](netMessage, client);
			}
			else
			{
				GameMain.LuaCs.Hook.Call("netMessageReceived", netMessage, header, client);
			}
		}
#endif
		public void Receive(string netMessageName, LuaCsAction callback)
		{
			LuaCsNetReceives[netMessageName] = callback;
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

		public void RequestPostHTTP(string url, LuaCsAction callback, string data, string contentType = "application/json")
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
							GameMain.LuaCs.Hook.Enqueue(callback, streamReader.ReadToEnd());
					}
					catch (Exception e)
					{
						GameMain.LuaCs.Hook.Enqueue(callback, e.ToString());
					}
				}), null);

			}
			catch (Exception e)
			{
				GameMain.LuaCs.Hook.Enqueue(callback, e.ToString());
			}
		}

		public void RequestGetHTTP(string url, LuaCsAction callback)
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
							GameMain.LuaCs.Hook.Enqueue(callback, streamReader.ReadToEnd());
					}
					catch (Exception e)
					{
						GameMain.LuaCs.Hook.Enqueue(callback, e.ToString());
					}
				}), null);
			}
			catch (Exception e)
			{
				GameMain.LuaCs.Hook.Enqueue(callback, e.ToString());
			}
		}

		public void CreateEntityEvent(INetSerializable entity, NetEntityEvent.IData extraData)
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

}