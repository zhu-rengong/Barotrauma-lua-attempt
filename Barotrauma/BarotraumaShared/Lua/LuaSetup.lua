LuaSetup = {}

local path = table.pack(...)[1]

package.path = {path .. "/?.lua"}

setmodulepaths(package.path)

-- Setup Libraries
require("LuaUserData")

require("DefaultRegister/RegisterShared")

if SERVER then
    require("DefaultRegister/RegisterServer")
else
    require("DefaultRegister/RegisterClient")
end

local function AddTableToGlobal(tbl)
    for k, v in pairs(tbl) do
        _G[k] = v
    end
end

if SERVER then
    AddTableToGlobal(require("DefaultLib/LibServer"))
else
    AddTableToGlobal(require("DefaultLib/LibClient"))
end

AddTableToGlobal(require("DefaultLib/LibShared"))

AddTableToGlobal(require("CompatibilityLib"))

require("DefaultHook")

Descriptors = LuaSetup.LuaUserData.Descriptors
LuaUserData = LuaSetup.LuaUserData

if not CSActive then
    LuaUserData.RegisterType = function (typeName)
        local descriptor = Descriptors[typeName]

        if descriptor == nil then
            error("Type '" .. typeName .. "' can't be registered", 2)
        else
            return descriptor
        end
    end

    local originalCreateStatic = LuaUserData.CreateStatic
    LuaUserData.CreateStatic = function (typeName, addCallMethod)
        local descriptor = Descriptors[typeName]

        if descriptor == nil then
            error("Unable to create static reference to type " .. typeName, 2)
        end

        return originalCreateStatic(typeName, addCallMethod)
    end
end

LuaSetup = nil

require("ModLoader")