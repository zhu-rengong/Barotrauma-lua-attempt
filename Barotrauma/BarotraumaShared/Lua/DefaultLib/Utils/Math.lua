math.lerp = function (a, b, t)
    return a * (1 - t) + b * t
end

math.clamp = function (value, min, max)
    return math.max(min, math.min(max, value))
end

math.round = function (value, decimals)
    decimals = decimals or 0
    local mult = 10 ^ decimals
    return math.floor(value * mult + 0.5) / mult
end

math.sign = function (value)
    return value >= 0 and 1 or -1
end

math.remap = function (value, inMin, inMax, outMin, outMax)
    return outMin + (outMax - outMin) * ((value - inMin) / (inMax - inMin))
end