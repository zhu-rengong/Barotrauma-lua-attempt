using System;
using System.Collections.Generic;
using System.Text;
using MoonSharp.Interpreter;
using Microsoft.Xna.Framework;
using Barotrauma.Networking;
using Barotrauma.Items.Components;
using System.IO;
using System.Net;
using System.Linq;
using System.Xml.Linq;
using FarseerPhysics.Dynamics;
using System.Reflection;
using HarmonyLib;
using MoonSharp.Interpreter.Interop;
using System.Diagnostics;

namespace Barotrauma
{
	public class LuaResult
	{
		object result;
		public LuaResult(object arg)
		{
			result = arg;
		}

		public bool IsNull()
		{
			if (result == null)
				return true;

			if (result is DynValue dynValue)
				return dynValue.IsNil();

			return false;
		}

		public bool Bool()
		{
			if (result is DynValue dynValue)
			{
				return dynValue.CastToBool();
			}

			return false;
		}

		public float Float()
		{
			if (result is DynValue dynValue)
			{
				var num = dynValue.CastToNumber();
				if (num == null) { return 0f; }
				return (float)num.Value;
			}

			return 0f;
		}

		public double Double()
		{
			if (result is DynValue dynValue)
			{
				var num = dynValue.CastToNumber();
				if (num == null) { return 0f; }
				return num.Value;
			}

			return 0f;
		}

		public string String()
		{
			if (result is DynValue dynValue)
			{
				var str = dynValue.CastToString();
				if (str == null) { return ""; }
				return str;
			}

			return "";
		}

		public object Object()
		{
			if (result is DynValue dynValue)
			{
				return dynValue.ToObject();
			}

			return null;
		}

		public DynValue DynValue()
		{
			if (result is DynValue dynValue)
			{
				return dynValue;
			}

			return null;
		}

		//public static implicit operator bool(LuaResult res) => res.Bool();
		//public static implicit operator float(LuaResult res) => res.Float();
		//public static implicit operator string(LuaResult res) => res.String();
		//public static implicit operator double(LuaResult res) => res.Double();
		//public static implicit operator DynValue(LuaResult res) => res.DynValue();
	}
}