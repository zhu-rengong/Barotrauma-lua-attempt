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
				return File.ReadAllText(file);
			}

			public override bool ScriptFileExists(string file)
			{
				return File.Exists(file);
			}

			public void RunFolder(string folder)
			{
				foreach (var str in DirSearch(folder))
				{
					var s = str.Replace("\\", "/");

					if (s.EndsWith(".lua"))
					{
						Console.WriteLine(s);

						try
						{
							lua.DoFile(s);
						}
						catch (Exception e)
						{
							if (e is InterpreterException)
							{

								Console.WriteLine(((InterpreterException)e).DecoratedMessage);
							}
							else
							{
								Console.WriteLine(e.ToString());
							}
						}
					}

				}
			}

			static string[] DirSearch(string sDir)
			{
				List<string> files = new List<string>();

				try
				{
					foreach (string f in Directory.GetFiles(sDir))
					{
						files.Add(f);
					}

					foreach (string d in Directory.GetDirectories(sDir))
					{
						foreach (string f in Directory.GetFiles(d))
						{
							files.Add(f);
						}
						DirSearch(d);
					}
				}
				catch (System.Exception excpt)
				{
					Console.WriteLine(excpt.Message);
				}

				return files.ToArray();
			}




		}
	}
}