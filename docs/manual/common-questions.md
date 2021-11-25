# Common Questions

## What does attempt to index a nil value mean!?
It means you tried to access a field of something that doesn't exist, for example:
```lua
local thisIsNil = nil
print(thisIsNil.what)
print(thisIsNil.something())
```

## Why i'm getting "cannot access field test of userdata<something>" errors?
Because you are trying to access a field in a C# class that doesn't exist.
```lua
print(Item.thisDoesntExist)
```

## I'm getting a super big error with things related to the C# side
It usually happens when you call something on the C# side and you provide nil inputs, you can get a better idea of it by analyzing the error message and trying to link it your lua code, for example:
```lua
Game.SendMessage(nil)
```

This will result in 
```
[LUA ERROR] System.NullReferenceException: Object reference not set to an instance of an object.
at Barotrauma.Networking.ChatMessage.GetChatMessageCommand(String message, String& messageWithoutCommand)
at Barotrauma.Networking.GameServer.SendChatMessage(String message, Nullable`1 type, Client senderClient, 
Character senderCharacter, PlayerConnectionChangeType changeType)
```
You can easily tell that the error has something to do with chat messages, and by looking back at your Lua code you can easily see whats causing it.

## How do i list all clients, characters and items?
```lua
for _, client in pairs(Client.ClientList) do

end

for _, character in pairs(Character.CharacterList) do

end

for _, item in pairs(Item.ItemList) do

end
```

## Running pairs() on an enumerator doesn't work!
pairs() Returns an enumerator that iterates through the entire table keys, if you already have an enumerator, you can just pass it in directly.
```lua
-- get first item ever created and loop through all the items stored inside it.
for item in Item.ItemList[1].OwnInventory.AllItems do

end
```

## How do i spawn an item?
```lua
local prefab = ItemPrefab.GetItemPrefab("screwdriver")
local firstPlayerCharacter = Client.ClientList[1].Character

-- Spawn on the world
Entity.Spawner.AddToSpawnQueue(prefab, firstPlayerCharacter.WorldPosition, nil, nil, function(item)
    print(item .. " Has been spawned.")
end)

-- Spawn inside an inventory
Entity.Spawner.AddToSpawnQueue(prefab, firstPlayerCharacter.Inventory, nil, nil, function(item)
    print(item .. " Has been spawned.")
end)
```

