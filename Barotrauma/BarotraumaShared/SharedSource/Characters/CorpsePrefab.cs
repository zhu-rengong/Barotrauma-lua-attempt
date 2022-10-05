﻿using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Barotrauma
{
    class CorpsePrefab : HumanPrefab
    {
        public static readonly PrefabCollection<CorpsePrefab> Prefabs = new PrefabCollection<CorpsePrefab>();

        public override void Dispose() { }

        public static CorpsePrefab Get(Identifier identifier)
        {
            if (Prefabs == null)
            {
                DebugConsole.ThrowError("Issue in the code execution order: job prefabs not loaded.");
                return null;
            }
            if (Prefabs.ContainsKey(identifier))
            {
                return Prefabs[identifier];
            }
            else
            {
                DebugConsole.ThrowError("Couldn't find a job prefab with the given identifier: " + identifier);
                return null;
            }
        }

        [Serialize(Level.PositionType.Wreck, IsPropertySaveable.No)]
        public Level.PositionType SpawnPosition { get; private set; }

        [Serialize(0, IsPropertySaveable.No)]
        public int MinMoney { get; private set; }

        [Serialize(0, IsPropertySaveable.No)]
        public int MaxMoney { get; private set; }

        public CorpsePrefab(ContentXElement element, CorpsesFile file) : base(element, file, npcSetIdentifier: Identifier.Empty) { }

        public static CorpsePrefab Random(Rand.RandSync sync = Rand.RandSync.Unsynced) => Prefabs.GetRandom(sync);
    }
}
