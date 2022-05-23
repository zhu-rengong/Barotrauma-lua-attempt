local Register = LuaSetup.Register
local RegisterBarotrauma = LuaSetup.RegisterBarotrauma

local localizedStrings = {
    "LocalizedString", "LimitLString", "WrappedLString", "AddedPunctuationLString", "CapitalizeLString", "ConcatLString", "FallbackLString", "FormattedLString", "InputTypeLString", "JoinLString", "LowerLString", "RawLString", "ReplaceLString", "ServerMsgLString", "SplitLString", "TagLString", "TrimLString", "UpperLString", "StripRichTagsLString",
}

for key, value in pairs(localizedStrings) do
    RegisterBarotrauma(value)
end

RegisterBarotrauma("Networking.ClientPeer")
RegisterBarotrauma("Networking.GameClient")

RegisterBarotrauma("Sounds.SoundManager")
RegisterBarotrauma("Sounds.OggSound")
RegisterBarotrauma("Sounds.VideoSound")
RegisterBarotrauma("Sounds.VoipSound")
RegisterBarotrauma("Sounds.SoundChannel")

RegisterBarotrauma("Sounds.LowpassFilter")
RegisterBarotrauma("Sounds.HighpassFilter")
RegisterBarotrauma("Sounds.BandpassFilter")
RegisterBarotrauma("Sounds.NotchFilter")
RegisterBarotrauma("Sounds.LowShelfFilter")
RegisterBarotrauma("Sounds.HighShelfFilter")
RegisterBarotrauma("Sounds.PeakFilter")

RegisterBarotrauma("Lights.LightManager")
RegisterBarotrauma("Lights.LightSource")
RegisterBarotrauma("Lights.LightSourceParams")

RegisterBarotrauma("ChatBox")
RegisterBarotrauma("GUICanvas")
RegisterBarotrauma("Anchor")
RegisterBarotrauma("Alignment")
RegisterBarotrauma("Pivot")
RegisterBarotrauma("Key")
RegisterBarotrauma("PlayerInput")
RegisterBarotrauma("ScalableFont")

Register("Microsoft.Xna.Framework.Graphics.SpriteBatch")
Register("Microsoft.Xna.Framework.Graphics.Texture2D")
Register("EventInput.KeyboardDispatcher")
Register("EventInput.KeyEventArgs")
Register("Microsoft.Xna.Framework.Input.Keys")
Register("Microsoft.Xna.Framework.Input.KeyboardState")

RegisterBarotrauma("Sprite")
RegisterBarotrauma("GUI")
RegisterBarotrauma("GUIStyle")
RegisterBarotrauma("GUILayoutGroup")
RegisterBarotrauma("GUITextBox")
RegisterBarotrauma("GUITextBlock")
RegisterBarotrauma("GUIButton")
RegisterBarotrauma("RectTransform")
RegisterBarotrauma("GUIFrame")
RegisterBarotrauma("GUITickBox")
RegisterBarotrauma("GUICustomComponent")
RegisterBarotrauma("GUIImage")
RegisterBarotrauma("GUIListBox")
RegisterBarotrauma("GUIScrollBar")
RegisterBarotrauma("GUIDropDown")
RegisterBarotrauma("GUINumberInput")
RegisterBarotrauma("GUIMessage")
RegisterBarotrauma("GUIMessageBox")
RegisterBarotrauma("GUIFont")
RegisterBarotrauma("GUIFontPrefab")
RegisterBarotrauma("GUIColorPicker")
RegisterBarotrauma("GUIProgressBar")

RegisterBarotrauma("Inventory+SlotReference")