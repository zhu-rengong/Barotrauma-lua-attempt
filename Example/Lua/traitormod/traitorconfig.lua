local config = {}

local codewords = {
	"pomegrenade", "tabacco", "fish", "nonsense", "europa", "clown",
	"thalamus", "hungry", "renegade", "angry", "green", "flamingos", "sink",
	"mask", "boomer", "sweet", "ice", "charybdis", "cult", "secret", "moloch",
	"husk", "rusted", "ruins", "red", "boat", "cats", "rats", "jeepers", "bench",
	"tire", "trunk", "blow sticks", "thrashers"
}

config.enableTraitors = true

config.codewords = codewords
config.amountCodewords = 2
config.traitorSpawnDelay = 60
config.nextMissionDelay = 60

config.traitorShipEnabled = true -- set this to false for the respawn shuttles to work
config.traitorShipChance = 18
config.traitorShipGodModeDistance = 4000

config.enableCommunicationsOffline = true
config.communicationsOfflineChance = 20

-- Gameplay Options
config.enableSabotage = true -- allow everyone to sabotage
config.enableWifiChat = true -- allow people to wire wifi components to the chat ingame

-- Traitor Selection Options
config.roundEndPercentageIncrease = 10
config.firstJoinPercentage = 10
config.traitorPercentageSet = 5
config.traitorPenalty = 5

-- >=12 players = 3 traitors, >=8 players = 2 traitors, default = 1 traitor
config.getAmountTraitors = function (amountClients)
	if amountClients >= 12 then return 3 end -- if theres 12 or more players, 3 traitors will be selected

    if amountClients >= 8 then return 2 end -- if theres 8 or more players, 2 traitors will be selected

    return 1 -- if theres less than 8 players, there will only one traitor
end

-- shipTraitors and normal traitors will be selected equally
config.getAmountShipTraitors = config.getAmountTraitors

return config;