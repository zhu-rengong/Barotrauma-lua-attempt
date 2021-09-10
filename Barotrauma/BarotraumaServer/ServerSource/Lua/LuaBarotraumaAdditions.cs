using System;
using System.Collections.Generic;
using System.Text;


namespace Barotrauma.Networking
{
	partial class Client
	{
		public static List<Client> ClientList
		{
			get
			{
				return GameMain.Server.ConnectedClients;
			}
		}

		public void SetClientCharacter(Character character)
		{
			GameMain.Server.SetClientCharacter(this, character);
		}

		public void Kick(string reason = "")
		{
			GameMain.Server.KickClient(this.Connection, reason);
		}

		public void Ban(string reason = "", bool range = false, float seconds = -1)
		{
			if (seconds == -1)
			{
				GameMain.Server.BanClient(this, reason, range, null);
			}
			else
			{
				GameMain.Server.BanClient(this, reason, range, TimeSpan.FromSeconds(seconds));
			}
		}

		public static void Unban(string player, string endpoint)
		{
			GameMain.Server.UnbanPlayer(player, endpoint);
		}

		public bool CheckPermission(ClientPermissions permissions)
		{
			return this.Permissions.HasFlag(permissions);
		}
	}

}

namespace Barotrauma 
{
	partial class CharacterInfo
	{
		public static CharacterInfo Create(string speciesName, string name = "", JobPrefab jobPrefab = null, string ragdollFileName = null, int variant = 0, Rand.RandSync randSync = Rand.RandSync.Unsynced, string npcIdentifier = "")
		{
			return new CharacterInfo(speciesName, name, name, jobPrefab, ragdollFileName, variant, randSync, npcIdentifier);
		}
	}
}