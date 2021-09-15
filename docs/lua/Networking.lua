-- luacheck: ignore 111

--[[--
Class providing networking related tasks.
]]
-- @code Networking
-- @pragma nostrip

local Networking = {}

--- Send a post HTTP Request.
-- treturn string result.
-- @realm server 
function Networking.RequestPostHTTP(url, textData, contentType) end

--- Send a get HTTP Request.
-- treturn string result.
-- @realm server 
function Networking.RequestGetHTTP(url) end