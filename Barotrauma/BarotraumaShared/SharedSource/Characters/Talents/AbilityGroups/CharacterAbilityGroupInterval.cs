﻿namespace Barotrauma.Abilities
{
    class CharacterAbilityGroupInterval : CharacterAbilityGroup
    {
        private float interval { get; set; }
        public float TimeSinceLastUpdate { get; private set; }

        private float effectDelay;
        private float effectDelayTimer;


        public CharacterAbilityGroupInterval(AbilityEffectType abilityEffectType, CharacterTalent characterTalent, ContentXElement abilityElementGroup) : 
            base(abilityEffectType, characterTalent, abilityElementGroup)
        {            
            // too many overlapping intervals could cause hitching? maybe randomize a little
            interval = abilityElementGroup.GetAttributeFloat("interval", 0f);
            effectDelay = abilityElementGroup.GetAttributeFloat("effectdelay", 0f);
        }
        public void UpdateAbilityGroup(float deltaTime)
        {
            if (!IsActive) { return; }
            TimeSinceLastUpdate += deltaTime;
            if (TimeSinceLastUpdate >= interval)
            {
                bool conditionsMatched = IsApplicable();
                effectDelayTimer = conditionsMatched ? effectDelayTimer + TimeSinceLastUpdate : 0f;
                conditionsMatched &= effectDelayTimer >= effectDelay;

                foreach (var characterAbility in characterAbilities)
                {
                    if (characterAbility.IsViable())
                    {
                        characterAbility.UpdateCharacterAbility(conditionsMatched, TimeSinceLastUpdate);
                    }
                }
                if (conditionsMatched)
                {
                    timesTriggered++;
                }
                TimeSinceLastUpdate = 0;
            }
        }
        private bool IsApplicable()
        {
            if (timesTriggered >= maxTriggerCount) { return false; }
            foreach (var abilityCondition in abilityConditions)
            {
                if (!abilityCondition.MatchesCondition())
                {
                    return false;
                }
            }
            return true;
        }
    }
}
