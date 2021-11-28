
Hook.HookMethod("Barotrauma.Item", "TryInteract", function (instance, p)
	if Hook.Call("itemInteract", instance, p.picker, p.ignoreRequiredItems, p.forceSelectKey, p.forceActionKey) == true then
		return false
	end
end, Hook.HookMethodType.Before)

Hook.HookMethod("Barotrauma.Item", "ApplyTreatment", function (instance, p)
	if Hook.Call("itemApplyTreatment", instance, p.user, p.character, p.targetLimb) then
		return false
	end
end, Hook.HookMethodType.Before)

Hook.HookMethod("Barotrauma.Item", "Combine", function (instance, p)
	if Hook.Call("itemCombine", instance, p.item, p.user) == true then
		return false
	end
end, Hook.HookMethodType.Before)

Hook.HookMethod("Barotrauma.Item", "Drop", function (instance, p)
	if Hook.Call("itemDrop", instance, p.dropper) == true then
		return false
	end
end, Hook.HookMethodType.Before)

Hook.HookMethod("Barotrauma.Item", "Equip", function (instance, p)
	if Hook.Call("itemEquip", instance, p.character) == true then
		return false
	end
end, Hook.HookMethodType.Before)

Hook.HookMethod("Barotrauma.Item", "Unequip", function (instance, p)
	if Hook.Call("itemUnequip", instance, p.character) == true then
		return false
	end
end, Hook.HookMethodType.Before)

Hook.HookMethod("Barotrauma.Networking.GameServer", "AssignJobs", function (instance, a)
	if Hook.Call("jobAssigned", a) == true then
		return false
	end
end, Hook.HookMethodType.After)