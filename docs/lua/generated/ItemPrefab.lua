-- luacheck: ignore 111

--[[--
Barotrauma ItemPrefab class with some additional functions and fields

Barotrauma source code: [ItemPrefab.cs](https://github.com/evilfactory/Barotrauma-lua-attempt/blob/master/Barotrauma/BarotraumaShared/SharedSource/Items/ItemPrefab.cs)
]]
-- @code ItemPrefab
-- @pragma nostrip

local ItemPrefab = {}

--- Add ItemPrefab to spawn queue and spawns it at the specified position
-- @tparam ItemPrefab itemPrefab
-- @tparam Vector2 position
-- @tparam function spawned
-- @realm server 
function ItemPrefab.AddToSpawnQueue(itemPrefab, position, spawned) end

--- Add ItemPrefab to spawn queue and spawns it inside the specified inventory
-- @tparam ItemPrefab itemPrefab
-- @tparam Inventory inventory
-- @tparam function spawned
-- @realm server 
function ItemPrefab.AddToSpawnQueue(itemPrefab, inventory, spawned) end

--- Get a item prefab via name or id
-- @tparam string itemNameOrId
-- @treturn ItemPrefab
-- @realm shared 
function ItemPrefab.GetItemPrefab(itemNameOrId) end

---
-- Identifier, the identifier of the prefab.
-- @realm shared
-- @string Identifier


--- RemoveByFile
-- @realm shared
-- @tparam string filePath
function ItemPrefab.RemoveByFile(filePath) end

--- LoadFromFile
-- @realm shared
-- @tparam ContentFile file
function ItemPrefab.LoadFromFile(file) end

--- LoadAll
-- @realm shared
-- @tparam IEnumerable`1 files
function ItemPrefab.LoadAll(files) end

--- InitFabricationRecipes
-- @realm shared
function ItemPrefab.InitFabricationRecipes() end

--- GenerateLegacyIdentifier
-- @realm shared
-- @tparam string name
-- @treturn string
function ItemPrefab.GenerateLegacyIdentifier(name) end

--- GetTreatmentSuitability
-- @realm shared
-- @tparam string treatmentIdentifier
-- @treturn number
function GetTreatmentSuitability(treatmentIdentifier) end

--- GetPriceInfo
-- @realm shared
-- @tparam Location location
-- @treturn PriceInfo
function GetPriceInfo(location) end

--- CanBeBoughtAtLocation
-- @realm shared
-- @tparam Location location
-- @tparam PriceInfo& priceInfo
-- @treturn bool
function CanBeBoughtAtLocation(location, priceInfo) end

--- Find
-- @realm shared
-- @tparam string name
-- @tparam string identifier
-- @treturn ItemPrefab
function ItemPrefab.Find(name, identifier) end

--- GetMinPrice
-- @realm shared
-- @treturn Nullable`1
function GetMinPrice() end

--- GetBuyPricesUnder
-- @realm shared
-- @tparam number maxCost
-- @treturn ImmutableDictionary`2
function GetBuyPricesUnder(maxCost) end

--- GetSellPricesOver
-- @realm shared
-- @tparam number minCost
-- @tparam bool sellingImportant
-- @treturn ImmutableDictionary`2
function GetSellPricesOver(minCost, sellingImportant) end

--- IsContainerPreferred
-- @realm shared
-- @tparam Item item
-- @tparam ItemContainer targetContainer
-- @tparam Boolean& isPreferencesDefined
-- @tparam Boolean& isSecondary
-- @treturn bool
function IsContainerPreferred(item, targetContainer, isPreferencesDefined, isSecondary) end

--- IsContainerPreferred
-- @realm shared
-- @tparam Item item
-- @tparam String[] identifiersOrTags
-- @tparam Boolean& isPreferencesDefined
-- @tparam Boolean& isSecondary
-- @treturn bool
function IsContainerPreferred(item, identifiersOrTags, isPreferencesDefined, isSecondary) end

--- IsContainerPreferred
-- @realm shared
-- @tparam IEnumerable`1 preferences
-- @tparam ItemContainer c
-- @treturn bool
function ItemPrefab.IsContainerPreferred(preferences, c) end

--- IsContainerPreferred
-- @realm shared
-- @tparam IEnumerable`1 preferences
-- @tparam IEnumerable`1 ids
-- @treturn bool
function ItemPrefab.IsContainerPreferred(preferences, ids) end

--- GetItemPrefab
-- @realm shared
-- @tparam string itemNameOrId
-- @treturn ItemPrefab
function ItemPrefab.GetItemPrefab(itemNameOrId) end

--- AddToSpawnQueue
-- @realm shared
-- @tparam ItemPrefab itemPrefab
-- @tparam Vector2 position
-- @tparam Object spawned
function ItemPrefab.AddToSpawnQueue(itemPrefab, position, spawned) end

--- AddToSpawnQueue
-- @realm shared
-- @tparam ItemPrefab itemPrefab
-- @tparam Inventory inventory
-- @tparam Object spawned
function ItemPrefab.AddToSpawnQueue(itemPrefab, inventory, spawned) end

--- Dispose
-- @realm shared
function Dispose() end

--- GetItemNameTextId
-- @realm shared
-- @treturn string
function GetItemNameTextId() end

--- GetHullNameTextId
-- @realm shared
-- @treturn string
function GetHullNameTextId() end

--- GetAllowedUpgrades
-- @realm shared
-- @treturn String[]
function GetAllowedUpgrades() end

--- HasSubCategory
-- @realm shared
-- @tparam string subcategory
-- @treturn bool
function HasSubCategory(subcategory) end

--- DebugCreateInstance
-- @realm shared
function DebugCreateInstance() end

--- NameMatches
-- @realm shared
-- @tparam string name
-- @tparam StringComparison comparisonType
-- @treturn bool
function NameMatches(name, comparisonType) end

--- NameMatches
-- @realm shared
-- @tparam IEnumerable`1 allowedNames
-- @tparam StringComparison comparisonType
-- @treturn bool
function NameMatches(allowedNames, comparisonType) end

--- IsLinkAllowed
-- @realm shared
-- @tparam MapEntityPrefab target
-- @treturn bool
function IsLinkAllowed(target) end

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
-- Name, Field of type string
-- @realm shared
-- @string Name

---
-- ConfigElement, Field of type XElement
-- @realm shared
-- @XElement ConfigElement

---
-- DeconstructItems, Field of type table
-- @realm shared
-- @table DeconstructItems

---
-- FabricationRecipes, Field of type table
-- @realm shared
-- @table FabricationRecipes

---
-- DeconstructTime, Field of type number
-- @realm shared
-- @number DeconstructTime

---
-- AllowDeconstruct, Field of type bool
-- @realm shared
-- @bool AllowDeconstruct

---
-- InteractDistance, Field of type number
-- @realm shared
-- @number InteractDistance

---
-- InteractPriority, Field of type number
-- @realm shared
-- @number InteractPriority

---
-- InteractThroughWalls, Field of type bool
-- @realm shared
-- @bool InteractThroughWalls

---
-- HideConditionBar, Field of type bool
-- @realm shared
-- @bool HideConditionBar

---
-- HideConditionInTooltip, Field of type bool
-- @realm shared
-- @bool HideConditionInTooltip

---
-- RequireBodyInsideTrigger, Field of type bool
-- @realm shared
-- @bool RequireBodyInsideTrigger

---
-- RequireCursorInsideTrigger, Field of type bool
-- @realm shared
-- @bool RequireCursorInsideTrigger

---
-- RequireCampaignInteract, Field of type bool
-- @realm shared
-- @bool RequireCampaignInteract

---
-- FocusOnSelected, Field of type bool
-- @realm shared
-- @bool FocusOnSelected

---
-- OffsetOnSelected, Field of type number
-- @realm shared
-- @number OffsetOnSelected

---
-- Health, Field of type number
-- @realm shared
-- @number Health

---
-- AllowSellingWhenBroken, Field of type bool
-- @realm shared
-- @bool AllowSellingWhenBroken

---
-- Indestructible, Field of type bool
-- @realm shared
-- @bool Indestructible

---
-- DamagedByExplosions, Field of type bool
-- @realm shared
-- @bool DamagedByExplosions

---
-- ExplosionDamageMultiplier, Field of type number
-- @realm shared
-- @number ExplosionDamageMultiplier

---
-- DamagedByProjectiles, Field of type bool
-- @realm shared
-- @bool DamagedByProjectiles

---
-- DamagedByMeleeWeapons, Field of type bool
-- @realm shared
-- @bool DamagedByMeleeWeapons

---
-- DamagedByRepairTools, Field of type bool
-- @realm shared
-- @bool DamagedByRepairTools

---
-- DamagedByMonsters, Field of type bool
-- @realm shared
-- @bool DamagedByMonsters

---
-- FireProof, Field of type bool
-- @realm shared
-- @bool FireProof

---
-- WaterProof, Field of type bool
-- @realm shared
-- @bool WaterProof

---
-- ImpactTolerance, Field of type number
-- @realm shared
-- @number ImpactTolerance

---
-- SonarSize, Field of type number
-- @realm shared
-- @number SonarSize

---
-- UseInHealthInterface, Field of type bool
-- @realm shared
-- @bool UseInHealthInterface

---
-- DisableItemUsageWhenSelected, Field of type bool
-- @realm shared
-- @bool DisableItemUsageWhenSelected

---
-- CargoContainerIdentifier, Field of type string
-- @realm shared
-- @string CargoContainerIdentifier

---
-- UseContainedSpriteColor, Field of type bool
-- @realm shared
-- @bool UseContainedSpriteColor

---
-- UseContainedInventoryIconColor, Field of type bool
-- @realm shared
-- @bool UseContainedInventoryIconColor

---
-- AddedRepairSpeedMultiplier, Field of type number
-- @realm shared
-- @number AddedRepairSpeedMultiplier

---
-- CannotRepairFail, Field of type bool
-- @realm shared
-- @bool CannotRepairFail

---
-- EquipConfirmationText, Field of type string
-- @realm shared
-- @string EquipConfirmationText

---
-- AllowRotatingInEditor, Field of type bool
-- @realm shared
-- @bool AllowRotatingInEditor

---
-- ShowContentsInTooltip, Field of type bool
-- @realm shared
-- @bool ShowContentsInTooltip

---
-- PreferredContainers, Field of type table
-- @realm shared
-- @table PreferredContainers

---
-- SwappableItem, Field of type SwappableItem
-- @realm shared
-- @SwappableItem SwappableItem

---
-- LevelCommonness, Field of type table
-- @realm shared
-- @table LevelCommonness

---
-- LevelQuantity, Field of type table
-- @realm shared
-- @table LevelQuantity

---
-- CanFlipX, Field of type bool
-- @realm shared
-- @bool CanFlipX

---
-- CanFlipY, Field of type bool
-- @realm shared
-- @bool CanFlipY

---
-- IsDangerous, Field of type bool
-- @realm shared
-- @bool IsDangerous

---
-- CanSpriteFlipX, Field of type bool
-- @realm shared
-- @bool CanSpriteFlipX

---
-- CanSpriteFlipY, Field of type bool
-- @realm shared
-- @bool CanSpriteFlipY

---
-- MaxStackSize, Field of type number
-- @realm shared
-- @number MaxStackSize

---
-- AllowDroppingOnSwap, Field of type bool
-- @realm shared
-- @bool AllowDroppingOnSwap

---
-- AllowDroppingOnSwapWith, Field of type IEnumerable`1
-- @realm shared
-- @IEnumerable`1 AllowDroppingOnSwapWith

---
-- Size, Field of type Vector2
-- @realm shared
-- @Vector2 Size

---
-- CanBeBought, Field of type bool
-- @realm shared
-- @bool CanBeBought

---
-- CanBeSold, Field of type bool
-- @realm shared
-- @bool CanBeSold

---
-- RandomDeconstructionOutput, Field of type bool
-- @realm shared
-- @bool RandomDeconstructionOutput

---
-- RandomDeconstructionOutputAmount, Field of type number
-- @realm shared
-- @number RandomDeconstructionOutputAmount

---
-- UIntIdentifier, Field of type number
-- @realm shared
-- @number UIntIdentifier

---
-- ResizeHorizontal, Field of type bool
-- @realm shared
-- @bool ResizeHorizontal

---
-- ResizeVertical, Field of type bool
-- @realm shared
-- @bool ResizeVertical

---
-- OriginalName, Field of type string
-- @realm shared
-- @string OriginalName

---
-- Identifier, Field of type string
-- @realm shared
-- @string Identifier

---
-- FilePath, Field of type string
-- @realm shared
-- @string FilePath

---
-- ContentPackage, Field of type ContentPackage
-- @realm shared
-- @ContentPackage ContentPackage

---
-- Tags, Field of type HashSet`1
-- @realm shared
-- @HashSet`1 Tags

---
-- Description, Field of type string
-- @realm shared
-- @string Description

---
-- AllowedUpgrades, Field of type string
-- @realm shared
-- @string AllowedUpgrades

---
-- HideInMenus, Field of type bool
-- @realm shared
-- @bool HideInMenus

---
-- Subcategory, Field of type string
-- @realm shared
-- @string Subcategory

---
-- Linkable, Field of type bool
-- @realm shared
-- @bool Linkable

---
-- AllowedLinks, Field of type table
-- @realm shared
-- @table AllowedLinks

---
-- Category, Field of type MapEntityCategory
-- @realm shared
-- @MapEntityCategory Category

---
-- SpriteColor, Field of type Color
-- @realm shared
-- @Color SpriteColor

---
-- Scale, Field of type number
-- @realm shared
-- @number Scale

---
-- Aliases, Field of type HashSet`1
-- @realm shared
-- @HashSet`1 Aliases

---
-- DefaultPrice, Field of type PriceInfo
-- @realm shared
-- @PriceInfo DefaultPrice

---
-- Triggers, Field of type table
-- @realm shared
-- @table Triggers

---
-- IsOverride, Field of type bool
-- @realm shared
-- @bool IsOverride

---
-- VariantOf, Field of type ItemPrefab
-- @realm shared
-- @ItemPrefab VariantOf

---
-- AllowAsExtraCargo, Field of type Nullable`1
-- @realm shared
-- @Nullable`1 AllowAsExtraCargo

---
-- ItemPrefab.Prefabs, Field of type PrefabCollection`1
-- @realm shared
-- @PrefabCollection`1 ItemPrefab.Prefabs

---
-- sprite, Field of type Sprite
-- @realm shared
-- @Sprite sprite

