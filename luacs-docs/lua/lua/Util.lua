-- luacheck: ignore 111

--[[--
Class providing several util functions.
]]
-- @code Util
-- @pragma nostrip

local Util = {}

--- Returns a table with all items that have the given identifier.
-- @realm shared 
Util.GetItemsById = function (id) end

--- Returns the Client that is currently controlling the Character, returns nil if no Client is controlling the Character.
-- @realm shared 
Util.FindClientCharacter = function (character) end