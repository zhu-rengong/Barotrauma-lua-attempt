local Register = LuaSetup.Register
local RegisterBarotrauma = LuaSetup.RegisterBarotrauma


local localizedStrings = {
    "LocalizedString", "AddedPunctuationLString", "CapitalizeLString", "ConcatLString", "FallbackLString", "FormattedLString", "InputTypeLString", "JoinLString", "LowerLString", "RawLString", "ReplaceLString", "ServerMsgLString", "SplitLString", "TagLString", "TrimLString", "UpperLString", "StripRichTagsLString",
}

for key, value in pairs(localizedStrings) do
    RegisterBarotrauma(value)
end

RegisterBarotrauma("Networking.GameServer")

RegisterBarotrauma("Networking.ServerPeer")
RegisterBarotrauma("Networking.ServerPeer+PendingClient")

RegisterBarotrauma("Traitor")
RegisterBarotrauma("Traitor+TraitorMission")