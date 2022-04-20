

using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;

namespace Barotrauma
{
  class LuaRequire {
    private Script lua { get; set; }
    private Dictionary<Tuple<string, Table>, DynValue> LoadedModules { get; set; }

    private bool GetExistingReturnValue(Tuple<string, Table> key, ref DynValue returnValue) {
      return LoadedModules.TryGetValue(key, out returnValue);
    }

    private void ExecuteModule(Tuple<string, Table> key, ref DynValue returnValue) {
      string moduleName = key.Item1;
      Table globalContext = key.Item2;
      returnValue = lua.Call(lua.RequireModule(moduleName, globalContext));
      LoadedModules[key] = returnValue;
    }

    // Lua modules that have been previously loaded by require() will
    // not be loaded again; instead, their initial return value is
    // preserved and returned again on subsequent attempts.
    public DynValue Require(string moduleName, Table globalContext) {
      DynValue returnValue = DynValue.Nil;
      var key = new Tuple<string, Table>(moduleName, globalContext);

      if (GetExistingReturnValue(key, ref returnValue)) return returnValue;
      ExecuteModule(key, ref returnValue);
      return returnValue;
    }

    public LuaRequire(Script lua) {
      this.lua = lua;
      LoadedModules = new Dictionary<Tuple<string, Table>, DynValue>();
    }
  }
}