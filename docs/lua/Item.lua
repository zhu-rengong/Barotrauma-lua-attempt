-- luacheck: ignore 111

--[[--
Barotrauma Item class with some additional functions and fields

Barotrauma source code: [Item.cs](https://github.com/evilfactory/Barotrauma-lua-attempt/blob/master/Barotrauma/BarotraumaShared/SharedSource/Items/Item.cs)
]]
-- @code Item

--- Adds to remove queue, use this instead of Remove, to prevent desync.
-- @realm server 
function AddToRemoveQueue(item) end


--- Sends a signal.
-- @realm server 
function SendSignal(signalOrString, connectionOrConnectionName) end

--- List of all items.
-- @treturn table
-- @realm shared 
ItemList = {}