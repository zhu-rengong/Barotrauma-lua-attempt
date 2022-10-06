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
        public static Dictionary<string, StringBuilder> OverloadedOperatorAnnotations = new Dictionary<string, StringBuilder>();

        public static List<(ClassMetadata derivedMetadata, ClassMetadata baseMetadata)> LuaClrBasePairs = new List<(ClassMetadata, ClassMetadata)>();

        public static List<ClassMetadata> LualyRecorder = new List<ClassMetadata>() { };

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

            foreach (var metadata in LualyRecorder)
            {
                LuaClrBasePairs.RemoveAll(pair => pair.derivedMetadata == metadata);
            }

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
                File.WriteAllText(Path.Combine(DocumentationRelativePath, $"{token}.lua"), builder.ToString());
            }
        }

        public static void LualyAll()
        {
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
