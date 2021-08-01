using System;
using System.Collections.Generic;
using System.Text;
using MoonSharp.Interpreter;
using Microsoft.Xna.Framework;

namespace Barotrauma
{

	public static class LuaCustomConverters
	{

		public static void RegisterAll()
		{

/*			// Vector 2

			Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Table, typeof(Vector2),
				dynVal => {
					Table table = dynVal.Table;
					float x = (float)((Double)table[1]);
					float y = (float)((Double)table[2]);
					return new Vector2(x, y);
				}
			);
			Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion<Vector2>(
				(script, vector) => {
					DynValue x = DynValue.NewNumber((double)vector.X);
					DynValue y = DynValue.NewNumber((double)vector.Y);
					DynValue dynVal = DynValue.NewTable(script, new DynValue[] { x, y });
					return dynVal;
				}
			);

			// Vector3

			Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Table, typeof(Vector3),
				dynVal => {
					Table table = dynVal.Table;
					float x = (float)((Double)table[1]);
					float y = (float)((Double)table[2]);
					float z = (float)((Double)table[3]);
					return new Vector3(x, y, z);
				}
			);
			Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion<Vector3>(
				(script, vector) => {
					DynValue x = DynValue.NewNumber((double)vector.X);
					DynValue y = DynValue.NewNumber((double)vector.Y);
					DynValue z = DynValue.NewNumber((double)vector.Z);
					DynValue dynVal = DynValue.NewTable(script, new DynValue[] { x, y, z });
					return dynVal;
				}
			);*/

		}

	}
}
