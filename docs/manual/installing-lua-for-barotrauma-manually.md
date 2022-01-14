## Installing Lua For Barotrauma
1 - Download [latest version of Barotrauma Lua](https://github.com/evilfactory/Barotrauma-lua-attempt/releases/tag/latest), choose the correct platform in the assets drop down.<br>
2 - Extract the zip file<br>
3 - Find the Content folder in your original Barotrauma game: <br>

 ![](https://cdn.discordapp.com/attachments/799752463619325968/833120013149929492/unknown.png)
 ![](https://cdn.discordapp.com/attachments/799752463619325968/833120379378991104/unknown.png)
 ![](https://cdn.discordapp.com/attachments/799752463619325968/833120841277374464/unknown.png)

4 - Copy the Content folder to the extracted folder <br>

![](https://cdn.discordapp.com/attachments/799752463619325968/833133217300742154/unknown.png)

5 - Done! Now run DedicatedServer.exe to run the modded server or run Barotrauma.exe to run the modded client.<br>

### Linux notice
Sometimes you will get steam initialization errors, most of the time it's because it's missing the linux64/steamclient.so binary, so you can just copy the binary from your steam instalation over to the folder and it should work.

## Adding to an existing server

To install to an existing server, do the 1-2 steps from before and copy/replace the following files in your existing server: 

- **DedicatedServer.deps.json** 
- **DedicatedServer.dll** 
- **DedicatedServer.pdb** 
- **0Harmony.dll** 
- **MoonSharp.Interpreter.dll** 
- **MonoMod.Common.dll**
- **Mono.Cecil.dll**
- **Mono.Cecil.Mdb.dll**
- **Mono.Cecil.Pdb.dll**
- **Mono.Cecil.Rocks.dll**
- file that starts with **mscordaccore_amd64_amd64_**
- and the **Lua/** folder

## Adding to an existing client

To install to an existing client, do the 1-2 steps from before and copy/replace the following files in your existing server: 

- **Barotrauma.deps.json** 
- **Barotrauma.dll** 
- **0Harmony.dll** 
- **MoonSharp.Interpreter.dll** 
- **MonoMod.Common.dll**
- **Mono.Cecil.dll**
- **Mono.Cecil.Mdb.dll**
- **Mono.Cecil.Pdb.dll**
- **Mono.Cecil.Rocks.dll**
- file that starts with **mscordaccore_amd64_amd64_**
- and the **Lua/** folder