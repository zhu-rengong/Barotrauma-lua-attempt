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
defaultLib["JobPrefab"] = CreateStatic("JobPrefab")
defaultLib["TraitorMessageType"] = CreateStatic("Networking.TraitorMessageType")
defaultLib["CauseOfDeathType"] = CreateStatic("CauseOfDeathType")
defaultLib["AfflictionPrefab"] = CreateStatic("AfflictionPrefab")
defaultLib["CharacterTeamType"] = CreateStatic("CharacterTeamType")
defaultLib["Vector2"] = LuaUserData.RegisterType("Microsoft.Xna.Framework.Vector2")
defaultLib["Vector3"] = LuaUserData.RegisterType("Microsoft.Xna.Framework.Vector3")
defaultLib["Vector4"] = LuaUserData.RegisterType("Microsoft.Xna.Framework.Vector4")
defaultLib["Color"] = LuaUserData.RegisterType("Microsoft.Xna.Framework.Color")
defaultLib["Point"] = LuaUserData.RegisterType("Microsoft.Xna.Framework.Point")
defaultLib["ChatMessage"] = CreateStatic("Networking.ChatMessage")
defaultLib["Hull"] = CreateStatic("Hull")
defaultLib["InvSlotType"] = CreateStatic("InvSlotType")
defaultLib["Gap"] = CreateStatic("Gap")
defaultLib["ContentPackage"] = CreateStatic("ContentPackage")
defaultLib["ClientPermissions"] = CreateStatic("Networking.ClientPermissions")
defaultLib["Signal"] = CreateStatic("Items.Components.Signal")
defaultLib["DeliveryMethod"] = CreateStatic("Networking.DeliveryMethod")
defaultLib["ClientPacketHeader"] = CreateStatic("Networking.ClientPacketHeader")
defaultLib["ServerPacketHeader"] = CreateStatic("Networking.ServerPacketHeader")
defaultLib["RandSync"] = CreateStatic("Rand+RandSync")
defaultLib["SubmarineInfo"] = CreateStatic("SubmarineInfo")
defaultLib["Rectangle"] = LuaUserData.RegisterType("Microsoft.Xna.Framework.Rectangle")
defaultLib["Entity"] = CreateStatic("Entity")
defaultLib["Physics"] = CreateStatic("Physics")

if SERVER then

elseif CLIENT then
	defaultLib["Sprite"] = CreateStatic("Sprite")
	defaultLib["Keys"] = LuaUserData.RegisterType("Microsoft.Xna.Framework.Input.Keys")
	defaultLib["PlayerInput"] = CreateStatic("PlayerInput")
end

return defaultLib