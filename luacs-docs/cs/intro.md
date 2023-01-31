# Barotrauma C# modding guide

## Introduction

C# modding is part of the  **[Lua For Barotrauma](https://github.com/evilfactory/Barotrauma-lua-attempt)** mod and requires the package `Cs For Barotrauma` to be turned on ([steam workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=2795927223)), in some cases the package is not needed and a prompt in game will be shown to enable it automatically.

This modding requires strict source structure, but comes with the benefits of being handled natively by game engine, witch removes many hurdles with type casting or similar issues.

## Starting a new mod

The main star of the show is Barotrauma::ACsMod class. It is what most of your mods will use to hook game methods, and execute custom code. If you are planning on creating new components it's possible to just inherit from ItemComponent.

### Important features

- Mods have full access to Harmony to allow you to perform any patches you wish.
- All utility classes can be accessed either by their type (i.e that have name that starts with `LuaCs...`) or through `GameMain.LuaCs` property (refer to [class documentation](#ltcd)).
- All C# code files must be located in `<mod_root>/CSharp/*` otherwise they won't be compiled
- To configure **server / client** execution behaviour create `RunConfig.xml` in `CSharp` directory, like is shown below (run types are `Standard`, `Forced` and `None`)
- Additionally you can specify what code runs where by either ***filepath*** or ***pre-processor statements***
- In case of filepath, your files must be located in either `CSharp/Shared/*`, `CSharp/Server/*` or `CSharp/Client/*`, for *shared* code, *server-side* code or *client-side* code respectively (in any other case, the code is assumed to be shared)
- I case of pre-processor, you can use `SERVER` or `CLIENT` definitions to separate code into *server-side* code and *client-side* code respectively

### Code example

A generic C# mod boilerplate:

File-tree:

```
 <mod_root>/
 ├─ CSharp/
 │  ├─ RunConfig.xml
 │  ├─ Shared/ExampleMod.cs
 │  ├─ Server/ExampleMod.cs
 │  └─ Client/ExampleMod.cs
 └─ dummyitem.xml
```

____
`<mod_root>/CSharp/RunConfig.xml`

```xml
<?xml version="1.0" encoding="utf-8"?>
<RunConfig>
  <Server>Standard</Server>
  <Client>Forced</Client>
</RunConfig>
```

____
`<mod_root>/CSharp/Shared/ExampleMod.cs`

```csharp
using System;
using Barotrauma;

namespace ExampleNamespace {
    partial class ExampleMod: ACsMod {
        public ExampleMod() {
            // shared code
            ...

            #if SERVER
                InitServer();
            #elif CLIENT
                InitClient();
            #endif
        }

        public override void Stop() {
            // stopping code, e.g. save custom data
             #if SERVER
                // server-side code
                ...
            #elif CLIENT
                // client-side code
                ...
            #endif
        }
    }
}
```

____
`<mod_root>/CSharp/Server/ExampleMod.cs`

```csharp
using System;
using Barotrauma;

namespace ExampleNamespace {
    partial class ExampleMod {
        public void InitServer() {
            // server-side initialization
            ...
        }
    }
}
```

____
`<mod_root>/CSharp/Client/ExampleMod.cs`

```csharp
using System;
using Barotrauma;

namespace ExampleNamespace {
    partial class ExampleMod {
        public void InitClient() {
            // client-side initialization
            ...
        }
    }
}
```

____
`<mod_root>/dummyitem.xml`

```xml
<Items></Items> 
```

## Links to class documentation {#ltcd}

- [Server documentation](../baro-server/html/index.html)
- [Client documentation](../baro-client/html/index.html)
