local traitormod = {}
traitormod.peoplePercentages = {}

traitormod.roundtraitors = {}
traitormod.selectedCodeResponses = {}
traitormod.selectedCodePhrases = {}
traitormod.gamemodes = {}

traitormod.disableRadioChat = false

local traitorAssignDelay = 0
local traitorsAssigned = true
local warningClients = {}

local config = dofile("lua/traitormod/traitorconfig.lua")
local util = dofile("lua/traitormod/util.lua")

Game.OverrideTraitors(config.enableTraitors) -- shutup old traitors
Game.AllowWifiChat(config.enableWifiChat)

traitormod.config = config

traitormod.setPercentage = function(client, amount)
    if client == nil then return end

    if traitormod.peoplePercentages[client.SteamID] == nil then
        traitormod.peoplePercentages[client.SteamID] = {}
        traitormod.peoplePercentages[client.SteamID].penalty = 1
        traitormod.peoplePercentages[client.SteamID].p =
            config.firstJoinPercentage
    end

    traitormod.peoplePercentages[client.SteamID].p = amount
end

traitormod.addPenalty = function(client, amount)
    if client == nil then return end

    if traitormod.peoplePercentages[client.SteamID] == nil then
        traitormod.peoplePercentages[client.SteamID] = {}
        traitormod.peoplePercentages[client.SteamID].penalty = 1
        traitormod.peoplePercentages[client.SteamID].p =
            config.firstJoinPercentage
    end

    traitormod.peoplePercentages[client.SteamID].penalty =
        traitormod.peoplePercentages[client.SteamID].penalty + amount
end

traitormod.getPenalty = function(client)
    if client == nil then return 0 end

    if traitormod.peoplePercentages[client.SteamID] == nil then
        traitormod.peoplePercentages[client.SteamID] = {}
        traitormod.peoplePercentages[client.SteamID].penalty = 1
        traitormod.peoplePercentages[client.SteamID].p =
            config.firstJoinPercentage
    end

    return traitormod.peoplePercentages[client.SteamID].penalty
end

traitormod.addPercentage = function(client, amount)
    if client == nil then return end

    if traitormod.peoplePercentages[client.SteamID] == nil then
        traitormod.peoplePercentages[client.SteamID] = {}
        traitormod.peoplePercentages[client.SteamID].penalty = 1
        traitormod.peoplePercentages[client.SteamID].p =
            config.firstJoinPercentage
    end

    traitormod.peoplePercentages[client.SteamID].p =
        traitormod.peoplePercentages[client.SteamID].p + amount
end

traitormod.getPercentage = function(client)
    if client == nil then return 0 end

    if traitormod.peoplePercentages[client.SteamID] == nil then
        traitormod.peoplePercentages[client.SteamID] = {}
        traitormod.peoplePercentages[client.SteamID].penalty = 1
        traitormod.peoplePercentages[client.SteamID].p =
            config.firstJoinPercentage
    end

    return traitormod.peoplePercentages[client.SteamID].p
end

traitormod.chooseCodes = function()
    local codewords = dofile("lua/traitormod/traitorconfig.lua").codewords -- copy

    local amount = config.amountCodewords

    for i = 1, amount, 1 do
        local rand = Random.Range(1, #codewords + 1)
        table.insert(traitormod.selectedCodeResponses, codewords[rand])
        table.remove(codewords, rand)
    end

    for i = 1, amount, 1 do
        local rand = Random.Range(1, #codewords + 1)
        table.insert(traitormod.selectedCodePhrases, codewords[rand])
        table.remove(codewords, rand)
    end

end

traitormod.selectPlayerPercentages = function(ignore)
    local players = util.GetValidPlayersNoBotsAndNoTraitors(
                        traitormod.roundtraitors)

    for i = 1, #players, 1 do
        for key, value in pairs(ignore) do
            if players[i] == value then table.remove(players, i) end
        end
    end

    local total = 0

    for key, value in pairs(players) do
        local client = util.clientChar(value)

        total = total + traitormod.getPercentage(client)
    end

    local rng = Random.Range(0, total)

    local addup = 0

    for key, value in pairs(players) do
        local client = util.clientChar(value)

        local percentage = traitormod.getPercentage(client)

        if rng >= addup and rng <= addup + percentage then return value end

        addup = addup + percentage

    end

    return nil

end

traitormod.chooseTraitors = function(amount)
    local traitors = {}

    for i = 1, amount, 1 do
        local found = traitormod.selectPlayerPercentages(traitors)

        if found == nil then
            print("Not enough players")
            break
        else
            table.insert(traitors, found)
        end
    end

    return traitors
end

traitormod.sendTraitorMessage = function(msg, client, notchatbox)
    if client == nil then return end

    if notchatbox == true then
        Game.SendDirectChatMessage("", msg, nil, 11, client)
    else
        Game.SendDirectChatMessage("", msg, nil, 7, client)
    end

    Game.SendDirectChatMessage("", msg, nil, 1, client)
end

traitormod.assignNormalTraitors = function(amount)
    local traitors = traitormod.chooseTraitors(amount)

    for key, value in pairs(traitors) do traitormod.roundtraitors[value] = {} end

    local targets = util.GetValidPlayersNoTraitors(traitormod.roundtraitors)

    for key, value in pairs(traitors) do
        traitormod.roundtraitors[value].objectiveType = "kill"
        traitormod.roundtraitors[value].objectiveTarget =
            targets[Random.Range(1, #targets + 1)]

        local mess =
            "You are a traitor! Your secret mission is to assassinate " ..
                traitormod.roundtraitors[value].objectiveTarget.name ..
                "! Avoid being loud, make the death look like an accident. We will provide you more information after you finish your current mission."

        if util.TableCount(traitormod.roundtraitors) > 1 then
            mess = mess ..
                       "\n\nIt is possible that there are other agents on this submarine. You dont know their names, but you do have a method of communication. Use the code words to greet the agent and code response to respond. Disguise such words in a normal-looking phrase so the crew doesn't suspect anything."
            mess = mess .. "\n\n The code words are: "

            for key, va in pairs(traitormod.selectedCodePhrases) do
                mess = mess .. "\"" .. va .. "\" "
            end

            mess = mess .. "\n The code response is: "

            for key, va in pairs(traitormod.selectedCodeResponses) do
                mess = mess .. "\"" .. va .. "\" "
            end
        end

        Game.Log(value.name ..
                     " Was assigned to be traitor, his first mission is to kill " ..
                     traitormod.roundtraitors[value].objectiveTarget.name, 6)

        traitormod.sendTraitorMessage(mess, util.clientChar(value))

    end
end

traitormod.chooseNextObjective = function(key, value)
    local players = util.GetValidPlayersNoTraitors(traitormod.roundtraitors)

    if #players == 0 then
        traitormod.sendTraitorMessage("Good job agent, You did it.",
                                      util.clientChar(key), true)

        value.needNewObjective = false

        return
    end

    local assassinate = players[Random.Range(1, #players + 1)]

    traitormod.sendTraitorMessage("Your next mission is to assassinate " ..
                                      assassinate.name, util.clientChar(key),
                                  true)

    Game.Log(key.name ..
                 " Was assigned another traitor mission, His mission is to kill " ..
                 assassinate.name, 6)

    value.objectiveTarget = assassinate
    value.objectiveType = "kill"
    value.needNewObjective = false
end

Hook.Add("roundStart", "traitor_start", function()

    Game.SendMessage(
        "We are using Custom Traitors Plugin by EvilFactory (https://steamcommunity.com/id/evilfactory/)\n Join discord.gg/f9zvNNuxu9",
        3)

    local players = util.GetValidPlayersNoBots()

    for key, value in pairs(players) do
        local client = util.clientChar(value)

        if traitormod.getPenalty(client) > 1 then
            traitormod.addPenalty(client, -1)
        end

        traitormod.addPercentage(client, config.roundEndPercentageIncrease /
                                     traitormod.getPenalty(client))
    end

    traitormod.chooseCodes()

    if config.enableTraitors then

        local rng = Random.Range(0, 100)

        if rng < config.traitorShipChance and config.traitorShipEnabled == true then
            local amount = config.getAmountShipTraitors(#util.GetValidPlayers())

            Game.Log("Infiltraition Gamemode selected, Random = " .. rng, 6)
            table.insert(traitormod.gamemodes, "Infiltraition")

            traitormod.traitorShipRoundStart(amount)
        else
            traitorsAssigned = false
            traitorAssignDelay = Timer.GetTime() + config.traitorSpawnDelay

            Game.Log("Assasination Gamemode was selected, Random = " .. rng, 6)
            table.insert(traitormod.gamemodes, "Assasination")

            if config.traitorShipEnabled then
                traitormod.spawnTraitorShipAndHide()
            end
        end

        if config.enableCommunicationsOffline then
            rng = Random.Range(0, 100)

            if rng < config.communicationsOfflineChance then
                table.insert(traitormod.gamemodes, "Offline Communications")
                traitormod.disableRadioChat = true

                Game.SendMessage(
                    "—-radiation closing in on our location, comms down!", 11)

                Game.SendMessage(
                    "—-radiation closing in on our location, comms down!", 3)

                for key, value in pairs(Player.GetAllCharacters()) do
                    if value.IsHuman then
                        Player.SetRadioRange(value, 0)
                    end
                end
            end
        end

    end

    if config.enableSabotage then
        for key, value in pairs(Player.GetAllClients()) do
            
            if value.Character then
                value.Character.IsTraitor = true 
            end
            
            Game.SendTraitorMessage(value, 'traitor', 'traitor',
                                    TraitorMessageType.Objective) -- enable everyone to sabotage
        end
    end

end)

Hook.Add("roundEnd", "traitor_end", function()
    if config.enableTraitors then

        local msg = "Traitors of the round: "

        for key, value in pairs(traitormod.roundtraitors) do
            msg = msg .. "\"" .. key.name .. "\" "
        end

        msg = msg .. "\n\nGamemodes:"

        for key, value in pairs(traitormod.gamemodes) do
            msg = msg .. " \"" .. value .. "\""
        end

        Game.SendMessage(msg, 1)

        for key, value in pairs(traitormod.roundtraitors) do
            local c = util.clientChar(key)
            traitormod.setPercentage(c, config.traitorPercentageSet)
            traitormod.addPenalty(c, config.traitorPenalty)
        end

    end

    traitormod.roundtraitors = {}
    traitormod.selectedCodeResponses = {}
    traitormod.selectedCodePhrases = {}
    traitormod.gamemodes = {}
    warningClients = {}
end)

Hook.Add("think", "traitor_think", function()
    if not traitorsAssigned and traitorAssignDelay < Timer.GetTime() then
        local amount = config.getAmountTraitors(#util.GetValidPlayers())
        traitormod.assignNormalTraitors(amount)

        traitorsAssigned = true
    end

    for key, value in pairs(Player.GetAllCharacters()) do

        if warningClients[value] == nil then

            if value.IsDead == true then
                local attackers = value.LastAttacker
                -- and attackers ~= value
                if util.characterIsTraitor(attackers, traitormod.roundtraitors) and
                    attackers ~= value then
                    traitormod.sendTraitorMessage(
                        "Your death was caused by a traitor on a secret mission.",
                        util.clientChar(value), true)
                end

                warningClients[value] = true
            end

        end
    end

    for key, value in pairs(traitormod.roundtraitors) do
        if key ~= nil and key.IsDead == false then

            if value.needNewObjective == true and Timer.GetTime() >
                value.objectiveTimer then
                traitormod.chooseNextObjective(key, value)
            end

            if value.objectiveTarget ~= nil then

                if value.objectiveTarget.IsDead then
                    traitormod.sendTraitorMessage(
                        "Great job, You killed " .. value.objectiveTarget.name ..
                            ". We will provide you your next mission shortly.",
                        util.clientChar(key), true)

                    value.objectiveTarget = nil

                    value.needNewObjective = true
                    value.objectiveTimer =
                        Timer.GetTime() + config.nextMissionDelay

                end

            end

        end
    end

end)

Hook.Add("chatMessage", "chatcommands", function(msg, client)

    if bit32.band(client.Permissions, 0x40) == 0x40 then

        if msg == "!toggletraitorship" then
            config.traitorShipEnabled = not config.traitorShipEnabled

            if config.traitorShipEnabled then
                Game.SendDirectChatMessage("", "Traitor Ship Enabled", nil, 1,
                                           client)
            else
                Game.SendDirectChatMessage("", "Traitor Ship Disabled", nil, 1,
                                           client)
            end

            Game.OverrideRespawnSub(config.traitorShipEnabled)

            return true
        end

        if msg == "!traitors" then
            local tosend = "Traitors of the round: "

            for key, value in pairs(traitormod.roundtraitors) do
                tosend = tosend .. "\"" .. key.name .. "\" "
            end

            traitormod.sendTraitorMessage(tosend, client)

            return true
        end

        if msg == "!traitoralive" then
            local num = 0

            for key, value in pairs(traitormod.roundtraitors) do
                if key.IsDead == false then num = num + 1 end
            end

            if num > 0 then
                traitormod.sendTraitorMessage("There are traitors alive.",
                                              client)
            else
                traitormod.sendTraitorMessage("All traitors are dead!", client)
            end

            return true
        end

        if msg == "!percentages" then
            local msg = ""
            for key, value in pairs(Player.GetAllClients()) do

                msg = msg .. value.name .. " - " ..
                          math.floor(traitormod.getPercentage(value)) .. "%\n"
            end

            traitormod.sendTraitorMessage(msg, client)

            return true
        end
    end

    if msg == "!percentage" then

        traitormod.sendTraitorMessage("You have " ..
                                          math.floor(
                                              traitormod.getPercentage(client)) ..
                                          "% of being traitor", client)

        return true
    end

    if util.stringstarts(msg, "!traitor") then
        if util.characterIsTraitor(client.Character, traitormod.roundtraitors) then
            local msg = "You are a traitor. "

            if traitormod.roundtraitors[client.Character].objectiveTarget ~= nil then
                msg = msg .. "\nCurrent Mission: kill " ..
                          traitormod.roundtraitors[client.Character]
                              .objectiveTarget.name
            end

            if util.TableCount(traitormod.roundtraitors) > 1 then

                msg = msg .. "\n\n The code words are: "

                for key, va in pairs(traitormod.selectedCodePhrases) do
                    msg = msg .. "\"" .. va .. "\" "
                end

                msg = msg .. "\n The code response is: "

                for key, va in pairs(traitormod.selectedCodeResponses) do
                    msg = msg .. "\"" .. va .. "\" "
                end

            end

            traitormod.sendTraitorMessage(msg, client)
        else
            traitormod.sendTraitorMessage("You are not a traitor.", client)
        end

        return true
    end
end)

Traitormod = traitormod

