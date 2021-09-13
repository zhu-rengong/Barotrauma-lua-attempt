-- luacheck: ignore 111

--[[--
Class providing game related things, Access fields and functions like that: Game.*
]]
-- @code Game

local Game = {}

--- Is the round started?
-- @realm shared 
RoundStarted = true

--- Is dedicated server?
-- @realm server 
IsDedicated = true

--- Server settings.
-- @realm server 
ServerSettings = true

--- Send chat message to every client.
-- @realm server 
function SendMessage(msg, messageType, sender, character) end

--- Send traitor message.
-- @realm server 
function SendTraitorMessage(client, msg, missionid, type) end


--- Send direct message.
-- @realm server 
function SendDirectChatMessage(sendername, text, senderCharacter, chatMessageType, client, iconStyle) end

--- Send direct message.
-- @realm server 
function SendDirectChatMessage(chatMessage, client) end

--- True to override traitors.
-- @realm server 
function OverrideTraitors(override) end

--- True to override respawn sub, stops players from being respawned.
-- @realm server 
function OverrideRespawnSub(override) end

--- True to make wifi chat always work.
-- @realm server 
function AllowWifiChat(override) end

--- True to prevent headsets from transmitting wifi signals.
-- @realm server 
function OverrideSignalRadio(override) end

--- True to disable spam filter.
-- @realm server 
function DisableSpamFilter(override) end

--- Log message to server logs.
-- @realm server 
function Log(message, ServerLogMessageType) end

--- Spawn explosion.
-- @realm server 
function Explode(pos, range, force, damage, structureDamage, itemDamage, empStrength, ballastFloraStrength) end

--- Get respawn sub submarine.
--@treturn Submarine Respawn Shuttle
-- @realm shared 
function GetRespawnSub() end

--- Dispatch respawn sub.
-- @realm server 
function DispatchRespawnSub() end

--- Execute console command.
-- @realm server 
function ExecuteCommand(command) end

--- Starts the game.
-- @realm server 
function StartGame() end

--- Gets all enabled content packages.
--@treturn table Table containing ContentPackages
-- @realm shared 
function GetEnabledContentPackages() end

--- Gets all enabled content packages by reading directly the player xml, useful when your mod doesn't have any xml.
--@treturn table Table containing ContentPackages
-- @realm shared 
function GetEnabledPackagesDirectlyFromFile() end