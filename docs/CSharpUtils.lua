
local class = ""

local generatedDocs = ""

local function addline(line)
    generatedDocs = generatedDocs .. line .. "\n"
end

for line, _ in io.lines("input.txt") do
    local words = {}
    for word in line:gmatch("%w+") do table.insert(words, word) end

    if words[1] == "Float" then words[1] = "number" end
    if words[1] == "Bool" then words[1] = "bool" end
    if words[1] == "String" then words[1] = "string" end

    addline("---")
    addline("-- " .. words[2] .. ", returns a " .. words[1] .. ".")
    addline("-- @realm shared")
    addline("-- @" .. words[1] .. " " .. words[2])
    addline("")

    Character = {}
end

print(generatedDocs)