LuaSetup.Register = function(typeName)
    local descriptor = LuaUserData.RegisterType(typeName)

    LuaSetup.Descriptors[typeName] = descriptor

    return descriptor
end

LuaSetup.RegisterBarotrauma = function(typeName)
    return LuaSetup.Register("Barotrauma." .. typeName)
end

LuaSetup.CreateStatic = function(typeName, addCallMethod)
	local staticUserdata = LuaUserData.CreateStatic(typeName)
	
	if addCallMethod then
		debug.setmetatable(staticUserdata, {
			__call = function(obj, ...) 
				local success, result = pcall(staticUserdata.__new, ...)

				if not success then
					error(result, 2)
				end

				return result
			end
		})
	end

	return staticUserdata
end