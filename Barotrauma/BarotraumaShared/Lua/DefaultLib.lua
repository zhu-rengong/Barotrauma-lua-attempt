local defaultLib = {}

require("DefaultRegister")

local CreateStatic = function (typeName)
	return LuaUserData.CreateStatic("Barotrauma." .. typeName)
end

defaultLib["WayPoint"] = CreateStatic("WayPoint")
defaultLib["SpawnType"] = CreateStatic("SpawnType")
defaultLib["ChatMessageType"] = CreateStatic("Networking.ChatMessageType")
defaultLib["ServerLog_MessageType"] = CreateStatic("Networking.ServerLog+MessageType")
defaultLib["ServerLogMessageType"] = CreateStatic("Networking.ServerLog+MessageType")
defaultLib["Submarine"] = CreateStatic("Submarine")
defaultLib["Client"] = CreateStatic("Networking.Client")
defaultLib["Character"] = CreateStatic("Character")
defaultLib["CharacterInfo"] = CreateStatic("CharacterInfo")
defaultLib["Item"] = CreateStatic("Item")
defaultLib["ItemPrefab"] = CreateStatic("ItemPrefab")
defaultLib["Level"] = CreateStatic("Level")
defaultLib["PositionType"] = CreateStatic("Level+PositionType")
defaultLib["Job"] = CreateStatic("Job")
defaultLib["JobPrefab"] = CreateStatic("JobPrefab")
defaultLib["TraitorMessageType"] = CreateStatic("Networking.TraitorMessageType")
defaultLib["CauseOfDeathType"] = CreateStatic("CauseOfDeathType")
defaultLib["AfflictionPrefab"] = CreateStatic("AfflictionPrefab")
defaultLib["CharacterTeamType"] = CreateStatic("CharacterTeamType")
defaultLib["Vector2"] = LuaUserData.CreateStatic("Microsoft.Xna.Framework.Vector2")
defaultLib["Vector3"] = LuaUserData.CreateStatic("Microsoft.Xna.Framework.Vector3")
defaultLib["Vector4"] = LuaUserData.CreateStatic("Microsoft.Xna.Framework.Vector4")
defaultLib["Color"] = LuaUserData.CreateStatic("Microsoft.Xna.Framework.Color")
defaultLib["Point"] = LuaUserData.CreateStatic("Microsoft.Xna.Framework.Point")
defaultLib["ChatMessage"] = CreateStatic("Networking.ChatMessage")
defaultLib["Hull"] = CreateStatic("Hull")
defaultLib["InvSlotType"] = CreateStatic("InvSlotType")
defaultLib["LimbType"] = CreateStatic("LimbType")
defaultLib["ActionType"] = CreateStatic("ActionType")
defaultLib["Gap"] = CreateStatic("Gap")
defaultLib["ContentPackage"] = CreateStatic("ContentPackage")
defaultLib["ClientPermissions"] = {
	None = 0x0,
	ManageRound = 0x1,
	Kick = 0x2,
	Ban = 0x4,
	Unban = 0x8,
	SelectSub = 0x10,
	SelectMode = 0x20,
	ManageCampaign = 0x40,
	ConsoleCommands = 0x80,
	ServerLog = 0x100,
	ManageSettings = 0x200,
	ManagePermissions = 0x400,
	KarmaImmunity = 0x800,
	All = 0xFFF
}
defaultLib["Signal"] = CreateStatic("Items.Components.Signal")
defaultLib["DeliveryMethod"] = CreateStatic("Networking.DeliveryMethod")
defaultLib["ClientPacketHeader"] = CreateStatic("Networking.ClientPacketHeader")
defaultLib["ServerPacketHeader"] = CreateStatic("Networking.ServerPacketHeader")
defaultLib["RandSync"] = CreateStatic("Rand+RandSync")
defaultLib["SubmarineInfo"] = CreateStatic("SubmarineInfo")
defaultLib["Rectangle"] = LuaUserData.CreateStatic("Microsoft.Xna.Framework.Rectangle")
defaultLib["Entity"] = CreateStatic("Entity")
defaultLib["Physics"] = CreateStatic("Physics")
defaultLib["FireSource"] = CreateStatic("FireSource")
defaultLib["TextManager"] = CreateStatic("TextManager")
defaultLib["NetEntityEvent"] = CreateStatic("Networking.NetEntityEvent")

defaultLib["AIObjective"] = CreateStatic("AIObjective")
defaultLib["AIObjectiveChargeBatteries"] = CreateStatic("AIObjectiveChargeBatteries")
defaultLib["AIObjectiveCleanupItem"] = CreateStatic("AIObjectiveCleanupItem")
defaultLib["AIObjectiveCleanupItems"] = CreateStatic("AIObjectiveCleanupItems")
defaultLib["AIObjectiveCombat"] = CreateStatic("AIObjectiveCombat")
defaultLib["AIObjectiveContainItem"] = CreateStatic("AIObjectiveContainItem")
defaultLib["AIObjectiveDecontainItem"] = CreateStatic("AIObjectiveDecontainItem")
defaultLib["AIObjectiveEscapeHandcuffs"] = CreateStatic("AIObjectiveEscapeHandcuffs")
defaultLib["AIObjectiveExtinguishFire"] = CreateStatic("AIObjectiveExtinguishFire")
defaultLib["AIObjectiveExtinguishFires"] = CreateStatic("AIObjectiveExtinguishFires")
defaultLib["AIObjectiveFightIntruders"] = CreateStatic("AIObjectiveFightIntruders")
defaultLib["AIObjectiveFindDivingGear"] = CreateStatic("AIObjectiveFindDivingGear")
defaultLib["AIObjectiveFindSafety"] = CreateStatic("AIObjectiveFindSafety")
defaultLib["AIObjectiveFixLeak"] = CreateStatic("AIObjectiveFixLeak")
defaultLib["AIObjectiveFixLeaks"] = CreateStatic("AIObjectiveFixLeaks")
defaultLib["AIObjectiveGetItem"] = CreateStatic("AIObjectiveGetItem")
defaultLib["AIObjectiveGoTo"] = CreateStatic("AIObjectiveGoTo")
defaultLib["AIObjectiveIdle"] = CreateStatic("AIObjectiveIdle")
defaultLib["AIObjectiveOperateItem"] = CreateStatic("AIObjectiveOperateItem")
defaultLib["AIObjectiveOperateItem"] = CreateStatic("AIObjectiveOperateItem")
defaultLib["AIObjectivePumpWater"] = CreateStatic("AIObjectivePumpWater")
defaultLib["AIObjectiveRepairItem"] = CreateStatic("AIObjectiveRepairItem")
defaultLib["AIObjectiveRepairItems"] = CreateStatic("AIObjectiveRepairItems")
defaultLib["AIObjectiveRescue"] = CreateStatic("AIObjectiveRescue")
defaultLib["AIObjectiveRescueAll"] = CreateStatic("AIObjectiveRescueAll")
defaultLib["AIObjectiveReturn"] = CreateStatic("AIObjectiveReturn")
defaultLib["CombatMode"] = CreateStatic("AIObjectiveCombat+CombatMode")

defaultLib["DisconnectReason"] = CreateStatic("Networking.DisconnectReason")


if SERVER then

elseif CLIENT then
	defaultLib["Sprite"] = CreateStatic("Sprite")
	defaultLib["Keys"] = LuaUserData.CreateStatic("Microsoft.Xna.Framework.Input.Keys")
	defaultLib["PlayerInput"] = CreateStatic("PlayerInput")

	GUI.CreateStaticValues()
end

return defaultLib
