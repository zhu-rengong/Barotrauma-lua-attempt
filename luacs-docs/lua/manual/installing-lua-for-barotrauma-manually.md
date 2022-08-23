# Installing Lua For Barotrauma Manually

**Notice**: the "Lua For Barotrauma" content package is not required if installed manually, but you may use it anyway if you wish to support the mod, since players automatically download packages when joining the server.

If you get TextManager errors, open `config_player.xml` in your server folder and change the language from "None" to "English"

## Adding to an existing server

1. Download [the latest server patch](https://github.com/evilfactory/LuaCsForBarotrauma/releases/tag/latest) (make sure to download the "patch" zip for your platform, e.g. `luacsforbarotrauma_patch_windows_server.zip`)

2. Extract the zip archive into your existing server folder and replace files as necessary

## Adding to an existing client

1. Download [the latest client patch](https://github.com/evilfactory/LuaCsForBarotrauma/releases/tag/latest) (make sure to download the "patch" zip for your platform, e.g. `luacsforbarotrauma_patch_windows_client.zip`)

2. Extract the zip archive into your game folder and replace files as necessary

## Installing from scratch
1. Download [the latest build](https://github.com/evilfactory/LuaCsForBarotrauma/releases/tag/latest) (make sure to download the "build" zip for your platform, e.g `luacsforbarotrauma_build_windows.zip`)
2. Extract the zip file
3. Find the Content folder in your original Barotrauma game:<br/>
  <img src="https://cdn.discordapp.com/attachments/799752463619325968/833120013149929492/unknown.png" width="500" /><br/>
  <img src="https://cdn.discordapp.com/attachments/799752463619325968/833120379378991104/unknown.png" width="500" /><br/>
  <img src="https://cdn.discordapp.com/attachments/799752463619325968/833120841277374464/unknown.png" width="500" />
4. Copy the Content folder to the extracted folder:<br/>
  <img src="https://cdn.discordapp.com/attachments/799752463619325968/833133217300742154/unknown.png" width="500" />
6. **Optional**: Copy `config_player.xml` from your original game so it retains your configurations
7. Done! Now run DedicatedServer.exe to run the modded server or run Barotrauma.exe to run the modded client

### Linux notice

Sometimes you will get steam initialization errors, most of the time it's because it's missing the `linux64/steamclient.so` binary, so you can just copy the binary from your steam installation over to the folder and it should work.

## Checking if everything is working

If the commands `reloadlua` or `cl_reloadlua` work without errors, it means you successfully installed the mod.
