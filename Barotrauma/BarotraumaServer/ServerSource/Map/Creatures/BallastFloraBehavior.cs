﻿using Barotrauma.Items.Components;
using Barotrauma.Networking;
using System;

namespace Barotrauma.MapCreatures.Behavior
{
    partial class BallastFloraBehavior
    {
        const float DamageUpdateInterval = 1.0f;

        private float damageUpdateTimer;

        partial void LoadPrefab(ContentXElement element)
        {
            foreach (var subElement in element.Elements())
            {
                switch (subElement.Name.ToString().ToLowerInvariant())
                {
                    case "branchsprite":
                    case "hiddenflowersprite":
                        break;
                    case "flowersprite":
                        flowerVariants++;
                        break;
                    case "leafsprite":
                        leafVariants++;
                        break;
                    case "targets":
                        LoadTargets(subElement);
                        break;
                }
            }
        }

        partial void UpdateDamage(float deltaTime)
        {
            damageUpdateTimer -= deltaTime;
            if (damageUpdateTimer > 0.0f) { return; }

            const int maxMessagesPerSecond = 10;
            int messages = 0;
            foreach (BallastFloraBranch branch in Branches)
            {
                //don't notify about minuscule amounts of damage (<= 1.0f)
                if (Math.Abs(branch.AccumulatedDamage) > 1.0f)
                {
                    CreateNetworkMessage(new BranchDamageEventData(branch));
                    branch.AccumulatedDamage = 0.0f;
                    messages++;
                    //throttle a bit: if a large ballast flora is withering, it can lead to a very large number of events otherwise
                    if (messages > maxMessagesPerSecond) { break; }
                }
            }
            damageUpdateTimer = DamageUpdateInterval;
        }

        public void ServerWrite(IWriteMessage msg, IEventData eventData)
        {
            msg.WriteByte((byte)eventData.NetworkHeader);
            
            switch (eventData)
            {
                case SpawnEventData _:
                    ServerWriteSpawn(msg);
                    break;
                case KillEventData _:
                    //do nothing
                    break;
                case BranchCreateEventData branchCreateEventData:
                    ServerWriteBranchGrowth(msg, branchCreateEventData.NewBranch, branchCreateEventData.Parent.ID);
                    break;
                case BranchDamageEventData branchDamageEventData:
                    ServerWriteBranchDamage(msg, branchDamageEventData.Branch);
                    break;
                case InfectEventData infectEventData:
                    ServerWriteInfect(msg, infectEventData.Item.ID, infectEventData.Infect, infectEventData.Infector);
                    break;
                case BranchRemoveEventData branchRemoveEventData:
                    ServerWriteBranchRemove(msg, branchRemoveEventData.Branch);
                    break;
            }
            
            msg.WriteSingle(PowerConsumptionTimer);
        }

        private void ServerWriteSpawn(IWriteMessage msg)
        {
            msg.WriteIdentifier(Prefab.Identifier);
            msg.WriteSingle(Offset.X);
            msg.WriteSingle(Offset.Y);
        }

        private void ServerWriteBranchGrowth(IWriteMessage msg, BallastFloraBranch branch, int parentId = -1)
        {
            var (x, y) = branch.Position;
            msg.WriteInt32(parentId);
            msg.WriteInt32((int)branch.ID);
            msg.WriteBoolean(branch.IsRootGrowth);
            msg.WriteRangedInteger((byte)branch.Type, 0b0000, 0b1111);
            msg.WriteRangedInteger((byte)branch.Sides, 0b0000, 0b1111);
            msg.WriteRangedInteger(branch.FlowerConfig.Serialize(), 0, 0xFFF);
            msg.WriteRangedInteger(branch.LeafConfig.Serialize(), 0, 0xFFF);
            msg.WriteUInt16((ushort)branch.MaxHealth);
            msg.WriteInt32((int)(x / VineTile.Size));
            msg.WriteInt32((int)(y / VineTile.Size));
            msg.WriteInt32(branch.ParentBranch == null ? -1 : Branches.IndexOf(branch.ParentBranch));
        }

        private void ServerWriteBranchDamage(IWriteMessage msg, BallastFloraBranch branch)
        {
            msg.WriteInt32((int)branch.ID);
            msg.WriteSingle(branch.Health);
        }
        
        private void ServerWriteInfect(IWriteMessage msg, UInt16 itemID, InfectEventData.InfectState infect, BallastFloraBranch infector = null)
        {
            msg.WriteUInt16(itemID);
            msg.WriteBoolean(infect == InfectEventData.InfectState.Yes);
            if (infect == InfectEventData.InfectState.Yes)
            {
                msg.WriteInt32(infector?.ID ?? -1);
            }
        }

        private void ServerWriteBranchRemove(IWriteMessage msg, BallastFloraBranch branch)
        {
            msg.WriteInt32(branch.ID);
        }

        public void CreateNetworkMessage(IEventData extraData)
        {
            GameMain.Server.CreateEntityEvent(Parent, new Hull.BallastFloraEventData(this, extraData));
        }
    }
}