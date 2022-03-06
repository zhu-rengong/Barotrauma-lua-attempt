-- luacheck: ignore 111

--[[--
Barotrauma.GameSettings
]]
-- @code Game.GameSettings
-- @pragma nostrip
local GameSettings = {}

--- SelectCorePackage
-- @realm shared
-- @tparam ContentPackage contentPackage
-- @tparam bool forceReloadAll
function SelectCorePackage(contentPackage, forceReloadAll) end

--- AutoSelectCorePackage
-- @realm shared
-- @tparam Enumerable toRemove
function AutoSelectCorePackage(toRemove) end

--- BackUpModOrder
-- @realm shared
function BackUpModOrder() end

--- SwapPackages
-- @realm shared
-- @tparam ContentPackage corePackage
-- @tparam table regularPackages
function SwapPackages(corePackage, regularPackages) end

--- RestoreBackupPackages
-- @realm shared
function RestoreBackupPackages() end

--- EnableRegularPackage
-- @realm shared
-- @tparam ContentPackage contentPackage
function EnableRegularPackage(contentPackage) end

--- DisableRegularPackage
-- @realm shared
-- @tparam ContentPackage contentPackage
function DisableRegularPackage(contentPackage) end

--- SortContentPackages
-- @realm shared
-- @tparam bool refreshAll
function SortContentPackages(refreshAll) end

--- EnableContentPackageItems
-- @realm shared
-- @tparam Enumerable unorderedFiles
function EnableContentPackageItems(unorderedFiles) end

--- DisableContentPackageItems
-- @realm shared
-- @tparam Enumerable unorderedFiles
function DisableContentPackageItems(unorderedFiles) end

--- RefreshContentPackageItems
-- @realm shared
-- @tparam Enumerable files
function RefreshContentPackageItems(files) end

--- LoadPlayerConfig
-- @realm shared
function LoadPlayerConfig() end

--- SaveNewPlayerConfig
-- @realm shared
-- @treturn bool
function SaveNewPlayerConfig() end

--- ResetToDefault
-- @realm shared
function ResetToDefault() end

--- AreJobPreferencesEqual
-- @realm shared
-- @tparam table compareTo
-- @treturn bool
function AreJobPreferencesEqual(compareTo) end

--- GetType
-- @realm shared
-- @treturn Type
function GetType() end

--- ToString
-- @realm shared
-- @treturn string
function ToString() end

--- Equals
-- @realm shared
-- @tparam Object obj
-- @treturn bool
function Equals(obj) end

--- GetHashCode
-- @realm shared
-- @treturn number
function GetHashCode() end

---
-- GraphicsWidth, Field of type number
-- @realm shared
-- @number GraphicsWidth

---
-- GraphicsHeight, Field of type number
-- @realm shared
-- @number GraphicsHeight

---
-- VSyncEnabled, Field of type bool
-- @realm shared
-- @bool VSyncEnabled

---
-- TextureCompressionEnabled, Field of type bool
-- @realm shared
-- @bool TextureCompressionEnabled

---
-- EnableSplashScreen, Field of type bool
-- @realm shared
-- @bool EnableSplashScreen

---
-- ParticleLimit, Field of type number
-- @realm shared
-- @number ParticleLimit

---
-- LightMapScale, Field of type number
-- @realm shared
-- @number LightMapScale

---
-- ChromaticAberrationEnabled, Field of type bool
-- @realm shared
-- @bool ChromaticAberrationEnabled

---
-- PauseOnFocusLost, Field of type bool
-- @realm shared
-- @bool PauseOnFocusLost

---
-- MuteOnFocusLost, Field of type bool
-- @realm shared
-- @bool MuteOnFocusLost

---
-- DynamicRangeCompressionEnabled, Field of type bool
-- @realm shared
-- @bool DynamicRangeCompressionEnabled

---
-- VoipAttenuationEnabled, Field of type bool
-- @realm shared
-- @bool VoipAttenuationEnabled

---
-- UseDirectionalVoiceChat, Field of type bool
-- @realm shared
-- @bool UseDirectionalVoiceChat

---
-- DisableVoiceChatFilters, Field of type bool
-- @realm shared
-- @bool DisableVoiceChatFilters

---
-- AudioOutputDevice, Field of type string
-- @realm shared
-- @string AudioOutputDevice

---
-- VoiceSetting, Field of type VoiceMode
-- @realm shared
-- @VoiceMode VoiceSetting

---
-- VoiceCaptureDevice, Field of type string
-- @realm shared
-- @string VoiceCaptureDevice

---
-- NoiseGateThreshold, Field of type number
-- @realm shared
-- @number NoiseGateThreshold

---
-- UseLocalVoiceByDefault, Field of type bool
-- @realm shared
-- @bool UseLocalVoiceByDefault

---
-- RequireSteamAuthentication, Field of type bool
-- @realm shared
-- @bool RequireSteamAuthentication

---
-- UseSteamMatchmaking, Field of type bool
-- @realm shared
-- @bool UseSteamMatchmaking

---
-- UseDualModeSockets, Field of type bool
-- @realm shared
-- @bool UseDualModeSockets

---
-- WindowMode, Field of type WindowMode
-- @realm shared
-- @WindowMode WindowMode

---
-- JobPreferences, Field of type table
-- @realm shared
-- @table JobPreferences

---
-- TeamPreference, Field of type CharacterTeamType
-- @realm shared
-- @CharacterTeamType TeamPreference

---
-- AimAssistAmount, Field of type number
-- @realm shared
-- @number AimAssistAmount

---
-- EnableMouseLook, Field of type bool
-- @realm shared
-- @bool EnableMouseLook

---
-- EnableRadialDistortion, Field of type bool
-- @realm shared
-- @bool EnableRadialDistortion

---
-- CrewMenuOpen, Field of type bool
-- @realm shared
-- @bool CrewMenuOpen

---
-- ChatOpen, Field of type bool
-- @realm shared
-- @bool ChatOpen

---
-- CorpseDespawnDelay, Field of type number
-- @realm shared
-- @number CorpseDespawnDelay

---
-- CorpsesPerSubDespawnThreshold, Field of type number
-- @realm shared
-- @number CorpsesPerSubDespawnThreshold

---
-- UnsavedSettings, Field of type bool
-- @realm shared
-- @bool UnsavedSettings

---
-- SoundVolume, Field of type number
-- @realm shared
-- @number SoundVolume

---
-- MusicVolume, Field of type number
-- @realm shared
-- @number MusicVolume

---
-- VoiceChatVolume, Field of type number
-- @realm shared
-- @number VoiceChatVolume

---
-- VoiceChatCutoffPrevention, Field of type number
-- @realm shared
-- @number VoiceChatCutoffPrevention

---
-- MicrophoneVolume, Field of type number
-- @realm shared
-- @number MicrophoneVolume

---
-- Language, Field of type string
-- @realm shared
-- @string Language

---
-- CurrentCorePackage, Field of type ContentPackage
-- @realm shared
-- @ContentPackage CurrentCorePackage

---
-- EnabledRegularPackages, Field of type IReadOnlyList`1
-- @realm shared
-- @IReadOnlyList`1 EnabledRegularPackages

---
-- AllEnabledPackages, Field of type Enumerable
-- @realm shared
-- @Enumerable AllEnabledPackages

---
-- ContentPackageSelectionDirtyNotification, Field of type bool
-- @realm shared
-- @bool ContentPackageSelectionDirtyNotification

---
-- ContentPackageSelectionDirty, Field of type bool
-- @realm shared
-- @bool ContentPackageSelectionDirty

---
-- ServerFilterElement, Field of type XElement
-- @realm shared
-- @XElement ServerFilterElement

---
-- DisableInGameHints, Field of type bool
-- @realm shared
-- @bool DisableInGameHints

---
-- AutomaticQuickStartEnabled, Field of type bool
-- @realm shared
-- @bool AutomaticQuickStartEnabled

---
-- AutomaticCampaignLoadEnabled, Field of type bool
-- @realm shared
-- @bool AutomaticCampaignLoadEnabled

---
-- TextManagerDebugModeEnabled, Field of type bool
-- @realm shared
-- @bool TextManagerDebugModeEnabled

---
-- TestScreenEnabled, Field of type bool
-- @realm shared
-- @bool TestScreenEnabled

---
-- ModBreakerMode, Field of type bool
-- @realm shared
-- @bool ModBreakerMode

---
-- MasterServerUrl, Field of type string
-- @realm shared
-- @string MasterServerUrl

---
-- RemoteContentUrl, Field of type string
-- @realm shared
-- @string RemoteContentUrl

---
-- AutoCheckUpdates, Field of type bool
-- @realm shared
-- @bool AutoCheckUpdates

---
-- PlayerName, Field of type string
-- @realm shared
-- @string PlayerName

---
-- LosMode, Field of type LosMode
-- @realm shared
-- @LosMode LosMode

---
-- GameSettings.HUDScale, Field of type number
-- @realm shared
-- @number GameSettings.HUDScale

---
-- GameSettings.InventoryScale, Field of type number
-- @realm shared
-- @number GameSettings.InventoryScale

---
-- GameSettings.TextScale, Field of type number
-- @realm shared
-- @number GameSettings.TextScale

---
-- CompletedTutorialNames, Field of type table
-- @realm shared
-- @table CompletedTutorialNames

---
-- IgnoredHints, Field of type HashSet`1
-- @realm shared
-- @HashSet`1 IgnoredHints

---
-- EncounteredCreatures, Field of type HashSet`1
-- @realm shared
-- @HashSet`1 EncounteredCreatures

---
-- KilledCreatures, Field of type HashSet`1
-- @realm shared
-- @HashSet`1 KilledCreatures

---
-- GameSettings.VerboseLogging, Field of type bool
-- @realm shared
-- @bool GameSettings.VerboseLogging

---
-- GameSettings.SaveDebugConsoleLogs, Field of type bool
-- @realm shared
-- @bool GameSettings.SaveDebugConsoleLogs

---
-- ShowLanguageSelectionPrompt, Field of type bool
-- @realm shared
-- @bool ShowLanguageSelectionPrompt

---
-- GameSettings.ShowOffensiveServerPrompt, Field of type bool
-- @realm shared
-- @bool GameSettings.ShowOffensiveServerPrompt

---
-- GameSettings.EnableSubmarineAutoSave, Field of type bool
-- @realm shared
-- @bool GameSettings.EnableSubmarineAutoSave

---
-- GameSettings.MaximumAutoSaves, Field of type number
-- @realm shared
-- @number GameSettings.MaximumAutoSaves

---
-- GameSettings.AutoSaveIntervalSeconds, Field of type number
-- @realm shared
-- @number GameSettings.AutoSaveIntervalSeconds

---
-- GameSettings.SubEditorBackgroundColor, Field of type Color
-- @realm shared
-- @Color GameSettings.SubEditorBackgroundColor

---
-- GameSettings.SubEditorMaxUndoBuffer, Field of type number
-- @realm shared
-- @number GameSettings.SubEditorMaxUndoBuffer

---
-- ShowTutorialSkipWarning, Field of type bool
-- @realm shared
-- @bool ShowTutorialSkipWarning

---
-- AudioDeviceNames, Field of type IList`1
-- @realm shared
-- @IList`1 AudioDeviceNames

---
-- CaptureDeviceNames, Field of type IList`1
-- @realm shared
-- @IList`1 CaptureDeviceNames

---
-- jobPreferences, Field of type table
-- @realm shared
-- @table jobPreferences

---
-- QuickStartSubmarineName, Field of type string
-- @realm shared
-- @string QuickStartSubmarineName

---
-- AutoUpdateWorkshopItems, Field of type bool
-- @realm shared
-- @bool AutoUpdateWorkshopItems

---
-- SuppressModFolderWatcher, Field of type bool
-- @realm shared
-- @bool SuppressModFolderWatcher

---
-- WaitingForAutoUpdate, Field of type bool
-- @realm shared
-- @bool WaitingForAutoUpdate

---
-- RecentlyEncounteredCreatures, Field of type HashSet`1
-- @realm shared
-- @HashSet`1 RecentlyEncounteredCreatures

---
-- CampaignDisclaimerShown, Field of type bool
-- @realm shared
-- @bool CampaignDisclaimerShown

---
-- EditorDisclaimerShown, Field of type bool
-- @realm shared
-- @bool EditorDisclaimerShown

---
-- GameSettings.SavePath, Field of type string
-- @realm shared
-- @string GameSettings.SavePath

---
-- GameSettings.PlayerSavePath, Field of type string
-- @realm shared
-- @string GameSettings.PlayerSavePath

---
-- GameSettings.VanillaContentPackagePath, Field of type string
-- @realm shared
-- @string GameSettings.VanillaContentPackagePath

---
-- GameSettings.MaxMicrophoneVolume, Field of type number
-- @realm shared
-- @number GameSettings.MaxMicrophoneVolume

