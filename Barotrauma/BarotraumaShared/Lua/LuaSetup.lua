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

local allPackages = ContentPackageManager.AllPackages
local localPackages = ContentPackageManager.LocalPackages
local enabledPackages = ContentPackageManager.EnabledPackages.All

local function endsWith(str, suffix)
    return str:sub(-string.len(suffix)) == suffix
end

local function getFileName(file)
    return file:match("^.+/(.+)$")
end

local function getPath(str)
    local sep = "/"
    return str:match("(.*"..sep..")")
end


local function executeProtected(s, folder)
    loadfile(s)(folder)
end

local function runFolder(folder, rootFolder, package)
    local search = File.DirSearch(folder)
    for i = 1, #search, 1 do
        local s = search[i]:gsub("\\", "/")

        if endsWith(s, ".lua") then
            print(string.format("%s: Executing %s", package.Name, getFileName(s)))
            local ok, result = pcall(executeProtected, s, rootFolder)
            if not ok then
                printerror(result)
            end
        end

    end
end


for package in enabledPackages do
    if package then
        local d = package.Path:gsub("\\", "/")
        d = d:gsub("/filelist.xml", "")

        table.insert(modulePaths, (d .. "/Lua/?.lua"))

        if File.DirectoryExists(d .. "/Lua/Autorun") then
            runFolder(d .. "/Lua/Autorun", d, package)
        end
    end
end

-- we don't want to execute workshop ForcedAutorun if we have a local Package
local executedLocalPackages = {}

for package in localPackages do
    if package then
        local d = package.Path:gsub("\\", "/")
        d = d:gsub("/filelist.xml", "")

        table.insert(modulePaths, (d .. "/Lua/?.lua"))

        if File.DirectoryExists(d .. "/Lua/ForcedAutorun") then
            runFolder(d .. "/Lua/ForcedAutorun", d, package)

            executedLocalPackages[package.Name] = true
        end
    end
end

for package in allPackages do
    if package and executedLocalPackages[package.Name] == nil then
        local d = package.Path:gsub("\\", "/")
        d = d:gsub("/filelist.xml", "")

        table.insert(modulePaths, (d .. "/Lua/?.lua"))

        if File.DirectoryExists(d .. "/Lua/ForcedAutorun") then
            runFolder(d .. "/Lua/ForcedAutorun", d, package)
        end
    end
end

setmodulepaths(modulePaths)

Hook.Add("stop", "luaSetup.stop", function ()
    print("Stopping Lua...")
end)

Hook.Call("loaded")