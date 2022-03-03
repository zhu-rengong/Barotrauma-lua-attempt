using System;
using MoonSharp.Interpreter;
using Barotrauma.Networking;

namespace Barotrauma
{

	partial class LuaSetup
	{
		public class LuaWriteOnlyMessage
		{
			private WriteOnlyMessage target;

			[MoonSharpHidden]
			public LuaWriteOnlyMessage(WriteOnlyMessage p)
			{
				target = p;
			}

			public void WriteByte(byte v) => target.Write(v);
			public void WriteUShort(ushort v) => target.Write(v);

            public void Write(bool val) => target.Write(val);
            public void WritePadBits() => target.WritePadBits();
            public void Write(byte val) => target.Write(val);
            public void Write(Int16 val) => target.Write(val);
            public void Write(UInt16 val) => target.Write(val);
            public void Write(Int32 val) => target.Write(val);
            public void Write(UInt32 val) => target.Write(val);
            public void Write(Int64 val) => target.Write(val);
            public void Write(UInt64 val) => target.Write(val);
            public void Write(Single val) => target.Write(val);
            public void Write(Double val) => target.Write(val);
            public void WriteColorR8G8B8(Microsoft.Xna.Framework.Color val) => target.WriteColorR8G8B8(val);
            public void WriteColorR8G8B8A8(Microsoft.Xna.Framework.Color val) => target.WriteColorR8G8B8A8(val);
            public void WriteVariableUInt32(UInt32 val) => target.WriteVariableUInt32(val);
            public void Write(string val) => target.Write(val);
            public void WriteRangedInteger(int val, int min, int max) => target.WriteRangedInteger(val, min, max);
            public void WriteRangedSingle(Single val, Single min, Single max, int bitCount) => target.WriteRangedSingle(val, min, max, bitCount);
            public void Write(byte[] val, int startIndex, int length) => target.Write(val, startIndex, length);

            public void PrepareForSending(ref byte[] outBuf, out bool isCompressed, out int outLength) => target.PrepareForSending(ref outBuf, out isCompressed, out outLength);

            public int BitPosition 
            {
                get { return target.BitPosition; }
                set { target.BitPosition = value; } 
            }
            public int BytePosition => target.BytePosition;
            public byte[] Buffer => target.Buffer;
            public int LengthBits
            {
                get { return target.LengthBits; }
                set { target.LengthBits = value; }
            }
            public int LengthBytes => target.LengthBytes;
        }
	}

}