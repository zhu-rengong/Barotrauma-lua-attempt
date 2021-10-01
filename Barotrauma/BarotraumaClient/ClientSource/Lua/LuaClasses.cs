using System;
using System.Collections.Generic;
using System.Text;
using Barotrauma.Networking;
using MoonSharp.Interpreter;

namespace Barotrauma
{
	partial class LuaSetup
	{
		public class LuaGUI
		{
			LuaSetup env;
			public LuaGUI(LuaSetup _env)
			{
				env = _env;

				RectTransform = UserData.CreateStatic<RectTransform>();
				LayoutGroup = UserData.CreateStatic<GUILayoutGroup>();
				Button = UserData.CreateStatic<GUIButton>();
				TextBox = UserData.CreateStatic<GUITextBox>();
				Canvas = UserData.CreateStatic<GUICanvas>();
				Frame = UserData.CreateStatic<GUIFrame>();
				TextBlock = UserData.CreateStatic<GUITextBlock>();
				TickBox = UserData.CreateStatic<GUITickBox>();
				Image = UserData.CreateStatic<GUIImage>();
				ListBox = UserData.CreateStatic<GUIListBox>();

				Anchor = UserData.CreateStatic<Anchor>();
				Alignment = UserData.CreateStatic<Alignment>();
				Pivot = UserData.CreateStatic<Pivot>();

			}

			public ChatBox ChatBox
			{
				get
				{
					return GameMain.Client.ChatBox;
				}
			}

			public static object RectTransform;
			public static object LayoutGroup;
			public static object Button;
			public static object TextBox;
			public static object Canvas;
			public static object Frame;
			public static object TextBlock;
			public static object TickBox;
			public static object Image;
			public static object ListBox;

			public static object Pivot;
			public static object Anchor;
			public static object Alignment;

		}
	}
}
