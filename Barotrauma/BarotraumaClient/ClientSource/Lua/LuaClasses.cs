using System;
using System.Collections.Generic;
using System.Text;
using Barotrauma.Networking;

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
			}

			public ChatBox ChatBox
			{
				get
				{
					return GameMain.Client.ChatBox;
				}
			}
		}
	}
}
