﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Barotrauma.Items.Components
{
    partial class Quality : ItemComponent
    {
        public const int MaxQuality = 3;

        public static readonly float[] QualityCommonnesses = new float[]
        {
            0.8f,
            0.15f,
            0.045f,
            0.005f,
        };

        public enum StatType
        {
            Condition,
            ExplosionRadius,
            ExplosionDamage,
            RepairSpeed,
            RepairToolStructureRepairMultiplier,
            RepairToolStructureDamageMultiplier,
            RepairToolDeattachTimeMultiplier,
            FirepowerMultiplier,
            StrikingPowerMultiplier,
            StrikingSpeedMultiplier,
            FiringRateMultiplier
        }

        private readonly Dictionary<StatType, float> statValues = new Dictionary<StatType, float>();

        private int qualityLevel;

        [Editable(MinValueInt = 0, MaxValueInt = MaxQuality), Serialize(0, IsPropertySaveable.Yes)]
        public int QualityLevel
        {
            get { return qualityLevel; }
            set 
            {
                if (value == qualityLevel) { return; }

                bool wasInFullCondition = item.IsFullCondition;
                qualityLevel = MathHelper.Clamp(value, 0, MaxQuality);
                item.RecalculateConditionValues();
                //set the condition to the new max condition
                if (wasInFullCondition && statValues.ContainsKey(StatType.Condition))
                {
                    item.Condition = item.MaxCondition;
                }
            }
        }

        public Quality(Item item, ContentXElement element) : base(item, element)
        {
            foreach (XElement subElement in element.Elements())
            {
                switch (subElement.Name.ToString().ToLower())
                {
                    case "stattype":
                    case "statvalue":
                    case "qualitystat":
                        string statTypeString = subElement.GetAttributeString("stattype", "");
                        if (!Enum.TryParse(statTypeString, true, out StatType statType))
                        {
                            DebugConsole.ThrowError("Invalid stat type type \"" + statTypeString + "\" in item (" + ((MapEntity)item).Prefab.Identifier + ")");
                        }
                        float statValue = subElement.GetAttributeFloat("value", 0f);
                        statValues.TryAdd(statType, statValue);                        
                        break;
                }
            }
        }

        public float GetValue(StatType statType)
        {
            if (!statValues.ContainsKey(statType)) { return 0.0f; }
            return statValues[statType] * qualityLevel;
        }
    }
}
