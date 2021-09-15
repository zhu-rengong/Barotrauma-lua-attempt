using System;
using System.Collections.Generic;
using System.Text;

namespace Barotrauma.Networking
{
	partial class Client
	{
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
	using Microsoft.Xna.Framework;

	partial class ItemPrefab
	{
		public static void AddToSpawnQueue(ItemPrefab itemPrefab, Vector2 position, object spawned = null)
		{
			EntitySpawner.Spawner.AddToSpawnQueue(itemPrefab, position, onSpawned: (Item item) =>
			{
				GameMain.Lua.CallFunction(spawned, new object[] { item });
			});
		}

		public static void AddToSpawnQueue(ItemPrefab itemPrefab, Inventory inventory, object spawned = null)
		{
			EntitySpawner.Spawner.AddToSpawnQueue(itemPrefab, inventory, onSpawned: (Item item) =>
			{
				GameMain.Lua.CallFunction(spawned, new object[] { item });
			});
		}
	}
}

namespace Barotrauma.Items.Components
{
	using Barotrauma.Networking;

	partial class CustomInterface
	{
		public void UpdateClients()
		{

			//notify all clients of the new state
			GameMain.Server.CreateEntityEvent(item, new object[]
			{
				NetEntityEvent.Type.ComponentState,
				item.GetComponentIndex(this)
			});

			item.CreateServerEvent(this);
		}
	}

	partial struct Signal
	{
		public static Signal Create(string value, int stepsTaken = 0, Character sender = null, Item source = null, float power = 0.0f, float strength = 1.0f)
		{
			return new Signal(value, stepsTaken, sender, source, power, strength);
		}
	}


}