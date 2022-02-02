
Hook.HookMethod(
	"Barotrauma.Item", "TryInteract",
	{
		"Barotrauma.Character",
		"System.Boolean",
		"System.Boolean",
		"System.Boolean"
	},
	function (instance, p)
		if Hook.Call("item.interact", instance, p.picker, p.ignoreRequiredItems, p.forceSelectKey, p.forceActionKey) == true then
			return false
		end
	end,
	Hook.HookMethodType.Before
)

Hook.HookMethod(
	"Barotrauma.Item", "ApplyTreatment",
	{
		"Barotrauma.Character",
		"Barotrauma.Character",
		"Barotrauma.Limb"
	},
	function (instance, p)
		if Hook.Call("item.applyTreatment", instance, p.user, p.character, p.targetLimb) then
			return false
		end
	end,
	Hook.HookMethodType.Before
)

Hook.HookMethod(
	"Barotrauma.Item", "Combine",
	{
		"Barotrauma.Item",
		"Barotrauma.Character"
	},
	function (instance, p)
		if Hook.Call("item.combine", instance, p.item, p.user) == true then
			return false
		end
	end,
	Hook.HookMethodType.Before
)

Hook.HookMethod(
	"Barotrauma.Item", "Drop",
	{
		"Barotrauma.Character",
		"System.Boolean"
	},
	function (instance, p)
		if Hook.Call("item.drop", instance, p.dropper) == true then
			return false
		end
	end,
	Hook.HookMethodType.Before
)

Hook.HookMethod(
	"Barotrauma.Item", "Equip",
	{
		"Barotrauma.Character"
	},
	function (instance, p)
		if Hook.Call("item.equip", instance, p.character) == true then
			return false
		end
	end,
	Hook.HookMethodType.Before
)

Hook.HookMethod(
	"Barotrauma.Item", "Unequip",
	{
		"Barotrauma.Character"
	},
	function (instance, p)
		if Hook.Call("item.unequip", instance, p.character) == true then
			return false
		end
	end,
	Hook.HookMethodType.Before
)
