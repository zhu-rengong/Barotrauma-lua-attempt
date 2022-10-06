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

        public class ZRG<T1>
        {
            public class COC<T2>
            {
                public class TLS<T3>
                {

                }
            }
        }

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


            Lualy<Dictionary<ZRG<int>, ZRG<int>>>();
            Lualy<Dictionary<ZRG<int>, ZRG<int>.COC<int>>>();
            Lualy<Tuple<ZRG<int>, Dictionary<ZRG<int>, ZRG<int>>>>();
            Lualy<ZRG<Tuple<ZRG<int>, Dictionary<ZRG<int>, ZRG<int>>>>.COC<bool>.TLS<string>>();
            Lualy<ZRG<int>>();
            Lualy<ZRG<int>.COC<bool>>();
            Lualy<ZRG<int>.COC<bool>.TLS<string>>();
            Lualy<Option<Barotrauma.Character.Attacker>>();
            Lualy<Barotrauma.Option<Barotrauma.Networking.AccountId>>();
            Lualy<Barotrauma.Networking.AccountId>();
            Lualy<Barotrauma.Networking.SteamId>();
            Lualy<Barotrauma.ItemPrefab>();
            Lualy<Barotrauma.Character>();
            Lualy<Barotrauma.Item>();
            Lualy<Barotrauma.PrefabCollection<Barotrauma.ItemPrefab>>();
            Lualy<Nullable<int>>();
            Lualy<Dictionary<Tuple<int, string>, List<Nullable<ValueTuple<bool, byte>>>>>();
        }
    }
}
