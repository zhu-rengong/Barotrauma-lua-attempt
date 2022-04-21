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
/// 把.net框架下定义的Class转换成lua文档的形式，包括字段、属性、方法、构造器的成员信息。
/// 文档特性：
/// 每个C#类型在lua文档中有单独的class名称格式
/// 可感知
///     类的继承关系
///     （非）公共、静态、实例成员
///     重载的方法、构造器
///     可选参数、参量参数
///     嵌套的数组、列表、字典、集合及元素类型
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

                    return new ClassMetadata(target);
                }
                return metadata;
            }

            public (bool state, string Name) GetLuaName(int luaName = LUA_NAME_CLASS, char split = '_')
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
                    var (state, mapName) = GetMapClassName(className);
                    if (state) { return (true, mapName); }

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

            public static (bool, string) GetMapClassName(string className)
            {
                switch (className)
                {
                    case "Boolean":
                        return (true, "boolean");
                    case "String":
                        return (true, "string");
                    //case "Single":
                    //case "Double":
                    //case "Int16":
                    //case "Int32":
                    //case "Int64":
                    //case "UInt16":
                    //case "UInt32":
                    //case "UInt64":
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

            Do(typeof(System.Object));

            Do(typeof(LuaByte), "Byte");
            Do(typeof(LuaUShort), "UShort");
            Do(typeof(LuaFloat), "Float");

            Do(typeof(MathUtils));
            
            Do(typeof(Rand));
            Do(typeof(Rand.RandSync), null, new string[] { "RandSync" });

            Do(typeof(PerformanceCounter));

            Do(typeof(GameMain));

            Do(typeof(SerializableProperty));

            Do(typeof(AddedPunctuationLString));
            Do(typeof(CapitalizeLString));
            Do(typeof(ConcatLString));
            Do(typeof(FallbackLString));
            Do(typeof(FormattedLString));
            Do(typeof(InputTypeLString));
            Do(typeof(JoinLString));
            Do(typeof(LocalizedString));
            Do(typeof(LowerLString));
            Do(typeof(RawLString));
            Do(typeof(ReplaceLString));
            Do(typeof(ServerMsgLString));
            Do(typeof(SplitLString));
            Do(typeof(TagLString));
            Do(typeof(TrimLString));
            Do(typeof(UpperLString));

            Do(typeof(RichString));
            Do(typeof(StripRichTagsLString));
            Do(typeof(RichTextData));

            Do(typeof(TextManager));
            Do(typeof(TextPack));

            Do(typeof(Identifier));
            Do(typeof(LanguageIdentifier));

            Do(typeof(ContentFile));
            Do(typeof(ContentPackage));
            Do(typeof(ContentPackageManager));
            Do(typeof(ContentPackageManager.PackageSource));
            Do(typeof(ContentPackageManager.EnabledPackages));
            Do(typeof(RegularPackage));
            Do(typeof(CorePackage));


            Do(typeof(Camera));

            Do(typeof(CauseOfDeathType));
            Do(typeof(CauseOfDeath));

            Do(typeof(SpawnType));
            Do(typeof(WayPoint));

            Do(typeof(Networking.ServerLog));
            Do(typeof(Networking.ServerLog.MessageType), null, new string[] { "ServerLog_MessageType", "ServerLogMessageType" });

            Do(typeof(PropertyConditional));
            Do(typeof(StatusEffect));
            Do(typeof(DelayedEffect));

            Do(typeof(FireSource));
            Do(typeof(DummyFireSource));

            Do(typeof(Explosion));

            #region Enum
            Do(typeof(TransitionMode));
            Do(typeof(ActionType));
            Do(typeof(AbilityEffectType));
            Do(typeof(StatTypes));
            Do(typeof(AbilityFlags));
            #endregion

            #region Game
            Do(typeof(Screen));
            Do(typeof(GameScreen));
            Do(typeof(NetLobbyScreen));

            Do(typeof(GameSettings));

            Do(typeof(GameSession));

            //Data
            Do(typeof(CampaignMetadata));
            Do(typeof(CharacterCampaignData));
            Do(typeof(Faction));
            Do(typeof(FactionPrefab));
            Do(typeof(Reputation));

            //Game Mode
            Do(typeof(GameModePreset));
            Do(typeof(GameMode));
#if CLIENT
            Do(typeof(TestGameMode));
            Do(typeof(TutorialMode));
#endif
            Do(typeof(CampaignMode));
#if CLIENT
            Do(typeof(SinglePlayerCampaign));
#endif
            Do(typeof(MultiPlayerCampaign));
            Do(typeof(CoOpMode));
            Do(typeof(MissionMode));
            Do(typeof(PvPMode));


            Do(typeof(AutoItemPlacer));
            Do(typeof(CargoManager));
            Do(typeof(CrewManager));
            Do(typeof(HireManager));
            Do(typeof(MedicalClinic));
            Do(typeof(ReadyCheck));

            #endregion


            #region Level
            Do(typeof(Level.InterestingPosition));
            Do(typeof(Level.PositionType), null, new string[] { "PositionType" });
            Do(typeof(Level));
            Do(typeof(LevelData));
            Do(typeof(LevelGenerationParams));
            Do(typeof(LevelObjectManager));
            Do(typeof(LevelObjectPrefab));
            Do(typeof(LevelObject));
            Do(typeof(LevelTrigger));
            Do(typeof(LevelWall));
            Do(typeof(DestructibleLevelWall));
            #endregion


            #region Location
            Do(typeof(LocationType));
            Do(typeof(Location));
            Do(typeof(LocationConnection));
            Do(typeof(LocationTypeChange));
            #endregion


            #region Entity
            Do(typeof(Entity));
            Do(typeof(EntitySpawner));
            Do(typeof(EntityGrid));

            Do(typeof(MapEntityCategory));
            Do(typeof(MapEntity));
            Do(typeof(Prefab));
            Do(typeof(PrefabWithUintIdentifier));
            Do(typeof(MapEntityPrefab));
            Do(typeof(CoreEntityPrefab));
            #endregion


            #region Character
            Do(typeof(CharacterTeamType));
            Do(typeof(CharacterPrefab));
            Do(typeof(CharacterInfo));
            Do(typeof(CharacterInfoPrefab));
            Do(typeof(Character));
            Do(typeof(AICharacter));
            Do(typeof(CharacterHealth));
            Do(typeof(CharacterHealth.LimbHealth));
            Do(typeof(CharacterInventory));
            Do(typeof(CharacterTalent));
            #endregion

            #region AI
            Do(typeof(AIState));

            Do(typeof(AIController));
            Do(typeof(EnemyAIController));
            Do(typeof(HumanAIController));

            Do(typeof(AITarget));
            Do(typeof(AITargetMemory));

            Do(typeof(AIChatMessage));
            Do(typeof(AIObjectiveManager));
            Do(typeof(AITrigger));

            Do(typeof(AIObjective));
            Do(typeof(AIObjectiveChargeBatteries));
            Do(typeof(AIObjectiveCleanupItem));
            Do(typeof(AIObjectiveCleanupItems));
            Do(typeof(AIObjectiveCombat));
            Do(typeof(AIObjectiveContainItem));
            Do(typeof(AIObjectiveDecontainItem));
            Do(typeof(AIObjectiveEscapeHandcuffs));
            Do(typeof(AIObjectiveExtinguishFire));
            Do(typeof(AIObjectiveExtinguishFires));
            Do(typeof(AIObjectiveFightIntruders));
            Do(typeof(AIObjectiveFindDivingGear));
            Do(typeof(AIObjectiveFindSafety));
            Do(typeof(AIObjectiveFixLeak));
            Do(typeof(AIObjectiveFixLeaks));
            Do(typeof(AIObjectiveGetItem));
            Do(typeof(AIObjectiveGetItems));
            Do(typeof(AIObjectiveGoTo));
            Do(typeof(AIObjectiveIdle));
            Do(typeof(AIObjectiveOperateItem));
            Do(typeof(AIObjectivePrepare));
            Do(typeof(AIObjectivePumpWater));
            Do(typeof(AIObjectiveRepairItem));
            Do(typeof(AIObjectiveRepairItems));
            Do(typeof(AIObjectiveRescue));
            Do(typeof(AIObjectiveRescueAll));
            Do(typeof(AIObjectiveReturn));
            Do(typeof(AIObjectiveCombat.CombatMode), null, new string[] { "CombatMode" });
            #endregion

            #region Ragdoll
            Do(typeof(LimbType));
            Do(typeof(Limb));
            Do(typeof(LimbJoint));
            Do(typeof(LimbPos));

            Do(typeof(Ragdoll));
            Do(typeof(AnimController));
            Do(typeof(FishAnimController));
            Do(typeof(HumanoidAnimController));

            Do(typeof(EditableParams));
            Do(typeof(RagdollParams));
            Do(typeof(AnimationParams));

            Do(typeof(SwimParams));
            Do(typeof(GroundedMovementParams));

            Do(typeof(HumanRagdollParams));
            Do(typeof(HumanGroundedParams));
            Do(typeof(HumanWalkParams));
            Do(typeof(HumanRunParams));
            Do(typeof(HumanCrouchParams));
            Do(typeof(HumanSwimParams));
            Do(typeof(HumanSwimFastParams));
            Do(typeof(HumanSwimSlowParams));

            Do(typeof(FishRagdollParams));
            Do(typeof(FishWalkParams));
            Do(typeof(FishGroundedParams));
            Do(typeof(FishRunParams));
            Do(typeof(FishSwimParams));
            Do(typeof(FishSwimFastParams));
            Do(typeof(FishSwimSlowParams));

            #endregion


            #region Skill
            Do(typeof(Skill));
            Do(typeof(SkillPrefab));
            Do(typeof(SkillSettings));
            #endregion


            #region Job
            Do(typeof(Job));
            Do(typeof(JobPrefab));
            #endregion


            #region Talent
            Do(typeof(TalentPrefab));
            Do(typeof(TalentOption));
            Do(typeof(TalentSubTree));
            Do(typeof(TalentTree));
            #endregion


            #region Item
            Do(typeof(ItemPrefab));
            Do(typeof(Item));
            Do(typeof(ItemInventory));
            Do(typeof(RelatedItem));
            #endregion


            #region Items.Components
            //Holdable
            Do(typeof(Items.Components.Holdable));
            Do(typeof(Items.Components.IdCard));
            Do(typeof(Items.Components.LevelResource));
            Do(typeof(Items.Components.MeleeWeapon));
            Do(typeof(Items.Components.Pickable));
            Do(typeof(Items.Components.Propulsion));
            Do(typeof(Items.Components.RangedWeapon));
            Do(typeof(Items.Components.RepairTool));
            Do(typeof(Items.Components.Sprayer));
            Do(typeof(Items.Components.Throwable));

            //Machine
            Do(typeof(Items.Components.Controller));
            Do(typeof(Items.Components.Deconstructor));
            Do(typeof(Items.Components.Engine));
            Do(typeof(Items.Components.Fabricator));
            Do(typeof(Items.Components.MiniMap));
            Do(typeof(Items.Components.OutpostTerminal));
            Do(typeof(Items.Components.OxygenGenerator));
            Do(typeof(Items.Components.Pump));
            Do(typeof(Items.Components.Reactor));
            Do(typeof(Items.Components.Sonar));
            Do(typeof(Items.Components.SonarTransducer));
            Do(typeof(Items.Components.Steering));
            Do(typeof(Items.Components.Vent));
            
            //Power
            Do(typeof(Items.Components.PowerContainer));
            Do(typeof(Items.Components.Powered));
            Do(typeof(Items.Components.PowerTransfer));

            //Signal
            Do(typeof(Items.Components.AdderComponent));
            Do(typeof(Items.Components.AndComponent));
            Do(typeof(Items.Components.ArithmeticComponent));
            Do(typeof(Items.Components.ButtonTerminal));
            Do(typeof(Items.Components.ColorComponent));
            Do(typeof(Items.Components.ConcatComponent));
            Do(typeof(Items.Components.Connection));
            Do(typeof(Items.Components.ConnectionPanel));
            Do(typeof(Items.Components.CustomInterface));
            Do(typeof(Items.Components.DelayComponent));
            Do(typeof(Items.Components.DivideComponent));
            Do(typeof(Items.Components.EqualsComponent));
            Do(typeof(Items.Components.ExponentiationComponent));
            Do(typeof(Items.Components.FunctionComponent));
            Do(typeof(Items.Components.GreaterComponent));
            Do(typeof(Items.Components.LightComponent));
            Do(typeof(Items.Components.MemoryComponent));
            Do(typeof(Items.Components.ModuloComponent));
            Do(typeof(Items.Components.MotionSensor));
            Do(typeof(Items.Components.MultiplyComponent));
            Do(typeof(Items.Components.NotComponent));
            Do(typeof(Items.Components.OrComponent));
            Do(typeof(Items.Components.OscillatorComponent));
            Do(typeof(Items.Components.OxygenDetector));
            Do(typeof(Items.Components.RegExFindComponent));
            Do(typeof(Items.Components.RelayComponent));
            Do(typeof(Items.Components.Signal));
            Do(typeof(Items.Components.SignalCheckComponent));
            Do(typeof(Items.Components.SmokeDetector));
            Do(typeof(Items.Components.StringComponent));
            Do(typeof(Items.Components.SubtractComponent));
            Do(typeof(Items.Components.Terminal));
            Do(typeof(Items.Components.TrigonometricFunctionComponent));
            Do(typeof(Items.Components.WaterDetector));
            Do(typeof(Items.Components.WifiComponent));
            Do(typeof(Items.Components.Wire));
            Do(typeof(Items.Components.XorComponent));

            //Commonly
            Do(typeof(Items.Components.DockingPort));
            Do(typeof(Items.Components.Door));
            Do(typeof(Items.Components.ElectricalDischarger));
            Do(typeof(Items.Components.EntitySpawnerComponent));
            Do(typeof(Items.Components.GeneticMaterial));
            Do(typeof(Items.Components.Growable));
            Do(typeof(Items.Components.ProducedItem));
            Do(typeof(Items.Components.VineTile));
            Do(typeof(Items.Components.GrowthSideExtension));
            Do(typeof(Items.Components.ItemComponent));
            Do(typeof(Items.Components.ItemContainer));
            Do(typeof(Items.Components.ItemLabel));
            Do(typeof(Items.Components.Ladder));
            Do(typeof(Items.Components.NameTag));
            Do(typeof(Items.Components.Planter));
            Do(typeof(Items.Components.Projectile));
            Do(typeof(Items.Components.Quality));
            Do(typeof(Items.Components.RemoteController));
            Do(typeof(Items.Components.Repairable));
            Do(typeof(Items.Components.Rope));
            Do(typeof(Items.Components.Scanner));
            Do(typeof(Items.Components.StatusHUD));
            Do(typeof(Items.Components.TriggerComponent));
            Do(typeof(Items.Components.Turret));
            Do(typeof(Items.Components.Wearable));

            #endregion


            #region Submarine
            Do(typeof(SubmarineInfo));
            Do(typeof(Submarine));
            Do(typeof(SubmarineBody));
            #endregion

            #region Structure
            Do(typeof(Structure));
            Do(typeof(StructurePrefab));
            #endregion

            #region Affliction
            Do(typeof(AfflictionPrefab));
            Do(typeof(Affliction));
            Do(typeof(AfflictionPrefabHusk));
            Do(typeof(AfflictionHusk));
            Do(typeof(AfflictionBleeding));
            Do(typeof(AfflictionPsychosis));
            Do(typeof(AfflictionSpaceHerpes));
            #endregion

            #region Attack
            Do(typeof(AttackContext));
            Do(typeof(AttackPattern));
            Do(typeof(AttackTarget));
            Do(typeof(Attack));
            Do(typeof(AttackResult));
            #endregion

            #region Inventroy and slot
            Do(typeof(InvSlotType));
#if CLIENT
            Do(typeof(InventorySlotItem));
            Do(typeof(VisualSlot));
#endif
            Do(typeof(Inventory));
            #endregion

            #region Command
            Do(typeof(Command));
#if CLIENT
            Do(typeof(TransformCommand));
            Do(typeof(AddOrDeleteCommand));
            Do(typeof(PropertyCommand));
            Do(typeof(InventoryMoveCommand));
            Do(typeof(InventoryPlaceCommand));
#endif
            #endregion

            #region Traitor
            Do(typeof(TraitorMissionPrefab));
            Do(typeof(TraitorMissionResult));
#if SERVER
            Do(typeof(Networking.TraitorMessageType));
            Do(typeof(TraitorManager));
            Do(typeof(Traitor));
#endif
            #endregion

            #region Physic
            Do(typeof(FarseerPhysics.Dynamics.World));
            Do(typeof(FarseerPhysics.Dynamics.Fixture));
            Do(typeof(Physics));
            Do(typeof(PhysicsBody));

            Do(typeof(Hull));
            Do(typeof(Gap));
            #endregion

            #region Geometry
            Do(typeof(Microsoft.Xna.Framework.Matrix), "Matrix");
            Do(typeof(Microsoft.Xna.Framework.Vector2), "Vector2");
            Do(typeof(Microsoft.Xna.Framework.Vector3), "Vector3");
            Do(typeof(Microsoft.Xna.Framework.Vector4), "Vector4");
            Do(typeof(Microsoft.Xna.Framework.Color), "Color");
            Do(typeof(Microsoft.Xna.Framework.Point), "Point");
            Do(typeof(Microsoft.Xna.Framework.Rectangle), "Rectangle");
#endregion

            #region Sprite
            Do(typeof(Sprite));
            Do(typeof(SpriteSheet));
            Do(typeof(ConditionalSprite));
            Do(typeof(WearableSprite));
            Do(typeof(DeformableSprite));

#if CLIENT
            Do(typeof(SpriteFallBackState));
            Do(typeof(SpriteRecorder));
            Do(typeof(DecorativeSprite));
            Do(typeof(BrokenItemSprite));
            Do(typeof(ContainedItemSprite));
            Do(typeof(VineSprite));
#endif
            #endregion

            #region Craft
            Do(typeof(DeconstructItem));
            Do(typeof(FabricationRecipe));
            Do(typeof(FabricationRecipe.RequiredItem));
            #endregion

            #region Upgrade
            Do(typeof(UpgradeCategory));
            Do(typeof(UpgradePrice));
            Do(typeof(UpgradeManager));
            Do(typeof(UpgradePrefab));
            Do(typeof(Upgrade));
            Do(typeof(PurchasedUpgrade));
#endregion

            #region Networking
            Do(typeof(Networking.NetConfig));
            Do(typeof(Networking.ServerSettings));

            Do(typeof(Networking.ChatMessageType));
            Do(typeof(Networking.ChatMessage));

            Do(typeof(Networking.ServerPacketHeader));
            Do(typeof(Networking.ClientPacketHeader));
            Do(typeof(Networking.DeliveryMethod));
            Do(typeof(Networking.IWriteMessage));
            Do(typeof(Networking.WriteOnlyMessage));
            Do(typeof(Networking.IReadMessage));
            Do(typeof(Networking.ReadOnlyMessage));
            Do(typeof(Networking.ReadWriteMessage));

            Do(typeof(Networking.ClientPermissions));
            Do(typeof(Networking.Client));
            Do(typeof(Networking.TempClient));
            Do(typeof(INetSerializableStruct));

            Do(typeof(Networking.NetworkConnectionStatus));
            Do(typeof(Networking.NetworkConnection));
            Do(typeof(Networking.PipeConnection));
            Do(typeof(Networking.LidgrenConnection));
            Do(typeof(Networking.SteamP2PConnection));

            Do(typeof(Networking.NetworkMember));

#if SERVER
            Do(typeof(Networking.GameServer));

            Do(typeof(Networking.ServerPeer));
            Do(typeof(Networking.LidgrenServerPeer));
            Do(typeof(Networking.SteamP2PServerPeer));
#endif

#if CLIENT
            Do(typeof(Networking.GameClient));

            Do(typeof(Networking.ClientPeer));
            Do(typeof(Networking.LidgrenClientPeer));
            Do(typeof(Networking.SteamP2PClientPeer));
            Do(typeof(Networking.SteamP2POwnerPeer));
#endif
            #endregion

            #region Keys
            Do(typeof(InputType));
            Do(typeof(Key));
#if CLIENT
            Do(typeof(EventInput.KeyEventArgs));
            Do(typeof(Microsoft.Xna.Framework.Input.Keys), "Keys");
#endif
            #endregion

            #region Particles
#if CLIENT
            Do(typeof(Particles.Particle));
            Do(typeof(Particles.ParticleEmitter));
            Do(typeof(Particles.ParticleManager));
            Do(typeof(Particles.ParticlePrefab));
#endif
            #endregion

            #region GUI
#if CLIENT
            Do(typeof(ScalableFont));

            Do(typeof(Anchor));
            Do(typeof(Pivot));
            Do(typeof(ScaleBasis));

            Do(typeof(ChatBox));
            Do(typeof(CrewManagement));
            Do(typeof(FileSelection));
            Do(typeof(Graph));

            Do(typeof(GUI), "Barotrauma_GUI");
            
            Do(typeof(GUIComponentStyle));
            Do(typeof(SpriteFallBackState));

            Do(typeof(GUISoundType));
            Do(typeof(CursorState));

            Do(typeof(PlayerInput));

            Do(typeof(GUIButton));
            Do(typeof(GUICanvas));
            Do(typeof(GUIColorPicker));
            Do(typeof(GUIComponent));
            Do(typeof(GUIContextMenu));
            Do(typeof(GUICustomComponent));
            Do(typeof(GUIDropDown));
            Do(typeof(GUIFrame));
            Do(typeof(GUIImage));
            Do(typeof(GUILayoutGroup));
            Do(typeof(GUIListBox));
            Do(typeof(GUIMessage));
            Do(typeof(GUIMessageBox));
            Do(typeof(GUINumberInput));
            Do(typeof(GUIProgressBar));
            Do(typeof(GUIRadioButtonGroup));
            Do(typeof(GUIScissorComponent));
            Do(typeof(GUIScrollBar));
            Do(typeof(GUIStyle));
            Do(typeof(GUITextBlock));
            Do(typeof(GUITextBox));
            Do(typeof(GUITickBox));
            Do(typeof(HUDLayoutSettings));
            Do(typeof(LoadingScreen));
            Do(typeof(MedicalClinicUI));
            Do(typeof(ParamsEditor));
            Do(typeof(RectTransform));
            Do(typeof(ShapeExtensions));
            Do(typeof(Store));
            Do(typeof(SubmarineSelection));
            Do(typeof(TabMenu));
            Do(typeof(UISprite));
            Do(typeof(UpgradeStore));
            Do(typeof(VideoPlayer));
            Do(typeof(VotingInterface));
            Do(typeof(Widget));
#endif
            Do(typeof(Alignment));
#endregion

            #region Screen
#if CLIENT
            Do(typeof(Barotrauma.CampaignEndScreen));
            Do(typeof(Barotrauma.EditorScreen));
            Do(typeof(Barotrauma.EventEditorScreen));
            Do(typeof(Barotrauma.LevelEditorScreen));
            Do(typeof(Barotrauma.MainMenuScreen));
            Do(typeof(Barotrauma.ParticleEditorScreen));
            Do(typeof(Barotrauma.RoundSummaryScreen));
            Do(typeof(Barotrauma.ServerListScreen));
            Do(typeof(Barotrauma.SpriteEditorScreen));
            Do(typeof(Barotrauma.SubEditorScreen));
            Do(typeof(Barotrauma.TestScreen));
#endif
#endregion

            Do(typeof(KarmaManager));
            Do(typeof(RespawnManager));
            
            Do(typeof(DebugConsole));

            Do(typeof(LuaUserData), "LuaUserData");
            Do(typeof(LuaGame), "Game");
            Do(typeof(LuaCsHook), "Hook");
            Do(typeof(LuaCsHook.HookMethodType), "Hook.HookMethodType");
            Do(typeof(LuaCsTimer), "Timer");
            Do(typeof(LuaCsFile), "File");
            Do(typeof(LuaCsNetworking), "Networking");
            Do(typeof(LuaCsSetup.LuaCsModStore), "ModStore");
            Do(typeof(LuaCsSetup.LuaCsModStore.CsModStore), "ModStore.CsModStore");
            Do(typeof(LuaCsSetup.LuaCsModStore.LuaModStore), "ModStore.LuaModStore");
            Do(typeof(MoonSharp.Interpreter.Interop.IUserDataDescriptor));

            AliasAnnotation.Do();
            DoGlobal();
        }

        public static void DoGlobal()
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
        }

        public static void Do(Type targetType, string aliasTable = null, string[] minorTableNames = null)
        {
            var metadata = ClassMetadata.Obtain(targetType);
            var luadocBuilder = new StringBuilder();
            string tableName = aliasTable ?? metadata.GetDefaultTableName();
            var (_, className) = metadata.GetLuaName();
            SingleLuaFileClassNameList.Add(className);
            
            luadocBuilder.Append($"---@meta\n\n");

            luadocBuilder.Append($"---'{targetType.FullName}'\n");

            bool nonGenericBaseType = targetType.BaseType != null && !targetType.BaseType.IsGenericType;

            if (nonGenericBaseType)
            {
                var subMetadata = ClassMetadata.Obtain(targetType.BaseType);
                var (_, subClassName) = subMetadata.GetLuaName();
                subMetadata.CollectAllToGlobal();
                luadocBuilder.Append($"---@class {className} : {subClassName}\n");
            }
            else
            {
                luadocBuilder.Append($"---@class {className}\n");
            }

            const BindingFlags Flags = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            var fields = nonGenericBaseType ? targetType.GetFields(Flags | BindingFlags.DeclaredOnly) : targetType.GetFields(Flags);

            foreach (var field in fields.Where(f => !f.Name.Contains("k__BackingField")).ToList())
            {
                var subMetadata = ClassMetadata.Obtain(field.FieldType);
                var (_, subClassName) = subMetadata.GetLuaName();
                subMetadata.CollectAllToGlobal();
                luadocBuilder.Append($"---\n");
                luadocBuilder.Append($"---{subMetadata.TargetType.FullName}\n");
                var (_, subTypeName) = subMetadata.GetLuaName(LUA_NAME_TYPE);
                luadocBuilder.Append($"---@field {field.Name} {subTypeName}\n");
            }

            var properties = nonGenericBaseType ? targetType.GetProperties(Flags | BindingFlags.DeclaredOnly) : targetType.GetProperties(Flags);

            foreach (var prop in properties)
            {
                var subMetadata = ClassMetadata.Obtain(prop.PropertyType);
                var (_, subClassName) = subMetadata.GetLuaName();
                subMetadata.CollectAllToGlobal();
                luadocBuilder.Append($"---\n");
                luadocBuilder.Append($"---{subMetadata.TargetType.FullName}\n");
                var (_, subTypeName) = subMetadata.GetLuaName(LUA_NAME_TYPE);
                luadocBuilder.Append($"---@field {prop.Name} {subTypeName}\n");
            }

            luadocBuilder.Append($"{tableName}={{}}\n\n");

            var methods = nonGenericBaseType ? targetType.GetMethods(Flags | BindingFlags.DeclaredOnly) : targetType.GetMethods(Flags);

            foreach (var groupMethod in (
                    from method in methods
                    where !method.Name.StartsWith("get_") && !method.Name.StartsWith("set_")
                    where !Regex.IsMatch(method.Name, @"<.*?>")
                    group method by method.Name
                )
            )
            {
                var methodArray = groupMethod.ToArray();
                var methodName = groupMethod.Key;
                var methodTempSb = new StringBuilder();
                for (int i = 0; i < methodArray.Length; i++)
                {
                    var method = methodArray[i];
                    var paramNames = "";
                    var parameters = method.GetParameters();
                    if (i != methodArray.Length - 1)
                    {
                        for (var j = 0; j < parameters.Length; j++)
                        {
                            var parameter = parameters[j];
                            var paramName = parameter.Name;
                            if (LUAKEYWORDS.Contains(paramName)) { paramName = $"_{paramName}"; }

                            if (j == 0) { methodTempSb.Append(@"---@overload fun("); }

                            var subMetadata = ClassMetadata.Obtain(parameter.ParameterType);
                            subMetadata.CollectAllToGlobal();
                            var typeName = AliasAnnotation.Analyze(parameter) ?? subMetadata.GetLuaName(LUA_NAME_TYPE).Name;
                            if (IsOptional(parameter)) { paramName += '?'; }
                            methodTempSb.Append((j == parameters.Length - 1 && IsParams(parameter))
                                ? $"...:{typeName}"
                                : $"{paramName}:{typeName}");

                            if (j != parameters.Length - 1)
                            {
                                methodTempSb.Append(", ");
                            }
                            else
                            {
                                if (method.ReturnType != typeof(void))
                                {
                                    var subSubMetadata = ClassMetadata.Obtain(method.ReturnType);
                                    var (_, subSubClassName) = subSubMetadata.GetLuaName();
                                    subSubMetadata.CollectAllToGlobal();
                                    var (_, subSubTypeName) = subSubMetadata.GetLuaName(LUA_NAME_TYPE);
                                    methodTempSb.Append($"):{subSubTypeName}\n");
                                }
                                else
                                {
                                    methodTempSb.Append($")\n");
                                }
                            }
                        }
                    }
                    else
                    {
                        for (var j = 0; j < parameters.Length; j++)
                        {
                            var parameter = parameters[j];
                            var paramName = parameter.Name;
                            var isParams = IsParams(parameter);
                            if (LUAKEYWORDS.Contains(paramName)) { paramName = $"_{paramName}"; }
                            paramNames += (j == parameters.Length - 1) ? (isParams ? "..." : paramName) : (paramName + ", ");

                            var subMetadata = ClassMetadata.Obtain(parameter.ParameterType);
                            subMetadata.CollectAllToGlobal();
                            var typeName = AliasAnnotation.Analyze(parameter) ?? subMetadata.GetLuaName(LUA_NAME_TYPE).Name;
                            if (IsOptional(parameter)) { paramName += '?'; }
                            methodTempSb.Append((j == parameters.Length - 1 && IsParams(parameter))
                                ? $"---@vararg {typeName}\n"
                                : $"---@param {paramName} {typeName}\n");
                        }

                        if (method.ReturnType != typeof(void))
                        {
                            var subMetadata = ClassMetadata.Obtain(method.ReturnType);
                            var (_, subClassName) = subMetadata.GetLuaName();
                            subMetadata.CollectAllToGlobal();
                            var (_, subTypeName) = subMetadata.GetLuaName(LUA_NAME_TYPE);
                            methodTempSb.Append($"---@return {subTypeName}\n");
                        }
                    }

                    if (i == methodArray.Length - 1)
                    {
                        luadocBuilder.Append(methodTempSb);
                        luadocBuilder.Append($"function {tableName}.{methodName}({paramNames}) end\n\n");
                    }
                }
            }

            var constructors = targetType.GetConstructors();

            var constructorTempSb = new StringBuilder();
            for (int i = 0; i < constructors.Length; i++)
            {
                var constructor = constructors[i];
                if (constructor.IsPrivate) { continue; }
                var paramNames = "";
                var parameters = constructor.GetParameters();
                if (i != constructors.Length - 1)
                {
                    for (var j = 0; j < parameters.Length; j++)
                    {
                        var parameter = parameters[j];
                        var paramName = parameter.Name;
                        if (LUAKEYWORDS.Contains(paramName)) { paramName = $"_{paramName}"; }

                        if (j == 0) { constructorTempSb.Append(@"---@overload fun("); }

                        var subMetadata = ClassMetadata.Obtain(parameter.ParameterType);
                        subMetadata.CollectAllToGlobal();
                        var typeName = AliasAnnotation.Analyze(parameter) ?? subMetadata.GetLuaName(LUA_NAME_TYPE).Name;
                        if (IsOptional(parameter)) { paramName += '?'; }
                        constructorTempSb.Append((j == parameters.Length - 1 && IsParams(parameter))
                            ? $"...:{typeName}"
                            : $"{paramName}:{typeName}");

                        if (j != parameters.Length - 1)
                        {
                            constructorTempSb.Append(", ");
                        }
                        else
                        {
                            constructorTempSb.Append($"):{className}\n");
                        }
                    }
                }
                else
                {
                    for (var j = 0; j < parameters.Length; j++)
                    {
                        var parameter = parameters[j];
                        var paramName = parameter.Name;
                        var isParams = IsParams(parameter);
                        if (LUAKEYWORDS.Contains(paramName)) { paramName = $"_{paramName}"; }
                        paramNames += (j == parameters.Length - 1) ? (isParams ? "..." : paramName) : (paramName + ", ");

                        var subMetadata = ClassMetadata.Obtain(parameter.ParameterType);
                        subMetadata.CollectAllToGlobal();
                        var typeName = AliasAnnotation.Analyze(parameter) ?? subMetadata.GetLuaName(LUA_NAME_TYPE).Name;
                        if (IsOptional(parameter)) { paramName += '?'; }
                        constructorTempSb.Append((j == parameters.Length - 1 && isParams)
                            ? $"---@vararg {typeName}\n"
                            : $"---@param {paramName} {typeName}\n");
                    }

                    constructorTempSb.Append($"---@return {className}\n");
                }

                if (i == constructors.Length - 1)
                {
                    luadocBuilder.Append(constructorTempSb);
                    luadocBuilder.Append($"function {tableName}({paramNames}) end\n");
                    luadocBuilder.Append($"{tableName}.__new = {tableName}\n\n");
                }
            }


            if (minorTableNames != null)
            {
                foreach (var minorTableName in minorTableNames)
                {
                    luadocBuilder.Append($"{minorTableName} = {tableName}\n");
                }
            }

            static bool IsParams(ParameterInfo param) => param.GetCustomAttribute<ParamArrayAttribute>(false) != null;
            static bool IsOptional(ParameterInfo param) => param.GetCustomAttribute<OptionalAttribute>(false) != null;

            try
            {
                var nameSpacePart = targetType.Name;
                var declarT = targetType.DeclaringType;
                var prefix = "";
                while (declarT != null)
                {
                    prefix = $@"{declarT.Name}.{prefix}";
                    declarT = declarT.DeclaringType;
                }

                var fileName = nameSpacePart != ""
                    ? (prefix != ""
                        ? $@"{nameSpacePart}.{prefix}{targetType.Name}"
                        : $@"{nameSpacePart}.{targetType.Name}"
                    )
                    : (prefix != ""
                        ? $@"{prefix}{targetType.Name}"
                        : $@"{targetType.Name}"
                    );

                File.WriteAllText($@"{BLuaDocPath}/{fileName}.lua", luadocBuilder.ToString());
            }
            catch (Exception ex)
            {
                File.WriteAllText("CrashReport.log", ex.Message);
                throw;
            }
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
                        return $"{PREFIX}{resolver.Alias}|{Alternative(param)}";
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

            public static void Do()
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