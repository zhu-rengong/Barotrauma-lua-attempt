local modulePaths = {}

local function endsWith(str, suffix)
    return str:sub(-string.len(suffix)) == suffix
end

local function runFolder(folder)
    for k, str in pairs(File.DirSearch(folder)) do
        local s = str:gsub("\\", "/")

        if endsWith(str, ".lua") then
            print(s);
            dofile(s);
        end

    end
end

for _, d in pairs(File.GetDirectories("Mods")) do
    d = d:gsub("\\", "/")

    table.insert(modulePaths, (d .. "/Lua/?.lua"))

    if File.DirectoryExists(d .. "/Lua/Autorun") then
        runFolder(d .. "/Lua/Autorun");
    end
end

setmodulepaths(modulePaths)
