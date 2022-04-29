local LUA_MOD_REQUIRE_PATH       = "/Lua/?.lua"
local LUA_MOD_AUTORUN_PATH       = "/Lua/Autorun"
local LUA_MOD_FORCEDAUTORUN_PATH = "/Lua/ForcedAutorun"

local allPackages     = ContentPackageManager.AllPackages
local localPackages   = ContentPackageManager.LocalPackages
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

local function assertTypes(expectedTypes, ...)
    local args = table.pack(...)
    assert(
        #args == #expectedTypes,
        string.format(
            "Assertion failed: incorrect number of args\n\texpected = %s\n\tgot = %s",
            #expectedTypes,
            #args
        )
    )
    for i = 1, #args do
        local arg = args[i]
        local expectedType = expectedTypes[i]
        assert(
            type(arg) == expectedType,
            string.format(
                "Assertion failed: incorrect argument type (arg #%d)\n\texpected = %s\n\tgot = %s",
                i,
                expectedType,
                type(arg)
            )
        )
    end
end

local function ExecutionQueue()
    local queue = {}
    local function processQueueFIFO()
        while queue[1] ~= nil do
            RunFolder(
                table.unpack(
                    table.remove(
                        queue,
                        1
                    )
                )
            )
        end
    end
    local function queueExecutionFIFO(...)
        assertTypes(
            { 'string', 'string', 'userdata' },
            ...
        )
        table.insert(
            queue,
            table.pack(...)
        )
    end
    return queueExecutionFIFO, processQueueFIFO
end

local queueAutorun,       processAutorun       = ExecutionQueue()
local queueForcedAutorun, processForcedAutorun = ExecutionQueue()

local function processPackages(packages, fn)
    for pkg in packages do
        if pkg then
            local pkgPath = pkg.Path
                :gsub("\\", "/")
                :gsub("/filelist.xml", "")
            fn(pkg, pkgPath)
        end
    end
end

processPackages(
    enabledPackages,
    function(pkg, pkgPath)
        table.insert(package.path, pkgPath .. LUA_MOD_REQUIRE_PATH)
        local autorunPath = pkgPath .. LUA_MOD_AUTORUN_PATH
        if File.DirectoryExists(autorunPath) then
            queueAutorun(autorunPath, pkgPath, pkg)
        end
    end
)

-- we don't want to execute workshop ForcedAutorun if we have a local Package
local executedLocalPackages = {}

processPackages(
    localPackages,
    function(pkg, pkgPath)
        table.insert(package.path, pkgPath .. LUA_MOD_REQUIRE_PATH)
        local forcedAutorunPath = pkgPath .. LUA_MOD_FORCEDAUTORUN_PATH
        if File.DirectoryExists(forcedAutorunPath) then
            queueForcedAutorun(forcedAutorunPath, pkgPath, pkg)
            executedLocalPackages[pkg.Name] = true
        end
    end
)

processPackages(
    allPackages,
    function(pkg, pkgPath)
        if not executedLocalPackages[pkg.Name] then
            table.insert(package.path, pkgPath .. LUA_MOD_REQUIRE_PATH)
            local forcedAutorunPath = pkgPath .. LUA_MOD_FORCEDAUTORUN_PATH
            if File.DirectoryExists(forcedAutorunPath) then
                queueForcedAutorun(forcedAutorunPath, pkgPath, pkg)
            end
        end
    end
)

setmodulepaths(package.path)
processAutorun()
processForcedAutorun()

Hook.Add("stop", "luaSetup.stop", function ()
    print("Stopping Lua...")
end)

Hook.Call("loaded")