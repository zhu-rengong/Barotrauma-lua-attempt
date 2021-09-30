-- luacheck: ignore 111

--[[--
Barotrauma Job class with some additional functions and fields

Barotrauma source code: [Job.cs](https://github.com/evilfactory/Barotrauma-lua-attempt/blob/master/Barotrauma/BarotraumaShared/SharedSource/Characters/Jobs/Job.cs)
]]
-- @code Job
-- @pragma nostrip

local Job = {}

--- Random
-- @realm shared
-- @tparam RandSync randSync
-- @treturn Job
function Job.Random(randSync) end

--- GetSkillLevel
-- @realm shared
-- @tparam string skillIdentifier
-- @treturn number
function GetSkillLevel(skillIdentifier) end

--- IncreaseSkillLevel
-- @realm shared
-- @tparam string skillIdentifier
-- @tparam number increase
function IncreaseSkillLevel(skillIdentifier, increase) end

--- GiveJobItems
-- @realm shared
-- @tparam Character character
-- @tparam WayPoint spawnPoint
function GiveJobItems(character, spawnPoint) end

--- Save
-- @realm shared
-- @tparam XElement parentElement
-- @treturn XElement
function Save(parentElement) end

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
-- Description, Field of type string
-- @realm shared
-- @string Description

---
-- Prefab, Field of type JobPrefab
-- @realm shared
-- @JobPrefab Prefab

---
-- Skills, Field of type table
-- @realm shared
-- @table Skills

---
-- PrimarySkill, Field of type Skill
-- @realm shared
-- @Skill PrimarySkill

---
-- Variant, Field of type number
-- @realm shared
-- @number Variant

