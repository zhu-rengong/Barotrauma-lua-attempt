-- luacheck: ignore 111

--[[--
Barotrauma Character class with some additional functions and fields

Barotrauma source code: [Character.cs](https://github.com/evilfactory/Barotrauma-lua-attempt/blob/master/Barotrauma/BarotraumaShared/SharedSource/Characters/Character.cs)
]]
-- @code Character
-- @pragma nostrip

local Character = {}
-- @field public name string @add name field to class Car, you'll see it in code completion


--- Creates a Character using CharacterInfo.
-- @treturn Character
-- @realm server 
-- @usage 
-- local vsauce = CharacterInfo.Create("human", "VSAUCE HERE")
-- local character = Character.Create(vsauce, CreateVector2(0, 0), "some random characters")
-- print(character)
function Character.Create(characterInfo, position, seed, id, isRemotePlayer, hasAi, ragdollParams) end


--- Teleports a character to a position.
-- @realm server 
-- @tparam Vector2 position
-- @usage 
-- Character.CharacterList[1].TeleportTo(CreateVector2(0, 0)) -- teleports first created characters to 0, 0
function TeleportTo(position) end


---
-- Character.CharacterList, Table containing all characters.
-- @realm shared
-- @Character Character.CharacterList



---
-- IsRemotelyControlled, returns a bool.
-- @realm shared
-- @bool IsRemotelyControlled

---
-- IsObserving, returns a bool.
-- @realm shared
-- @bool IsObserving

---
-- IsBot, returns a bool.
-- @realm shared
-- @bool IsBot

---
-- IsDead, returns a bool.
-- @realm shared
-- @bool IsDead

---
-- IsHuman, returns a bool.
-- @realm shared
-- @bool IsHuman

---
-- IsMale, returns a bool.
-- @realm shared
-- @bool IsMale

---
-- IsFemale, returns a bool.
-- @realm shared
-- @bool IsFemale

---
-- CanSpeak, returns a bool.
-- @realm shared
-- @bool CanSpeak

---
-- NeedsAir, returns a bool.
-- @realm shared
-- @bool NeedsAir

---
-- NeedsWater, returns a bool.
-- @realm shared
-- @bool NeedsWater

---
-- NeedsOxygen, returns a bool.
-- @realm shared
-- @bool NeedsOxygen

---
-- IsTraitor, returns a bool.
-- @realm shared
-- @bool IsTraitor

---
-- HideFace, returns a bool.
-- @realm shared
-- @bool HideFace

---
-- LockHands, returns a bool.
-- @realm shared
-- @bool LockHands

---
-- CanMove, returns a bool.
-- @realm shared
-- @bool CanMove

---
-- CanInteract, returns a bool.
-- @realm shared
-- @bool CanInteract

---
-- ObstructVision, returns a bool.
-- @realm shared
-- @bool ObstructVision

---
-- IsRagdolled, returns a bool.
-- @realm shared
-- @bool IsRagdolled

---
-- IsIncapacitated, returns a bool.
-- @realm shared
-- @bool IsIncapacitated

---
-- IsUnconscious, returns a bool.
-- @realm shared
-- @bool IsUnconscious

---
-- IsPet, returns a bool.
-- @realm shared
-- @bool IsPet

---
-- UseHullOxygen, returns a bool.
-- @realm shared
-- @bool UseHullOxygen

---
-- CanBeSelected, returns a bool.
-- @realm shared
-- @bool CanBeSelected

---
-- CanBeDragged, returns a bool.
-- @realm shared
-- @bool CanBeDragged

---
-- CanInventoryBeAccessed, returns a bool.
-- @realm shared
-- @bool CanInventoryBeAccessed

---
-- GodMode, returns a bool.
-- @realm shared
-- @bool GodMode

---
-- IsInFriendlySub, returns a bool.
-- @realm shared
-- @bool IsInFriendlySub

---
-- Position, returns a Vector2.
-- @realm shared
-- @Vector2 Position

---
-- DrawPosition, returns a Vector2.
-- @realm shared
-- @Vector2 DrawPosition

---
-- CursorPosition, returns a Vector2.
-- @realm shared
-- @Vector2 CursorPosition

---
-- SmoothedCursorPosition, returns a Vector2.
-- @realm shared
-- @Vector2 SmoothedCursorPosition

---
-- CursorWorldPosition, returns a Vector2.
-- @realm shared
-- @Vector2 CursorWorldPosition

---
-- LowPassMultiplier, returns a number.
-- @realm shared
-- @number LowPassMultiplier

---
-- Oxygen, returns a number.
-- @realm shared
-- @number Oxygen

---
-- OxygenAvailable, returns a number.
-- @realm shared
-- @number OxygenAvailable

---
-- Stun, returns a number.
-- @realm shared
-- @number Stun

---
-- Vitality, returns a number.
-- @realm shared
-- @number Vitality

---
-- MaxVitality, returns a number.
-- @realm shared
-- @number MaxVitality

---
-- Health, returns a number.
-- @realm shared
-- @number Health

---
-- HealthPercentage, returns a number.
-- @realm shared
-- @number HealthPercentage

---
-- SpeechImpediment, returns a number.
-- @realm shared
-- @number SpeechImpediment

---
-- PressureTimer, returns a number.
-- @realm shared
-- @number PressureTimer

---
-- CurrentSpeed, returns a number.
-- @realm shared
-- @number CurrentSpeed

---
-- FocusedItem, returns a Item.
-- @realm shared
-- @Item FocusedItem

---
-- PickingItem, returns a Item.
-- @realm shared
-- @Item PickingItem

---
-- Name, returns a string.
-- @realm shared
-- @string Name

---
-- DisplayName, returns a string.
-- @realm shared
-- @string DisplayName

---
-- LogName, returns a string.
-- @realm shared
-- @string LogName

---
-- Inventory, returns a CharacterInventory.
-- @realm shared
-- @CharacterInventory Inventory

---
-- CauseOfDeath, returns a CauseOfDeath.
-- @realm shared
-- @CauseOfDeath CauseOfDeath