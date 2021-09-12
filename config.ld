
file = {
	"docs/lua"
}

module_file = {

}

dir = "docs/html"
project = "LuaForBarotrauma"
title = "LuaForBarotrauma Documentation"

no_space_before_args = true
style = "docs/css"
template = "docs/templates"
format = "markdown"
ignore = true
topics = "docs/manual"
use_markdown_titles = true
kind_names = {module = "Classes", topic = "Manual"}
merge = true
sort = true
sort_modules = true

simple_args_string = true -- we show optionals/defaults outside of the display name
strip_metamethod_prefix = true -- remove the name of the table when displaying metamethod names
no_viewed_topic_at_top = true -- don't put the currently viewed topic at the top
use_new_templates = true -- new templating system
pretty_urls = true -- avoid showing .html in urls
pretty_topic_names = true -- strips extension from manual filenames, this does not check filename collisions

custom_tags = {
	{"realm", hidden = true},
	{"internal", hidden = true}
}

custom_display_name_handler = function(item, default_handler)
	if (item.type == "function" and item.module) then
		if (item.module.type == "classmod" or item.module.type == "panel") then
			return item.module.mod_name .. ":" .. default_handler(item)
		elseif (item.module.type == "hooks") then
			return item.module.mod_name:upper() .. ":" .. default_handler(item)
		end
	end

	return default_handler(item)
end


new_type("code", "Code", true)

tparam_alias("Client", "client")
tparam_alias("Character", "character")
tparam_alias("Submarine", "submarine")
tparam_alias("Item", "item")

tparam_alias("string", "string")
tparam_alias("bool", "boolean")
tparam_alias("function", "function")
tparam_alias("tab", "table")
