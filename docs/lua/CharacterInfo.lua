-- luacheck: ignore 111

--[[--
Barotrauma CharacterInfo class with some additional functions and fields

Barotrauma source code: [CharacterInfo.cs](https://github.com/evilfactory/Barotrauma-lua-attempt/blob/master/Barotrauma/BarotraumaShared/SharedSource/Characters/CharacterInfo.cs)
]]
-- @code CharacterInfo

--- Creates a Character using CharacterInfo.
-- @treturn Character
-- @realm server 
function Create(characterInfo, position, seed, id, isRemotePlayer, hasAi, ragdollParams) end