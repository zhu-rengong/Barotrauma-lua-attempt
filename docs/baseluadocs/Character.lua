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
-- local vsauce = CharacterInfo("human", "VSAUCE HERE")
-- local character = Character.Create(vsauce, Vector2(0, 0), "some random characters")
-- print(character)
function Character.Create(characterInfo, position, seed, id, isRemotePlayer, hasAi, ragdollParams) end


--- Teleports a character to a position.
-- @realm server 
-- @tparam Vector2 position
-- @usage 
-- Character.CharacterList[1].TeleportTo(Vector2(0, 0)) -- teleports first created characters to 0, 0
function TeleportTo(position) end


---
-- Character.CharacterList, Table containing all characters.
-- @realm shared
-- @Character Character.CharacterList

