using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Barotrauma.Networking;
using System.Threading.Tasks;
using Barotrauma.Items.Components;
using System.IO;
using System.Net;
using System.Linq;
using System.Xml.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis;

namespace Barotrauma
{
    public static partial class LuaForSumneko
    {
        public readonly static string DocumentationRelativePath = "LuaForSumenko";

        public static ImmutableHashSet<string> LuaKeyWords = ImmutableHashSet.Create(
            "and", "break", "do", "else", "elseif", "end", "false",
            "for", "function", "goto", "if", "in", "local", "nil",
            "not", "or", "repeat", "return", "then", "true", "until", "while"
        );

        public static Dictionary<int, StringBuilder> LuaClrDefinitions = new Dictionary<int, StringBuilder>();

        public static List<(ClassMetadata derivedMetadata, ClassMetadata baseMetadata)> LuaClrBasePairs = new List<(ClassMetadata, ClassMetadata)>();

        public static List<ClassMetadata> LualyRecorder = new List<ClassMetadata>() { };

        public static Dictionary<int, StringBuilder> OverloadedOperatorAnnotations = new Dictionary<int, StringBuilder>();
        private static string[] _overloadedOperators = { "op_UnaryNegation", "op_Addition", "op_Subtraction", "op_Multiply", "op_Division" };
        private static bool IsOverloadedOperatorMethod(MethodInfo method)
            => method.IsSpecialName && _overloadedOperators.Contains(method.Name);

        public static void Execute()
        {
            if (!Directory.Exists(DocumentationRelativePath))
            {
                Directory.CreateDirectory(DocumentationRelativePath);
            }
            else
            {
                foreach (var subfile in Directory.GetFiles(DocumentationRelativePath))
                {
                    File.Delete(subfile);
                }
            }

            LualyAll();

            LuaClrBasePairs.RemoveAll(pair => LualyRecorder.Contains(pair.derivedMetadata));

            var _0Global = new StringBuilder();
            ExplanNewLine(_0Global);
            foreach (var pair in LuaClrBasePairs)
            {
                if (pair.baseMetadata == ClassMetadata.Empty)
                {
                    _0Global.AppendLine($@"---@class {pair.derivedMetadata.LuaClrName}");
                }
                else
                {
                    _0Global.AppendLine($@"---@class {pair.derivedMetadata.LuaClrName} : {pair.baseMetadata.LuaClrName}");
                }
            }
            _0Global.Insert(0, "---@meta\n");
            File.WriteAllText(Path.Combine(DocumentationRelativePath, $"{nameof(_0Global)}.lua"), _0Global.ToString());

            foreach (var (token, builder) in LuaClrDefinitions)
            {
                int startIndex = builder.ToString().IndexOf("[placeHolder:operators]");
                builder.Remove(startIndex, "[placeHolder:operators]".Length + 2); // remove the last newline, @field must be defined after @class
                if (OverloadedOperatorAnnotations.TryGetValue(token, out StringBuilder ops))
                {
                    builder.Insert(startIndex, ops.ToString());
                }

                File.WriteAllText(Path.Combine(DocumentationRelativePath, $"{token}.lua"), builder.ToString());
            }
        }

        public static void LualyAll()
        {
            Lualy<System.Object>();
            Lualy<System.String>();
            Lualy<System.Boolean>();
            Lualy<System.SByte>();
            Lualy<System.Byte>();
            Lualy<System.Int16>();
            Lualy<System.UInt16>();
            Lualy<System.Int32>();
            Lualy<System.UInt32>();
            Lualy<System.Int64>();
            Lualy<System.UInt64>();
            Lualy<System.Single>();
            Lualy<System.Double>();

            Lualy<Microsoft.Xna.Framework.Matrix>(new string[] { "Matrix" });
            Lualy<Microsoft.Xna.Framework.Vector2>(new string[] { "Vector2" });
            Lualy<Microsoft.Xna.Framework.Vector3>(new string[] { "Vector3" });
            Lualy<Microsoft.Xna.Framework.Vector4>(new string[] { "Vector4" });
            Lualy<Microsoft.Xna.Framework.Color>(new string[] { "Color" });
            Lualy<Microsoft.Xna.Framework.Point>(new string[] { "Point" });
            Lualy<Microsoft.Xna.Framework.Rectangle>(new string[] { "Rectangle" });

            Lualy<Option<Barotrauma.Character.Attacker>>();
            Lualy<Barotrauma.Option<Barotrauma.Networking.AccountId>>();
            Lualy<Barotrauma.Networking.AccountId>();
            Lualy<Barotrauma.Networking.SteamId>();
            Lualy<Barotrauma.ItemPrefab>();
            Lualy<Barotrauma.Character>();
            Lualy<Barotrauma.Item>();
            Lualy<Barotrauma.PrefabCollection<Barotrauma.ItemPrefab>>();
        }
    }
}
