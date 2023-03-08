Util = {}

local itemDictionary = {}

local function AddItem(item)
    local id = item.Prefab.Identifier.Value
    if itemDictionary[id] == nil then
        itemDictionary[id] = {}
    end

    table.insert(itemDictionary[id], item)
end

Hook.Add("item.created", "luaSetup.util.itemDictionary", function (item)
    AddItem(item)
end)

Hook.Add("roundEnd", "luaSetup.util.itemDictionary", function (item)
    itemDictionary = {}
end)

for _, item in pairs(Item.ItemList) do
    AddItem(item)
end

Util.GetItemsById = function(id)
    return itemDictionary[id]
end

Util.FindClientCharacter = function(character)
    if CLIENT and Game.IsSingleplayer then return nil end

    for _, client in pairs(Client.ClientList) do
        if client.Character == character then
            return client
        end
    end
end