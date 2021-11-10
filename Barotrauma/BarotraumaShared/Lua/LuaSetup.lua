-- Config
local runDisabledMods = false
local modulePaths = {"Lua/?.lua"}
setmodulepaths(modulePaths)

-- Setup Libraries

local defaultLib = require("DefaultLib")

for key, value in pairs(defaultLib) do
    _G[key] = value
end

require("DefaultHook")

-- Execute Mods

if SERVER and Game.IsDedicated then
    runDisabledMods = true

    print("LUA LOADER: Dedicated server detected, loading mods regardless being disabled.")
end

if runDisabledMods then 
    print("LUA LOADER: Mods will be executed regardless being enabled or not. Lua/LuaSetup.lua")
else
    print("LUA LOADER: Only enabled mods will be executed. Lua/LuaSetup.lua")
end

local enabledPackages = Game.GetEnabledContentPackages()

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

if not runDisabledMods then

    for _, package in pairs(enabledPackages) do
        local d = package.path:gsub("\\", "/")
        d = d:gsub("/filelist.xml", "")

        table.insert(modulePaths, (d .. "/Lua/?.lua"))

        if File.DirectoryExists(d .. "/Lua/Autorun") then
            runFolder(d .. "/Lua/Autorun");
        end
    end

else
    for _, d in pairs(File.GetDirectories("Mods")) do
        d = d:gsub("\\", "/")
    
        table.insert(modulePaths, (d .. "/Lua/?.lua"))
    
        if File.DirectoryExists(d .. "/Lua/Autorun") then
            runFolder(d .. "/Lua/Autorun");
        end
    end
end


setmodulepaths(modulePaths)
