-- luacheck: ignore 111

--[[--
Class providing networking related tasks.
]]
-- @code Networking

--- Send a post HTTP Request.
-- treturn string result.
-- @realm server 
function RequestPostHTTP(url, textData, contentType) end

--- Send a get HTTP Request.
-- treturn string result.
-- @realm server 
function RequestGetHTTP(url) end