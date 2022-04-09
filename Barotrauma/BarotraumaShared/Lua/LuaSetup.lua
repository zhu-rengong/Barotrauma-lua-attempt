-- Config

local path = table.pack(...)[1]

local modulePaths = {path .. "/?.lua"}
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

local enabledPackages = ContentPackageManager.EnabledPackages.All

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
    
    for package in enabledPackages do
        if package then
            local d = package.Path:gsub("\\", "/")
            d = d:gsub("/filelist.xml", "")

            table.insert(modulePaths, (d .. "/Lua/?.lua"))

            if File.DirectoryExists(d .. "/Lua/Autorun") then
                runFolder(d .. "/Lua/Autorun")
            end
        end
    end
    
end

setmodulepaths(modulePaths)

Hook.Add("stop", "luaSetup.stop", function ()
    print("Stopping Lua...")
end)

Hook.Call("loaded")