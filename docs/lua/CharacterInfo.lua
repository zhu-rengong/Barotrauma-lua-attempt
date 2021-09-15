-- luacheck: ignore 111

--[[--
Barotrauma CharacterInfo class with some additional functions and fields

Barotrauma source code: [CharacterInfo.cs](https://github.com/evilfactory/Barotrauma-lua-attempt/blob/master/Barotrauma/BarotraumaShared/SharedSource/Characters/CharacterInfo.cs)
]]
-- @code CharacterInfo

--- Creates a CharacterInfo.
-- @treturn CharacterInfo
-- @realm server 
-- @usage 
-- local vsauce = CharacterInfo.Create("human", "VSAUCE HERE")
-- local character = Character.Create(vsauce, CreateVector2(0, 0), "some random characters")
-- print(character)
function Create(speciesName, name, jobPrefab, ragdollFileName, variant, randSync, npcIdentifier) end