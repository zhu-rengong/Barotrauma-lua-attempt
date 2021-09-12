-- luacheck: ignore 111

--[[--
Barotrauma Character class with some additional functions and fields

Barotrauma source code: [Character.cs](https://github.com/evilfactory/Barotrauma-lua-attempt/blob/master/Barotrauma/BarotraumaShared/SharedSource/Characters/Character.cs)
]]
-- @code Character


--- Creates a CharacterInfo.
-- @treturn CharacterInfo
-- @realm server 
function Create(speciesName, name, jobPrefab, ragdollFileName, variant, randSync, npcIdentifier) end

--- List of all characters.
-- @treturn table
-- @realm shared 
CharacterList = {}