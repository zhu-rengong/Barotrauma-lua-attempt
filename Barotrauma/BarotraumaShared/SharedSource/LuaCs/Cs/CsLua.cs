using Barotrauma;
using MoonSharp.Interpreter;
using System.Collections.Generic;

namespace Barotrauma
{
    partial class LuaCsSetup {
        public class CsLua
        {
            private LuaCsSetup setup;
            public Table Globals { get; private set; }

            public CsLua(LuaCsSetup setup)
            {
                this.setup = setup;
                Globals = setup.lua.Globals;
            }

            public DynValue DoString(string code) => setup.DoString(code);
        }
    }
}