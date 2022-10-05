﻿using Barotrauma.Networking;
using Microsoft.Xna.Framework;
using System;

namespace Barotrauma.Items.Components
{
    partial class Wire : ItemComponent, IDrawableComponent, IServerSerializable
    {
        private readonly struct ServerEventData : IEventData
        {
            public readonly int EventIndex;
            
            public ServerEventData(int eventIndex)
            {
                EventIndex = eventIndex;
            }
        }
        
        public void CreateNetworkEvent()
        {
            if (GameMain.Server == null) return;
            //split into multiple events because one might not be enough to fit all the nodes
            int eventCount = Math.Max((int)Math.Ceiling(nodes.Count / (float)MaxNodesPerNetworkEvent), 1);
            for (int i = 0; i < eventCount; i++)
            {
                item.CreateServerEvent(this, new ServerEventData(i));
            }
        }

        public override bool ValidateEventData(NetEntityEvent.IData data)
            => TryExtractEventData<ServerEventData>(data, out _);

        public void ServerEventWrite(IWriteMessage msg, Client c, NetEntityEvent.IData extraData = null)
        {
            var eventData = ExtractEventData<ServerEventData>(extraData);
            int eventIndex = eventData.EventIndex;
            int nodeStartIndex = eventIndex * MaxNodesPerNetworkEvent;
            int nodeCount = MathHelper.Clamp(nodes.Count - nodeStartIndex, 0, MaxNodesPerNetworkEvent);

            msg.WriteRangedInteger(eventIndex, 0, (int)Math.Ceiling(MaxNodeCount / (float)MaxNodesPerNetworkEvent));
            msg.WriteRangedInteger(nodeCount, 0, MaxNodesPerNetworkEvent);
            for (int i = nodeStartIndex; i < nodeStartIndex + nodeCount; i++)
            {
                msg.WriteSingle(nodes[i].X);
                msg.WriteSingle(nodes[i].Y);
            }
        }

        public void ServerEventRead(IReadMessage msg, Client c)
        {
            int nodeCount = msg.ReadByte();
            Vector2 lastNodePos = Vector2.Zero;
            if (nodeCount > 0)
            {
                lastNodePos = new Vector2(msg.ReadSingle(), msg.ReadSingle());
            }

            if (!item.CanClientAccess(c)) { return; }

            if (nodes.Count > nodeCount)
            {
                nodes.RemoveRange(nodeCount, nodes.Count - nodeCount);
            }
            if (nodeCount > 0)
            {
                if (nodeCount > nodes.Count)
                {
                    nodes.Add(lastNodePos);
                }
                else
                {
                    nodes[nodes.Count - 1] = lastNodePos;
                }
            }
            CreateNetworkEvent();
        }
    }
}
