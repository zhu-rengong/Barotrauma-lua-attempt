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
            RegisterAction<float>();
            RegisterAction<Microsoft.Xna.Framework.Graphics.SpriteBatch, float>();

            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Function, typeof(GUIComponent.SecondaryButtonDownHandler), v => { var function = v.Function; return (GUIComponent.SecondaryButtonDownHandler)((GUIComponent a1, object a2) => new LuaResult(GameMain.Lua.CallFunction(function, a1, a2)).Bool()); });

            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Function, typeof(GUIButton.OnClickedHandler), v => { var function = v.Function; return (GUIButton.OnClickedHandler)((GUIButton a1, object a2) => new LuaResult(GameMain.Lua.CallFunction(function, a1, a2)).Bool()); });
            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Function, typeof(GUIButton.OnButtonDownHandler), v => { var function = v.Function; return (GUIButton.OnButtonDownHandler)(() => new LuaResult(GameMain.Lua.CallFunction(function)).Bool()); });
            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Function, typeof(GUIButton.OnPressedHandler), v => { var function = v.Function; return (GUIButton.OnPressedHandler)(() => new LuaResult(GameMain.Lua.CallFunction(function)).Bool()); });

            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Function, typeof(GUIColorPicker.OnColorSelectedHandler), v => { var function = v.Function; return (GUIColorPicker.OnColorSelectedHandler)((GUIColorPicker a1, Color a2) => new LuaResult(GameMain.Lua.CallFunction(function, a1, a2)).Bool()); });

            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Function, typeof(GUIDropDown.OnSelectedHandler), v => { var function = v.Function; return (GUIDropDown.OnSelectedHandler)((GUIComponent a1, object a2) => new LuaResult(GameMain.Lua.CallFunction(function, a1, a2)).Bool()); });

            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Function, typeof(GUIListBox.OnSelectedHandler), v => { var function = v.Function; return (GUIListBox.OnSelectedHandler)((GUIComponent a1, object a2) => new LuaResult(GameMain.Lua.CallFunction(function, a1, a2)).Bool()); });
            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Function, typeof(GUIListBox.OnRearrangedHandler), v => { var function = v.Function; return (GUIListBox.OnRearrangedHandler)((GUIListBox a1, object a2) => GameMain.Lua.CallFunction(function, a1, a2)); });
            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Function, typeof(GUIListBox.CheckSelectedHandler), v => { var function = v.Function; return (GUIListBox.CheckSelectedHandler)(() => new LuaResult(GameMain.Lua.CallFunction(function)).Object()); });

            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Function, typeof(GUINumberInput.OnValueChangedHandler), v => { var function = v.Function; return (GUINumberInput.OnValueChangedHandler)((GUINumberInput a1) => GameMain.Lua.CallFunction(function, a1)); });

            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Function, typeof(GUIProgressBar.ProgressGetterHandler), v => { var function = v.Function; return (GUIProgressBar.ProgressGetterHandler)(() => new LuaResult(GameMain.Lua.CallFunction(function)).Float()); });

            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Function, typeof(GUIRadioButtonGroup.RadioButtonGroupDelegate), v => { var function = v.Function; return (GUIRadioButtonGroup.RadioButtonGroupDelegate)((GUIRadioButtonGroup a1, int? a2) => GameMain.Lua.CallFunction(function, a1, a2)); });

            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Function, typeof(GUIScrollBar.OnMovedHandler), v => { var function = v.Function; return (GUIScrollBar.OnMovedHandler)((GUIScrollBar a1, float a2) => new LuaResult(GameMain.Lua.CallFunction(function, a1, a2)).Bool()); });
            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Function, typeof(GUIScrollBar.ScrollConversion), v => { var function = v.Function; return (GUIScrollBar.ScrollConversion)((GUIScrollBar a1, float a2) => new LuaResult(GameMain.Lua.CallFunction(function, a1, a2)).Float()); });

            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Function, typeof(GUITextBlock.TextGetterHandler), v => { var function = v.Function; return (GUITextBlock.TextGetterHandler)(() => new LuaResult(GameMain.Lua.CallFunction(function, new object[] { })).String()); });

            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Function, typeof(GUITextBox.OnEnterHandler), v => { var function = v.Function; return (GUITextBox.OnEnterHandler)((GUITextBox a1, string a2) => new LuaResult(GameMain.Lua.CallFunction(function, a1, a2)).Bool()); });
            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Function, typeof(GUITextBox.OnTextChangedHandler), v => { var function = v.Function; return (GUITextBox.OnTextChangedHandler)((GUITextBox a1, string a2) => new LuaResult(GameMain.Lua.CallFunction(function, a1, a2)).Bool()); });
            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Function, typeof(TextBoxEvent), v => { var function = v.Function; return (TextBoxEvent)((GUITextBox a1, Microsoft.Xna.Framework.Input.Keys a2) => GameMain.Lua.CallFunction(function, a1, a2)); });

            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Function, typeof(GUITickBox.OnSelectedHandler), v => { var function = v.Function; return (GUITickBox.OnSelectedHandler)((GUITickBox a1) => new LuaResult(GameMain.Lua.CallFunction(function, a1)).Bool()); });
#endif


            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Table, typeof(Pair<JobPrefab, int>), v =>
            {
                return new Pair<JobPrefab, int>((JobPrefab)v.Table.Get(1).ToObject(), (int)v.Table.Get(2).CastToNumber());
            });

            Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion<UInt64>((Script script, UInt64 v) => 
            {
                return DynValue.NewString(v.ToString());
            });

            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.UserData, typeof(object), v =>
            {
                if (v.UserData.Object is LuaByte lbyte)
                {
                    return lbyte.Value;
                }
                else if (v.UserData.Object is LuaUShort lushort)
                {
                    return lushort.Value;
                }
                else if (v.UserData.Object is LuaFloat lfloat)
                {
                    return lfloat.Value;
                }
                return v.UserData.Object;
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

        public static void RegisterAction<T1, T2>()
        {
            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Function, typeof(Action<T1, T2>), v =>
            {
                var function = v.Function;
                return (Action<T1, T2>)((a1, a2) => GameMain.Lua.CallFunction(function, a1, a2));
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
