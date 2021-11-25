-- luacheck: ignore 111

--[[--
Barotrauma EntitySpawner class with some additional functions and fields

Barotrauma source code: [EntitySpawner.cs](https://github.com/evilfactory/Barotrauma-lua-attempt/blob/master/Barotrauma/BarotraumaShared/SharedSource/Networking/EntitySpawner.cs)
]]
-- @code Entity.Spawner
-- @pragma nostrip


--- CreateNetworkEvent
-- @realm shared
-- @tparam Entity entity
-- @tparam bool remove
function CreateNetworkEvent(entity, remove) end

--- ServerWrite
-- @realm shared
-- @tparam IWriteMessage message
-- @tparam Client client
-- @tparam Object[] extraData
function ServerWrite(message, client, extraData) end

--- ToString
-- @realm shared
-- @treturn string
function ToString() end

--- AddToSpawnQueue
-- @realm shared
-- @tparam ItemPrefab itemPrefab
-- @tparam Vector2 worldPosition
-- @tparam Nullable`1 condition
-- @tparam Nullable`1 quality
-- @tparam function onSpawned
function AddToSpawnQueue(itemPrefab, worldPosition, condition, quality, onSpawned) end

--- AddToSpawnQueue
-- @realm shared
-- @tparam ItemPrefab itemPrefab
-- @tparam Vector2 position
-- @tparam Submarine sub
-- @tparam Nullable`1 condition
-- @tparam Nullable`1 quality
-- @tparam function onSpawned
function AddToSpawnQueue(itemPrefab, position, sub, condition, quality, onSpawned) end

--- AddToSpawnQueue
-- @realm shared
-- @tparam ItemPrefab itemPrefab
-- @tparam Inventory inventory
-- @tparam Nullable`1 condition
-- @tparam Nullable`1 quality
-- @tparam function onSpawned
-- @tparam bool spawnIfInventoryFull
-- @tparam bool ignoreLimbSlots
-- @tparam InvSlotType slot
function AddToSpawnQueue(itemPrefab, inventory, condition, quality, onSpawned, spawnIfInventoryFull, ignoreLimbSlots, slot) end

--- AddToSpawnQueue
-- @realm shared
-- @tparam string speciesName
-- @tparam Vector2 worldPosition
-- @tparam function onSpawn
function AddToSpawnQueue(speciesName, worldPosition, onSpawn) end

--- AddToSpawnQueue
-- @realm shared
-- @tparam string speciesName
-- @tparam Vector2 position
-- @tparam Submarine sub
-- @tparam function onSpawn
function AddToSpawnQueue(speciesName, position, sub, onSpawn) end

--- AddToSpawnQueue
-- @realm shared
-- @tparam string speciesName
-- @tparam Vector2 worldPosition
-- @tparam CharacterInfo characterInfo
-- @tparam function onSpawn
function AddToSpawnQueue(speciesName, worldPosition, characterInfo, onSpawn) end

--- AddToRemoveQueue
-- @realm shared
-- @tparam Entity entity
function AddToRemoveQueue(entity) end

--- AddToRemoveQueue
-- @realm shared
-- @tparam Item item
function AddToRemoveQueue(item) end

--- IsInSpawnQueue
-- @realm shared
-- @tparam Predicate`1 predicate
-- @treturn bool
function IsInSpawnQueue(predicate) end

--- CountSpawnQueue
-- @realm shared
-- @tparam Predicate`1 predicate
-- @treturn number
function CountSpawnQueue(predicate) end

--- IsInRemoveQueue
-- @realm shared
-- @tparam Entity entity
-- @treturn bool
function IsInRemoveQueue(entity) end

--- Update
-- @realm shared
-- @tparam bool createNetworkEvents
function Update(createNetworkEvents) end

--- Reset
-- @realm shared
function Reset() end

--- FreeID
-- @realm shared
function FreeID() end

--- Remove
-- @realm shared
function Remove() end

--- GetType
-- @realm shared
-- @treturn Type
function GetType() end

--- Equals
-- @realm shared
-- @tparam Object obj
-- @treturn bool
function Equals(obj) end

--- GetHashCode
-- @realm shared
-- @treturn number
function GetHashCode() end

---
-- Removed, Field of type bool
-- @realm shared
-- @bool Removed

---
-- IdFreed, Field of type bool
-- @realm shared
-- @bool IdFreed

---
-- SimPosition, Field of type Vector2
-- @realm shared
-- @Vector2 SimPosition

---
-- Position, Field of type Vector2
-- @realm shared
-- @Vector2 Position

---
-- WorldPosition, Field of type Vector2
-- @realm shared
-- @Vector2 WorldPosition

---
-- DrawPosition, Field of type Vector2
-- @realm shared
-- @Vector2 DrawPosition

---
-- Submarine, Field of type Submarine
-- @realm shared
-- @Submarine Submarine

---
-- AiTarget, Field of type AITarget
-- @realm shared
-- @AITarget AiTarget

---
-- InDetectable, Field of type bool
-- @realm shared
-- @bool InDetectable

---
-- SpawnTime, Field of type number
-- @realm shared
-- @number SpawnTime

---
-- ID, Field of type number
-- @realm shared
-- @number ID

