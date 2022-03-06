-- luacheck: ignore 111

--[[--
Barotrauma CharacterInfo class with some additional functions and fields

Barotrauma source code: [CharacterInfo.cs](https://github.com/evilfactory/Barotrauma-lua-attempt/blob/master/Barotrauma/BarotraumaShared/SharedSource/Characters/CharacterInfo.cs)
]]
-- @code CharacterInfo
-- @pragma nostrip

local CharacterInfo = {}

--- RecreateHead
-- @realm shared
-- @tparam HeadInfo headInfo
function RecreateHead(headInfo) end

--- RecreateHead
-- @realm shared
-- @tparam number headID
-- @tparam Race race
-- @tparam Gender gender
-- @tparam number hairIndex
-- @tparam number beardIndex
-- @tparam number moustacheIndex
-- @tparam number faceAttachmentIndex
function RecreateHead(headID, race, gender, hairIndex, beardIndex, moustacheIndex, faceAttachmentIndex) end

--- RefreshHead
-- @realm shared
function RefreshHead() end

--- LoadHeadAttachments
-- @realm shared
function LoadHeadAttachments() end

--- AddEmpty
-- @realm shared
-- @tparam Enumerable elements
-- @tparam WearableType type
-- @tparam number commonness
-- @treturn table
function CharacterInfo.AddEmpty(elements, type, commonness) end

--- GetRandomElement
-- @realm shared
-- @tparam Enumerable elements
-- @treturn XElement
function GetRandomElement(elements) end

--- IsValidIndex
-- @realm shared
-- @tparam number index
-- @tparam table list
-- @treturn bool
function CharacterInfo.IsValidIndex(index, list) end

--- IncreaseSkillLevel
-- @realm shared
-- @tparam string skillIdentifier
-- @tparam number increase
-- @tparam bool gainedFromAbility
function IncreaseSkillLevel(skillIdentifier, increase, gainedFromAbility) end

--- SetSkillLevel
-- @realm shared
-- @tparam string skillIdentifier
-- @tparam number level
function SetSkillLevel(skillIdentifier, level) end

--- GiveExperience
-- @realm shared
-- @tparam number amount
-- @tparam bool isMissionExperience
function GiveExperience(amount, isMissionExperience) end

--- SetExperience
-- @realm shared
-- @tparam number newExperience
function SetExperience(newExperience) end

--- GetTotalTalentPoints
-- @realm shared
-- @treturn number
function GetTotalTalentPoints() end

--- GetAvailableTalentPoints
-- @realm shared
-- @treturn number
function GetAvailableTalentPoints() end

--- GetProgressTowardsNextLevel
-- @realm shared
-- @treturn number
function GetProgressTowardsNextLevel() end

--- GetExperienceRequiredForCurrentLevel
-- @realm shared
-- @treturn number
function GetExperienceRequiredForCurrentLevel() end

--- GetExperienceRequiredToLevelUp
-- @realm shared
-- @treturn number
function GetExperienceRequiredToLevelUp() end

--- GetCurrentLevel
-- @realm shared
-- @treturn number
function GetCurrentLevel() end

--- Rename
-- @realm shared
-- @tparam string newName
function Rename(newName) end

--- ResetName
-- @realm shared
function ResetName() end

--- Save
-- @realm shared
-- @tparam XElement parentElement
-- @treturn XElement
function Save(parentElement) end

--- SaveOrders
-- @realm shared
-- @tparam XElement parentElement
-- @tparam OrderInfo[] orders
function CharacterInfo.SaveOrders(parentElement, orders) end

--- SaveOrderData
-- @realm shared
-- @tparam CharacterInfo characterInfo
-- @tparam XElement parentElement
function CharacterInfo.SaveOrderData(characterInfo, parentElement) end

--- SaveOrderData
-- @realm shared
function SaveOrderData() end

--- ApplyOrderData
-- @realm shared
-- @tparam Character character
-- @tparam XElement orderData
function CharacterInfo.ApplyOrderData(character, orderData) end

--- ApplyOrderData
-- @realm shared
function ApplyOrderData() end

--- LoadOrders
-- @realm shared
-- @tparam XElement ordersElement
-- @treturn table
function CharacterInfo.LoadOrders(ordersElement) end

--- ApplyHealthData
-- @realm shared
-- @tparam Character character
-- @tparam XElement healthData
function CharacterInfo.ApplyHealthData(character, healthData) end

--- ClearCurrentOrders
-- @realm shared
function ClearCurrentOrders() end

--- Remove
-- @realm shared
function Remove() end

--- ClearSavedStatValues
-- @realm shared
function ClearSavedStatValues() end

--- ClearSavedStatValues
-- @realm shared
-- @tparam StatTypes statType
function ClearSavedStatValues(statType) end

--- RemoveSavedStatValuesOnDeath
-- @realm shared
function RemoveSavedStatValuesOnDeath() end

--- ResetSavedStatValue
-- @realm shared
-- @tparam string statIdentifier
function ResetSavedStatValue(statIdentifier) end

--- GetSavedStatValue
-- @realm shared
-- @tparam StatTypes statType
-- @treturn number
function GetSavedStatValue(statType) end

--- GetSavedStatValue
-- @realm shared
-- @tparam StatTypes statType
-- @tparam string statIdentifier
-- @treturn number
function GetSavedStatValue(statType, statIdentifier) end

--- ChangeSavedStatValue
-- @realm shared
-- @tparam StatTypes statType
-- @tparam number value
-- @tparam string statIdentifier
-- @tparam bool removeOnDeath
-- @tparam number maxValue
-- @tparam bool setValue
function ChangeSavedStatValue(statType, value, statIdentifier, removeOnDeath, maxValue, setValue) end

--- Create
-- @realm shared
-- @tparam string speciesName
-- @tparam string name
-- @tparam JobPrefab jobPrefab
-- @tparam string ragdollFileName
-- @tparam number variant
-- @tparam RandSync randSync
-- @tparam string npcIdentifier
-- @treturn CharacterInfo
function CharacterInfo.Create(speciesName, name, jobPrefab, ragdollFileName, variant, randSync, npcIdentifier) end

--- ServerWrite
-- @realm shared
-- @tparam IWriteMessage msg
function ServerWrite(msg) end

--- GetUnlockedTalentsInTree
-- @realm shared
-- @treturn Enumerable
function GetUnlockedTalentsInTree() end

--- GetEndocrineTalents
-- @realm shared
-- @treturn Enumerable
function GetEndocrineTalents() end

--- CheckDisguiseStatus
-- @realm shared
-- @tparam bool handleBuff
-- @tparam IdCard idCard
function CheckDisguiseStatus(handleBuff, idCard) end

--- GetManualOrderPriority
-- @realm shared
-- @tparam Order order
-- @treturn number
function GetManualOrderPriority(order) end

--- GetRandomName
-- @realm shared
-- @tparam RandSync randSync
-- @treturn string
function GetRandomName(randSync) end

--- SelectRandomColor
-- @realm shared
-- @tparam ImmutableArray`1& array
-- @treturn Color
function CharacterInfo.SelectRandomColor(array) end

--- GetRandomGender
-- @realm shared
-- @tparam RandSync randSync
-- @treturn Gender
function GetRandomGender(randSync) end

--- GetRandomRace
-- @realm shared
-- @tparam RandSync randSync
-- @treturn Race
function GetRandomRace(randSync) end

--- GetRandomHeadID
-- @realm shared
-- @tparam RandSync randSync
-- @treturn number
function GetRandomHeadID(randSync) end

--- GetIdentifier
-- @realm shared
-- @treturn number
function GetIdentifier() end

--- GetIdentifierUsingOriginalName
-- @realm shared
-- @treturn number
function GetIdentifierUsingOriginalName() end

--- FilterByTypeAndHeadID
-- @realm shared
-- @tparam Enumerable elements
-- @tparam WearableType targetType
-- @tparam number headSpriteId
-- @treturn Enumerable
function FilterByTypeAndHeadID(elements, targetType, headSpriteId) end

--- FilterElementsByGenderAndRace
-- @realm shared
-- @tparam Enumerable elements
-- @tparam Gender gender
-- @tparam Race race
-- @treturn Enumerable
function FilterElementsByGenderAndRace(elements, gender, race) end

--- IsMatchingGender
-- @realm shared
-- @tparam Gender gender
-- @tparam Gender myGender
-- @treturn bool
function CharacterInfo.IsMatchingGender(gender, myGender) end

--- IsMatchingRace
-- @realm shared
-- @tparam Race race
-- @tparam Race myRace
-- @treturn bool
function CharacterInfo.IsMatchingRace(race, myRace) end

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
-- Head, Field of type HeadInfo
-- @realm shared
-- @HeadInfo Head

---
-- Heads, Field of type table
-- @realm shared
-- @table Heads

---
-- HasNickname, Field of type bool
-- @realm shared
-- @bool HasNickname

---
-- OriginalName, Field of type string
-- @realm shared
-- @string OriginalName

---
-- DisplayName, Field of type string
-- @realm shared
-- @string DisplayName

---
-- SpeciesName, Field of type string
-- @realm shared
-- @string SpeciesName

---
-- ExperiencePoints, Field of type number
-- @realm shared
-- @number ExperiencePoints

---
-- UnlockedTalents, Field of type HashSet`1
-- @realm shared
-- @HashSet`1 UnlockedTalents

---
-- AdditionalTalentPoints, Field of type number
-- @realm shared
-- @number AdditionalTalentPoints

---
-- HeadSprite, Field of type Sprite
-- @realm shared
-- @Sprite HeadSprite

---
-- Portrait, Field of type Sprite
-- @realm shared
-- @Sprite Portrait

---
-- AttachmentSprites, Field of type table
-- @realm shared
-- @table AttachmentSprites

---
-- CharacterConfigElement, Field of type XElement
-- @realm shared
-- @XElement CharacterConfigElement

---
-- CharacterInfo.HighestManualOrderPriority, Field of type number
-- @realm shared
-- @number CharacterInfo.HighestManualOrderPriority

---
-- CurrentOrders, Field of type table
-- @realm shared
-- @table CurrentOrders

---
-- SpriteTags, Field of type table
-- @realm shared
-- @table SpriteTags

---
-- PersonalityTrait, Field of type NPCPersonalityTrait
-- @realm shared
-- @NPCPersonalityTrait PersonalityTrait

---
-- HeadSpriteId, Field of type number
-- @realm shared
-- @number HeadSpriteId

---
-- Gender, Field of type Gender
-- @realm shared
-- @Gender Gender

---
-- Race, Field of type Race
-- @realm shared
-- @Race Race

---
-- HairIndex, Field of type number
-- @realm shared
-- @number HairIndex

---
-- BeardIndex, Field of type number
-- @realm shared
-- @number BeardIndex

---
-- MoustacheIndex, Field of type number
-- @realm shared
-- @number MoustacheIndex

---
-- FaceAttachmentIndex, Field of type number
-- @realm shared
-- @number FaceAttachmentIndex

---
-- HairColor, Field of type Color
-- @realm shared
-- @Color HairColor

---
-- FacialHairColor, Field of type Color
-- @realm shared
-- @Color FacialHairColor

---
-- SkinColor, Field of type Color
-- @realm shared
-- @Color SkinColor

---
-- HairElement, Field of type XElement
-- @realm shared
-- @XElement HairElement

---
-- BeardElement, Field of type XElement
-- @realm shared
-- @XElement BeardElement

---
-- MoustacheElement, Field of type XElement
-- @realm shared
-- @XElement MoustacheElement

---
-- FaceAttachment, Field of type XElement
-- @realm shared
-- @XElement FaceAttachment

---
-- Ragdoll, Field of type RagdollParams
-- @realm shared
-- @RagdollParams Ragdoll

---
-- IsAttachmentsLoaded, Field of type bool
-- @realm shared
-- @bool IsAttachmentsLoaded

---
-- Wearables, Field of type Enumerable
-- @realm shared
-- @Enumerable Wearables

---
-- InventoryData, Field of type XElement
-- @realm shared
-- @XElement InventoryData

---
-- HealthData, Field of type XElement
-- @realm shared
-- @XElement HealthData

---
-- OrderData, Field of type XElement
-- @realm shared
-- @XElement OrderData

---
-- Name, Field of type string
-- @realm shared
-- @string Name

---
-- Character, Field of type Character
-- @realm shared
-- @Character Character

---
-- Job, Field of type Job
-- @realm shared
-- @Job Job

---
-- Salary, Field of type number
-- @realm shared
-- @number Salary

---
-- OmitJobInPortraitClothing, Field of type bool
-- @realm shared
-- @bool OmitJobInPortraitClothing

---
-- IsDisguised, Field of type bool
-- @realm shared
-- @bool IsDisguised

---
-- IsDisguisedAsAnother, Field of type bool
-- @realm shared
-- @bool IsDisguisedAsAnother

---
-- ragdollFileName, Field of type string
-- @realm shared
-- @string ragdollFileName

---
-- StartItemsGiven, Field of type bool
-- @realm shared
-- @bool StartItemsGiven

---
-- IsNewHire, Field of type bool
-- @realm shared
-- @bool IsNewHire

---
-- CauseOfDeath, Field of type CauseOfDeath
-- @realm shared
-- @CauseOfDeath CauseOfDeath

---
-- TeamID, Field of type CharacterTeamType
-- @realm shared
-- @CharacterTeamType TeamID

---
-- ID, Field of type number
-- @realm shared
-- @number ID

---
-- HasGenders, Field of type bool
-- @realm shared
-- @bool HasGenders

---
-- HasRaces, Field of type bool
-- @realm shared
-- @bool HasRaces

---
-- HairColors, Field of type ImmutableArray`1
-- @realm shared
-- @ImmutableArray`1 HairColors

---
-- FacialHairColors, Field of type ImmutableArray`1
-- @realm shared
-- @ImmutableArray`1 FacialHairColors

---
-- SkinColors, Field of type ImmutableArray`1
-- @realm shared
-- @ImmutableArray`1 SkinColors

---
-- MissionsCompletedSinceDeath, Field of type number
-- @realm shared
-- @number MissionsCompletedSinceDeath

---
-- SavedStatValues, Field of type table
-- @realm shared
-- @table SavedStatValues

---
-- CharacterInfo.MaxCurrentOrders, Field of type number
-- @realm shared
-- @number CharacterInfo.MaxCurrentOrders

