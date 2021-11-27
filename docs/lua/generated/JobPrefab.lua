-- luacheck: ignore 111

--[[--
Barotrauma JobPrefab class with some additional functions and fields

Barotrauma source code: [JobPrefab.cs](https://github.com/evilfactory/Barotrauma-lua-attempt/blob/master/Barotrauma/BarotraumaShared/SharedSource/Characters/Jobs/JobPrefab.cs)
]]
-- @code JobPrefab
-- @pragma nostrip

local JobPrefab = {}

--- Dispose
-- @realm shared
function Dispose() end

--- Get
-- @realm shared
-- @tparam string identifier
-- @treturn JobPrefab
function JobPrefab.Get(identifier) end

--- Random
-- @realm shared
-- @tparam RandSync sync
-- @treturn JobPrefab
function JobPrefab.Random(sync) end

--- LoadAll
-- @realm shared
-- @tparam Enumerable files
function JobPrefab.LoadAll(files) end

--- LoadFromFile
-- @realm shared
-- @tparam ContentFile file
function JobPrefab.LoadFromFile(file) end

--- RemoveByFile
-- @realm shared
-- @tparam string filePath
function JobPrefab.RemoveByFile(filePath) end

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
-- JobPrefab.ItemRepairPriorities, Field of type IReadOnlyDictionary`2
-- @realm shared
-- @IReadOnlyDictionary`2 JobPrefab.ItemRepairPriorities

---
-- UIColor, Field of type Color
-- @realm shared
-- @Color UIColor

---
-- Identifier, Field of type string
-- @realm shared
-- @string Identifier

---
-- Name, Field of type string
-- @realm shared
-- @string Name

---
-- IdleBehavior, Field of type BehaviorType
-- @realm shared
-- @BehaviorType IdleBehavior

---
-- OriginalName, Field of type string
-- @realm shared
-- @string OriginalName

---
-- ContentPackage, Field of type ContentPackage
-- @realm shared
-- @ContentPackage ContentPackage

---
-- Description, Field of type string
-- @realm shared
-- @string Description

---
-- OnlyJobSpecificDialog, Field of type bool
-- @realm shared
-- @bool OnlyJobSpecificDialog

---
-- InitialCount, Field of type number
-- @realm shared
-- @number InitialCount

---
-- AllowAlways, Field of type bool
-- @realm shared
-- @bool AllowAlways

---
-- MaxNumber, Field of type number
-- @realm shared
-- @number MaxNumber

---
-- MinNumber, Field of type number
-- @realm shared
-- @number MinNumber

---
-- MinKarma, Field of type number
-- @realm shared
-- @number MinKarma

---
-- PriceMultiplier, Field of type number
-- @realm shared
-- @number PriceMultiplier

---
-- Commonness, Field of type number
-- @realm shared
-- @number Commonness

---
-- VitalityModifier, Field of type number
-- @realm shared
-- @number VitalityModifier

---
-- HiddenJob, Field of type bool
-- @realm shared
-- @bool HiddenJob

---
-- PrimarySkill, Field of type SkillPrefab
-- @realm shared
-- @SkillPrefab PrimarySkill

---
-- FilePath, Field of type string
-- @realm shared
-- @string FilePath

---
-- Element, Field of type XElement
-- @realm shared
-- @XElement Element

---
-- ClothingElement, Field of type XElement
-- @realm shared
-- @XElement ClothingElement

---
-- Variants, Field of type number
-- @realm shared
-- @number Variants

---
-- ItemSets, Field of type table
-- @realm shared
-- @table ItemSets

---
-- PreviewItems, Field of type table
-- @realm shared
-- @table PreviewItems

---
-- Skills, Field of type table
-- @realm shared
-- @table Skills

---
-- AutonomousObjectives, Field of type table
-- @realm shared
-- @table AutonomousObjectives

---
-- AppropriateOrders, Field of type table
-- @realm shared
-- @table AppropriateOrders

---
-- Icon, Field of type Sprite
-- @realm shared
-- @Sprite Icon

---
-- IconSmall, Field of type Sprite
-- @realm shared
-- @Sprite IconSmall

---
-- JobPrefab.Prefabs, Field of type PrefabCollection`1
-- @realm shared
-- @PrefabCollection`1 JobPrefab.Prefabs

---
-- JobPrefab.NoJobElement, Field of type XElement
-- @realm shared
-- @XElement JobPrefab.NoJobElement

