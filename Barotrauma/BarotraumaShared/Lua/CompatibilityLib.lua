-- Contains things to be removed later, they exist only for compatibility reasons.

local compatibilityLib = {}

LuaUserData.AddMethod(LuaUserData.RegisterType("Barotrauma.LuaSetup+LuaUserData"), "AddCallMetaMember", function (v)
    print("AddCallMetaMember is deprecated, use debug.setmetatable instead.")
end)

compatibilityLib.CreateVector2 = Vector2.__new
compatibilityLib.CreateVector3 = Vector3.__new
compatibilityLib.CreateVector4 = Vector4.__new

local luaPlayer = {}

luaPlayer.GetAllCharacters = function ()
    return Character.CharacterList
end

luaPlayer.GetAllClients = function ()
    return Client.ClientList
end

luaPlayer.SetClientCharacter = function (client, character)
    client.SetClientCharacter(character)
end

luaPlayer.SetCharacterTeam = function (character, team)
    character.TeamID = team
end

luaPlayer.SetClientTeam = function (client, team)
    client.TeamID = team
end

luaPlayer.Kick = function (client, reason)
    client.Kick(reason)
end

luaPlayer.Ban = function (client, reason, range, seconds)
    client.Ban(reason, range, seconds)
end

luaPlayer.UnbanPlayer = function (player, endpoint)
    Client.Unban(player, endpoint)
end

luaPlayer.SetSpectatorPos = function ()
    
end

luaPlayer.SetRadioRange = function (character, range)
    if (character.Inventory == nil) then return end

    for item in character.Inventory.AllItems do
        if item ~= nil and item.Prefab.Identifier == "headset" then
            item.GetComponentString("WifiComponent").Range = range;
        end
    end
end

luaPlayer.CheckPermission = function (client, permissions)
    return client.CheckPermission(permissions)
end

compatibilityLib["Player"] = luaPlayer

return compatibilityLib