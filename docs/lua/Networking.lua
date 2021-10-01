-- luacheck: ignore 111

--[[--
Class providing networking related tasks.
]]
-- @code Networking
-- @pragma nostrip

local Networking = {}

--- Send a post HTTP Request, callback is called with an argument result string.
-- @realm server 
function Networking.RequestPostHTTP(url, callback, textData, contentType) end

--- Send a get HTTP Request, callback is called with an argument result string.
-- @realm server 
function Networking.RequestGetHTTP(url, callback) end

--- Creates a new net message, returns an IWriteMessage
-- @treturn IWriteMessage netMessage
-- @realm shared 
function Networking.Start(netMessageName) end

--- Sends a net message to the server.
-- @realm client
function Networking.Send(netMessage, deliveryMethod) end

--- Sends a net message to a connection, if the connection is null, then it sends the message to all connections
-- @realm server 
function Networking.Send(netMessage, connection, deliveryMethod) end

--- Adds a function to listen for lua net messages
-- @realm shared 
function Networking.Receive(netMessageName, callback) end