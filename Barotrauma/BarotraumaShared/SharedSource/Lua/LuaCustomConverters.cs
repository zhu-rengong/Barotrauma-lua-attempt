using System;
using System.Collections.Generic;
using System.Text;
using MoonSharp.Interpreter;
using Microsoft.Xna.Framework;
using FarseerPhysics.Dynamics;

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


            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Function, typeof(Func<Fixture, Vector2, Vector2, float, float>), v =>
            {
                var function = v.Function;
                return (Func<Fixture, Vector2, Vector2, float, float>)((Fixture a, Vector2 b, Vector2 c, float d) => new LuaResult(function.Call(a, b, c, d)).Float());
            });

#if CLIENT
            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Function, typeof(GUIButton.OnClickedHandler), v =>
            {

                var function = v.Function;
                return (GUIButton.OnClickedHandler)((GUIButton a, object b) => new LuaResult(GameMain.Lua.CallFunction(function, a, b)).Bool());
            });

            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Function, typeof(GUITextBlock.TextGetterHandler), v =>
            {
                var function = v.Function;
                return (GUITextBlock.TextGetterHandler)(() => new LuaResult(GameMain.Lua.CallFunction(function, new object[] {})).String());
            });

            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Function, typeof(GUITextBox.OnTextChangedHandler), v =>
            {
                var function = v.Function;
                return (GUITextBox.OnTextChangedHandler)((GUITextBox a, string b) => new LuaResult(GameMain.Lua.CallFunction(function, a, b)).Bool());
            });

            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Function, typeof(GUITextBox.OnEnterHandler), v =>
            {
                var function = v.Function;
                return (GUITextBox.OnEnterHandler)((GUITextBox a, string b) => new LuaResult(GameMain.Lua.CallFunction(function, a, b)).Bool());
            });

            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Function, typeof(GUITickBox.OnSelectedHandler), v =>
            {
                var function = v.Function;
                return (GUITickBox.OnSelectedHandler)((GUITickBox a) => new LuaResult(GameMain.Lua.CallFunction(function, a)).Bool());
            });

#endif


            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Table, typeof(Pair<JobPrefab, int>), v =>
            {
                return new Pair<JobPrefab, int>((JobPrefab)v.Table.Get(1).ToObject(), (int)v.Table.Get(2).CastToNumber());
            });

            Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion<UInt64>((Script script, UInt64 v) => 
            {
                return DynValue.NewString(v.ToString());
            });
        }

        public static void RegisterAction<T>()
        {
            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Function, typeof(Action<T>), v =>
            {
                var function = v.Function;
                return (Action<T>)(p => GameMain.Lua.CallFunction(function, p));
            });
        }

        public static void RegisterAction()
        {
            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Function, typeof(Action), v => 
            {
                var function = v.Function;
                return (Action)(() => GameMain.Lua.CallFunction(function));
            });
        }

    }

}
