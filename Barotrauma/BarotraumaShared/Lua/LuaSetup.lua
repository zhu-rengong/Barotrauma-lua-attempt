-- Config
local runDisabledMods = false
local modulePaths = {"Lua/?.lua", "Mods/LuaForBarotrauma/Lua/?.lua"}
setmodulepaths(modulePaths)

-- Setup Libraries

local defaultLib = require("DefaultLib")

for key, value in pairs(defaultLib) do
    _G[key] = value
end

local compatibilityLib = require("CompatibilityLib")

for key, value in pairs(compatibilityLib) do
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

local enabledPackages = Game.GameSettings.AllEnabledPackages

local function endsWith(str, suffix)
    return str:sub(-string.len(suffix)) == suffix
end

local function executeProtected(s)
    print(s)
    dofile(s)
end

local function runFolder(folder)
    local search = File.DirSearch(folder)
    for i = 1, #search, 1 do
        local s = search[i]:gsub("\\", "/")

        if endsWith(s, ".lua") then
            executeProtected(s)
        end

    end
end

if SERVER then

    if not runDisabledMods then
    
        for package in enabledPackages do
            if package then
                local d = package.path:gsub("\\", "/")
                d = d:gsub("/filelist.xml", "")
    
                table.insert(modulePaths, (d .. "/Lua/?.lua"))
    
                if File.DirectoryExists(d .. "/Lua/Autorun") then
                    runFolder(d .. "/Lua/Autorun")
                end
            end
        end
    
    else
        for _, d in pairs(File.GetDirectories("Mods")) do
            d = d:gsub("\\", "/")
        
            table.insert(modulePaths, (d .. "/Lua/?.lua"))
        
            if File.DirectoryExists(d .. "/Lua/Autorun") then
                runFolder(d .. "/Lua/Autorun")
            end
        end
    end
    
end

for _, d in pairs(File.GetDirectories("Mods")) do
    d = d:gsub("\\", "/")

    if File.DirectoryExists(d .. "/Lua/ForcedAutorun") then
        table.insert(modulePaths, (d .. "/Lua/?.lua"))
        runFolder(d .. "/Lua/ForcedAutorun")
    end
end

setmodulepaths(modulePaths)

Hook.Add("stop", "luaSetup.stop", function ()
    print("Stopping Lua...")
end)

Hook.Call("loaded")