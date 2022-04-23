LuaSetup.Register = function(typeName)
    local descriptor = LuaUserData.RegisterType(typeName)

    LuaSetup.Descriptors[typeName] = descriptor

    return descriptor
end

LuaSetup.RegisterBarotrauma = function(typeName)
    return LuaSetup.Register("Barotrauma." .. typeName)
end

LuaSetup.AddCallMetaTable = function (userdata)
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

LuaSetup.CreateStatic = function(typeName, addCallMethod)
	local staticUserdata = LuaUserData.CreateStatic(typeName)
	
	if addCallMethod then
		LuaSetup.AddCallMetaTable(staticUserdata)
	end

	return staticUserdata
end