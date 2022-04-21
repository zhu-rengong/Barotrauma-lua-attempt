using Barotrauma.Items.Components;
using Barotrauma.Networking;
using MoonSharp.Interpreter;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Xml.Linq;

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

		public static void Wait(LuaCsAction action, int millisecondDelay)
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

		public static FileStream OpenRead(string path)
		{
			if (!IsPathAllowedException(path))
				return null;

			return File.Open(path, FileMode.Open, FileAccess.Read);
		}
		public static FileStream OpenWrite(string path)
		{
			if (!IsPathAllowedException(path))
				return null;

			if (File.Exists(path)) return File.Open(path, FileMode.Truncate, FileAccess.Write);
			else return File.Open(path, FileMode.Create, FileAccess.Write);
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


	class LuaCsConfig
    {
		private enum ValueType
        {
			None,
			Text,
			Integer,
			Decimal,
			Boolean,
			Collection,
			Object,
			Enum
        }

		private static Type[] LoadDocTypes(XElement typesElem)
        {
			var result = new List<Type>();
			foreach (var elem in typesElem.Elements())
            {
				var type = Type.GetType(elem.Value);
				if (type == null) throw new Exception($"Type {elem.Value} not found.");
				result.Add(type);

			}
			return result.ToArray();
        }

		private static IEnumerable<XElement> SaveDocTypes(IEnumerable<Type> types)
		{
			return types.Select(t => new XElement("Type", t.ToString()));
		}

		private static Type GetTypeAttr(Type[] types, XElement elem)
        {
			var idx = elem.GetAttributeInt("Type", -1);
			if (idx < 0 || idx >= types.Length) throw new Exception($"Type index '{idx}' is outside of saved types bounds");
			return types[idx];
		}
		private static ValueType GetValueType(XElement elem)
        {
			Enum.TryParse(typeof(ValueType), elem.Attribute("Value")?.Value, out object result);
			if (result != null) return (ValueType)result;
			else return ValueType.None;
		}
		private static object ParseValue(Type[] types, XElement elem)
        {
			var type = GetValueType(elem);

			if (elem.IsEmpty) return null;
			if (type == ValueType.Enum)
            {
				var tType = GetTypeAttr(types, elem);
				if (tType == null || !tType.IsSubclassOf(typeof(Enum))) return null;
				if (Enum.TryParse(tType, elem.Value, out object result)) return result;
				else return null;
			}
			if (type == ValueType.Collection)
			{
				var tType = GetTypeAttr(types, elem);
				var tInt = tType.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
				var gArg = tInt.GetGenericArguments()[0];
				if (tType == null || !tType.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>))) return null;

				object result = null;

				if (result == null) {
					var ctor = tType.GetConstructors(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault(c =>
					{
						var param = c.GetParameters();
						return param.Count() == 1 && param.Any(p => p.ParameterType.IsGenericType && p.ParameterType.GetGenericTypeDefinition() == typeof(IEnumerable<>));
					});
					if (ctor != null)
					{
						var elements = elem.Elements().Select(x => ParseValue(types, x));
						var castElems = typeof(Enumerable).GetMethod("Cast").MakeGenericMethod(gArg).Invoke(elements, new object[] { elements });
						result = ctor.Invoke(new object[] { castElems });
					}
				}

				if (result == null)
				{
					var ctor = tType.GetConstructors(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault(c => c.GetParameters().Count() == 0);
					var addMethod = tType.GetMethods(BindingFlags.Instance | BindingFlags.Public).FirstOrDefault(m =>
					{
						if (m.Name != "Add") return false;
						var param = m.GetParameters();
						return param.Count() == 1 && param[0].ParameterType == gArg;
					});
					if (ctor != null && addMethod != null)
					{
						var elements = elem.Elements().Select(x => ParseValue(types, x));
						result = ctor.Invoke(null);
						foreach (var el in elements) addMethod.Invoke(result, new object[] { el });
					}
				}

				if (result == null)
				{
					var ctor = tType.GetConstructors(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault();
					var setMethod = tType.GetMethods(BindingFlags.Instance | BindingFlags.Public).FirstOrDefault(m =>
					{
						if (m.Name != "Set") return false;
						var param = m.GetParameters();
						return param.Count() == 2 && param[0].ParameterType == typeof(int) && param[1].ParameterType == gArg;
					});
					if (ctor != null || setMethod != null)
					{
						var elements = elem.Elements().Select(x => ParseValue(types, x));
						result = ctor.Invoke(new object[] { elements.Count() });
						int i = 0;
						foreach (var el in elements)
						{
							setMethod.Invoke(result, new object[] { i, el });
							i++;
						}
					}
				}

				return result;
			}
			else if (type == ValueType.Text) return elem.Value;
			else if (type == ValueType.Integer)
			{
				int.TryParse(elem.Value, out var num);
				return num;
			}
			else if (type == ValueType.Decimal)
            {
				float.TryParse(elem.Value, out var num);
				return num;
			}
			else if (type == ValueType.Boolean)
			{
				bool.TryParse(elem.Value, out var boolean);
				return boolean;
			}
			else if (type == ValueType.Object)
            {
				var tType = GetTypeAttr(types, elem);
				if (tType == null) return null;

				IEnumerable<FieldInfo> fields = tType.GetFields(BindingFlags.Instance | BindingFlags.Public)
					.Concat(tType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic));
				IEnumerable<PropertyInfo> properties = tType.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.GetSetMethod() != null)
					.Concat(tType.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic).Where(p => p.GetSetMethod() != null));

				object result = null;
				var ctor = tType.GetConstructors(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault(c => c.GetParameters().Count() == 0);
				if (ctor == null)
				{
					if (!tType.IsValueType) return null;
					result = Activator.CreateInstance(tType);
				}
				else result = ctor.Invoke(null);

				foreach(var el in elem.Elements())
                {
					var value = ParseValue(types, el);

					var field = fields.FirstOrDefault(f => f.Name == el.Name.LocalName);
					if (field != null) field.SetValue(result, value);
					var property = properties.FirstOrDefault(p => p.Name == el.Name.LocalName);
					if (property != null) property.SetValue(result, value);
                }
				return result;
			}
			else return elem.Value;

        }

		private static void AddTypeAttr(List<Type> types, Type type, XElement elem)
        {
			if (!types.Contains(type)) types.Add(type);
			elem.SetAttributeValue("Type", types.IndexOf(type));
		}

		private static XElement ParseObject(List<Type> types, string name, object value)
        {
			XElement result = new XElement(name);

			if (value != null)
			{
				var tType = value.GetType();

				if (tType.IsEnum)
                {
					result.SetAttributeValue("Value", ValueType.Enum);
					AddTypeAttr(types, tType, result);

					result.Value = Enum.GetName(tType, value) ?? "";
                }
				else if (value is string str)
				{
					result.SetAttributeValue("Value", ValueType.Text);
					result.Value = str;
				}
				else if (value is int integer)
				{
					result.SetAttributeValue("Value", ValueType.Integer);
					result.Value = integer.ToString();
				}
				else if (value is float || value is double)
				{
					result.SetAttributeValue("Value", ValueType.Decimal);
					result.Value = value.ToString();
				}
				else if (value is bool boolean)
				{
					result.SetAttributeValue("Value", ValueType.Boolean);
					result.Value = boolean.ToString();
				}
				else if (tType.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
				{
					result.SetAttributeValue("Value", ValueType.Collection);
					AddTypeAttr(types, tType, result);

					var enumerator = (IEnumerator)tType.GetMethod("GetEnumerator").Invoke(value, null);
					while (enumerator.MoveNext())
					{
						var elVal = ParseObject(types, "Item", enumerator.Current);
						result.Add(elVal);
					}
				}
				else if (tType.IsClass || tType.IsValueType)
                {
					result.SetAttributeValue("Value", ValueType.Object);
					AddTypeAttr(types, tType, result);

					IEnumerable<FieldInfo> fields = tType.GetFields(BindingFlags.Instance | BindingFlags.Public)
						.Concat(tType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic));
					IEnumerable<PropertyInfo> properties = tType.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.GetSetMethod() != null)
						.Concat(tType.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic).Where(p => p.GetSetMethod() != null));

					foreach(var field in fields) result.Add(ParseObject(types, field.Name, field.GetValue(value)));
					foreach (var property in properties) result.Add(ParseObject(types, property.Name, property.GetValue(value)));
				}
				else
				{
					result.SetAttributeValue("Value", ValueType.None);
					result.Value = value.ToString();
				}
			}

			return result;
        }


		public static T Load<T>(FileStream file)
		{
			var doc = XDocument.Load(file);

			var rootElems = doc.Root.Elements().ToArray();
			var types = rootElems[0];
			var elem = rootElems[1];

			var dict = ParseValue(LoadDocTypes(types), elem);
			if (dict.GetType() == typeof(T)) return (T)dict;
			else throw new Exception($"Loaded configuration is not of the type '{typeof(T).Name}'");
		}

		public static void Save(FileStream file, object obj)
		{
			var types = new List<Type>();
			var elem = ParseObject(types, "Root", obj);
			var root = new XElement("Configuration", new XElement("Types", SaveDocTypes(types)), elem);

			var doc = new XDocument(root);
			doc.Save(file);
		}

		public static T Load<T>(string path)
		{
			using (var file = LuaCsFile.OpenRead(path)) return Load<T>(file);
		}

		public static void Save(string path, object obj)
        {
			using (var file = LuaCsFile.OpenWrite(path)) Save(file, obj);
		}
    }
}