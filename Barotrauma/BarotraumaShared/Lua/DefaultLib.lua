local defaultLib = {}

local descriptors = require("DefaultRegister")

local CreateStatic = function (typeName, addCallMethod)
	local staticUserdata = LuaUserData.CreateStatic(typeName)
	
	if addCallMethod then
		debug.setmetatable(staticUserdata, {
			__call = function(obj, ...) 
				return staticUserdata.__new(...)
			end
		})
	end

	return staticUserdata
end

defaultLib["Descriptors"] = descriptors

defaultLib["Byte"] = CreateStatic("Barotrauma.LuaByte", true)
defaultLib["UShort"] = CreateStatic("Barotrauma.LuaUShort", true)
defaultLib["Float"] = CreateStatic("Barotrauma.LuaFloat", true)

defaultLib["SpawnType"] = CreateStatic("Barotrauma.SpawnType")
defaultLib["ChatMessageType"] = CreateStatic("Barotrauma.Networking.ChatMessageType")
defaultLib["ServerLog_MessageType"] = CreateStatic("Barotrauma.Networking.ServerLog+MessageType")
defaultLib["ServerLogMessageType"] = CreateStatic("Barotrauma.Networking.ServerLog+MessageType")
defaultLib["PositionType"] = CreateStatic("Barotrauma.Level+PositionType")
defaultLib["InvSlotType"] = CreateStatic("Barotrauma.InvSlotType")
defaultLib["LimbType"] = CreateStatic("Barotrauma.LimbType")
defaultLib["ActionType"] = CreateStatic("Barotrauma.ActionType")
defaultLib["DeliveryMethod"] = CreateStatic("Barotrauma.Networking.DeliveryMethod")
defaultLib["ClientPacketHeader"] = CreateStatic("Barotrauma.Networking.ClientPacketHeader")
defaultLib["ServerPacketHeader"] = CreateStatic("Barotrauma.Networking.ServerPacketHeader")
defaultLib["RandSync"] = CreateStatic("Barotrauma.Rand+RandSync")
defaultLib["DisconnectReason"] = CreateStatic("Barotrauma.Networking.DisconnectReason")
defaultLib["TraitorMessageType"] = CreateStatic("Barotrauma.Networking.TraitorMessageType")
defaultLib["CombatMode"] = CreateStatic("Barotrauma.AIObjectiveCombat+CombatMode")
defaultLib["CauseOfDeathType"] = CreateStatic("Barotrauma.CauseOfDeathType")
defaultLib["CharacterTeamType"] = CreateStatic("Barotrauma.CharacterTeamType")

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


defaultLib["WayPoint"] = CreateStatic("Barotrauma.WayPoint", true)
defaultLib["Submarine"] = CreateStatic("Barotrauma.Submarine", true)
defaultLib["Client"] = CreateStatic("Barotrauma.Networking.Client", true)
defaultLib["Character"] = CreateStatic("Barotrauma.Character")
defaultLib["CharacterInfo"] = CreateStatic("Barotrauma.CharacterInfo", true)
defaultLib["Item"] = CreateStatic("Barotrauma.Item", true)
defaultLib["ItemPrefab"] = CreateStatic("Barotrauma.ItemPrefab", true)
defaultLib["FactionPrefab"] = CreateStatic("Barotrauma.FactionPrefab", true)
defaultLib["Level"] = CreateStatic("Barotrauma.Level")
defaultLib["Job"] = CreateStatic("Barotrauma.Job", true)
defaultLib["JobPrefab"] = CreateStatic("Barotrauma.JobPrefab", true)
defaultLib["AfflictionPrefab"] = CreateStatic("Barotrauma.AfflictionPrefab", true)
defaultLib["ChatMessage"] = CreateStatic("Barotrauma.Networking.ChatMessage")
defaultLib["Hull"] = CreateStatic("Barotrauma.Hull", true)
defaultLib["Gap"] = CreateStatic("Barotrauma.Gap", true)
defaultLib["ContentPackage"] = CreateStatic("Barotrauma.ContentPackage", true)
defaultLib["Signal"] = CreateStatic("Barotrauma.Items.Components.Signal", true)
defaultLib["SubmarineInfo"] = CreateStatic("Barotrauma.SubmarineInfo", true)
defaultLib["Entity"] = CreateStatic("Barotrauma.Entity", true)
defaultLib["Physics"] = CreateStatic("Barotrauma.Physics")
defaultLib["FireSource"] = CreateStatic("Barotrauma.FireSource", true)
defaultLib["TextManager"] = CreateStatic("Barotrauma.TextManager")
defaultLib["NetEntityEvent"] = CreateStatic("Barotrauma.Networking.NetEntityEvent")
defaultLib["Screen"] = CreateStatic("Barotrauma.Screen")
defaultLib["AttackResult"] = CreateStatic("Barotrauma.AttackResult", true)

defaultLib["AIObjective"] = CreateStatic("Barotrauma.AIObjective", true)
defaultLib["AIObjectiveChargeBatteries"] = CreateStatic("Barotrauma.AIObjectiveChargeBatteries", true)
defaultLib["AIObjectiveCleanupItem"] = CreateStatic("Barotrauma.AIObjectiveCleanupItem", true)
defaultLib["AIObjectiveCleanupItems"] = CreateStatic("Barotrauma.AIObjectiveCleanupItems", true)
defaultLib["AIObjectiveCombat"] = CreateStatic("Barotrauma.AIObjectiveCombat", true)
defaultLib["AIObjectiveContainItem"] = CreateStatic("Barotrauma.AIObjectiveContainItem", true)
defaultLib["AIObjectiveDecontainItem"] = CreateStatic("Barotrauma.AIObjectiveDecontainItem", true)
defaultLib["AIObjectiveEscapeHandcuffs"] = CreateStatic("Barotrauma.AIObjectiveEscapeHandcuffs", true)
defaultLib["AIObjectiveExtinguishFire"] = CreateStatic("Barotrauma.AIObjectiveExtinguishFire", true)
defaultLib["AIObjectiveExtinguishFires"] = CreateStatic("Barotrauma.AIObjectiveExtinguishFires", true)
defaultLib["AIObjectiveFightIntruders"] = CreateStatic("Barotrauma.AIObjectiveFightIntruders", true)
defaultLib["AIObjectiveFindDivingGear"] = CreateStatic("Barotrauma.AIObjectiveFindDivingGear", true)
defaultLib["AIObjectiveFindSafety"] = CreateStatic("Barotrauma.AIObjectiveFindSafety", true)
defaultLib["AIObjectiveFixLeak"] = CreateStatic("Barotrauma.AIObjectiveFixLeak", true)
defaultLib["AIObjectiveFixLeaks"] = CreateStatic("Barotrauma.AIObjectiveFixLeaks", true)
defaultLib["AIObjectiveGetItem"] = CreateStatic("Barotrauma.AIObjectiveGetItem", true)
defaultLib["AIObjectiveGoTo"] = CreateStatic("Barotrauma.AIObjectiveGoTo", true)
defaultLib["AIObjectiveIdle"] = CreateStatic("Barotrauma.AIObjectiveIdle", true)
defaultLib["AIObjectiveOperateItem"] = CreateStatic("Barotrauma.AIObjectiveOperateItem", true)
defaultLib["AIObjectiveOperateItem"] = CreateStatic("Barotrauma.AIObjectiveOperateItem", true)
defaultLib["AIObjectivePumpWater"] = CreateStatic("Barotrauma.AIObjectivePumpWater", true)
defaultLib["AIObjectiveRepairItem"] = CreateStatic("Barotrauma.AIObjectiveRepairItem", true)
defaultLib["AIObjectiveRepairItems"] = CreateStatic("Barotrauma.AIObjectiveRepairItems", true)
defaultLib["AIObjectiveRescue"] = CreateStatic("Barotrauma.AIObjectiveRescue", true)
defaultLib["AIObjectiveRescueAll"] = CreateStatic("Barotrauma.AIObjectiveRescueAll", true)
defaultLib["AIObjectiveReturn"] = CreateStatic("Barotrauma.AIObjectiveReturn", true)

defaultLib["Vector2"] = CreateStatic("Microsoft.Xna.Framework.Vector2", true)
defaultLib["Vector3"] = CreateStatic("Microsoft.Xna.Framework.Vector3", true)
defaultLib["Vector4"] = CreateStatic("Microsoft.Xna.Framework.Vector4", true)
defaultLib["Color"] = CreateStatic("Microsoft.Xna.Framework.Color", true)
defaultLib["Point"] = CreateStatic("Microsoft.Xna.Framework.Point", true)
defaultLib["Rectangle"] = CreateStatic("Microsoft.Xna.Framework.Rectangle", true)

if SERVER then

elseif CLIENT then
	defaultLib["Sprite"] = CreateStatic("Barotrauma.Sprite", true)
	defaultLib["PlayerInput"] = CreateStatic("Barotrauma.PlayerInput", true)

	defaultLib["Keys"] = CreateStatic("Microsoft.Xna.Framework.Input.Keys", true)

	defaultLib["GUI"] = {
		RectTransform = CreateStatic("Barotrauma.RectTransform", true),
		LayoutGroup = CreateStatic("Barotrauma.GUILayoutGroup", true),
		Button = CreateStatic("Barotrauma.GUIButton", true),
		TextBox = CreateStatic("Barotrauma.GUITextBox", true),
		Canvas = CreateStatic("Barotrauma.GUICanvas", true),
		Frame = CreateStatic("Barotrauma.GUIFrame", true),
		TextBlock = CreateStatic("Barotrauma.GUITextBlock", true),
		TickBox = CreateStatic("Barotrauma.GUITickBox", true),
		Image = CreateStatic("Barotrauma.GUIImage", true),
		ListBox = CreateStatic("Barotrauma.GUIListBox", true),
		ScrollBar = CreateStatic("Barotrauma.GUIScrollBar", true),
		DropDown = CreateStatic("Barotrauma.GUIDropDown", true),

		Screen = CreateStatic("Barotrauma.Screen"),

		Anchor = CreateStatic("Barotrauma.Anchor"),
		Alignment = CreateStatic("Barotrauma.Alignment"),
		Pivot = CreateStatic("Barotrauma.Pivot"),
	}
end

return defaultLib
