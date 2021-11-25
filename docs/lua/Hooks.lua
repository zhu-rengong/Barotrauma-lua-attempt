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

--- Game's fixed update rate, gets called normally 60 times a second.
-- @realm shared
function think() end

--- Gets called everytime someone sends a chat message, return true to cancel message
-- @tparam string message
-- @tparam client sender
-- @realm shared
function chatMessage(message, sender) end

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

--- Gets called every time an item is interacted, eg: picking item on ground, fixing something with wrench
-- @realm shared
function itemInteract(item, characterPicker, ignoreRequiredItemsBool, forceSelectKeyBool, forceActionKeyBool) end

--- Gets called every time two items are combined, eg: drag an half empty magazine to another half empty magazine to combine
-- @realm shared
function itemCombine(item, otherItem, userCharacter) end


--- Gets called every time an item is moved from one inventory slot to another, return true to cancel
-- @realm shared
function inventoryPutItem(inventory, item, characterUser, index, removeItemBool) end

--- Gets called every time items are swapped, return true to cancel
-- @realm shared
function inventoryItemSwap(inventory, item, characterUser, index, swapWholeStackBool) end

--- 
-- @realm shared
function changeFallDamage() end

--- Gets called every update when a gap passes oxygen
-- @realm shared
function gapOxygenUpdate(gap, hull1, hull2) end

---  Gets called everytime an Item receives a wire signal
-- @realm shared
function signalReceived(signal, connection) end

---  Same as signalReceived, but gets called only when needed by specifying your component, better performance.
-- @realm shared
function signalReceived.YourComponentIdentifier(signal, connection) end

--- Gets called everytime a WifiComponent starts transmitting a signal
-- @realm shared
function wifiSignalTransmitted(wifiComponent, signal, sentFromChat) end

--- Gets called everytime something is logged to the Server Log, do not call print() inside this function, i repeat, do not call print() inside this function.
-- @realm shared
function serverLog(text, serverLogMessageType) end

--- Called each time a new round start job has been assigned, this context allows for you to change the role before it's applied in game.
-- @realm shared
function jobAssigned(text, serverLogMessageType) end

--- Check if a client is allowed to hear radio voice to another client, return true to allow, false to disallow.
-- @realm shared
function canUseVoiceRadio(sender, receiver) end

--- Changes the local voice range, return a number to change the local voice range, 1 = normal, 0.5 = lower range, 2 = higher range.
-- @realm shared
function changeLocalVoiceRange(sender, receiver) end

--- Called right before a message is going to be sent, return true to stop message from being sent.
-- @realm shared
function modifyChatMessage(chatMessage, wifiComponentSender) end

--- Called when the script execution is terminating.
-- @realm shared
function stop() end

---
-- @realm shared
function husk.clientControl(client, husk) end

---
-- @realm shared
function traitor.traitorAssigned(traitor) end

--- Return true to accept traitor candidate.
-- @realm shared
function traitor.findTraitorCandidate(character, team) end