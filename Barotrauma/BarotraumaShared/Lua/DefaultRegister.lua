local descriptors = {}

local function Register(typeName)
    local descriptor = LuaUserData.RegisterType(typeName)

    descriptors[typeName] = descriptor

    return descriptor
end

local function RegisterBarotrauma(typeName)
    return Register("Barotrauma." .. typeName)
end

Register("System.TimeSpan")

if SERVER then
    RegisterBarotrauma("Networking.GameServer")
end

RegisterBarotrauma("LuaByte")
RegisterBarotrauma("LuaUShort")
RegisterBarotrauma("LuaFloat")

RegisterBarotrauma("Level+InterestingPosition")

RegisterBarotrauma("Job")
RegisterBarotrauma("JobPrefab")
RegisterBarotrauma("Level")
RegisterBarotrauma("WayPoint")
RegisterBarotrauma("Character")
RegisterBarotrauma("Item")
RegisterBarotrauma("Submarine")
RegisterBarotrauma("Networking.Client")
RegisterBarotrauma("Networking.NetworkConnection")
RegisterBarotrauma("Networking.LidgrenConnection")
RegisterBarotrauma("Networking.SteamP2PConnection")
RegisterBarotrauma("AfflictionPrefab")
RegisterBarotrauma("Affliction")
RegisterBarotrauma("CharacterHealth")
RegisterBarotrauma("AnimController")
RegisterBarotrauma("Limb")
RegisterBarotrauma("Ragdoll")
RegisterBarotrauma("Networking.ChatMessage")
RegisterBarotrauma("CharacterHealth+LimbHealth")
RegisterBarotrauma("AttackResult")
RegisterBarotrauma("Attack")
RegisterBarotrauma("Entity")
RegisterBarotrauma("EntitySpawner")
RegisterBarotrauma("MapEntity")
RegisterBarotrauma("MapEntityPrefab")
RegisterBarotrauma("CauseOfDeath")
RegisterBarotrauma("Hull")
RegisterBarotrauma("Gap")
RegisterBarotrauma("PhysicsBody")
RegisterBarotrauma("AbilityFlags")
RegisterBarotrauma("ItemPrefab")
RegisterBarotrauma("SerializableProperty")

RegisterBarotrauma("StatusEffect")
RegisterBarotrauma("FireSource")
RegisterBarotrauma("ContentPackage")
RegisterBarotrauma("SubmarineBody")
RegisterBarotrauma("Explosion")
RegisterBarotrauma("Networking.ServerSettings")
RegisterBarotrauma("Networking.ServerSettings+SavedClientPermission")
RegisterBarotrauma("Inventory")
RegisterBarotrauma("ItemInventory")
RegisterBarotrauma("CharacterInventory")
RegisterBarotrauma("Inventory+ItemSlot")
RegisterBarotrauma("FireSource")

local barotraumaComponentsToRegister = { "DockingPort", "Door", "GeneticMaterial", "Growable", "Holdable", "LevelResource", "ItemComponent", "ItemLabel", "LightComponent", "Controller", "Deconstructor", "Engine", "Fabricator", "OutpostTerminal", "Pump", "Reactor", "Steering", "PowerContainer", "Projectile", "Repairable", "Rope", "Scanner", "ButtonTerminal", "ConnectionPanel", "CustomInterface", "MemoryComponent", "Terminal", "WifiComponent", "Wire", "TriggerComponent", "ElectricalDischarger", "EntitySpawnerComponent", "ProducedItem", "VineTile", "GrowthSideExtension", "IdCard", "MeleeWeapon", "Pickable", "AbilityItemPickingTime", "Propulsion", "RangedWeapon", "AbilityRangedWeapon", "RepairTool", "Sprayer", "Throwable", "ItemContainer", "AbilityItemContainer", "Ladder", "LimbPos", "AbilityDeconstructedItem", "AbilityItemCreationMultiplier", "AbilityItemDeconstructedInventory", "MiniMap", "OxygenGenerator", "Sonar", "SonarTransducer", "Vent", "NameTag", "Planter", "Powered", "PowerTransfer", "Quality", "RemoteController", "AdderComponent", "AndComponent", "ArithmeticComponent", "ColorComponent", "ConcatComponent", "Connection", "DelayComponent", "DivideComponent", "EqualsComponent", "ExponentiationComponent", "FunctionComponent", "GreaterComponent", "ModuloComponent", "MotionSensor", "MultiplyComponent", "NotComponent", "OrComponent", "OscillatorComponent", "OxygenDetector", "RegExFindComponent", "RelayComponent", "SignalCheckComponent", "SmokeDetector", "StringComponent", "SubtractComponent", "TrigonometricFunctionComponent", "WaterDetector", "XorComponent", "StatusHUD", "Turret", "Wearable", "CustomInterface", "CustomInterface+CustomInterfaceElement"
}

for key, value in pairs(barotraumaComponentsToRegister) do
    RegisterBarotrauma("Items.Components." .. value)
end

RegisterBarotrauma("AIController")
RegisterBarotrauma("EnemyAIController")
RegisterBarotrauma("HumanAIController")
RegisterBarotrauma("AICharacter")
RegisterBarotrauma("AITarget")
RegisterBarotrauma("AITargetMemory")
RegisterBarotrauma("AIChatMessage")
RegisterBarotrauma("AIObjectiveManager")
RegisterBarotrauma("AITrigger")
RegisterBarotrauma("WreckAI")
RegisterBarotrauma("WreckAIConfig")

RegisterBarotrauma("AIObjectiveChargeBatteries")
RegisterBarotrauma("AIObjective")
RegisterBarotrauma("AIObjectiveCleanupItem")
RegisterBarotrauma("AIObjectiveCleanupItems")
RegisterBarotrauma("AIObjectiveCombat")
RegisterBarotrauma("AIObjectiveContainItem")
RegisterBarotrauma("AIObjectiveDecontainItem")
RegisterBarotrauma("AIObjectiveEscapeHandcuffs")
RegisterBarotrauma("AIObjectiveExtinguishFire")
RegisterBarotrauma("AIObjectiveExtinguishFires")
RegisterBarotrauma("AIObjectiveFightIntruders")
RegisterBarotrauma("AIObjectiveFindDivingGear")
RegisterBarotrauma("AIObjectiveFindSafety")
RegisterBarotrauma("AIObjectiveFixLeak")
RegisterBarotrauma("AIObjectiveFixLeaks")
RegisterBarotrauma("AIObjectiveGetItem")
RegisterBarotrauma("AIObjectiveGoTo")
RegisterBarotrauma("AIObjectiveIdle")
RegisterBarotrauma("AIObjectiveOperateItem")
RegisterBarotrauma("AIObjectiveOperateItem")
RegisterBarotrauma("AIObjectivePumpWater")
RegisterBarotrauma("AIObjectiveRepairItem")
RegisterBarotrauma("AIObjectiveRepairItems")
RegisterBarotrauma("AIObjectiveRescue")
RegisterBarotrauma("AIObjectiveRescueAll")
RegisterBarotrauma("AIObjectiveReturn")

RegisterBarotrauma("TalentPrefab")
RegisterBarotrauma("TalentOption")
RegisterBarotrauma("TalentSubTree")
RegisterBarotrauma("TalentTree")
RegisterBarotrauma("CharacterTalent")
RegisterBarotrauma("Upgrade")
RegisterBarotrauma("UpgradeCategory")
RegisterBarotrauma("UpgradePrefab")
RegisterBarotrauma("UpgradeManager")

RegisterBarotrauma("Screen")
RegisterBarotrauma("GameScreen")
RegisterBarotrauma("GameSession")
RegisterBarotrauma("GameSettings")
RegisterBarotrauma("CrewManager")

RegisterBarotrauma("GameMode")
RegisterBarotrauma("MissionMode")
RegisterBarotrauma("PvPMode")
RegisterBarotrauma("Mission")
RegisterBarotrauma("CampaignMode")
RegisterBarotrauma("CoOpMode")

RegisterBarotrauma("CampaignMetadata")


RegisterBarotrauma("Faction")
RegisterBarotrauma("FactionPrefab")

RegisterBarotrauma("Location")
RegisterBarotrauma("LocationConnection")
RegisterBarotrauma("LocationType")
RegisterBarotrauma("LocationTypeChange")

RegisterBarotrauma("DebugConsole+Command")

RegisterBarotrauma("TextManager")

local descriptor = RegisterBarotrauma("NetLobbyScreen")

if SERVER then
    LuaUserData.MakeFieldAccessible(descriptor, "subs")
end

RegisterBarotrauma("Networking.IWriteMessage")
RegisterBarotrauma("Networking.IReadMessage")
RegisterBarotrauma("Networking.NetEntityEvent")
RegisterBarotrauma("Networking.INetSerializable")
Register("Lidgren.Network.NetIncomingMessage")
Register("Lidgren.Network.NetConnection")
Register("System.Net.IPEndPoint")
Register("System.Net.IPAddress")

RegisterBarotrauma("Skill")
RegisterBarotrauma("SkillPrefab")
RegisterBarotrauma("TraitorMissionPrefab")
RegisterBarotrauma("TraitorMissionResult")

Register("FarseerPhysics.Dynamics.Body")
Register("FarseerPhysics.Dynamics.World")
Register("FarseerPhysics.Dynamics.Fixture")
RegisterBarotrauma("Physics")

RegisterBarotrauma("Camera")
RegisterBarotrauma("Key")

RegisterBarotrauma("PrefabCollection`1[[Barotrauma.ItemPrefab]]")
RegisterBarotrauma("PrefabCollection`1[[Barotrauma.JobPrefab]]")
RegisterBarotrauma("PrefabCollection`1[[Barotrauma.CharacterPrefab]]")
RegisterBarotrauma("PrefabCollection`1[[Barotrauma.AfflictionPrefab]]")
RegisterBarotrauma("PrefabCollection`1[[Barotrauma.TalentPrefab]]")

RegisterBarotrauma("Pair`2[[Barotrauma.JobPrefab],[System.Int32]]")

RegisterBarotrauma("Range`1[System.Single]")

RegisterBarotrauma("CharacterInfo")
RegisterBarotrauma("Items.Components.Signal")
RegisterBarotrauma("SubmarineInfo")

RegisterBarotrauma("MapCreatures.Behavior.BallastFloraBehavior")
RegisterBarotrauma("MapCreatures.Behavior.BallastFloraBranch")

Register("Microsoft.Xna.Framework.Vector2")
Register("Microsoft.Xna.Framework.Vector3")
Register("Microsoft.Xna.Framework.Vector4")
Register("Microsoft.Xna.Framework.Color")
Register("Microsoft.Xna.Framework.Point")
Register("Microsoft.Xna.Framework.Rectangle")
Register("Microsoft.Xna.Framework.Matrix")

if SERVER then
RegisterBarotrauma("Networking.ServerPeer")
RegisterBarotrauma("Networking.ServerPeer+PendingClient")

RegisterBarotrauma("Traitor")
RegisterBarotrauma("Traitor+TraitorMission")

elseif CLIENT then

RegisterBarotrauma("Networking.ClientPeer")

RegisterBarotrauma("ChatBox")
RegisterBarotrauma("GUICanvas")
RegisterBarotrauma("Anchor")
RegisterBarotrauma("Alignment")
RegisterBarotrauma("Pivot")
RegisterBarotrauma("Key")
RegisterBarotrauma("PlayerInput")
RegisterBarotrauma("ScalableFont")

Register("Microsoft.Xna.Framework.Graphics.Texture2D")
Register("EventInput.KeyEventArgs")
Register("Microsoft.Xna.Framework.Input.Keys")

RegisterBarotrauma("Sprite")
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

end

return descriptors