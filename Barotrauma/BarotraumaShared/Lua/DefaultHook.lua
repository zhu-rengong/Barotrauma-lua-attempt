
Hook.HookMethod("Barotrauma.Item", "TryInteract", "itemInteract", nil) -- instance, picker, ignoreRequiredItems, forceSelectKey, forceActionKey
Hook.HookMethod("Barotrauma.Item", "Use", "itemUse", function (instance, deltaTime, character, targetLimb) return {instance, character, targetLimb} end)
Hook.HookMethod("Barotrauma.Item", "SecondaryUse", "itemSecondaryUse", function (instance, deltaTime, character) return {instance, character} end)
Hook.HookMethod("Barotrauma.Item", "ApplyTreatment", "itemApplyTreatment", nil) -- instance, user, character, targetLimb
Hook.HookMethod("Barotrauma.Item", "Combine", "itemCombine", nil) -- instance, item, user
Hook.HookMethod("Barotrauma.Item", "Drop", "itemDrop", function (instance, dropper, createNetworkEvent)	return {instance, dropper} end)
Hook.HookMethod("Barotrauma.Item", "Equip", "itemEquip", nil) -- instance, character
Hook.HookMethod("Barotrauma.Item", "Unequip", "itemUnequip", nil) -- instance, character

