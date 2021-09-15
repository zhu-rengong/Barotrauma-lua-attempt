# Getting started

If you want to learn how Lua works and the syntax, you can check these websites: [https://www.lua.org/manual/5.2/](https://www.lua.org/manual/5.2/) [https://www.tutorialspoint.com/lua/lua_overview.htm](https://www.tutorialspoint.com/lua/lua_overview.htm)

## How mods are executed
When the server finishes loading everything, Lua For Barotrauma starts up and reads the file `Lua/MoonsharpSetup.lua` and executes it, this Lua script then looks for Mods in the Mods folder, and tries to execute Lua scripts inside the Lua/Autorun folder, so for example, if you have a Mod named TheTest, and inside this mod you have a file named Lua/Autorun/test.lua, the test.lua will be executed automatically.

## Creating your first mod
When creating a new Lua mod, you will need to create a new folder on the **Mods** folder, then inside this folder you will need to create a folder called **Lua**, and then inside the Lua folder you create a folder called **Autorun**, inside this folder you can add your lua scripts that will be executed automatically, it will look something like this `Mods/MyMod/Lua/Autorun/test.lua`

Now you can open **test.lua** in your favorite text editor (vscode recommended) and type in **print("Hello, world")**, now start the server by hosting a game or running the server executable manually, you will see in your console a text appear with the text that you entered in print, you can now type in the server console reloadlua, this will re-execute all the Lua scripts. You can also type in lua (script) to run a short lua snippet, like this `lua print('Hello, world')`, remember to remove double quotes, because Barotrauma console automatically formats those.

## Learning the libraries
In the sidebar of the documentation, you can see a tab named Code, in there you can check out all the functions and fields that each class has, each one of them has a box with a color on it, where <span class="realm server"></span> means Server-Side, <span class="realm client"></span> means Client-Side and <span class="realm shared"></span> means both Server-Side and Client-Side, by clicking on them you can learn more about them. Not everything is documented here, theres stuff missing that still needs to be added, if you want to find more in-depth functions and fields in the Barotrauma classes, you should check the Barotrauma source code.