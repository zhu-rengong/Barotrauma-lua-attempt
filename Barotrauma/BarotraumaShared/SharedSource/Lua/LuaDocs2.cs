using System;
using System.Collections.Generic;
using System.Text;
using MoonSharp.Interpreter;
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

/// <summary>
/// 把定义的Class转换成lua文档的形式，包括字段、属性、方法、构造器的成员信息。
/// 文档特性：
/// 每个C#类型在lua文档中有单独的class名称格式
/// 可感知
///     类的继承关系
///     （非）公共、静态、实例成员
///     重载的方法、构造器
///     可选参数、参量参数
///     嵌套的数组、列表、字典、集合及元素类型
///     重载操作运算符+-*/
///     Vallina 原版内容包预设的参数值
/// </summary>
namespace Barotrauma
{

    public static class LuaDocs2
    {
        const int LUA_NAME_CLASS = 1;
        const int LUA_NAME_TYPE = 2;

        const string AliasDir = @"alias";
        const string SharedPath = @"baroluadocs/types/shared";
#if SERVER
        const string BLuaDocPath = @"baroluadocs/types/server";
#elif CLIENT
        const string BLuaDocPath = @"baroluadocs/types/client";
#endif

        public static ImmutableHashSet<string> LUAKEYWORDS = ImmutableHashSet.Create(
            "and", "break", "do", "else", "elseif", "end", "false", "for", "function", "goto", "if", "in", "local", "nil", "not", "or", "repeat", "return", "then", "true", "until", "while"
        );

        public static ImmutableList<(string targetClassName, string baseClassName)> DefaultGLuaDef = ImmutableList.Create(
            ("any", ""),
            ("number", ""),
            ("boolean", ""),
            ("string", ""),
            ("table", ""),
            ("function", "")
        );

        public static Dictionary<string, StringBuilder> ClassDefinition = new Dictionary<string, StringBuilder>();
        public static Dictionary<string, StringBuilder> OverloadedOperatorAnnotations = new Dictionary<string, StringBuilder>();

        public static List<(string targetClassName, string baseClassName)> GlobalLuaDef = new List<(string, string)>(DefaultGLuaDef);

        public static List<string> SingleLuaFileClassNameList = new List<string>() { };

        public class ClassMetadata
        {
            public readonly static HashSet<ClassMetadata> Set = new HashSet<ClassMetadata>();
            public Type OriginalType;
            public Type ResolveT;
            public Type TargetType => ResolveT != null ? ResolveT : OriginalType;
            public bool ArrIndexer = false;
            public bool VIndexer = false;
            public bool KVIndexer = false;
            public bool Indexer => ArrIndexer || VIndexer || KVIndexer;
            public Type ElementType;
            public Type ValueType;
            public (Type Key, Type Value)? KVType;
            public bool IsDelegate = false;
            public MethodInfo DelegateMehtod;

            public static ClassMetadata QuickRetrieve(Type target)
            {
                try
                {
                    return Set.First(metadata => metadata.OriginalType == target);
                }
                catch
                {
                    return null;
                }
            }

            public ClassMetadata(Type originalType)
            {
                OriginalType = originalType;
                Set.Add(this);
            }

            public static ClassMetadata Obtain(Type target)
            {
                var metadata = QuickRetrieve(target);
                if (metadata == null)
                {
                    {
                        if (ResolveNullableArgumentType(target, out Type argumentType))
                        {
                            return new ClassMetadata(target)
                            {
                                ResolveT = argumentType
                            };
                        }
                    }

                    {
                        if (ResolveArrayElementType(target, out Type elementType))
                        {
                            return new ClassMetadata(target)
                            {
                                ArrIndexer = true,
                                ElementType = elementType
                            };
                        }
                    }

                    {
                        if (ResolveVIndexerArgumentType(target, out Type argumentType))
                        {
                            return new ClassMetadata(target)
                            {
                                VIndexer = true,
                                ValueType = argumentType
                            };
                        }
                    }

                    {
                        if (ResolveKVIndexerArgumentType(target, out (Type, Type)? argumentTypes))
                        {
                            return new ClassMetadata(target)
                            {
                                KVIndexer = true,
                                KVType = argumentTypes
                            };
                        }
                    }

                    {
                        if (target.IsSubclassOf(typeof(Delegate)))
                        {
                            var delmi = target.GetMethod("Invoke");
                            if (delmi != null)
                            {
                                return new ClassMetadata(target)
                                {
                                    IsDelegate = true,
                                    DelegateMehtod = delmi
                                };
                            }
                        }
                    }

                    return new ClassMetadata(target);
                }
                return metadata;
            }

            public (bool state, string Name) GetLuaName(int luaName = LUA_NAME_CLASS, char split = '_', bool tryGetMappedName = true)
            {
                if (luaName == LUA_NAME_TYPE)
                {
                    if (Indexer)
                    {
                        if (ArrIndexer) { return (true, Obtain(ElementType).GetLuaName(LUA_NAME_TYPE, split).Name + @"[]"); }
                        if (VIndexer) { return (true, Obtain(ValueType).GetLuaName(LUA_NAME_TYPE, split).Name + @"[]"); }
                        if (KVIndexer)
                        {
                            var key = Obtain(KVType.Value.Key).GetLuaName(LUA_NAME_TYPE, split).Name;
                            var value = Obtain(KVType.Value.Value).GetLuaName(LUA_NAME_TYPE, split).Name;
                            return (true, $@"table<{key}, {value}>");
                        }
                        return (true, "table");
                    }
                    else if (IsDelegate)
                    {
                        var methodSB = new StringBuilder();
                        var parameters = DelegateMehtod.GetParameters();
                        ExplanOverloadMethodStartForGenLuaType(methodSB);
                        for (var j = 0; j < parameters.Length; j++)
                        {
                            var parameter = parameters[j];

                            ExplanOverloadMethodParam(methodSB, parameter);
                            if (j != parameters.Length - 1)
                            {
                                methodSB.Append(", ");
                            }
                        }
                        ExplanOverloadMethodEnd(methodSB, DelegateMehtod);
                        return (true, $"({methodSB.ToString()})");
                    }
                    else if (TargetType.IsGenericType)
                    {
                        return IsCompatibleTable(TargetType) ? (true, "table") : (true, Obtain(typeof(System.Object)).GetLuaName(LUA_NAME_TYPE).Name);
                    }
                    else
                    {
                        return ClassName(TargetType, split);
                    }
                }

                if (Indexer || TargetType.IsGenericType) { return (false, null); }

                return ClassName(TargetType, split);

                (bool, string) ClassName(Type tgt, char split)
                {
                    string className = tgt.Name;
                    className = Regex.Replace(className, @"\[\]|`1|`2|`3|&", "");

                    if (tryGetMappedName)
                    {
                        var (state, mapName) = GetMapClassName(tgt.Namespace, className);
                        if (state) { return (true, mapName); }
                    }

                    var nameSpacePart = tgt.Namespace;
                    var declarT = tgt.DeclaringType;
                    var prefix = "";
                    while (declarT != null)
                    {
                        prefix = $@"{declarT.Name}.{prefix}";
                        declarT = declarT.DeclaringType;
                    }
                    className = (nameSpacePart != ""
                        ? (prefix != ""
                            ? $@"{nameSpacePart}.{prefix}{className}"
                            : $@"{nameSpacePart}.{className}"
                        )
                        : (prefix != ""
                            ? $@"{prefix}{className}"
                            : $@"{className}"
                        )).Replace('.', split);

                    return (true, className);
                }
            }

            public string GetDefaultTableName(char split = '_')
            {
                var nameSpacePart = TargetType.Namespace.StartsWith("Barotrauma") ?
                    "" : $@"{TargetType.Namespace}";
                nameSpacePart = nameSpacePart.Replace('.', split);
                // double dots to be replaced with split for expressing the root table name
                if (nameSpacePart != "")
                {
                    nameSpacePart = nameSpacePart + "..";
                }

                var declarT = TargetType.DeclaringType;
                var prefix = "";
                while (declarT != null)
                {
                    prefix = $@"{declarT.Name}.{prefix}";
                    declarT = declarT.DeclaringType;
                }

                return (nameSpacePart != ""
                    ? (prefix != ""
                        ? $@"{nameSpacePart}.{prefix}{TargetType.Name}"
                        : $@"{nameSpacePart}.{TargetType.Name}"
                    )
                    : (prefix != ""
                        ? $@"{prefix}{TargetType.Name}"
                        : $@"{TargetType.Name}"
                    )).Replace("...", $"{split}");
            }

            public static (bool, string) GetMapClassName(string nameSpace, string className)
            {
                if (nameSpace.StartsWith("System"))
                {
                    switch (className)
                    {
                        case "Object":
                            return (true, $"{ClassMetadata.Obtain(typeof(Object)).GetLuaName(LUA_NAME_TYPE, tryGetMappedName: false).Name}|any");
                        case "Boolean":
                            return (true, $"{ClassMetadata.Obtain(typeof(Boolean)).GetLuaName(LUA_NAME_TYPE, tryGetMappedName: false).Name}|boolean");
                        case "String":
                            return (true, $"{ClassMetadata.Obtain(typeof(String)).GetLuaName(LUA_NAME_TYPE, tryGetMappedName: false).Name}|string");
                        case "Single":
                            return (true, $"{ClassMetadata.Obtain(typeof(Single)).GetLuaName(LUA_NAME_TYPE, tryGetMappedName: false).Name}|number");
                        case "Double":
                            return (true, $"{ClassMetadata.Obtain(typeof(Double)).GetLuaName(LUA_NAME_TYPE, tryGetMappedName: false).Name}|number");
                        case "SByte":
                            return (true, $"{ClassMetadata.Obtain(typeof(SByte)).GetLuaName(LUA_NAME_TYPE, tryGetMappedName: false).Name}|number");
                        case "Byte":
                            return (true, $"{ClassMetadata.Obtain(typeof(Byte)).GetLuaName(LUA_NAME_TYPE, tryGetMappedName: false).Name}|number");
                        case "Int16":
                            return (true, $"{ClassMetadata.Obtain(typeof(Int16)).GetLuaName(LUA_NAME_TYPE, tryGetMappedName: false).Name}|number");
                        case "UInt16":
                            return (true, $"{ClassMetadata.Obtain(typeof(UInt16)).GetLuaName(LUA_NAME_TYPE, tryGetMappedName: false).Name}|number");
                        case "Int32":
                            return (true, $"{ClassMetadata.Obtain(typeof(Int32)).GetLuaName(LUA_NAME_TYPE, tryGetMappedName: false).Name}|number");
                        case "UInt32":
                            return (true, $"{ClassMetadata.Obtain(typeof(UInt32)).GetLuaName(LUA_NAME_TYPE, tryGetMappedName: false).Name}|number");
                        case "Int64":
                            return (true, $"{ClassMetadata.Obtain(typeof(Int64)).GetLuaName(LUA_NAME_TYPE, tryGetMappedName: false).Name}|number");
                        case "UInt64":
                            return (true, $"{ClassMetadata.Obtain(typeof(UInt64)).GetLuaName(LUA_NAME_TYPE, tryGetMappedName: false).Name}|number");
                        default:
                            break;
                    }
                }

                switch (className)
                {
                    case "T": return (true, "T");
                    case "T1": return (true, "T1");
                    case "T2": return (true, "T2");
                    case "T3": return (true, "T3");
                    case "T4": return (true, "T4");
                    case "T5": return (true, "T5");
                    case "T6": return (true, "T6");
                    case "T7": return (true, "T7");
                    case "T8": return (true, "T8");
                    case "T9": return (true, "T9");
                    case "T10": return (true, "T10");
                    case "T11": return (true, "T11");
                    case "T12": return (true, "T12");
                    case "T13": return (true, "T13");
                    case "T14": return (true, "T14");
                    case "T15": return (true, "T15");
                    case "T16": return (true, "T16");
                    case "T17": return (true, "T17");
                }

                return (false, className);
            }


            public void CollectSelfToGlobal()
            {
                var (state, className) = GetLuaName();
                if (state && !GlobalLuaDef.Exists(def => def.targetClassName == className))
                {
                    if (TargetType.BaseType != null && !TargetType.BaseType.IsGenericType)
                    {
                        var metadata = Obtain(TargetType.BaseType);
                        var (subState, baseClassName) = metadata.GetLuaName();
                        if (subState)
                        {
                            GlobalLuaDef.Add((className, baseClassName));
                            metadata.CollectAllToGlobal();
                        }
                    }
                    else
                    {
                        GlobalLuaDef.Add((className, ""));
                    }
                }
            }

            public void CollectAllToGlobal()
            {
                CollectSelfToGlobal();
                var allTypes = new List<Type>();
                if (ArrIndexer) { allTypes.Add(ElementType); }
                if (VIndexer) { allTypes.Add(ValueType); }
                if (KVIndexer) { allTypes.AddRange(new Type[] { KVType.Value.Key, KVType.Value.Value }); }
                foreach (var type in allTypes) { Obtain(type).CollectAllToGlobal(); }
            }
        }

        static void Initialize()
        {
            
            var paths = new string[] { BLuaDocPath, SharedPath,
                Path.Combine(BLuaDocPath, AliasDir), Path.Combine(SharedPath, AliasDir) };

            for (int i = 0; i < paths.Length; i++)
            {
                var path = paths[i];
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                else
                {
                    foreach (var subfile in Directory.GetFiles(path))
                    {
                        File.Delete(subfile);
                    }
                }
            }
        }

        private static bool IsCompatibleTable(Type type) =>
            type.IsArray ||
            type.GetInterface("IList") != null ||
            type.GetInterface("IReadOnlyList") != null ||
            type.GetInterface("IDictionary") != null ||
            type.GetInterface("IReadOnlyDictionary") != null;

        private static bool ResolveNullableArgumentType(Type type, out Type argumentType)
        {
            argumentType = null;

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                var genericArguments = type.GetGenericArguments();
                argumentType = genericArguments[0];
                return true;
            }

            return false;
        }

        private static bool ResolveArrayElementType(Type type, out Type elementType)
        {
            elementType = null;

            if (type.IsArray)
            {
                elementType = type.GetElementType();
                if (ResolveNullableArgumentType(elementType, out Type argumentType))
                { elementType = argumentType; }
                return true;
            }

            return false;
        }

        private static bool ResolveVIndexerArgumentType(Type type, out Type argumentType)
        {
            argumentType = null;

            if (type.IsGenericType)
            {
                var genericTypeDefinition = type.GetGenericTypeDefinition();
                bool Judg(Type t) => genericTypeDefinition == t;
                if (Judg(typeof(IList<>)) ||
                    Judg(typeof(IReadOnlyList<>)) ||
                    Judg(typeof(List<>)) ||
                    Judg(typeof(HashSet<>)) ||
                    Judg(typeof(IEnumerable<>)))
                {
                    var genericArguments = type.GetGenericArguments();
                    argumentType = genericArguments[0];
                    if (ResolveNullableArgumentType(argumentType, out Type nullableArgumentType))
                    { argumentType = nullableArgumentType; }
                    return true;
                }
            }

            return false;
        }

        private static bool ResolveKVIndexerArgumentType(Type type, out (Type, Type)? argumentTypes)
        {
            argumentTypes = null;

            if (type.IsGenericType)
            {
                var genericTypeDefinition = type.GetGenericTypeDefinition();
                bool Judg(Type t) => genericTypeDefinition == t;
                if (Judg(typeof(IDictionary<,>)) ||
                    Judg(typeof(IReadOnlyDictionary<,>)) ||
                    Judg(typeof(Dictionary<,>)))
                {
                    var genericArguments = type.GetGenericArguments();
                    for (int i = 0; i < 2; i++)
                    {
                        if (ResolveNullableArgumentType(genericArguments[i], out Type nullableArgumentType))
                        {
                            genericArguments[i] = nullableArgumentType;
                        }
                    }
                    argumentTypes = (genericArguments[0], genericArguments[1]);
                    return true;
                }
            }

            return false;
        }

        public static void Work()
        {
            Initialize();

            Gen(typeof(System.Object));
            Gen(typeof(System.Boolean));
            Gen(typeof(System.String));
            Gen(typeof(System.Byte));
            Gen(typeof(System.SByte));
            Gen(typeof(System.Int16));
            Gen(typeof(System.UInt16));
            Gen(typeof(System.Int32));
            Gen(typeof(System.UInt32));
            Gen(typeof(System.Int64));
            Gen(typeof(System.UInt64));
            Gen(typeof(System.Single));
            Gen(typeof(System.Double));
            #region Steam
            Gen(typeof(Steamworks.Friend));
            Gen(typeof(Steamworks.Ugc.Item));

            #endregion

            Gen(typeof(LuaSByte), "SByte");
            Gen(typeof(LuaByte), "Byte");
            Gen(typeof(LuaInt16), "Int16", new string[] { "Short" });
            Gen(typeof(LuaUInt16), "UInt16", new string[] { "UShort" });
            Gen(typeof(LuaInt32), "Int32");
            Gen(typeof(LuaUInt32), "UInt32");
            Gen(typeof(LuaInt64), "Int64");
            Gen(typeof(LuaUInt64), "UInt64");
            Gen(typeof(LuaSingle), "Single", new string[] { "Float" });
            Gen(typeof(LuaDouble), "Double");

            Gen(typeof(MathUtils));

            Gen(typeof(Rand));
            Gen(typeof(Rand.RandSync), null, new string[] { "RandSync" });

            Gen(typeof(PerformanceCounter));

            Gen(typeof(GameMain));

            Gen(typeof(SerializableProperty));

            #region String
            Gen(typeof(AddedPunctuationLString));
            Gen(typeof(CapitalizeLString));
            Gen(typeof(ConcatLString));
            Gen(typeof(FallbackLString));
            Gen(typeof(FormattedLString));
            Gen(typeof(InputTypeLString));
            Gen(typeof(JoinLString));
            Gen(typeof(LocalizedString));
            Gen(typeof(LowerLString));
            Gen(typeof(RawLString));
            Gen(typeof(ReplaceLString));
            Gen(typeof(ServerMsgLString));
            Gen(typeof(SplitLString));
            Gen(typeof(TagLString));
            Gen(typeof(TrimLString));
            Gen(typeof(UpperLString));

            Gen(typeof(RichString));
            Gen(typeof(StripRichTagsLString));
            Gen(typeof(RichTextData));

            Gen(typeof(TextManager));
            Gen(typeof(TextPack));

            Gen(typeof(Identifier));
            Gen(typeof(LanguageIdentifier));
            #endregion

            Gen(typeof(ContentFile));
            Gen(typeof(ContentPackage));
            Gen(typeof(ContentPackageManager));
            Gen(typeof(ContentPackageManager.PackageSource));
            Gen(typeof(ContentPackageManager.EnabledPackages));
            Gen(typeof(RegularPackage));
            Gen(typeof(CorePackage));
            Gen(typeof(ContentXElement));


            Gen(typeof(Camera));

            Gen(typeof(CauseOfDeathType));
            Gen(typeof(CauseOfDeath));

            Gen(typeof(SpawnType));
            Gen(typeof(WayPoint));

            Gen(typeof(Networking.ServerLog));
            Gen(typeof(Networking.ServerLog.MessageType), null, new string[] { "ServerLog_MessageType", "ServerLogMessageType" });

            Gen(typeof(PropertyConditional));
            Gen(typeof(StatusEffect));
            Gen(typeof(DelayedEffect));

            Gen(typeof(FireSource));
            Gen(typeof(DummyFireSource));

            Gen(typeof(Explosion));

            #region Enum
            Gen(typeof(TransitionMode));
            Gen(typeof(ActionType));
            Gen(typeof(AbilityEffectType));
            Gen(typeof(StatTypes));
            Gen(typeof(AbilityFlags));
            #endregion

            #region Game
            Gen(typeof(Screen));
            Gen(typeof(GameScreen));
            Gen(typeof(NetLobbyScreen));

            Gen(typeof(GameSettings));

            Gen(typeof(GameSession));

            //Data
            Gen(typeof(CampaignMetadata));
            Gen(typeof(CharacterCampaignData));
            Gen(typeof(Faction));
            Gen(typeof(FactionPrefab));
            Gen(typeof(Reputation));

            //Game Mode
            Gen(typeof(GameModePreset));
            Gen(typeof(GameMode));
#if CLIENT
            Gen(typeof(TestGameMode));
            Gen(typeof(TutorialMode));
#endif
            Gen(typeof(CampaignMode));
#if CLIENT
            Gen(typeof(SinglePlayerCampaign));
#endif
            Gen(typeof(MultiPlayerCampaign));
            Gen(typeof(CoOpMode));
            Gen(typeof(MissionMode));
            Gen(typeof(PvPMode));


            Gen(typeof(AutoItemPlacer));
            Gen(typeof(CargoManager));
            Gen(typeof(CrewManager));
            Gen(typeof(HireManager));
            Gen(typeof(MedicalClinic));
            Gen(typeof(ReadyCheck));

            #endregion

            Gen(typeof(GameDifficulty));
            Gen(typeof(StartingBalanceAmount));

            #region Level
            Gen(typeof(Level.InterestingPosition));
            Gen(typeof(Level.PositionType), null, new string[] { "PositionType" });
            Gen(typeof(Level));
            Gen(typeof(LevelData));
            Gen(typeof(LevelGenerationParams));
            Gen(typeof(LevelObjectManager));
            Gen(typeof(LevelObjectPrefab));
            Gen(typeof(LevelObject));
            Gen(typeof(LevelTrigger));
            Gen(typeof(LevelWall));
            Gen(typeof(DestructibleLevelWall));
            Gen(typeof(Biome));
            Gen(typeof(Map));
            Gen(typeof(Radiation));
            #endregion


            #region Location
            Gen(typeof(PriceInfo));
            Gen(typeof(LocationType));
            Gen(typeof(Location));
            Gen(typeof(LocationConnection));
            Gen(typeof(LocationTypeChange));
            #endregion


            #region Entity
            Gen(typeof(Entity));
            Gen(typeof(EntitySpawner));
            Gen(typeof(EntityGrid));

            Gen(typeof(MapEntityCategory));
            Gen(typeof(MapEntity));
            Gen(typeof(Prefab));
            Gen(typeof(PrefabWithUintIdentifier));
            Gen(typeof(MapEntityPrefab));
            Gen(typeof(CoreEntityPrefab));
            #endregion


            #region Character
            Gen(typeof(CharacterType));
            Gen(typeof(CharacterTeamType));
            Gen(typeof(CharacterPrefab));
            Gen(typeof(CharacterInfo));
            Gen(typeof(CharacterInfo.HeadInfo));
            Gen(typeof(CharacterInfo.HeadPreset));
            Gen(typeof(CharacterInfoPrefab));
            Gen(typeof(Character));
            Gen(typeof(AICharacter));
            Gen(typeof(CharacterHealth));
            Gen(typeof(CharacterHealth.LimbHealth));
            Gen(typeof(CharacterInventory));
            Gen(typeof(CharacterTalent));

            Gen(typeof(CharacterParams));
            Gen(typeof(CharacterParams.AIParams));
            Gen(typeof(CharacterParams.HealthParams));
            Gen(typeof(CharacterParams.InventoryParams));
            Gen(typeof(CharacterParams.ParticleParams));
            Gen(typeof(CharacterParams.SoundParams));
            Gen(typeof(CharacterParams.SubParam));
            Gen(typeof(CharacterParams.TargetParams));

            Gen(typeof(MapCreatures.Behavior.BallastFloraBehavior));
            Gen(typeof(MapCreatures.Behavior.BallastFloraBranch));
            Gen(typeof(PetBehavior));
            #endregion

            Gen(typeof(OrderCategory));
            Gen(typeof(OrderPrefab));
            Gen(typeof(Order));
            Gen(typeof(OrderTarget));

            #region AI
            Gen(typeof(AIState));

            Gen(typeof(AIController));
            Gen(typeof(EnemyAIController));
            Gen(typeof(HumanAIController));

            Gen(typeof(AITarget));
            Gen(typeof(AITargetMemory));

            Gen(typeof(AIChatMessage));
            Gen(typeof(AIObjectiveManager));
            Gen(typeof(AITrigger));

            Gen(typeof(AIObjective));
            Gen(typeof(AIObjectiveChargeBatteries));
            Gen(typeof(AIObjectiveCleanupItem));
            Gen(typeof(AIObjectiveCleanupItems));
            Gen(typeof(AIObjectiveCombat));
            Gen(typeof(AIObjectiveContainItem));
            Gen(typeof(AIObjectiveDecontainItem));
            Gen(typeof(AIObjectiveEscapeHandcuffs));
            Gen(typeof(AIObjectiveExtinguishFire));
            Gen(typeof(AIObjectiveExtinguishFires));
            Gen(typeof(AIObjectiveFightIntruders));
            Gen(typeof(AIObjectiveFindDivingGear));
            Gen(typeof(AIObjectiveFindSafety));
            Gen(typeof(AIObjectiveFixLeak));
            Gen(typeof(AIObjectiveFixLeaks));
            Gen(typeof(AIObjectiveGetItem));
            Gen(typeof(AIObjectiveGetItems));
            Gen(typeof(AIObjectiveGoTo));
            Gen(typeof(AIObjectiveIdle));
            Gen(typeof(AIObjectiveOperateItem));
            Gen(typeof(AIObjectivePrepare));
            Gen(typeof(AIObjectivePumpWater));
            Gen(typeof(AIObjectiveRepairItem));
            Gen(typeof(AIObjectiveRepairItems));
            Gen(typeof(AIObjectiveRescue));
            Gen(typeof(AIObjectiveRescueAll));
            Gen(typeof(AIObjectiveReturn));
            Gen(typeof(AIObjectiveCombat.CombatMode), null, new string[] { "CombatMode" });
            #endregion

            #region Ragdoll
            Gen(typeof(LimbType));
            Gen(typeof(Limb));
            Gen(typeof(LimbJoint));
            Gen(typeof(LimbPos));

            Gen(typeof(Ragdoll));
            Gen(typeof(AnimController));
            Gen(typeof(FishAnimController));
            Gen(typeof(HumanoidAnimController));

            Gen(typeof(EditableParams));
            Gen(typeof(RagdollParams));
            Gen(typeof(AnimationParams));

            Gen(typeof(SwimParams));
            Gen(typeof(GroundedMovementParams));

            Gen(typeof(HumanRagdollParams));
            Gen(typeof(HumanGroundedParams));
            Gen(typeof(HumanWalkParams));
            Gen(typeof(HumanRunParams));
            Gen(typeof(HumanCrouchParams));
            Gen(typeof(HumanSwimParams));
            Gen(typeof(HumanSwimFastParams));
            Gen(typeof(HumanSwimSlowParams));

            Gen(typeof(FishRagdollParams));
            Gen(typeof(FishWalkParams));
            Gen(typeof(FishGroundedParams));
            Gen(typeof(FishRunParams));
            Gen(typeof(FishSwimParams));
            Gen(typeof(FishSwimFastParams));
            Gen(typeof(FishSwimSlowParams));

            #endregion


            #region Skill
            Gen(typeof(Skill));
            Gen(typeof(SkillPrefab));
            Gen(typeof(SkillSettings));
            #endregion


            #region Job
            Gen(typeof(Job));
            Gen(typeof(JobPrefab));
            Gen(typeof(JobVariant));
            #endregion

            #region Decal
            Gen(typeof(Decal));
            Gen(typeof(DecalManager));
            Gen(typeof(DecalPrefab));
            #endregion


            #region Talent
            Gen(typeof(TalentPrefab));
            Gen(typeof(TalentOption));
            Gen(typeof(TalentSubTree));
            Gen(typeof(TalentTree));
            #endregion


            #region Item
            Gen(typeof(ItemPrefab));
            Gen(typeof(Item));
            Gen(typeof(ItemInventory));
            Gen(typeof(RelatedItem));
            #endregion


            #region Items.Components
            //Holdable
            Gen(typeof(Items.Components.Holdable));
            Gen(typeof(Items.Components.IdCard));
            Gen(typeof(Items.Components.LevelResource));
            Gen(typeof(Items.Components.MeleeWeapon));
            Gen(typeof(Items.Components.Pickable));
            Gen(typeof(Items.Components.Propulsion));
            Gen(typeof(Items.Components.RangedWeapon));
            Gen(typeof(Items.Components.RepairTool));
            Gen(typeof(Items.Components.Sprayer));
            Gen(typeof(Items.Components.Throwable));

            //Machine
            Gen(typeof(Items.Components.Controller));
            Gen(typeof(Items.Components.Deconstructor));
            Gen(typeof(Items.Components.Engine));
            Gen(typeof(Items.Components.Fabricator));
            Gen(typeof(Items.Components.MiniMap));
            Gen(typeof(Items.Components.OutpostTerminal));
            Gen(typeof(Items.Components.OxygenGenerator));
            Gen(typeof(Items.Components.Pump));
            Gen(typeof(Items.Components.Reactor));
            Gen(typeof(Items.Components.Sonar));
            Gen(typeof(Items.Components.SonarTransducer));
            Gen(typeof(Items.Components.Steering));
            Gen(typeof(Items.Components.Vent));

            //Power
            Gen(typeof(Items.Components.PowerContainer));
            Gen(typeof(Items.Components.Powered));
            Gen(typeof(Items.Components.PowerTransfer));

            //Signal
            Gen(typeof(Items.Components.AdderComponent));
            Gen(typeof(Items.Components.AndComponent));
            Gen(typeof(Items.Components.ArithmeticComponent));
            Gen(typeof(Items.Components.ButtonTerminal));
            Gen(typeof(Items.Components.ColorComponent));
            Gen(typeof(Items.Components.ConcatComponent));
            Gen(typeof(Items.Components.Connection));
            Gen(typeof(Items.Components.ConnectionPanel));
            Gen(typeof(Items.Components.CustomInterface));
            Gen(typeof(Items.Components.DelayComponent));
            Gen(typeof(Items.Components.DivideComponent));
            Gen(typeof(Items.Components.EqualsComponent));
            Gen(typeof(Items.Components.ExponentiationComponent));
            Gen(typeof(Items.Components.FunctionComponent));
            Gen(typeof(Items.Components.GreaterComponent));
            Gen(typeof(Items.Components.LightComponent));
            Gen(typeof(Items.Components.MemoryComponent));
            Gen(typeof(Items.Components.ModuloComponent));
            Gen(typeof(Items.Components.MotionSensor));
            Gen(typeof(Items.Components.MultiplyComponent));
            Gen(typeof(Items.Components.NotComponent));
            Gen(typeof(Items.Components.OrComponent));
            Gen(typeof(Items.Components.OscillatorComponent));
            Gen(typeof(Items.Components.OxygenDetector));
            Gen(typeof(Items.Components.RegExFindComponent));
            Gen(typeof(Items.Components.RelayComponent));
            Gen(typeof(Items.Components.Signal));
            Gen(typeof(Items.Components.SignalCheckComponent));
            Gen(typeof(Items.Components.SmokeDetector));
            Gen(typeof(Items.Components.StringComponent));
            Gen(typeof(Items.Components.SubtractComponent));
            Gen(typeof(Items.Components.Terminal));
            Gen(typeof(Items.Components.TrigonometricFunctionComponent));
            Gen(typeof(Items.Components.WaterDetector));
            Gen(typeof(Items.Components.WifiComponent));
            Gen(typeof(Items.Components.Wire));
            Gen(typeof(Items.Components.XorComponent));

            //Commonly
            Gen(typeof(Items.Components.DockingPort));
            Gen(typeof(Items.Components.Door));
            Gen(typeof(Items.Components.ElectricalDischarger));
            Gen(typeof(Items.Components.EntitySpawnerComponent));
            Gen(typeof(Items.Components.GeneticMaterial));
            Gen(typeof(Items.Components.Growable));
            Gen(typeof(Items.Components.ProducedItem));
            Gen(typeof(Items.Components.VineTile));
            Gen(typeof(Items.Components.GrowthSideExtension));
            Gen(typeof(Items.Components.ItemComponent));
            Gen(typeof(Items.Components.ItemContainer));
            Gen(typeof(Items.Components.ItemLabel));
            Gen(typeof(Items.Components.Ladder));
            Gen(typeof(Items.Components.NameTag));
            Gen(typeof(Items.Components.Planter));
            Gen(typeof(Items.Components.Projectile));
            Gen(typeof(Items.Components.Quality));
            Gen(typeof(Items.Components.RemoteController));
            Gen(typeof(Items.Components.Repairable));
            Gen(typeof(Items.Components.Rope));
            Gen(typeof(Items.Components.Scanner));
            Gen(typeof(Items.Components.StatusHUD));
            Gen(typeof(Items.Components.TriggerComponent));
            Gen(typeof(Items.Components.Turret));
            Gen(typeof(Items.Components.Wearable));

            #endregion


            #region Submarine
            Gen(typeof(SubmarineInfo));
            Gen(typeof(Submarine));
            Gen(typeof(SubmarineBody));
            #endregion

            #region Structure
            Gen(typeof(Structure));
            Gen(typeof(StructurePrefab));
            #endregion

            #region Affliction
            Gen(typeof(AfflictionPrefab));
            Gen(typeof(Affliction));
            Gen(typeof(AfflictionPrefabHusk));
            Gen(typeof(AfflictionHusk));
            Gen(typeof(AfflictionBleeding));
            Gen(typeof(AfflictionPsychosis));
            Gen(typeof(AfflictionSpaceHerpes));
            #endregion

            #region Attack
            Gen(typeof(AttackContext));
            Gen(typeof(AttackPattern));
            Gen(typeof(AttackTarget));
            Gen(typeof(Attack));
            Gen(typeof(AttackResult));
            #endregion

            #region Inventroy and slot
            Gen(typeof(InvSlotType));
#if CLIENT
            Gen(typeof(InventorySlotItem));
            Gen(typeof(VisualSlot));
#endif
            Gen(typeof(Inventory));
            #endregion

            #region Command
            Gen(typeof(Command));
#if CLIENT
            Gen(typeof(TransformCommand));
            Gen(typeof(AddOrDeleteCommand));
            Gen(typeof(PropertyCommand));
            Gen(typeof(InventoryMoveCommand));
            Gen(typeof(InventoryPlaceCommand));
#endif
            #endregion

            #region Traitor
            Gen(typeof(TraitorMissionPrefab));
            Gen(typeof(TraitorMissionResult));
#if SERVER
            Gen(typeof(Networking.TraitorMessageType));
            Gen(typeof(TraitorManager));
            Gen(typeof(Traitor));
            Gen(typeof(Traitor.TraitorMission));
            Gen(typeof(Traitor.Objective));
            Gen(typeof(Traitor.Goal));
#endif
            #endregion

            #region Physic
            Gen(typeof(FarseerPhysics.Dynamics.World));
            Gen(typeof(FarseerPhysics.Dynamics.Fixture));
            Gen(typeof(Physics));
            Gen(typeof(PhysicsBody));

            Gen(typeof(Hull));
            Gen(typeof(Gap));
            #endregion

            #region Geometry
#if CLIENT
            Gen(typeof(Microsoft.Xna.Framework.Graphics.SpriteBatch));
            Gen(typeof(Microsoft.Xna.Framework.Graphics.Texture2D));
#endif
            Gen(typeof(Microsoft.Xna.Framework.Matrix), "Matrix");
            Gen(typeof(Microsoft.Xna.Framework.Vector2), "Vector2");
            Gen(typeof(Microsoft.Xna.Framework.Vector3), "Vector3");
            Gen(typeof(Microsoft.Xna.Framework.Vector4), "Vector4");
            Gen(typeof(Microsoft.Xna.Framework.Color), "Color");
            Gen(typeof(Microsoft.Xna.Framework.Point), "Point");
            Gen(typeof(Microsoft.Xna.Framework.Rectangle), "Rectangle");
            #endregion

            #region Sprite
            Gen(typeof(Sprite));
            Gen(typeof(SpriteSheet));
            Gen(typeof(ConditionalSprite));
            Gen(typeof(WearableType));
            Gen(typeof(WearableSprite));
            Gen(typeof(DeformableSprite));

#if CLIENT
            Gen(typeof(SpriteRecorder));
            Gen(typeof(DecorativeSprite));
            Gen(typeof(BrokenItemSprite));
            Gen(typeof(ContainedItemSprite));
            Gen(typeof(VineSprite));
#endif
            #endregion

            #region Craft
            Gen(typeof(DeconstructItem));
            Gen(typeof(PreferredContainer));
            Gen(typeof(SwappableItem));
            Gen(typeof(FabricationRecipe));
            Gen(typeof(FabricationRecipe.RequiredItemByIdentifier));
            Gen(typeof(FabricationRecipe.RequiredItemByTag));
            Gen(typeof(FabricationRecipe.RequiredItem));
            #endregion

            #region Upgrade
            Gen(typeof(UpgradeCategory));
            Gen(typeof(UpgradePrice));
            Gen(typeof(UpgradeManager));
            Gen(typeof(UpgradePrefab));
            Gen(typeof(Upgrade));
            Gen(typeof(PurchasedUpgrade));
            #endregion

            #region Networking
            Gen(typeof(Item.EventType));
            Gen(typeof(Item.ComponentStateEventData));
            Gen(typeof(Item.InventoryStateEventData));
            Gen(typeof(Item.ChangePropertyEventData));
            Gen(typeof(Item.ApplyStatusEffectEventData));

            Gen(typeof(Character.EventType));
            Gen(typeof(Character.InventoryStateEventData));
            Gen(typeof(Character.ControlEventData));
            Gen(typeof(Character.CharacterStatusEventData));
            Gen(typeof(Character.TreatmentEventData));
            Gen(typeof(Character.SetAttackTargetEventData));
            Gen(typeof(Character.ExecuteAttackEventData));
            Gen(typeof(Character.AssignCampaignInteractionEventData));
            Gen(typeof(Character.ObjectiveManagerStateEventData));
            Gen(typeof(Character.AddToCrewEventData));
            Gen(typeof(Character.UpdateExperienceEventData));
            Gen(typeof(Character.UpdatePermanentStatsEventData));
            Gen(typeof(Character.UpdateSkillsEventData));
            Gen(typeof(Character.UpdateTalentsEventData));

            Gen(typeof(Networking.NetConfig));
            Gen(typeof(Networking.ServerSettings));

            Gen(typeof(Networking.ChatMessageType));
            Gen(typeof(Networking.ChatMessage));

            Gen(typeof(Networking.ServerPacketHeader));
            Gen(typeof(Networking.ClientPacketHeader));
            Gen(typeof(Networking.DeliveryMethod));
            Gen(typeof(Networking.IWriteMessage));
            Gen(typeof(Networking.WriteOnlyMessage));
            Gen(typeof(Networking.IReadMessage));
            Gen(typeof(Networking.ReadOnlyMessage));
            Gen(typeof(Networking.ReadWriteMessage));

            Gen(typeof(Networking.ClientPermissions));
            Gen(typeof(Networking.Client));
            Gen(typeof(Networking.TempClient));
            Gen(typeof(INetSerializableStruct));

            Gen(typeof(Networking.NetworkConnectionStatus));
            Gen(typeof(Networking.NetworkConnection));
            Gen(typeof(Networking.PipeConnection));
            Gen(typeof(Networking.LidgrenConnection));
            Gen(typeof(Networking.SteamP2PConnection));

            Gen(typeof(Networking.NetworkMember));

            Gen(typeof(Networking.BanList));
            Gen(typeof(Networking.BannedPlayer));

#if SERVER
            Gen(typeof(Networking.GameServer));

            Gen(typeof(Networking.ServerPeer));
            Gen(typeof(Networking.LidgrenServerPeer));
            Gen(typeof(Networking.SteamP2PServerPeer));
#endif

#if CLIENT
            Gen(typeof(Networking.GameClient));

            Gen(typeof(Networking.ClientPeer));
            Gen(typeof(Networking.LidgrenClientPeer));
            Gen(typeof(Networking.SteamP2PClientPeer));
            Gen(typeof(Networking.SteamP2POwnerPeer));
#endif
            #endregion

            #region Keys
            Gen(typeof(InputType));
            Gen(typeof(Key));
#if CLIENT
            Gen(typeof(EventInput.KeyboardDispatcher));
            Gen(typeof(EventInput.KeyEventArgs));
            Gen(typeof(Microsoft.Xna.Framework.Input.Keys), "Keys");
#endif
            #endregion

            #region Particles
#if CLIENT
            Gen(typeof(Particles.Particle));
            Gen(typeof(Particles.ParticleEmitter));
            Gen(typeof(Particles.ParticleManager));
            Gen(typeof(Particles.ParticlePrefab));
#endif
            #endregion

            #region GUI
            Gen(typeof(NumberType));

#if CLIENT
            Gen(typeof(ScalableFont));

            Gen(typeof(Anchor));
            Gen(typeof(Pivot));
            Gen(typeof(ScaleBasis));

            Gen(typeof(ChatBox));
            Gen(typeof(CrewManagement));
            Gen(typeof(FileSelection));
            Gen(typeof(Graph));

            Gen(typeof(GUI), "Barotrauma_GUI");

            Gen(typeof(GUIComponentStyle));
            Gen(typeof(SpriteFallBackState));

            Gen(typeof(GUISoundType));
            Gen(typeof(CursorState));

            Gen(typeof(PlayerInput));

            Gen(typeof(GUIFont));
            Gen(typeof(GUIFontPrefab));

            Gen(typeof(GUISpritePrefab));
            Gen(typeof(GUISprite));
            Gen(typeof(GUISpriteSheetPrefab));
            Gen(typeof(GUISpriteSheet));
            Gen(typeof(GUICursorPrefab));
            Gen(typeof(GUICursor));

            Gen(typeof(GUIButton));
            Gen(typeof(GUICanvas));
            Gen(typeof(GUIColorPicker));
            Gen(typeof(GUIComponent));
            Gen(typeof(GUIComponent.ComponentState));
            Gen(typeof(GUIContextMenu));
            Gen(typeof(GUICustomComponent));
            Gen(typeof(GUIDropDown));
            Gen(typeof(GUIFrame));
            Gen(typeof(GUIImage));
            Gen(typeof(GUILayoutGroup));
            Gen(typeof(GUIListBox));
            Gen(typeof(GUIMessage));
            Gen(typeof(GUIMessageBox));
            Gen(typeof(GUINumberInput));

            Gen(typeof(GUIProgressBar));
            Gen(typeof(GUIRadioButtonGroup));
            Gen(typeof(GUIScissorComponent));
            Gen(typeof(GUIScrollBar));
            Gen(typeof(GUIStyle));
            Gen(typeof(GUITextBlock));
            Gen(typeof(GUITextBox));
            Gen(typeof(GUITickBox));
            Gen(typeof(HUDLayoutSettings));
            Gen(typeof(LoadingScreen));
            Gen(typeof(MedicalClinicUI));
            Gen(typeof(ParamsEditor));
            Gen(typeof(RectTransform));
            Gen(typeof(ShapeExtensions));
            Gen(typeof(Store));
            Gen(typeof(SubmarineSelection));
            Gen(typeof(TabMenu));
            Gen(typeof(UISprite));
            Gen(typeof(UpgradeStore));
            Gen(typeof(VideoPlayer));
            Gen(typeof(VotingInterface));
            Gen(typeof(Widget));
#endif
            Gen(typeof(Alignment));
            #endregion

            #region Lights & Sounds
            Gen(typeof(ChatMode));
            Gen(typeof(Networking.VoipConfig));
            Gen(typeof(Networking.VoipQueue));
#if SERVER
            Gen(typeof(Networking.VoipServer));
#endif
#if CLIENT
            Gen(typeof(Lights.LightManager));
            Gen(typeof(Lights.LightSource));
            Gen(typeof(Lights.LightSourceParams));


            Gen(typeof(Sounds.SoundManager));
            Gen(typeof(Sounds.OggSound));
            Gen(typeof(Sounds.VideoSound));
            Gen(typeof(Sounds.VoipSound));
            Gen(typeof(Networking.VoipClient));
            Gen(typeof(Networking.VoipCapture));
            Gen(typeof(Sounds.SoundChannel));
            Gen(typeof(RoundSound));
            Gen(typeof(Items.Components.ItemSound));

            Gen(typeof(Sounds.LowpassFilter));
            Gen(typeof(Sounds.HighpassFilter));
            Gen(typeof(Sounds.BandpassFilter));
            Gen(typeof(Sounds.NotchFilter));
            Gen(typeof(Sounds.HighShelfFilter));
            Gen(typeof(Sounds.LowShelfFilter));
            Gen(typeof(Sounds.PeakFilter));
#endif
            #endregion

            #region Screen
#if CLIENT
            Gen(typeof(Barotrauma.CampaignEndScreen));
            Gen(typeof(Barotrauma.EditorScreen));
            Gen(typeof(Barotrauma.EventEditorScreen));
            Gen(typeof(Barotrauma.LevelEditorScreen));
            Gen(typeof(Barotrauma.MainMenuScreen));
            Gen(typeof(Barotrauma.ParticleEditorScreen));
            Gen(typeof(Barotrauma.RoundSummaryScreen));
            Gen(typeof(Barotrauma.ServerListScreen));
            Gen(typeof(Barotrauma.SpriteEditorScreen));
            Gen(typeof(Barotrauma.SubEditorScreen));
            Gen(typeof(Barotrauma.TestScreen));
            Gen(typeof(Barotrauma.CharacterEditor.CharacterEditorScreen));
#endif
            #endregion

            Gen(typeof(KarmaManager));
            Gen(typeof(RespawnManager));

            Gen(typeof(DebugConsole));
            Gen(typeof(DebugConsole.Command));

            Gen(typeof(LuaUserData), "LuaUserData");
            Gen(typeof(LuaGame), "Game");
            Gen(typeof(LuaCsPatch));
            Gen(typeof(LuaCsAction));
            Gen(typeof(LuaCsFunc));
            Gen(typeof(LuaCsPatchFunc));
            Gen(typeof(LuaCsHook), "Hook");
            Gen(typeof(LuaCsHook.HookMethodType), "Hook.HookMethodType");
            Gen(typeof(LuaCsHook.ParameterTable), "Hook.ParameterTable");
            Gen(typeof(LuaCsTimer), "Timer");
            Gen(typeof(LuaCsFile), "File");
            Gen(typeof(LuaCsNetworking), "Networking");
            Gen(typeof(LuaCsSteam), "Steam");
            Gen(typeof(LuaCsPerformanceCounter), "PerformanceCounter");
            Gen(typeof(LuaCsSetup.LuaCsModStore), "ModStore");
            Gen(typeof(LuaCsSetup.LuaCsModStore.CsModStore), "ModStore.CsModStore");
            Gen(typeof(LuaCsSetup.LuaCsModStore.LuaModStore), "ModStore.LuaModStore");
            Gen(typeof(MoonSharp.Interpreter.Interop.IUserDataDescriptor));

            AliasAnnotation.Gen();
            GenCommon();
        }

        public static void GenCommon()
        {
            var global = new StringBuilder($"---@meta\n\n");

            foreach (var className in SingleLuaFileClassNameList)
            {
                GlobalLuaDef.RemoveAll(def => def.targetClassName == className);
            }

            foreach (var def in DefaultGLuaDef)
            {
                GlobalLuaDef.RemoveAll(subDef => subDef.targetClassName == def.targetClassName);
            }

            foreach (var def in GlobalLuaDef)
            {
                var str = def.baseClassName == "" ? $"---@class {def.targetClassName}\n" : $"---@class {def.targetClassName} : {def.baseClassName}\n";
                global.Insert(9, str);
            }

            try
            {
                File.WriteAllText(@$"{BLuaDocPath}/Global.lua", global.ToString());
            }
            catch (Exception ex)
            {
                File.WriteAllText("reportexception_global.txt", ex.Message);
                throw;
            }

            foreach (var (fileName, builder) in ClassDefinition)
            {
                if (OverloadedOperatorAnnotations.TryGetValue(fileName, out StringBuilder ops))
                {
                    ops.Remove(ops.Length - 1, 1); // remove the last newline, @field must be defined after @class
                    builder.Replace("[placeHolder:operators]", ops.ToString());
                }
                else
                {
                    builder.Replace("[placeHolder:operators]", "---");
                }

                try
                {
                    File.WriteAllText($@"{BLuaDocPath}/{fileName}.lua", builder.ToString());
                }
                catch (Exception ex)
                {
                    File.WriteAllText("CrashReport.log", ex.Message);
                    throw;
                }
            }
        }

        private static void ExplanField(StringBuilder builder, FieldInfo field)
        {
            var metadata = ClassMetadata.Obtain(field.FieldType);
            metadata.CollectAllToGlobal();
            ExplanAnnotationPrefix(builder);
            ExplanNewLine(builder);
            ExplanAnnotationPrefix(builder);
            builder.Append("`Field` ");
            if (field.IsPublic) { builder.Append("`Public`"); }
            else if (field.IsPrivate) { builder.Append("`Private`"); }
            else { builder.Append("`NonPublic`"); }
            builder.Append(" " + (field.IsStatic ? "`Static`" : "`Instance`"));
            ExplanNewLine(builder);
            ExplanAnnotationPrefix(builder);
            builder.Append($"{metadata.TargetType.FullName}");
            ExplanNewLine(builder);
            var (_, typeName) = metadata.GetLuaName(LUA_NAME_TYPE);
            ExplanAnnotationField(builder, field.Name, typeName);
        }

        private static void ExplanProperty(StringBuilder builder, PropertyInfo property)
        {
            var metadata = ClassMetadata.Obtain(property.PropertyType);
            metadata.CollectAllToGlobal();

            ExplanAnnotationPrefix(builder);
            ExplanNewLine(builder);

            if (property.GetMethod != null)
            {
                ExplanAnnotationPrefix(builder);
                builder.Append("`Getter` ");
                ExplanMethodModifiers(builder, GetMethodModifiers(property.GetMethod));
                ExplanNewLine(builder);
            }

            if (property.SetMethod != null)
            {
                ExplanAnnotationPrefix(builder);
                builder.Append("`Setter` ");
                ExplanMethodModifiers(builder, GetMethodModifiers(property.SetMethod));
                ExplanNewLine(builder);
            }

            ExplanAnnotationPrefix(builder);
            builder.Append($"{metadata.TargetType.FullName}");
            ExplanNewLine(builder);
            var (_, typeName) = metadata.GetLuaName(LUA_NAME_TYPE);
            ExplanAnnotationProperty(builder, property.Name, typeName);
        }

        private static string[] _overloadedOperatorMethodNames = { "op_UnaryNegation", "op_Addition", "op_Subtraction", "op_Multiply", "op_Division" };
        private static bool IsOverloadedOperatorMethod(MethodInfo method)
        {
            return method.IsSpecialName && _overloadedOperatorMethodNames.Contains(method.Name);
        }

        private static StringBuilder ObtainOverloadedOperatorAnnotations(Type type)
        {
            string fileName = GetLuaClassFileName(type);
            if (OverloadedOperatorAnnotations.ContainsKey(fileName))
            {
                return OverloadedOperatorAnnotations[fileName];
            }
            else
            {
                var opsBuilder = new StringBuilder();
                ExplanAnnotationPrefix(opsBuilder);
                ExplanNewLine(opsBuilder);
                OverloadedOperatorAnnotations.Add(fileName, opsBuilder);
                return opsBuilder;
            }
        }

        private static void ExplanAnnotationPrefix(StringBuilder builder) { builder.Append("---"); }
        private static void ExplanAnnotationField(StringBuilder builder, string fieldName, string typeName) { ExplanAnnotationPrefix(builder); builder.Append($"@field {fieldName} {typeName}"); }
        private static void ExplanAnnotationProperty(StringBuilder builder, string propertyName, string typeName) { ExplanAnnotationPrefix(builder); builder.Append($"@field {propertyName} {typeName}"); }
        private static void ExplanAnnotationUnOperator(StringBuilder builder, string op, string outType) { ExplanAnnotationPrefix(builder); builder.Append($"@operator {op}:{outType}"); }
        private static void ExplanAnnotationBinOperator(StringBuilder builder, string op, string inpType, string outType) { ExplanAnnotationPrefix(builder); builder.Append($"@operator {op}({inpType}):{outType}"); }
        private static void ExplanNewLine(StringBuilder builder, int line = 1) { for (int i = 0; i < line; i++) builder.Append("\n"); }
        private static bool IsParamsParam(ParameterInfo param) => param.GetCustomAttribute<ParamArrayAttribute>(false) != null;
        private static bool IsOptionalParam(ParameterInfo param) => param.GetCustomAttribute<OptionalAttribute>(false) != null;
        private static string MakeNonConflictParam(string name) { if (LUAKEYWORDS.Contains(name)) return $"_{name}"; else return name; }
        private static string MakeParamsParam(string name) => name.Substring(0, name.Length - 2);
        private static string MakeOverloadMethodParamsParam(string name) => $"...:{MakeParamsParam(name)}";
        private static string MakePrimaryMethodParamsParam(string name) => $"---@vararg {MakeParamsParam(name)}";

        private static void ExplanOverloadMethodStartForGenLuaType(StringBuilder builder) => builder.Append(@"fun(");
        private static void ExplanOverloadMethodStart(StringBuilder builder) => builder.Append(@"---@overload fun(");
        private static void ExplanOverloadMethodEnd(StringBuilder builder, MethodInfo method)
        {
            if (method.ReturnType != typeof(void))
            {
                var metadata = ClassMetadata.Obtain(method.ReturnType);
                metadata.CollectAllToGlobal();
                var (_, typeName) = metadata.GetLuaName(LUA_NAME_TYPE);
                builder.Append($"):{typeName}");
            }
            else
            {
                builder.Append(")");
            }
        }
        private static void ExplanOverloadConstructorEnd(StringBuilder builder, string className)
        {
            builder.Append($"):{className}");
        }
        private static void ExplanOverloadMethodParam(StringBuilder builder, ParameterInfo parameter)
        {
            var paramName = parameter.Name;
            paramName = MakeNonConflictParam(paramName);
            if (IsOptionalParam(parameter)) { paramName += '?'; }
            var metadata = ClassMetadata.Obtain(parameter.ParameterType);
            metadata.CollectAllToGlobal();
            var typeName = AliasAnnotation.Analyze(parameter) ?? metadata.GetLuaName(LUA_NAME_TYPE).Name;
            builder.Append(IsParamsParam(parameter) ? MakeOverloadMethodParamsParam(typeName) : $"{paramName}:{typeName}");
        }

        private static void ExplanPrimaryMethodEnd(StringBuilder builder, MethodInfo method)
        {
            var metadata = ClassMetadata.Obtain(method.ReturnType);
            metadata.CollectAllToGlobal();
            var (_, typeName) = metadata.GetLuaName(LUA_NAME_TYPE);
            builder.Append($"---@return {typeName}");
        }
        private static void ExplanPrimaryConstructorEnd(StringBuilder builder, string className)
        {
            builder.Append($"---@return {className}");
        }
        private static void ExplanPrimaryMethodParam(StringBuilder builder, ParameterInfo parameter)
        {
            var paramName = parameter.Name;
            paramName = MakeNonConflictParam(paramName);
            if (IsOptionalParam(parameter)) { paramName += '?'; }
            var metadata = ClassMetadata.Obtain(parameter.ParameterType);
            metadata.CollectAllToGlobal();
            var typeName = AliasAnnotation.Analyze(parameter) ?? metadata.GetLuaName(LUA_NAME_TYPE).Name;
            builder.Append(IsParamsParam(parameter) ? $"{MakePrimaryMethodParamsParam(typeName)}" : $"---@param {paramName} {typeName}");
        }

        private static uint GetMethodModifiers(MethodBase methodBase)
        {
            uint result = 0x00;
            if (methodBase.IsPublic) { result |= 0x01; }
            if (methodBase.IsPrivate) { result |= 0x02; }
            if (methodBase.IsStatic) { result |= 0x04; }
            if (methodBase.IsAbstract) { result |= 0x08; }
            if (methodBase.IsVirtual) { result |= 0x10; }
            return result;
        }

        private static void ExplanMethodModifiers(StringBuilder builder, uint modifiers)
        {
            if ((modifiers & 0x01) > 0) { builder.Append("`Public`"); }
            else if ((modifiers & 0x02) > 0) { builder.Append("`Private`"); }
            else { builder.Append("`NonPublic`"); }
            builder.Append(" " + (((modifiers & 0x04) > 0) ? "`Static`" : "`Instance`"));
            if ((modifiers & 0x08) > 0) { builder.Append(" `Abstract`"); }
            if ((modifiers & 0x10) > 0) { builder.Append(" `Virtual`"); }
        }

        private static string GetLuaClassFileName(Type type)
        {
            var nameSpacePart = type.Namespace;
            var declarT = type.DeclaringType;
            var prefix = "";
            while (declarT != null)
            {
                prefix = $@"{declarT.Name}.{prefix}";
                declarT = declarT.DeclaringType;
            }

            return nameSpacePart != ""
                ? (prefix != ""
                    ? $@"{nameSpacePart}.{prefix}{type.Name}"
                    : $@"{nameSpacePart}.{type.Name}"
                )
                : (prefix != ""
                    ? $@"{prefix}{type.Name}"
                    : $@"{type.Name}"
                );
        }

        private static void ExplanMethods(StringBuilder builder, string className, string tableName, MethodBase[] methods, string methodName)
        {
            var methodSB = new StringBuilder();
            for (int i = 0; i < methods.Length; i++)
            {
                var method = methods[i];
                var paramList = new List<string>();
                var parameters = method.GetParameters();
                for (var j = 0; j < parameters.Length; j++)
                {
                    var parameter = parameters[j];
                    paramList.Add(MakeNonConflictParam(parameter.Name));
                    if (i != methods.Length - 1) // belong to the other overload methods
                    {

                        if (j == 0) ExplanOverloadMethodStart(methodSB);
                        ExplanOverloadMethodParam(methodSB, parameter);
                        if (j != parameters.Length - 1)
                        {
                            methodSB.Append(", ");
                        }
                        else
                        {
                            if (method is MethodInfo)
                            {
                                ExplanOverloadMethodEnd(methodSB, method as MethodInfo);
                            }
                            else
                            {
                                ExplanOverloadConstructorEnd(methodSB, className);
                            }

                            ExplanNewLine(methodSB);
                        }
                    }
                    else // the default overload method
                    {
                        ExplanPrimaryMethodParam(methodSB, parameter);
                        ExplanNewLine(methodSB);

                        if (j == parameters.Length - 1)
                        {
                            if (method is MethodInfo)
                            {
                                var mi = method as MethodInfo;
                                if (mi.ReturnType != typeof(void))
                                {
                                    ExplanPrimaryMethodEnd(methodSB, mi);
                                    ExplanNewLine(methodSB);
                                }
                            }
                            else
                            {
                                ExplanPrimaryConstructorEnd(methodSB, className);
                                ExplanNewLine(methodSB);
                            }
                        }
                    }
                }

                if (i == methods.Length - 1)
                {
                    builder.Append(methodSB);
                    string methodBaseName = methodName == null ? "" : $".{methodName}";
                    string paramSquence = paramList.ToArray().Aggregate("", (state, value) =>
                    {
                        if (state.Equals("")) return value;
                        return $"{state}, {value}";
                    });
                    builder.Append($"function {tableName}{methodBaseName}({paramSquence}) end");
                    ExplanNewLine(builder, 2);
                }
            }
        }

        public static void Gen(Type targetType, string aliasTable = null, string[] minorTableNames = null)
        {
            var metadata = ClassMetadata.Obtain(targetType);
            var builder = new StringBuilder();
            string tableName = aliasTable ?? metadata.GetDefaultTableName();
            var (_, className) = metadata.GetLuaName(tryGetMappedName: false);
            SingleLuaFileClassNameList.Add(className);

            builder.Append($"---@meta");
            ExplanNewLine(builder, 2);

            builder.Append($"---'{targetType.FullName}'");
            ExplanNewLine(builder);

            bool nonGenericBaseType = targetType.BaseType != null && !targetType.BaseType.IsGenericType;

            if (nonGenericBaseType)
            {
                var subMetadata = ClassMetadata.Obtain(targetType.BaseType);
                var (_, subClassName) = subMetadata.GetLuaName();
                subMetadata.CollectAllToGlobal();
                builder.Append($"---@class {className} : {subClassName}");
                ExplanNewLine(builder);
            }
            else
            {
                builder.Append($"---@class {className}");
                ExplanNewLine(builder);
            }

            builder.Append("[placeHolder:operators]");
            ExplanNewLine(builder);

            const BindingFlags Flags = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            var fields = nonGenericBaseType ? targetType.GetFields(Flags | BindingFlags.DeclaredOnly) : targetType.GetFields(Flags);

            foreach (var field in fields.Where(f => !f.Name.Contains("k__BackingField")).ToList())
            {
                ExplanField(builder, field);
                ExplanNewLine(builder);
            }

            var properties = nonGenericBaseType ? targetType.GetProperties(Flags | BindingFlags.DeclaredOnly) : targetType.GetProperties(Flags);

            foreach (var prop in properties)
            {
                ExplanProperty(builder, prop);
                ExplanNewLine(builder);
            }

            builder.Append($"{tableName}={{}}");
            ExplanNewLine(builder, 2);

            var methods = nonGenericBaseType ? targetType.GetMethods(Flags | BindingFlags.DeclaredOnly) : targetType.GetMethods(Flags);
            if (methods.Length > 0)
            {
                foreach (var groupMethod in (
                        from method in methods
                        where !method.Name.StartsWith("get_") && !method.Name.StartsWith("set_")
                        where !Regex.IsMatch(method.Name, @"<.*?>")
                        group method by method.Name
                    )
                )
                {
                    string methodName = null;
                    if (Regex.Match(groupMethod.Key, @"\.(?<name>[^\.]+)$").Groups.TryGetValue("name", out Group group))
                    {
                        methodName = group.Value;
                    }
                    else
                    {
                        methodName = groupMethod.Key;
                    }

                    foreach (var groupMethodByModifers in (
                            from method in groupMethod.ToArray()
                            group method by GetMethodModifiers(method)
                        )
                    )
                    {
                        ExplanAnnotationPrefix(builder);
                        ExplanMethodModifiers(builder, groupMethodByModifers.Key);
                        ExplanNewLine(builder);
                        ExplanMethods(builder, className, tableName, groupMethodByModifers.ToArray(), methodName);
                    }
                }
            }

            var constructors = targetType.GetConstructors(Flags | BindingFlags.DeclaredOnly).ToArray();

            if (constructors.Length > 0)
            {
                foreach (var groupConstructorByModifers in (
                            from ctor in constructors
                            group ctor by GetMethodModifiers(ctor)
                        )
                    )
                {
                    ExplanAnnotationPrefix(builder);
                    ExplanMethodModifiers(builder, groupConstructorByModifers.Key);
                    ExplanNewLine(builder);
                    ExplanMethods(builder, className, tableName, constructors, null);

                    ExplanAnnotationPrefix(builder);
                    ExplanMethodModifiers(builder, groupConstructorByModifers.Key);
                    ExplanNewLine(builder);
                    ExplanMethods(builder, className, tableName, constructors, "__new");
                }
            }

            if (minorTableNames != null)
            {
                foreach (var minorTableName in minorTableNames)
                {
                    builder.Append($"{minorTableName} = {tableName}");
                    ExplanNewLine(builder);
                }
            }

            foreach (var method in methods)
            {
                if (!IsOverloadedOperatorMethod(method)) { continue; }

                switch (method.Name)
                {
                    case "op_UnaryNegation":
                        ExplanUnOps(method, targetType, "unm");
                        break;
                    case "op_Addition":
                        ExplanBinOps(method, targetType, "add");
                        break;
                    case "op_Subtraction":
                        ExplanBinOps(method, targetType, "sub");
                        break;
                    case "op_Multiply":
                        ExplanBinOps(method, targetType, "mul");
                        break;
                    case "op_Division":
                        ExplanBinOps(method, targetType, "div");
                        break;
                    default:
                        break;
                }

                void ExplanBinOps(MethodInfo method, Type type, string op)
                {
                    var tgtOps = (method.GetParameters()[0].ParameterType == type)
                        ? ObtainOverloadedOperatorAnnotations(targetType)
                        : ObtainOverloadedOperatorAnnotations(method.GetParameters()[0].ParameterType);
                    ExplanAnnotationBinOperator(
                        tgtOps,
                        op,
                        ClassMetadata.Obtain(method.GetParameters()[1].ParameterType).GetLuaName(LUA_NAME_TYPE).Name,
                        ClassMetadata.Obtain(method.ReturnType).GetLuaName(LUA_NAME_TYPE).Name);
                    ExplanNewLine(tgtOps);
                }

                void ExplanUnOps(MethodInfo method, Type type, string op)
                {
                    var tgtOps = ObtainOverloadedOperatorAnnotations(targetType);
                    ExplanAnnotationUnOperator(
                        tgtOps,
                        op,
                        ClassMetadata.Obtain(method.ReturnType).GetLuaName(LUA_NAME_TYPE).Name);
                    ExplanNewLine(tgtOps);
                }
            }

            ClassDefinition.Add(GetLuaClassFileName(targetType), builder);
            
        }

        public static class AliasAnnotation
        {
            public const string PREFIX = "alias.annotation.";

            public static Func<ParameterInfo, string> Alternative = (param) => ClassMetadata.Obtain(param.ParameterType).GetLuaName(LUA_NAME_TYPE).Name;

            public static string Analyze(ParameterInfo param)
            {
                foreach (var resolver in Resolvers)
                {
                    if (resolver.Match(param))
                    {
                        return $"{Alternative(param)}|{PREFIX}{resolver.Alias}";
                    }
                }

                return null;
            }

            public static List<(Func<ParameterInfo, bool> Match, string Alias, Func<StringBuilder, string> Content)> Resolvers;

            static AliasAnnotation()
            {
                Resolvers = new List<(Func<ParameterInfo, bool>, string, Func<StringBuilder, string>)>()
                {
#if CLIENT
                    AliasAnnotation.GUIComponentStyle,
#endif
                    AliasAnnotation.SkillIdentifier,
                    AliasAnnotation.JobIdentifier,
                    AliasAnnotation.AfflictionIdentifier,
                    AliasAnnotation.AfflictionType,
                    AliasAnnotation.ItemIdentifier
                };
            }

            public static void Gen()
            {
                foreach (var resolver in Resolvers)
                {
                    var content = resolver.Content(new StringBuilder($"---@alias {PREFIX}{resolver.Alias}\n"));
                    File.WriteAllText(@$"{BLuaDocPath}/{AliasDir}/{resolver.Alias}.lua", content);
                }
            }

#if CLIENT
            public static (Func<ParameterInfo, bool> Match, string Alias, Func<StringBuilder, string> Content) GUIComponentStyle = (
                param => param.GetCustomAttribute<LuaAlias.GUIComponentStyleAttribute>(false) != null,
                "gui.component.style",
                (sb) =>
                {
                    foreach (var identifier in GUIStyle.ComponentStyles.Keys) sb.Append($"---| \"{identifier.Value}\"\n");
                    return sb.ToString();
                }
            );
#endif

            public static (Func<ParameterInfo, bool> Match, string Alias, Func<StringBuilder, string> Content) SkillIdentifier = (
                param => param.GetCustomAttribute<LuaAlias.SkillIdentifierAttribute>(false) != null,
                "skill.identifier",
                (sb) =>
                {
                    var skillPrefabs = new HashSet<string>();
                    foreach (var prefab in JobPrefab.Prefabs)
                        foreach (var skill in prefab.Skills)
                            skillPrefabs.Add(skill.Identifier.Value);

                    var distinct = new HashSet<string>();
                    foreach (var skill in skillPrefabs)
                        if (!distinct.Contains(skill))
                            distinct.Add(skill);

                    foreach (var skill in distinct)
                        sb.Append($"---| \"{skill}\"\n");

                    return sb.ToString();
                }
            );

            public static (Func<ParameterInfo, bool> Match, string Alias, Func<StringBuilder, string> Content) JobIdentifier = (
                param => param.GetCustomAttribute<LuaAlias.JobIdentifierAttribute>(false) != null,
                "job.identifier",
                (sb) =>
                {
                    foreach (var identifier in JobPrefab.Prefabs.Keys) sb.Append($"---| \"{identifier.Value}\"\n");
                    return sb.ToString();
                }
            );

            public static (Func<ParameterInfo, bool> Match, string Alias, Func<StringBuilder, string> Content) AfflictionIdentifier = (
                param => param.GetCustomAttribute<LuaAlias.AfflictionIdentifierAttribute>(false) != null,
                "affliction.identifier",
                (sb) =>
                {
                    foreach (var identifier in AfflictionPrefab.Prefabs.Keys) sb.Append($"---| \"{identifier.Value}\"\n");
                    return sb.ToString();
                }
            );

            public static (Func<ParameterInfo, bool> Match, string Alias, Func<StringBuilder, string> Content) AfflictionType = (
                param => param.GetCustomAttribute<LuaAlias.AfflictionTypeAttribute>(false) != null,
                "affliction.type",
                (sb) =>
                {
                    var afflictionTypes = new HashSet<string>();
                    foreach (var prefab in AfflictionPrefab.Prefabs)
                        afflictionTypes.Add(prefab.AfflictionType.ToString());

                    var distinct = new HashSet<string>();
                    foreach (var type in afflictionTypes)
                        if (!distinct.Contains(type))
                            distinct.Add(type);

                    foreach (var type in distinct)
                        sb.Append($"---| \"{type}\"\n");

                    return sb.ToString();
                }
            );

            public static (Func<ParameterInfo, bool> Match, string Alias, Func<StringBuilder, string> Content) ItemIdentifier = (
                param => param.GetCustomAttribute<LuaAlias.ItemIdentifierAttribute>(false) != null,
                "item.identifier",
                (sb) =>
                {
                    foreach (var identifier in ItemPrefab.Prefabs.Keys) sb.Append($"---| \"{identifier.Value}\"\n");
                    return sb.ToString();
                }
            );
        }

    }
}
