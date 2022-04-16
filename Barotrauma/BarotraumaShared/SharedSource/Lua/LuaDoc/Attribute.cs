using System;
using MoonSharp.Interpreter;
using Barotrauma.Networking;

namespace Barotrauma
{
    public class LuaAlias
    {
        [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
        public class GUIComponentStyleAttribute : Attribute { }

        [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
        public class SkillIdentifierAttribute : Attribute { }

        [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
        public class JobIdentifierAttribute : Attribute { }

        [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
        public class AfflictionIdentifierAttribute : Attribute { }

        [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
        public class AfflictionTypeAttribute : Attribute { }

        [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
        public class ItemIdentifierAttribute : Attribute { }
    }
}