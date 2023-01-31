using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Barotrauma.Networking;

namespace Barotrauma
{
    partial class LuaCsNetworking
    {
        private enum LuaCsClientToServer
        {
            NetMessageId,
            NetMessageString,
            RequestSingleId,
            RequestAllIds,
        }

        private enum LuaCsServerToClient
        {
            NetMessageId,
            NetMessageString,
            ReceiveIds
        }

        public bool RestrictMessageSize = true;

        private Dictionary<string, LuaCsAction> netReceives = new Dictionary<string, LuaCsAction>();
        private Dictionary<ushort, string> idToString = new Dictionary<ushort, string>();
        private Dictionary<string, ushort> stringToId = new Dictionary<string, ushort>();

        public void Initialize()
        {
#if CLIENT
            SendSyncMessage();
#endif
        }

        public IWriteMessage Start()
        {
            return new WriteOnlyMessage();
        }

        public string IdToString(ushort id)
        {
            if (idToString.ContainsKey(id)) { return idToString[id]; }

            return null;
        }

        public ushort StringToId(string name)
        {
            if (stringToId.ContainsKey(name)) { return stringToId[name]; }

            return 0;
        }

        private void HandleNetMessage(IReadMessage netMessage, string name, Client client = null)
        {
            if (netReceives.ContainsKey(name))
            {
                try
                {
                    netReceives[name](netMessage, client);
                }
                catch (Exception e)
                {
                    LuaCsLogger.LogError($"Exception thrown inside NetMessageReceive({name})", LuaCsMessageOrigin.CSharpMod);
                    LuaCsLogger.HandleException(e, LuaCsMessageOrigin.CSharpMod);
                }
            }
            else
            {
                if (GameSettings.CurrentConfig.VerboseLogging)
                {
#if SERVER
                    LuaCsLogger.LogError($"Received NetMessage for unknown name {name} from {GameServer.ClientLogName(client)}.");
#else
                    LuaCsLogger.LogError($"Received NetMessage for unknown name {name} from server.");
#endif
                }
            }
        }

        private void HandleNetMessageString(IReadMessage netMessage, Client client = null)
        {
            string name = netMessage.ReadString();

            HandleNetMessage(netMessage, name, client);
        }

        public void HttpRequest(string url, LuaCsAction callback, string data = null, string method = "POST", string contentType = "application/json")
        {
            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                httpWebRequest.ContentType = contentType;
                httpWebRequest.Method = method;

                if (data != null)
                {
                    using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                        streamWriter.Write(data);
                }

                httpWebRequest.BeginGetResponse(new AsyncCallback((IAsyncResult result) =>
                {
                    try
                    {
                        var httpResponse = httpWebRequest.EndGetResponse(result);
                        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                        {
                            string responseResult = streamReader.ReadToEnd();
                            GameMain.LuaCs.Timer.Wait((object[] par) => { callback(responseResult); }, 0);
                        }
                    }
                    catch (Exception e)
                    {
                        GameMain.LuaCs.Timer.Wait((object[] par) => { callback(e.Message); }, 0);
                    }
                }), null);

            }
            catch (Exception e)
            {
                GameMain.LuaCs.Timer.Wait((object[] par) => { callback(e.Message); }, 0);
            }
        }

        public void HttpPost(string url, LuaCsAction callback, string data, string contentType = "application/json")
        {
            HttpRequest(url, callback, data, "POST", contentType);
        }


        public void HttpGet(string url, LuaCsAction callback)
        {
            HttpRequest(url, callback, null, "GET");
        }

        public void CreateEntityEvent(INetSerializable entity, NetEntityEvent.IData extraData)
        {
            GameMain.NetworkMember.CreateEntityEvent(entity, extraData);
        }

        public ushort LastClientListUpdateID
        {
            get { return GameMain.NetworkMember.LastClientListUpdateID; }
            set { GameMain.NetworkMember.LastClientListUpdateID = value; }
        }
    }
}
