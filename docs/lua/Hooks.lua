-- luacheck: ignore 111

--[[--
Hooks are basically functions that get called when events happen in-game, like chat messages.
]]
-- @code Hook
-- @pragma nostrip

local Hook = {}


--- Adds a hook.
-- @tparam string eventname event name
-- @tparam string hookname hook name
-- @tparam function func callback
-- @realm shared
-- @usage 
-- Hook.Add("characterDeath", "characterDeathExample", function(character)
--    print(character)
-- end)
function Hook.Add(eventname, hookname, func) end

--- Removes a hook.
-- @tparam string eventname event name
-- @tparam string hookname hook name
-- @realm shared
-- @usage 
-- Hook.Remove("characterDeath", "characterDeathExample")
function Hook.Remove(eventname, hookname) end

--- Calls a hook.
-- @tparam string eventname event name
-- @tparam table parameters parameters to be passed in 
-- @realm shared
-- @usage 
-- Hook.Add("think", "happyDebuggingSuckers", function()
--      Hook.Call("characterDead", {}) -- ruin someone's day
-- end)
function Hook.Call(eventname, parameters) end

--- Gets called everytime someone sends a chat message, return true to cancel message
-- @tparam string message
-- @tparam client sender
-- @realm shared
function chatMessage(message, sender) end

--- Called every update
-- @realm shared
function think() end

--- Called when a client connects
-- @tparam client connectedClient
-- @realm shared
function clientConnected(connectedClient) end

--- Called when a client disconnects
-- @tparam client disconnectedClient
-- @realm shared
function clientDisconnected(disconnectedClient) end


--- Called on round start
-- @realm shared
function roundStart() end

--- Called on round end
-- @realm shared
function roundEnd() end

--- Gets callled everytime a character is created.
-- @tparam character createdCharacter
-- @realm shared
function characterCreated(createdCharacter) end

--- Gets called everytime a Character dies.
-- @tparam Character character A dead Character.
-- @realm shared
-- @usage 
-- Hook.Add("characterDeath", "characterDeathExample", function(character)
--    print(character)
-- end)
function characterDeath(character) end

--- Gets called every time an affliction is applied.
-- @realm shared
function afflictionApplied(affliction, characterHealth, limb) end
--- Gets called every time an affliction updates.
-- @realm shared
function afflictionUpdate(affliction, characterHealth, limb) end
--- Gets called every time an Item gets "Used".
-- @realm shared
function itemUse(item, itemUser, targetLimb) end
--- Same as itemUse.
-- @realm shared
function itemSecondaryUse(item, itemUser) end
--- Gets called whenever an item is used as a treatment (eg. bandages).
-- @realm shared
function itemApplyTreatment(item, usingCharacter, targetCharacter, limb) end

--- Gets called whenever an item is dropped, You can return true to cancel.
-- @realm shared
function itemDrop(item, character) end  

--- Gets called whenever an item is equipped. Return true to cancel.
-- @realm shared
function itemEquip(item, character) end


--- Same as itemEquip, but for unequipping.
-- @realm shared
function itemUnequip(item, character) end
--- 
-- @realm shared
function changeFallDamage() end
--- Gets called every update when a gap passes oxygen
-- @realm shared
function gapOxygenUpdate(gap, hull1, hull2) end
---  Gets called everytime an Item receives a wire signal
-- @realm shared
function signalReceived(connection, signal) end
---  Same as signalReceived, but gets called only when needed by specifying your component, better performance.
-- @realm shared
function signalReceived.YourComponentIdentifier(connection, signal) end
--- Gets called everytime a WifiComponent starts transmitting a signal
-- @realm shared
function wifiSignalTransmitted(wifiComponent, signal, sentFromChat) end
--- Gets called everytime something is logged to the Server Log
-- @realm shared
function serverLog(text, serverLogMessageType) end