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


            RegisterFunc<Fixture, Vector2, Vector2, float, float>();
            RegisterFunc<AIObjective, bool>();

            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Function, typeof(LuaCsAction), v => (LuaCsAction)( args => GameMain.LuaCs.CallLuaFunction(v.Function, args) ));
            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Function, typeof(LuaCsFunc), v => (LuaCsFunc)( args => new LuaResult(GameMain.LuaCs.CallLuaFunction(v.Function, args)) ));
            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Function, typeof(LuaCsPatch), v => (LuaCsPatch)( (self, args) => new LuaResult(GameMain.LuaCs.CallLuaFunction(v.Function, self, args)) ));
            Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion(typeof(LuaResult), (Script s, object v) => (v as LuaResult).DynValue());

#if CLIENT
            RegisterAction<float>();
            RegisterAction<Microsoft.Xna.Framework.Graphics.SpriteBatch, float>();

            {
                object Call(object function, params object[] arguments) => GameMain.LuaCs.CallLuaFunction(function, arguments);
                void RegisterHandler<T>(Func<Closure, T> converter)
                    => Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Function, typeof(T), v => converter(v.Function));

                RegisterHandler(f => (GUIComponent.SecondaryButtonDownHandler)(
				(a1, a2) => new LuaResult(Call(f, a1, a2)).Bool()));

                RegisterHandler(f => (GUIButton.OnClickedHandler)(
				(a1, a2) => new LuaResult(Call(f, a1, a2)).Bool()));
                RegisterHandler(f => (GUIButton.OnButtonDownHandler)(
				() => new LuaResult(Call(f)).Bool()));
                RegisterHandler(f => (GUIButton.OnPressedHandler)(
				() => new LuaResult(Call(f)).Bool()));

                RegisterHandler(f => (GUIColorPicker.OnColorSelectedHandler)(
				(a1, a2) => new LuaResult(Call(f, a1, a2)).Bool()));

                RegisterHandler(f => (GUIDropDown.OnSelectedHandler)(
				(a1, a2) => new LuaResult(Call(f, a1, a2)).Bool()));

                RegisterHandler(f => (GUIListBox.OnSelectedHandler)(
				(a1, a2) => new LuaResult(Call(f, a1, a2)).Bool()));
                RegisterHandler(f => (GUIListBox.OnRearrangedHandler)(
				(a1, a2) => Call(f, a1, a2)));
                RegisterHandler(f => (GUIListBox.CheckSelectedHandler)(
				() => new LuaResult(Call(f)).Object()));

                RegisterHandler(f => (GUINumberInput.OnValueEnteredHandler)(
                (a1) => Call(f, a1)));
                RegisterHandler(f => (GUINumberInput.OnValueChangedHandler)(
				(a1) => Call(f, a1)));

                RegisterHandler(f => (GUIProgressBar.ProgressGetterHandler)(
				() => new LuaResult(Call(f)).Float()));

                RegisterHandler(f => (GUIRadioButtonGroup.RadioButtonGroupDelegate)(
				(a1, a2) => Call(f, a1, a2)));

                RegisterHandler(f => (GUIScrollBar.OnMovedHandler)(
				(a1, a2) => new LuaResult(Call(f, a1, a2)).Bool()));
                RegisterHandler(f => (GUIScrollBar.ScrollConversion)(
				(a1, a2) => new LuaResult(Call(f, a1, a2)).Float()));

                RegisterHandler(f => (GUITextBlock.TextGetterHandler)(
				() => new LuaResult(Call(f, new object[] { })).String()));

                RegisterHandler(f => (GUITextBox.OnEnterHandler)(
				(a1, a2) => new LuaResult(Call(f, a1, a2)).Bool()));
                RegisterHandler(f => (GUITextBox.OnTextChangedHandler)(
				(a1, a2) => new LuaResult(Call(f, a1, a2)).Bool()));
                RegisterHandler(f => (TextBoxEvent)(
				(a1, a2) => Call(f, a1, a2)));

                RegisterHandler(f => (GUITickBox.OnSelectedHandler)(
				(a1) => new LuaResult(Call(f, a1)).Bool()));

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

            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.String, typeof(UInt64), v =>
            {
                return UInt64.Parse(v.String);
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

            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.ClrFunction, typeof(Action<T>), v =>
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

            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.ClrFunction, typeof(Action<T1, T2>), v =>
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

            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.ClrFunction, typeof(Action), v =>
            {
                var function = v.Function;
                return (Action)(() => GameMain.LuaCs.CallLuaFunction(function));
            });
        }

        public static void RegisterFunc<T1>()
        {
            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Function, typeof(Func<T1>), v =>
            {
                var function = v.Function;
                return (Func<T1>)(() => function.Call().ToObject<T1>());
            });

            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.ClrFunction, typeof(Func<T1>), v =>
            {
                var function = v.Function;
                return (Func<T1>)(() => function.Call().ToObject<T1>());
            });
        }

        public static void RegisterFunc<T1, T2>()
        {
            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Function, typeof(Func<T1, T2>), v =>
            {
                var function = v.Function;
                return (Func<T1, T2>)((T1 a) => function.Call(a).ToObject<T2>());
            });

            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.ClrFunction, typeof(Func<T1, T2>), v =>
            {
                var function = v.Function;
                return (Func<T1, T2>)((T1 a) => function.Call(a).ToObject<T2>());
            });
        }

        public static void RegisterFunc<T1, T2, T3, T4, T5>()
        {
            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Function, typeof(Func<T1, T2, T3, T4, T5>), v =>
            {
                var function = v.Function;
                return (Func<T1, T2, T3, T4, T5>)((T1 a, T2 b, T3 c, T4 d) => function.Call(a, b, c, d).ToObject<T5>());
            });

            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Function, typeof(Func<T1, T2, T3, T4, T5>), v =>
            {
                var function = v.Function;
                return (Func<T1, T2, T3, T4, T5>)((T1 a, T2 b, T3 c, T4 d) => function.Call(a, b, c, d).ToObject<T5>());
            });
        }

    }

}
