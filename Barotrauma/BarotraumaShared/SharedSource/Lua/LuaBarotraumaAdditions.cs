using System;
using System.Collections.Generic;
using System.Text;
using MoonSharp.Interpreter;
using Microsoft.Xna.Framework;

namespace Barotrauma.Networking
{
	partial class Client
	{
		public static List<Client> ClientList
		{
			get
			{
#if SERVER
				return GameMain.Server.ConnectedClients;
#else
				return GameMain.Client.ConnectedClients;
#endif
			}
		}

	}

}

namespace Barotrauma 
{
	using Barotrauma.Networking;
	using System.Linq;
	using System.Reflection;



	partial class Character
	{
		
	}

	partial class AfflictionPrefab
	{
		public static AfflictionPrefab[] ListArray
		{
			get
			{
				return List.ToArray();
			}
		}
	}

	partial class CharacterInfo
	{
		public static CharacterInfo Create(string speciesName, string name = "", JobPrefab jobPrefab = null, string ragdollFileName = null, int variant = 0, Rand.RandSync randSync = Rand.RandSync.Unsynced, string npcIdentifier = "")
		{
			return new CharacterInfo(speciesName, name, name, jobPrefab, ragdollFileName, variant, randSync, npcIdentifier);
		}
	}

	partial class Item
	{
		public static void AddToRemoveQueue(Item item)
		{
			EntitySpawner.Spawner.AddToRemoveQueue(item);
		}

		public object GetComponentString(string component)
		{
			Type type = Type.GetType("Barotrauma.Items.Components." + component);

			if (type == null)
				return null;

			MethodInfo method = typeof(Item).GetMethod(nameof(Item.GetComponent));
			MethodInfo generic = method.MakeGenericMethod(type);
			return generic.Invoke(this, null);
		}

	}

	partial class ItemPrefab
	{

		public static ItemPrefab GetItemPrefab(string itemNameOrId)
		{
			ItemPrefab itemPrefab =
			(MapEntityPrefab.Find(itemNameOrId, identifier: null, showErrorMessages: false) ??
			MapEntityPrefab.Find(null, identifier: itemNameOrId, showErrorMessages: false)) as ItemPrefab;

			return itemPrefab;
		}
	}

}

namespace Barotrauma.Items.Components
{
	using Barotrauma.Networking;

	partial class CustomInterface
	{
	}

	partial struct Signal
	{
	}
}