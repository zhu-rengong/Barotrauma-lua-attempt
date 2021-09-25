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
				GUILayoutGroup = UserData.CreateStatic<GUILayoutGroup>();
				GUIButton = UserData.CreateStatic<GUIButton>();
				GUITextBox = UserData.CreateStatic<GUITextBox>();
				Anchor = UserData.CreateStatic<Anchor>();
			}

			public ChatBox ChatBox
			{
				get
				{
					return GameMain.Client.ChatBox;
				}
			}

			public static object RectTransform;
			public static object GUILayoutGroup;
			public static object GUIButton;
			public static object GUITextBox;
			public static object Anchor;
		}
	}
}
