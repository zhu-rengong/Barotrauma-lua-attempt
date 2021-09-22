-- luacheck: ignore 111

--[[--
Barotrauma CharacterInfo class with some additional functions and fields

Barotrauma source code: [CharacterInfo.cs](https://github.com/evilfactory/Barotrauma-lua-attempt/blob/master/Barotrauma/BarotraumaShared/SharedSource/Characters/CharacterInfo.cs)
]]
-- @code CharacterInfo
-- @pragma nostrip

local CharacterInfo = {}

--- Instantiates a new CharacterInfo.
-- @treturn CharacterInfo
-- @realm server 
-- @usage 
-- local vsauce = CharacterInfo.__new("human", "VSAUCE HERE")
-- local character = Character.Create(vsauce, Vector2.__new(0, 0), "some random characters")
-- print(character)
function CharacterInfo.__new(speciesName, name, jobPrefab, ragdollFileName, variant, randSync, npcIdentifier) end