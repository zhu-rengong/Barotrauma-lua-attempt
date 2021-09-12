-- luacheck: ignore 111

--[[--
Class providing filesystem functionality.
]]
-- @code File

--- Read contents from path
-- @treturn string file contents
-- @realm shared 
function Read(path) end

--- Write text to file
-- @realm shared 
function Write(path, text) end

--- Check if file exists.
-- @treturn bool
-- @realm shared 
function Exists(path) end

--- Check if directory exists.
-- @treturn bool
-- @realm shared 
function DirectoryExists(path) end

--- Check if directory exists.
-- @treturn table table containing all files
-- @realm shared 
function GetFiles(path) end

--- List all directories.
-- @treturn table table containing all directories
-- @realm shared 
function GetDirectories(path) end

--- Search directory for all files including sub directories.
-- @treturn table table containing all files
-- @realm shared 
function DirSearch(path) end