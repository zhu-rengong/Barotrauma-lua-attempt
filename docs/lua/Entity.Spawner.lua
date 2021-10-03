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

--- AddToSpawnQueue
-- @realm shared
-- @tparam ItemPrefab itemPrefab
-- @tparam Vector2 worldPosition
-- @tparam Nullable`1 condition
-- @tparam function onSpawned
function AddToSpawnQueue(itemPrefab, worldPosition, condition, onSpawned) end

--- AddToSpawnQueue
-- @realm shared
-- @tparam ItemPrefab itemPrefab
-- @tparam Vector2 position
-- @tparam Submarine sub
-- @tparam Nullable`1 condition
-- @tparam function onSpawned
function AddToSpawnQueue(itemPrefab, position, sub, condition, onSpawned) end

--- AddToSpawnQueue
-- @realm shared
-- @tparam ItemPrefab itemPrefab
-- @tparam Inventory inventory
-- @tparam Nullable`1 condition
-- @tparam function onSpawned
-- @tparam bool spawnIfInventoryFull
-- @tparam bool ignoreLimbSlots
function AddToSpawnQueue(itemPrefab, inventory, condition, onSpawned, spawnIfInventoryFull, ignoreLimbSlots) end

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



---
-- SpawnTime, Field of type number
-- @realm shared
-- @number SpawnTime

