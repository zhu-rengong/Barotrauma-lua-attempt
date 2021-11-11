
Hook.HookMethod("Barotrauma.Item", "TryInteract", function (instance, p)
	Hook.Call("itemInteract", instance, p.picker, p.ignoreRequiredItems, p.forceSelectKey, p.forceActionKey)
end, Hook.HookMethodType.Before)

Hook.HookMethod("Barotrauma.Item", "ApplyTreatment", function (instance, p)
	Hook.Call("itemApplyTreatment", instance, p.user, p.character, p.targetLimb)
end, Hook.HookMethodType.Before)

Hook.HookMethod("Barotrauma.Item", "Combine", function (instance, p)
	Hook.Call("itemCombine", instance, p.item, p.user)
end, Hook.HookMethodType.Before)

Hook.HookMethod("Barotrauma.Item", "Drop", function (instance, p)
	Hook.Call("itemDrop", instance, p.dropper)
end, Hook.HookMethodType.Before)

Hook.HookMethod("Barotrauma.Item", "Equip", function (instance, p)
	Hook.Call("itemEquip", instance, p.character)
end, Hook.HookMethodType.Before)

Hook.HookMethod("Barotrauma.Item", "Unequip", function (instance, p)
	Hook.Call("itemUnequip", instance, p.character)
end, Hook.HookMethodType.Before)