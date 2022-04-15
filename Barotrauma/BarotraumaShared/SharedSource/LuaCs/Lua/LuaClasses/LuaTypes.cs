using System;
using System.ComponentModel;

namespace Barotrauma
{
	public class LuaByte
	{
		public byte Value;

		public LuaByte(byte v)
		{
			Value = v;
		}

		public static implicit operator byte(LuaByte lb) => lb.Value;
	}

	public class LuaUShort
	{
		public ushort Value;

		public LuaUShort(ushort v)
		{
			Value = v;
		}

		public static implicit operator ushort(LuaUShort lb) => lb.Value;
	}

	public class LuaFloat
	{
		public float Value;

		public LuaFloat(float v)
		{
			Value = v;
		}

		public static implicit operator float(LuaFloat lb) => lb.Value;
	}
}