-- luacheck: ignore 111

--[[--
Class providing timing related things.
]]
-- @code Timer
-- @pragma nostrip

Timer = {}

--- Get time in seconds.
-- @treturn number current time in seconds
-- @realm shared 
function Timer.GetTime() end