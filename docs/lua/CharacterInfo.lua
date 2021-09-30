-- luacheck: ignore 111

--[[--
Barotrauma CharacterInfo class with some additional functions and fields

Barotrauma source code: [CharacterInfo.cs](https://github.com/evilfactory/Barotrauma-lua-attempt/blob/master/Barotrauma/BarotraumaShared/SharedSource/Characters/CharacterInfo.cs)
]]
-- @code CharacterInfo
-- @pragma nostrip

local CharacterInfo = {}

--- ApplyHealthData
-- @realm shared
-- @tparam Character character
-- @tparam XElement healthData
function CharacterInfo.ApplyHealthData(character, healthData) end

--- ReloadHeadAttachments
-- @realm shared
function ReloadHeadAttachments() end

--- ResetHeadAttachments
-- @realm shared
function ResetHeadAttachments() end

--- ClearCurrentOrders
-- @realm shared
function ClearCurrentOrders() end

--- Remove
-- @realm shared
function Remove() end

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
-- @usage 
-- local vsauce = CharacterInfo("human", "VSAUCE HERE")
-- local character = Character.Create(vsauce, Vector2(0, 0), "some random characters")
-- print(character)
function CharacterInfo(speciesName, name, jobPrefab, ragdollFileName, variant, randSync, npcIdentifier) end

--- ServerWrite
-- @realm shared
-- @tparam IWriteMessage msg
function ServerWrite(msg) end

--- CheckDisguiseStatus
-- @realm shared
-- @tparam bool handleBuff
-- @tparam IdCard idCard
function CheckDisguiseStatus(handleBuff, idCard) end

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
-- @tparam IEnumerable`1 elements
-- @tparam WearableType targetType
-- @tparam number headSpriteId
-- @treturn IEnumerable`1
function FilterByTypeAndHeadID(elements, targetType, headSpriteId) end

--- FilterElementsByGenderAndRace
-- @realm shared
-- @tparam IEnumerable`1 elements
-- @tparam Gender gender
-- @tparam Race race
-- @treturn IEnumerable`1
function FilterElementsByGenderAndRace(elements, gender, race) end

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

--- LoadHeadSprite
-- @realm shared
function LoadHeadSprite() end

--- LoadHeadAttachments
-- @realm shared
function LoadHeadAttachments() end

--- IncreaseSkillLevel
-- @realm shared
-- @tparam string skillIdentifier
-- @tparam number increase
-- @tparam Vector2 pos
function IncreaseSkillLevel(skillIdentifier, increase, pos) end

--- SetSkillLevel
-- @realm shared
-- @tparam string skillIdentifier
-- @tparam number level
-- @tparam Vector2 pos
function SetSkillLevel(skillIdentifier, level, pos) end

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
-- Wearables, Field of type IEnumerable`1
-- @realm shared
-- @IEnumerable`1 Wearables

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
-- CharacterInfo.MaxCurrentOrders, Field of type number
-- @realm shared
-- @number CharacterInfo.MaxCurrentOrders

