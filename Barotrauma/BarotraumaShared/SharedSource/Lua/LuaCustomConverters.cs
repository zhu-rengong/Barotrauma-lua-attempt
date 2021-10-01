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
            RegisterAction<Item>();
            RegisterAction<Character>();
            RegisterAction<Entity>();
            RegisterAction();
        }

        public static void RegisterAction<T>()
        {
            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Function, typeof(Action<T>), v =>
            {
                var function = v.Function;
                return (Action<T>)(p => function.Call(p));
            });
        }



        public static void RegisterAction()
        {
            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Function, typeof(Action), v => 
            {
                var function = v.Function;
                return (Action)(() => function.Call());
            });
        }
    }
}
