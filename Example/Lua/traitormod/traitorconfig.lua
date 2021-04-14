local config = {}

local codewords = {
	"pomegrenade", "tabacco", "fish", "nonsense", "europa", "clown",
	"thalamus", "hungry", "renegade", "angry", "green", "flamingos", "sink",
	"mask", "boomer", "sweet", "ice", "charybdis", "cult", "secret", "moloch",
	"husk", "rusted", "ruins", "red", "boat", "cats", "rats", "jeepers", "bench",
	"tire", "trunk", "blow sticks", "thrashers"
}

config.codewords = codewords
config.amountCodewords = 2
config.traitorSpawnDelay = 60
config.nextMissionDelay = 60
config.traitorShipChance = 15
config.traitorShipGodModeDistance = 4000

-- Traitor Selection Options
config.roundEndPercentageIncrease = 10
config.firstJoinPercentage = 10
config.traitorPercentageSet = 5
config.traitorPenalty = 5

-- >=12 players = 3 traitors, >=8 players = 2 traitors, default = 1 traitor
config.getAmountTraitors = function (amountClients)
	if amountClients >= 12 then return 3 end

    if amountClients >= 8 then return 2 end

    return 1
end

-- shipTraitors and normal traitors will be selected equally
config.getAmountShipTraitors = config.getAmountTraitors

return config;