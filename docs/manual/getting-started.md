# Getting started

If you want to learn how Lua works and the syntax, you can check these websites: [https://www.lua.org/manual/5.2/](https://www.lua.org/manual/5.2/) [https://www.tutorialspoint.com/lua/lua_overview.htm](https://www.tutorialspoint.com/lua/lua_overview.htm)

## How mods are executed
When the server finishes loading everything, Lua For Barotrauma starts up and reads the file `Lua/LuaSetup.lua` and executes it, this Lua script registers classes to be available on the Lua side, creates static references and puts them in the global space, and then goes on each mod and executes the mod's autorun folder. By default it only executes mods enabled in the settings menu, you can bypass that by editing the file `LuaSetup.lua` and setting `local runDisabledMods = false` to `local runDisabledMods = true`.

## Creating your first mod
When creating a new Lua mod, you will need to create a new folder on the **Mods** folder, then inside this folder you will need to create a folder called **Lua**, and then inside the Lua folder you create a folder called **Autorun**, inside this folder you can add your Lua scripts that will be executed automatically, it will look something like this `Mods/MyMod/Lua/Autorun/test.lua`, remember that if you are using the default LuaSetup settings, your mod will only be executed if its enabled in the game's settings, so you will need to create a `filelist.xml` as unusual.

Now you can open **test.lua** in your favorite text editor (<a href="https://code.visualstudio.com/" target="_blank">VSCode</a> with the <a href="https://marketplace.visualstudio.com/items?itemName=sumneko.lua" target="_blank">Lua Sumneko extension</a> recommended) and type in **print("Hello, world")**, now start the server by hosting a game or running the server executable manually, you will see in your console a text appear with the text that you entered in print, you can now type in the server console `reloadlua`, this will re-execute all the Lua scripts. You can also type in `lua yourscript` to run a short lua snippet, like this `lua print('Hello, world')`, remember to remove double quotes, because Barotrauma console automatically formats those.

## Including other files

If you wish to separate your Lua scripts into multiple files, you can do it by either having multiple scripts in the Autorun folder, or having a single script that is responsible for executing the rest of your Lua scripts, for that you will need to get relative paths to your mod, here's an example of how to do it:

```
-- this variable will be accessible to any other script, so you can use it to get the mod's path.
MyModGlobal = {}

MyModGlobal.Path = table.pack(...)[1]

dofile(MyModGlobal.Path .. "/Lua/yourscript.lua")
```

## Learning the libraries
In the sidebar of the documentation, you can see a tab named Code, in there you can check out all the functions and fields that each class has, each one of them has a box with a color on it, where <span class="realm server"></span> means Server-Side, <span class="realm client"></span> means Client-Side and <span class="realm shared"></span> means both Server-Side and Client-Side, by clicking on them you can learn more about them. Not everything is documented here, theres stuff missing that still needs to be added, if you want to find more in-depth functions and fields in the Barotrauma classes, you should check the Barotrauma source code.

See <a href="../lua-examples" target="_blank">Lua Examples</a>, <a href="../common-questions" target="_blank">Common Questions</a>