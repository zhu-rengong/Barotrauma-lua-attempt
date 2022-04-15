local allPackages = ContentPackageManager.AllPackages
local localPackages = ContentPackageManager.LocalPackages
local enabledPackages = ContentPackageManager.EnabledPackages.All

local function EndsWith(str, suffix)
    return str:sub(-string.len(suffix)) == suffix
end

local function GetFileName(file)
    return file:match("^.+/(.+)$")
end

local function ExecuteProtected(s, folder)
    loadfile(s)(folder)
end

local function RunFolder(folder, rootFolder, package)
    local search = File.DirSearch(folder)
    for i = 1, #search, 1 do
        local s = search[i]:gsub("\\", "/")

        if EndsWith(s, ".lua") then
            print(string.format("%s: Executing %s", package.Name, GetFileName(s)))
            local ok, result = pcall(ExecuteProtected, s, rootFolder)
            if not ok then
                printerror(result)
            end
        end

    end
end


for contentPackage in enabledPackages do
    if contentPackage then
        local d = contentPackage.Path:gsub("\\", "/")
        d = d:gsub("/filelist.xml", "")

        table.insert(package.path, (d .. "/Lua/?.lua"))

        if File.DirectoryExists(d .. "/Lua/Autorun") then
            RunFolder(d .. "/Lua/Autorun", d, contentPackage)
        end
    end
end

-- we don't want to execute workshop ForcedAutorun if we have a local Package
local executedLocalPackages = {}

for contentPackage in localPackages do
    if contentPackage then
        local d = contentPackage.Path:gsub("\\", "/")
        d = d:gsub("/filelist.xml", "")

        table.insert(package.path, (d .. "/Lua/?.lua"))

        if File.DirectoryExists(d .. "/Lua/ForcedAutorun") then
            RunFolder(d .. "/Lua/ForcedAutorun", d, contentPackage)

            executedLocalPackages[contentPackage.Name] = true
        end
    end
end

for contentPackage in allPackages do
    if contentPackage and executedLocalPackages[contentPackage.Name] == nil then
        local d = contentPackage.Path:gsub("\\", "/")
        d = d:gsub("/filelist.xml", "")

        table.insert(package.path, (d .. "/Lua/?.lua"))

        if File.DirectoryExists(d .. "/Lua/ForcedAutorun") then
            RunFolder(d .. "/Lua/ForcedAutorun", d, contentPackage)
        end
    end
end

setmodulepaths(package.path)

Hook.Add("stop", "luaSetup.stop", function ()
    print("Stopping Lua...")
end)

Hook.Call("loaded")