using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;

namespace Barotrauma
{
	partial class LuaSetup {

		public class LuaScriptLoader : ScriptLoaderBase
		{
			public LuaSetup lua;

			public LuaScriptLoader(LuaSetup l)
			{
				lua = l;
			}


			public override object LoadFile(string file, Table globalContext)
			{
				if (!LuaFile.IsPathAllowedLuaException(file, false)) return null;

				return File.ReadAllText(file);
			}

			public override bool ScriptFileExists(string file)
			{
				if (!LuaFile.IsPathAllowedLuaException(file, false)) return false;

				return File.Exists(file);
			}

		}
	}
}