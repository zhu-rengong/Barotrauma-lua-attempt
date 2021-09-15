-- luacheck: ignore 111

--[[--
Barotrauma Signal struct with some additional functions and fields

Barotrauma source code: [Signal.cs](https://github.com/evilfactory/Barotrauma-lua-attempt/blob/master/Barotrauma/BarotraumaShared/SharedSource/Items/Components/Signal/Signal.cs)
]]
-- @code Signal
-- @pragma nostrip

Signal = {}

--- Creates a Signal.
-- @treturn Signal
-- @realm shared 
function Signal.Create(stringValue, stepsTaken, characterSender, itemSource, power, strength) end