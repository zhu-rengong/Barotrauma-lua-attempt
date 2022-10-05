﻿using Barotrauma.Networking;

namespace Barotrauma
{
    partial class TraitorMissionResult
    {
        public TraitorMissionResult(Traitor.TraitorMission mission)
        {
            MissionIdentifier = mission.Identifier;
            EndMessage = mission.GlobalEndMessage;
            Success = mission.IsCompleted;
            foreach (Traitor traitor in mission.Traitors.Values)
            {
                Characters.Add(traitor.Character);
            }
        }

        public TraitorMissionResult(Identifier identifier, string globalEndMessage, bool isCompleted, Character[] characters = null)
        {
            MissionIdentifier = identifier;
            EndMessage = globalEndMessage;
            Success = isCompleted;
            if (Characters != null)
            {
                Characters.AddRange(characters);
            }
        }

        public void ServerWrite(IWriteMessage msg)
        {
            msg.WriteIdentifier(MissionIdentifier);
            msg.WriteString(EndMessage);
            msg.WriteBoolean(Success);
            msg.WriteByte((byte)Characters.Count);
            foreach (Character character in Characters)
            {
                msg.WriteUInt16(character.ID);
            }
        }
    }
}
