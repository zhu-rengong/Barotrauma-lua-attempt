local clrLuaUserData = LuaUserData
local luaUserData = {}

luaUserData.Descriptors = {}

LuaSetup.LuaUserData = luaUserData

luaUserData.UnregisterType = clrLuaUserData.UnregisterType
luaUserData.RegisterGenericType = clrLuaUserData.RegisterGenericType
luaUserData.UnregisterGenericType = clrLuaUserData.UnregisterGenericType
luaUserData.IsTargetType = clrLuaUserData.IsTargetType
luaUserData.GetType = clrLuaUserData.GetType
luaUserData.CreateEnumTable = clrLuaUserData.CreateEnumTable
luaUserData.MakeFieldAccessible = clrLuaUserData.MakeFieldAccessible
luaUserData.MakeMethodAccessible = clrLuaUserData.MakeMethodAccessible
luaUserData.AddMethod = clrLuaUserData.AddMethod
luaUserData.AddField = clrLuaUserData.AddField
luaUserData.RemoveMember = clrLuaUserData.RemoveMember
luaUserData.CreateUserDataFromDescriptor = clrLuaUserData.CreateUserDataFromDescriptor
luaUserData.CreateUserDataFromType = clrLuaUserData.CreateUserDataFromType

luaUserData.RegisterType = function(typeName)
    local descriptor = clrLuaUserData.RegisterType(typeName)

    luaUserData.Descriptors[typeName] = descriptor

    return descriptor
end

luaUserData.RegisterTypeBarotrauma = function(typeName)
    return luaUserData.RegisterType("Barotrauma." .. typeName)
end

luaUserData.AddCallMetaTable = function (userdata)
	debug.setmetatable(userdata, {
		__call = function(obj, ...) 
			local success, result = pcall(userdata.__new, ...)

			if not success then
				error(result, 2)
			end

			return result
		end
	})
end

luaUserData.CreateStatic = function(typeName, addCallMethod)
	local staticUserdata = clrLuaUserData.CreateStatic(typeName)

	if addCallMethod then
		luaUserData.AddCallMetaTable(staticUserdata)
	end

	return staticUserdata
end