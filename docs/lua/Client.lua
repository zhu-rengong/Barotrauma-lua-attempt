-- luacheck: ignore 111

--[[--
Barotrauma Character class with some additional functions and fields

Barotrauma source code: [Client.cs](https://github.com/evilfactory/Barotrauma-lua-attempt/blob/master/Barotrauma/BarotraumaShared/SharedSource/Networking/Client.cs)
]]
-- @code Client
-- @pragma nostrip

local Client = {}

--- Sets the client character.
-- @realm server 
function Client:SetClientCharacter(character) end

--- Kick a client.
-- @realm server 
function Client:Kick(reason) end

--- Ban a client.
-- @realm server 
function Client:Ban(reason, range, seconds) end

--- Checks permissions, Client.Permissions.
-- @realm server 
function Client:CheckPermission(permissions) end

--- Unban a client.
-- @realm server 
function Client.Unban(player, endpoint) end


--- List of all connected clients.
-- @treturn table
-- @realm shared 
Client.ClientList = {}