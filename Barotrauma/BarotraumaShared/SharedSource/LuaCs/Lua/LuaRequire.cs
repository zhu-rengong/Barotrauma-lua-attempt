

using System;
using System.Collections.Generic;
using System.IO;
using MoonSharp.Interpreter;

namespace Barotrauma
{
  class LuaRequire {
    private Script lua { get; set; }
    private Dictionary<string, DynValue> loadedModules { get; set; }

    private bool getExistingReturnValue(string moduleName, ref DynValue returnValue)
    {
      return loadedModules.TryGetValue(
        moduleName,
        out returnValue
      );
    }

    private string fixContentPackagePath(string contentPackagePath)
    {
      contentPackagePath = Path.TrimEndingDirectorySeparator(
        new FileInfo(contentPackagePath)  // filelist.xml
          .Directory
          .FullName
          .CleanUpPathCrossPlatform()
      );
      
      return contentPackagePath;
    }
    private string getContentPackagePath(string path)
    {
      IEnumerable<ContentPackage> allContentPackages = ContentPackageManager.AllPackages;
      foreach (ContentPackage contentPackage in allContentPackages)
      {
        string contentPackagePath = fixContentPackagePath(contentPackage.Path);
        if (path.StartsWith(contentPackagePath))
        {
          return contentPackagePath;
        }
      }
      
      // Return null if we can't find a content package that
      // this module belongs to.
      return null;
    }

    private string getContentPackagePath(string moduleName, Table environment)
    {
      string filePath = lua.Options
        .ScriptLoader
        .ResolveModuleName(
          moduleName,
          environment
        );
      filePath = Path.TrimEndingDirectorySeparator(
        new FileInfo(filePath)
          .Directory
          .FullName
          .CleanUpPathCrossPlatform()
      );

      return getContentPackagePath(filePath);
    }

    private void saveReturnValue(string moduleName, DynValue returnValue)
    {
      loadedModules[moduleName] = returnValue;
    }

    private void executeModule(string moduleName, Table environment, ref DynValue returnValue)
    {
      DynValue loadFunc = lua.RequireModule(
          moduleName,
          environment
      );
      string packagePath = getContentPackagePath(
        moduleName,
        environment
      );

      returnValue = lua.Call(
        loadFunc,
        packagePath
      );

    }

    // Lua modules that have been previously loaded by require() will
    // not be loaded again; instead, their initial return value is
    // preserved and returned again on subsequent attempts.
    public DynValue Require(string moduleName, Table globalContext)
    {
      DynValue returnValue = null;
      Table environment = globalContext ?? lua.Globals;

      if (getExistingReturnValue(moduleName, ref returnValue))
        return returnValue;
      
      executeModule(moduleName, environment, ref returnValue);
      if (
        returnValue == null
        || returnValue.IsNil()
        || returnValue.IsVoid()
      )
        returnValue = DynValue.NewBoolean(true);
      saveReturnValue(moduleName, returnValue);
      return returnValue;
    }

    public LuaRequire(Script lua)
    {
      this.lua = lua;
      loadedModules = new Dictionary<string, DynValue>();
    }
  }
}