local Register = LuaSetup.Register
local RegisterBarotrauma = LuaSetup.RegisterBarotrauma

Register("System.TimeSpan")
Register("System.Exception")

RegisterBarotrauma("LuaByte")
RegisterBarotrauma("LuaUShort")
RegisterBarotrauma("LuaFloat")

RegisterBarotrauma("Level+InterestingPosition")

RegisterBarotrauma("RichString")
RegisterBarotrauma("Identifier")
RegisterBarotrauma("LanguageIdentifier")

RegisterBarotrauma("Job")
RegisterBarotrauma("JobPrefab")
RegisterBarotrauma("Level")
RegisterBarotrauma("WayPoint")

RegisterBarotrauma("Character")
RegisterBarotrauma("CharacterPrefab")
RegisterBarotrauma("CharacterInfo")
RegisterBarotrauma("CharacterInfoPrefab")
RegisterBarotrauma("CharacterHealth")
RegisterBarotrauma("CharacterHealth+LimbHealth")
RegisterBarotrauma("CharacterInventory")
RegisterBarotrauma("CharacterParams")
RegisterBarotrauma("CharacterParams+AIParams")
RegisterBarotrauma("CharacterParams+TargetParams")
RegisterBarotrauma("CharacterParams+InventoryParams")
RegisterBarotrauma("CharacterParams+HealthParams")
RegisterBarotrauma("CharacterParams+ParticleParams")
RegisterBarotrauma("CharacterParams+SoundParams")

RegisterBarotrauma("Item")
RegisterBarotrauma("DeconstructItem")
RegisterBarotrauma("FabricationRecipe")
RegisterBarotrauma("PreferredContainer")
RegisterBarotrauma("SwappableItem")
RegisterBarotrauma("FabricationRecipe+RequiredItemByIdentifier")
RegisterBarotrauma("FabricationRecipe+RequiredItemByTag")

RegisterBarotrauma("Submarine")
RegisterBarotrauma("INetSerializableStruct")
RegisterBarotrauma("Networking.Client")
RegisterBarotrauma("Networking.TempClient")
RegisterBarotrauma("Networking.NetworkConnection")
RegisterBarotrauma("Networking.LidgrenConnection")
RegisterBarotrauma("Networking.SteamP2PConnection")
RegisterBarotrauma("AfflictionPrefab")
RegisterBarotrauma("Affliction")
RegisterBarotrauma("AnimController")
RegisterBarotrauma("Limb")
RegisterBarotrauma("Ragdoll")
RegisterBarotrauma("RagdollParams")
RegisterBarotrauma("Networking.ChatMessage")
RegisterBarotrauma("AttackResult")
RegisterBarotrauma("Attack")
RegisterBarotrauma("Entity")
RegisterBarotrauma("EntitySpawner")
RegisterBarotrauma("MapEntity")
RegisterBarotrauma("MapEntityPrefab")
RegisterBarotrauma("CauseOfDeath")
RegisterBarotrauma("Hull")
RegisterBarotrauma("Structure")
RegisterBarotrauma("Gap")
RegisterBarotrauma("PhysicsBody")
RegisterBarotrauma("AbilityFlags")
RegisterBarotrauma("ItemPrefab")
RegisterBarotrauma("SerializableProperty")
RegisterBarotrauma("InputType")

RegisterBarotrauma("FireSource")

RegisterBarotrauma("StatusEffect")

RegisterBarotrauma("ContentPackageManager")
RegisterBarotrauma("ContentPackageManager+PackageSource")
RegisterBarotrauma("ContentPackageManager+EnabledPackages")
RegisterBarotrauma("ContentPackage")
RegisterBarotrauma("RegularPackage")
RegisterBarotrauma("CorePackage")
RegisterBarotrauma("ContentXElement")

RegisterBarotrauma("SubmarineBody")
RegisterBarotrauma("Explosion")
RegisterBarotrauma("Networking.ServerSettings")
RegisterBarotrauma("Networking.ServerSettings+SavedClientPermission")
RegisterBarotrauma("Inventory")
RegisterBarotrauma("ItemInventory")
RegisterBarotrauma("Inventory+ItemSlot")
RegisterBarotrauma("FireSource")

local componentsToRegister = { "DockingPort", "Door", "GeneticMaterial", "Growable", "Holdable", "LevelResource", "ItemComponent", "ItemLabel", "LightComponent", "Controller", "Deconstructor", "Engine", "Fabricator", "OutpostTerminal", "Pump", "Reactor", "Steering", "PowerContainer", "Projectile", "Repairable", "Rope", "Scanner", "ButtonTerminal", "ConnectionPanel", "CustomInterface", "MemoryComponent", "Terminal", "WifiComponent", "Wire", "TriggerComponent", "ElectricalDischarger", "EntitySpawnerComponent", "ProducedItem", "VineTile", "GrowthSideExtension", "IdCard", "MeleeWeapon", "Pickable", "AbilityItemPickingTime", "Propulsion", "RangedWeapon", "AbilityRangedWeapon", "RepairTool", "Sprayer", "Throwable", "ItemContainer", "AbilityItemContainer", "Ladder", "LimbPos", "AbilityDeconstructedItem", "AbilityItemCreationMultiplier", "AbilityItemDeconstructedInventory", "MiniMap", "OxygenGenerator", "Sonar", "SonarTransducer", "Vent", "NameTag", "Planter", "Powered", "PowerTransfer", "Quality", "RemoteController", "AdderComponent", "AndComponent", "ArithmeticComponent", "ColorComponent", "ConcatComponent", "Connection", "DelayComponent", "DivideComponent", "EqualsComponent", "ExponentiationComponent", "FunctionComponent", "GreaterComponent", "ModuloComponent", "MotionSensor", "MultiplyComponent", "NotComponent", "OrComponent", "OscillatorComponent", "OxygenDetector", "RegExFindComponent", "RelayComponent", "SignalCheckComponent", "SmokeDetector", "StringComponent", "SubtractComponent", "TrigonometricFunctionComponent", "WaterDetector", "XorComponent", "StatusHUD", "Turret", "Wearable",
"GridInfo", "PowerSourceGroup"
}

for key, value in pairs(componentsToRegister) do
    RegisterBarotrauma("Items.Components." .. value)
end

LuaUserData.MakeFieldAccessible(RegisterBarotrauma("Items.Components.CustomInterface"), "customInterfaceElementList")
RegisterBarotrauma("Items.Components.CustomInterface+CustomInterfaceElement")


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
RegisterBarotrauma("AIObjectivePumpWater")
RegisterBarotrauma("AIObjectiveRepairItem")
RegisterBarotrauma("AIObjectiveRepairItems")
RegisterBarotrauma("AIObjectiveRescue")
RegisterBarotrauma("AIObjectiveRescueAll")
RegisterBarotrauma("AIObjectiveReturn")

RegisterBarotrauma("Order")
RegisterBarotrauma("OrderPrefab")
RegisterBarotrauma("OrderTarget")

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
RegisterBarotrauma("Wallet")

RegisterBarotrauma("Faction")
RegisterBarotrauma("FactionPrefab")

RegisterBarotrauma("Location")
RegisterBarotrauma("LocationConnection")
RegisterBarotrauma("LocationType")
RegisterBarotrauma("LocationTypeChange")

RegisterBarotrauma("DebugConsole")
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
RegisterBarotrauma("SkillSettings")

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
RegisterBarotrauma("PrefabCollection`1[[Barotrauma.TalentTree]]")
RegisterBarotrauma("PrefabCollection`1[[Barotrauma.OrderPrefab]]")

RegisterBarotrauma("PrefabSelector`1[[Barotrauma.SkillSettings]]")

RegisterBarotrauma("Pair`2[[Barotrauma.JobPrefab],[System.Int32]]")

RegisterBarotrauma("Range`1[System.Single]")

RegisterBarotrauma("Items.Components.Signal")
RegisterBarotrauma("SubmarineInfo")

RegisterBarotrauma("MapCreatures.Behavior.BallastFloraBehavior")
RegisterBarotrauma("MapCreatures.Behavior.BallastFloraBranch")

RegisterBarotrauma("PetBehavior")

Register("Microsoft.Xna.Framework.Vector2")
Register("Microsoft.Xna.Framework.Vector3")
Register("Microsoft.Xna.Framework.Vector4")
Register("Microsoft.Xna.Framework.Color")
Register("Microsoft.Xna.Framework.Point")
Register("Microsoft.Xna.Framework.Rectangle")
Register("Microsoft.Xna.Framework.Matrix")

local friend = Register("Steamworks.Friend")

LuaUserData.RemoveMember(friend, "InviteToGame")
LuaUserData.RemoveMember(friend, "SendMessage")

local workshopItem = Register("Steamworks.Ugc.Item")

LuaUserData.RemoveMember(workshopItem, "Subscribe")
LuaUserData.RemoveMember(workshopItem, "DownloadAsync")
LuaUserData.RemoveMember(workshopItem, "Unsubscribe")
LuaUserData.RemoveMember(workshopItem, "AddFavorite")
LuaUserData.RemoveMember(workshopItem, "RemoveFavorite")
LuaUserData.RemoveMember(workshopItem, "Vote")
LuaUserData.RemoveMember(workshopItem, "GetUserVote")
LuaUserData.RemoveMember(workshopItem, "Edit")