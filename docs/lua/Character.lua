-- luacheck: ignore 111

--[[--
Barotrauma Character class with some additional functions and fields

Barotrauma source code: [Character.cs](https://github.com/evilfactory/Barotrauma-lua-attempt/blob/master/Barotrauma/BarotraumaShared/SharedSource/Characters/Character.cs)
]]
-- @code Character
-- @pragma nostrip

local Character = {}

--- Creates a Character using CharacterInfo.
-- @treturn Character
-- @realm server 
-- @usage 
-- local vsauce = CharacterInfo.Create("human", "VSAUCE HERE")
-- local character = Character.Create(vsauce, CreateVector2(0, 0), "some random characters")
-- print(character)
function Character.Create(characterInfo, position, seed, id, isRemotePlayer, hasAi, ragdollParams) end


--- List of all characters.
-- @treturn table
-- @realm shared 
Character.CharacterList = {}