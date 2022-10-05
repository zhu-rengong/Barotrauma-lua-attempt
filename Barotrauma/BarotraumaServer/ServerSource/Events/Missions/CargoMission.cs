﻿using Barotrauma.Networking;

namespace Barotrauma
{
    partial class CargoMission : Mission
    {
        public override void ServerWriteInitial(IWriteMessage msg, Client c)
        {
            base.ServerWriteInitial(msg, c);
            msg.WriteUInt16((ushort)items.Count);
            foreach (Item item in items)
            {
                item.WriteSpawnData(msg, 
                    item.ID, 
                    parentInventoryIDs.ContainsKey(item) ? parentInventoryIDs[item] : Entity.NullEntityID,
                    parentItemContainerIndices.ContainsKey(item) ? parentItemContainerIndices[item] : (byte)0,
                    inventorySlotIndices.ContainsKey(item) ? inventorySlotIndices[item] : -1);
            }
        }
    }
}
