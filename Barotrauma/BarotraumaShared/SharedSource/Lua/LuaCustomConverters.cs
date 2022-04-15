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
            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Function, typeof(Closure), v => v.Function);
            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Function, typeof(CsAction), v => (CsAction)( args => GameMain.LuaCs.CallLuaFunction(v.Function, args) ));
            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Function, typeof(CsFunc), v => (CsFunc)( args => new LuaResult(GameMain.LuaCs.CallLuaFunction(v.Function, args)).Object() ));
            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Function, typeof(CsPatch), v => (CsPatch)( (self, args) => new LuaResult(GameMain.LuaCs.CallLuaFunction(v.Function, self, args)).Object() ));

#if CLIENT
            RegisterAction<float>();
            RegisterAction<Microsoft.Xna.Framework.Graphics.SpriteBatch, float>();

            {
                object Call(object function, params object[] arguments) => GameMain.LuaCs.CallLuaFunction(function, arguments);
                void RegisterHandler<T>(Func<Closure, object> converter)
                {
                    Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Function, typeof(T), v => converter(v.Function));
                }

                RegisterHandler<GUIComponent.SecondaryButtonDownHandler>(f =>
                               (GUIComponent.SecondaryButtonDownHandler)((GUIComponent a1, object a2) => new LuaResult(Call(f, a1, a2)).Bool()));

                RegisterHandler<GUIButton.OnClickedHandler>(f =>
                               (GUIButton.OnClickedHandler)((GUIButton a1, object a2) => new LuaResult(Call(f, a1, a2)).Bool()));
                RegisterHandler<GUIButton.OnButtonDownHandler>(f =>
                               (GUIButton.OnButtonDownHandler)(() => new LuaResult(Call(f)).Bool()));
                RegisterHandler<GUIButton.OnPressedHandler>(f =>
                               (GUIButton.OnPressedHandler)(() => new LuaResult(Call(f)).Bool()));

                RegisterHandler<GUIColorPicker.OnColorSelectedHandler>(f =>
                               (GUIColorPicker.OnColorSelectedHandler)((GUIColorPicker a1, Color a2) => new LuaResult(Call(f, a1, a2)).Bool()));

                RegisterHandler<GUIDropDown.OnSelectedHandler>(f =>
                               (GUIDropDown.OnSelectedHandler)((GUIComponent a1, object a2) => new LuaResult(Call(f, a1, a2)).Bool()));

                RegisterHandler<GUIListBox.OnSelectedHandler>(f =>
                               (GUIListBox.OnSelectedHandler)((GUIComponent a1, object a2) => new LuaResult(Call(f, a1, a2)).Bool()));
                RegisterHandler<GUIListBox.OnRearrangedHandler>(f =>
                               (GUIListBox.OnRearrangedHandler)((GUIListBox a1, object a2) => Call(f, a1, a2)));
                RegisterHandler<GUIListBox.CheckSelectedHandler>(f =>
                               (GUIListBox.CheckSelectedHandler)(() => new LuaResult(Call(f)).Object()));

                RegisterHandler<GUINumberInput.OnValueChangedHandler>(f =>
                               (GUINumberInput.OnValueChangedHandler)((GUINumberInput a1) => Call(f, a1)));

                RegisterHandler<GUIProgressBar.ProgressGetterHandler>(f =>
                               (GUIProgressBar.ProgressGetterHandler)(() => new LuaResult(Call(f)).Float()));

                RegisterHandler<GUIRadioButtonGroup.RadioButtonGroupDelegate>(f =>
                               (GUIRadioButtonGroup.RadioButtonGroupDelegate)((GUIRadioButtonGroup a1, int? a2) => Call(f, a1, a2)));

                RegisterHandler<GUIScrollBar.OnMovedHandler>(f =>
                               (GUIScrollBar.OnMovedHandler)((GUIScrollBar a1, float a2) => new LuaResult(Call(f, a1, a2)).Bool()));
                RegisterHandler<GUIScrollBar.ScrollConversion>(f =>
                               (GUIScrollBar.ScrollConversion)((GUIScrollBar a1, float a2) => new LuaResult(Call(f, a1, a2)).Float()));

                RegisterHandler<GUITextBlock.TextGetterHandler>(f =>
                               (GUITextBlock.TextGetterHandler)(() => new LuaResult(Call(f, new object[] { })).String()));

                RegisterHandler<GUITextBox.OnEnterHandler>(f =>
                               (GUITextBox.OnEnterHandler)((GUITextBox a1, string a2) => new LuaResult(Call(f, a1, a2)).Bool()));
                RegisterHandler<GUITextBox.OnTextChangedHandler>(f =>
                               (GUITextBox.OnTextChangedHandler)((GUITextBox a1, string a2) => new LuaResult(Call(f, a1, a2)).Bool()));
                RegisterHandler<TextBoxEvent>(f =>
                               (TextBoxEvent)((GUITextBox a1, Microsoft.Xna.Framework.Input.Keys a2) => Call(f, a1, a2)));

                RegisterHandler<GUITickBox.OnSelectedHandler>(f =>
                               (GUITickBox.OnSelectedHandler)((GUITickBox a1) => new LuaResult(Call(f, a1)).Bool()));

            }
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
                return (Action<T>)(p => GameMain.LuaCs.CallLuaFunction(function, p));
            });
        }

        public static void RegisterAction<T1, T2>()
        {
            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Function, typeof(Action<T1, T2>), v =>
            {
                var function = v.Function;
                return (Action<T1, T2>)((a1, a2) => GameMain.LuaCs.CallLuaFunction(function, a1, a2));
            });
        }

        public static void RegisterAction()
        {
            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Function, typeof(Action), v => 
            {
                var function = v.Function;
                return (Action)(() => GameMain.LuaCs.CallLuaFunction(function));
            });
        }

    }

}
