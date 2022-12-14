﻿namespace Barotrauma.Abilities
{
    class CharacterAbilityModifyStat : CharacterAbility
    {
        private readonly StatTypes statType;
        private readonly float value;
        bool lastState;
        public override bool AllowClientSimulation => true;

        public CharacterAbilityModifyStat(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
        {
            statType = CharacterAbilityGroup.ParseStatType(abilityElement.GetAttributeString("stattype", ""), CharacterTalent.DebugIdentifier);
            value = abilityElement.GetAttributeFloat("value", 0f);
        }

        protected override void VerifyState(bool conditionsMatched, float timeSinceLastUpdate)
        {
            if (conditionsMatched != lastState)
            {
                Character.ChangeStat(statType, conditionsMatched ? value : -value);
                lastState = conditionsMatched;
            }
        }
    }
}
