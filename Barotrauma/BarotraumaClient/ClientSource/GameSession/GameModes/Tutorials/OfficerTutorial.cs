﻿using System;
using System.Collections.Generic;
using Barotrauma.IO;
using System.Xml.Linq;
using System.Linq;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using Barotrauma.Extensions;
using Microsoft.Xna.Framework;

namespace Barotrauma.Tutorials
{
    class OfficerTutorial : ScenarioTutorial
    {
        // Other tutorial items
        private LightComponent tutorial_mechanicFinalDoorLight;
        private Steering tutorial_submarineSteering;

        // Room 1
        private float shakeTimer = 1f;
        private float shakeAmount = 20f;

        // Room 2
        private MotionSensor officer_equipmentObjectiveSensor;
        private ItemContainer officer_equipmentCabinet;
        private Door officer_firstDoor;
        private LightComponent officer_firstDoorLight;

        // Room 3
        private MotionSensor officer_crawlerSensor;
        private Character officer_crawler;
        private Vector2 officer_crawlerSpawnPos;
        private Door officer_secondDoor;
        private LightComponent officer_secondDoorLight;

        // Room 4
        private MotionSensor officer_somethingBigSensor;
        private ItemContainer officer_coilgunLoader;
        private ItemContainer officer_ammoShelf_1;
        private ItemContainer officer_ammoShelf_2;
        private PowerContainer officer_superCapacitor;
        private Item officer_coilgunPeriscope;
        private Character officer_hammerhead;
        private Vector2 officer_hammerheadSpawnPos;
        private Door officer_thirdDoor;
        private LightComponent officer_thirdDoorLight;

        // Room 5
        private MotionSensor officer_rangedWeaponSensor;
        private ItemContainer officer_rangedWeaponCabinet;
        private ItemContainer officer_rangedWeaponHolder;
        private Door officer_fourthDoor;
        private LightComponent officer_fourthDoorLight;

        // Room 6
        private MotionSensor officer_mudraptorObjectiveSensor;
        private Vector2 officer_mudraptorSpawnPos;
        private Character officer_mudraptor;
        private Door tutorial_securityFinalDoor;
        private LightComponent tutorial_securityFinalDoorLight;

        // Submarine
        private Door tutorial_submarineDoor;
        private LightComponent tutorial_submarineDoorLight;
        private MotionSensor tutorial_enteredSubmarineSensor;
        private Item officer_subAmmoBox_1;
        private Item officer_subAmmoBox_2;
        private ItemContainer officer_subAmmoShelf;
        private ItemContainer officer_subLoader_1;
        private ItemContainer officer_subLoader_2;
        private PowerContainer officer_subSuperCapacitor_1;
        private PowerContainer officer_subSuperCapacitor_2;

        // Variables
        private LocalizedString radioSpeakerName;
        private Character officer;
        private float superCapacitorRechargeRate = 10;
        private Sprite officer_gunIcon;
        private Color officer_gunIconColor;

        public OfficerTutorial() : base("tutorial.securityofficertraining".ToIdentifier(),
            new Segment(
                "Mechanic.Equipment".ToIdentifier(),
                "Mechanic.EquipmentObjective".ToIdentifier(),
                TutorialContentType.TextOnly,
                textContent: new Segment.Text { Tag = "Mechanic.EquipmentText".ToIdentifier(), Width = 450, Height = 80, Anchor = Anchor.Center }),
            new Segment(
                "Officer.MeleeWeapon".ToIdentifier(),
                "Officer.MeleeWeaponObjective".ToIdentifier(),
                TutorialContentType.TextOnly,
                textContent: new Segment.Text { Tag = "Officer.MeleeWeaponText".ToIdentifier(), Width = 450, Height = 80, Anchor = Anchor.Center }),
            new Segment(
                "Officer.Crawler".ToIdentifier(),
                "Officer.CrawlerObjective".ToIdentifier(),
                TutorialContentType.TextOnly,
                textContent: new Segment.Text { Tag = "Officer.CrawlerText".ToIdentifier(), Width = 450, Height = 80, Anchor = Anchor.Center }),
            new Segment(
                "Officer.SomethingBig".ToIdentifier(),
                "Officer.SomethingBigObjective".ToIdentifier(),
                TutorialContentType.ManualVideo,
                textContent: new Segment.Text { Tag = "Officer.SomethingBigText".ToIdentifier(), Width = 700, Height = 80, Anchor = Anchor.Center },
                videoContent: new Segment.Video { File = "tutorial_loaders.webm", TextTag = "Officer.SomethingBigText".ToIdentifier(), Width = 700, Height = 80 }),
            new Segment(
                "Officer.Hammerhead".ToIdentifier(),
                "Officer.HammerheadObjective".ToIdentifier(),
                TutorialContentType.TextOnly,
                textContent: new Segment.Text { Tag = "Officer.HammerheadText".ToIdentifier(), Width = 450, Height = 80, Anchor = Anchor.Center }),
            new Segment(
                "Officer.RangedWeapon".ToIdentifier(),
                "Officer.RangedWeaponObjective".ToIdentifier(),
                TutorialContentType.ManualVideo,
                textContent: new Segment.Text { Tag = "Officer.RangedWeaponText".ToIdentifier(), Width = 450, Height = 80, Anchor = Anchor.Center },
                videoContent: new Segment.Video { File = "tutorial_ranged.webm", TextTag = "Officer.RangedWeaponText".ToIdentifier(), Width = 450, Height = 80 }),
            new Segment(
                "Officer.Mudraptor".ToIdentifier(),
                "Officer.MudraptorObjective".ToIdentifier(),
                TutorialContentType.TextOnly,
                textContent: new Segment.Text { Tag = "Officer.MudraptorText".ToIdentifier(), Width = 450, Height = 80, Anchor = Anchor.Center }),
            new Segment(
                "Officer.ArmSubmarine".ToIdentifier(),
                "Officer.ArmSubmarineObjective".ToIdentifier(),
                TutorialContentType.TextOnly,
                textContent: new Segment.Text { Tag = "Officer.ArmSubmarineText".ToIdentifier(), Width = 450, Height = 80, Anchor = Anchor.Center }))
        { }

        protected override CharacterInfo GetCharacterInfo()
        {
            return new CharacterInfo(
                CharacterPrefab.HumanSpeciesName,
                jobOrJobPrefab: new Job(
                    JobPrefab.Prefabs["securityofficer"], Rand.RandSync.Unsynced, 0,
                    new Skill("medical".ToIdentifier(), 20),
                    new Skill("weapons".ToIdentifier(), 70),
                    new Skill("mechanical".ToIdentifier(), 20),
                    new Skill("electrical".ToIdentifier(), 20),
                    new Skill("helm".ToIdentifier(), 20)));
        }

        protected override void Initialize()
        {
            radioSpeakerName = TextManager.Get("Tutorial.Radio.Speaker");
            officer = Character.Controlled;

            foreach (Item item in officer.Inventory.AllItemsMod)
            {
                if (item.HasTag("clothing") || item.HasTag("identitycard") || item.HasTag("mobileradio")) { continue; }
                item.Unequip(officer);
                officer.Inventory.RemoveItem(item);
            }

            var gunOrder = OrderPrefab.Prefabs["operateweapons"];
            officer_gunIcon = gunOrder.SymbolSprite;
            officer_gunIconColor = gunOrder.Color;

            var bandage = FindOrGiveItem(officer, "antibleeding1".ToIdentifier());
            bandage.Unequip(officer);
            officer.Inventory.RemoveItem(bandage);
            FindOrGiveItem(officer, "antibleeding1".ToIdentifier());

            // Other tutorial items
            tutorial_mechanicFinalDoorLight = Item.ItemList.Find(i => i.HasTag("tutorial_mechanicfinaldoorlight")).GetComponent<LightComponent>();
            tutorial_submarineSteering = Item.ItemList.Find(i => i.HasTag("command")).GetComponent<Steering>();

            tutorial_submarineSteering.CanBeSelected = false;
            foreach (ItemComponent ic in tutorial_submarineSteering.Item.Components)
            {
                ic.CanBeSelected = false;
            }

            SetDoorAccess(null, tutorial_mechanicFinalDoorLight, false);

            // Room 2
            officer_equipmentObjectiveSensor = Item.ItemList.Find(i => i.HasTag("officer_equipmentobjectivesensor")).GetComponent<MotionSensor>();
            officer_equipmentCabinet = Item.ItemList.Find(i => i.HasTag("officer_equipmentcabinet")).GetComponent<ItemContainer>();
            officer_firstDoor = Item.ItemList.Find(i => i.HasTag("officer_firstdoor")).GetComponent<Door>();
            officer_firstDoorLight = Item.ItemList.Find(i => i.HasTag("officer_firstdoorlight")).GetComponent<LightComponent>();

            SetDoorAccess(officer_firstDoor, officer_firstDoorLight, false);

            // Room 3
            officer_crawlerSensor = Item.ItemList.Find(i => i.HasTag("officer_crawlerobjectivesensor")).GetComponent<MotionSensor>();
            officer_crawlerSpawnPos = Item.ItemList.Find(i => i.HasTag("officer_crawlerspawn")).WorldPosition;
            officer_secondDoor = Item.ItemList.Find(i => i.HasTag("officer_seconddoor")).GetComponent<Door>();
            officer_secondDoorLight = Item.ItemList.Find(i => i.HasTag("officer_seconddoorlight")).GetComponent<LightComponent>();

            SetDoorAccess(officer_secondDoor, officer_secondDoorLight, false);

            // Room 4
            officer_somethingBigSensor = Item.ItemList.Find(i => i.HasTag("officer_somethingbigobjectivesensor")).GetComponent<MotionSensor>();
            officer_coilgunLoader = Item.ItemList.Find(i => i.HasTag("officer_coilgunloader")).GetComponent<ItemContainer>();
            officer_superCapacitor = Item.ItemList.Find(i => i.HasTag("officer_supercapacitor")).GetComponent<PowerContainer>();
            officer_coilgunPeriscope = Item.ItemList.Find(i => i.HasTag("officer_coilgunperiscope"));
            officer_hammerheadSpawnPos = Item.ItemList.Find(i => i.HasTag("officer_hammerheadspawn")).WorldPosition;
            officer_thirdDoor = Item.ItemList.Find(i => i.HasTag("officer_thirddoor")).GetComponent<Door>();
            officer_thirdDoorLight = Item.ItemList.Find(i => i.HasTag("officer_thirddoorlight")).GetComponent<LightComponent>();
            officer_ammoShelf_1 = Item.ItemList.Find(i => i.HasTag("officer_ammoshelf_1")).GetComponent<ItemContainer>();
            officer_ammoShelf_2 = Item.ItemList.Find(i => i.HasTag("officer_ammoshelf_2")).GetComponent<ItemContainer>();

            SetDoorAccess(officer_thirdDoor, officer_thirdDoorLight, false);

            // Room 5
            officer_rangedWeaponSensor = Item.ItemList.Find(i => i.HasTag("officer_rangedweaponobjectivesensor")).GetComponent<MotionSensor>();
            officer_rangedWeaponCabinet = Item.ItemList.Find(i => i.HasTag("officer_rangedweaponcabinet")).GetComponent<ItemContainer>();
            officer_rangedWeaponHolder = Item.ItemList.Find(i => i.HasTag("officer_rangedweaponholder")).GetComponent<ItemContainer>();
            officer_fourthDoor = Item.ItemList.Find(i => i.HasTag("officer_fourthdoor")).GetComponent<Door>();
            officer_fourthDoorLight = Item.ItemList.Find(i => i.HasTag("officer_fourthdoorlight")).GetComponent<LightComponent>();

            SetDoorAccess(officer_fourthDoor, officer_fourthDoorLight, false);

            // Room 6
            officer_mudraptorObjectiveSensor = Item.ItemList.Find(i => i.HasTag("officer_mudraptorobjectivesensor")).GetComponent<MotionSensor>();
            officer_mudraptorSpawnPos = Item.ItemList.Find(i => i.HasTag("officer_mudraptorspawn")).WorldPosition;
            tutorial_securityFinalDoor = Item.ItemList.Find(i => i.HasTag("tutorial_securityfinaldoor")).GetComponent<Door>();
            tutorial_securityFinalDoorLight = Item.ItemList.Find(i => i.HasTag("tutorial_securityfinaldoorlight")).GetComponent<LightComponent>();

            SetDoorAccess(tutorial_securityFinalDoor, tutorial_securityFinalDoorLight, false);

            // Submarine
            tutorial_submarineDoor = Item.ItemList.Find(i => i.HasTag("tutorial_submarinedoor")).GetComponent<Door>();
            tutorial_submarineDoorLight = Item.ItemList.Find(i => i.HasTag("tutorial_submarinedoorlight")).GetComponent<LightComponent>();
            tutorial_enteredSubmarineSensor = Item.ItemList.Find(i => i.HasTag("tutorial_enteredsubmarinesensor")).GetComponent<MotionSensor>();
            officer_subAmmoBox_1 = Item.ItemList.Find(i => i.HasTag("officer_subammobox_1"));
            officer_subAmmoBox_2 = Item.ItemList.Find(i => i.HasTag("officer_subammobox_2"));
            officer_subLoader_1 = Item.ItemList.Find(i => i.HasTag("officer_subloader_1")).GetComponent<ItemContainer>();
            officer_subLoader_2 = Item.ItemList.Find(i => i.HasTag("officer_subloader_2")).GetComponent<ItemContainer>();
            officer_subSuperCapacitor_1 = Item.ItemList.Find(i => i.HasTag("officer_subsupercapacitor_1")).GetComponent<PowerContainer>();
            officer_subSuperCapacitor_2 = Item.ItemList.Find(i => i.HasTag("officer_subsupercapacitor_2")).GetComponent<PowerContainer>();
            officer_subAmmoShelf = Item.ItemList.Find(i => i.HasTag("officer_subammoshelf")).GetComponent<ItemContainer>();
            SetDoorAccess(tutorial_submarineDoor, tutorial_submarineDoorLight, true);

            GameAnalyticsManager.AddDesignEvent("Tutorial:OfficerTutorial:Started");
            GameAnalyticsManager.AddDesignEvent("Tutorial:Started");
        }

        public override IEnumerable<CoroutineStatus> UpdateState()
        {
            while (GameMain.Instance.LoadingScreenOpen) yield return null;

            yield return new WaitForSeconds(0.01f);

            // Room 1
            SoundPlayer.PlayDamageSound("StructureBlunt", 10, Character.Controlled.WorldPosition);
            while (shakeTimer > 0.0f) // Wake up, shake
            {
                shakeTimer -= 0.1f;
                GameMain.GameScreen.Cam.Shake = shakeAmount;
                yield return new WaitForSeconds(0.1f, false);
            }

            GameMain.GameSession?.CrewManager.AddSinglePlayerChatMessage(radioSpeakerName, TextManager.Get("Officer.Radio.WakeUp"), ChatMessageType.Radio, null);

            // Room 2
            do { yield return null; } while (!officer_equipmentObjectiveSensor.MotionDetected);
            GameMain.GameSession?.CrewManager.AddSinglePlayerChatMessage(radioSpeakerName, TextManager.Get("Officer.Radio.Equipment"), ChatMessageType.Radio, null);
            yield return new WaitForSeconds(3f, false);
            //TriggerTutorialSegment(0, GameSettings.CurrentConfig.KeyMap.Bindings[InputType.Select], GameSettings.CurrentConfig.KeyMap.Bindings[InputType.Deselect]); // Retrieve equipment
            SetHighlight(officer_equipmentCabinet.Item, true);
            bool firstSlotRemoved = false;
            bool secondSlotRemoved = false;
            bool thirdSlotRemoved = false;
            do
            {
                if (IsSelectedItem(officer_equipmentCabinet.Item))
                {
                    if (!firstSlotRemoved)
                    {
                        HighlightInventorySlot(officer_equipmentCabinet.Inventory, 0, highlightColor, .5f, .5f, 0f);
                        if (officer_equipmentCabinet.Inventory.GetItemAt(0) == null) { firstSlotRemoved = true; }
                    }

                    if (!secondSlotRemoved)
                    {
                        HighlightInventorySlot(officer_equipmentCabinet.Inventory, 1, highlightColor, .5f, .5f, 0f);
                        if (officer_equipmentCabinet.Inventory.GetItemAt(1) == null) { secondSlotRemoved = true; }
                    }

                    if (!thirdSlotRemoved)
                    {
                        HighlightInventorySlot(officer_equipmentCabinet.Inventory, 2, highlightColor, .5f, .5f, 0f);
                        if (officer_equipmentCabinet.Inventory.GetItemAt(2) == null) { thirdSlotRemoved = true; }
                    }

                    for (int i = 0; i < officer.Inventory.visualSlots.Length; i++)
                    {
                        if (officer.Inventory.GetItemAt(i) == null) { HighlightInventorySlot(officer.Inventory, i, highlightColor, .5f, .5f, 0f); }
                    }
                }

                yield return null;
            } while (!officer_equipmentCabinet.Inventory.IsEmpty()); // Wait until looted
            //RemoveCompletedObjective(segments[0]);
            GameAnalyticsManager.AddDesignEvent("Tutorial:OfficerTutorial:Objective0");
            SetHighlight(officer_equipmentCabinet.Item, false);
            do { yield return null; } while (IsSelectedItem(officer_equipmentCabinet.Item));
            TriggerTutorialSegment(1, GameSettings.CurrentConfig.KeyMap.KeyBindText(InputType.Aim), GameSettings.CurrentConfig.KeyMap.KeyBindText(InputType.Shoot)); // Equip melee weapon & armor
            do
            {
                if (!officer.HasEquippedItem("stunbaton".ToIdentifier()))
                {
                    HighlightInventorySlot(officer.Inventory, "stunbaton".ToIdentifier(), highlightColor, .5f, .5f, 0f);
                }
                if (!officer.HasEquippedItem("bodyarmor".ToIdentifier()))
                {
                    HighlightInventorySlot(officer.Inventory, "bodyarmor".ToIdentifier(), highlightColor, .5f, .5f, 0f);
                }
                if (!officer.HasEquippedItem("ballistichelmet1".ToIdentifier()))
                {
                    HighlightInventorySlot(officer.Inventory, "ballistichelmet1".ToIdentifier(), highlightColor, .5f, .5f, 0f);
                }
                yield return new WaitForSeconds(1f, false);
            } while (!officer.HasEquippedItem("stunbaton".ToIdentifier()) || !officer.HasEquippedItem("bodyarmor".ToIdentifier()) || !officer.HasEquippedItem("ballistichelmet1".ToIdentifier()));
            RemoveCompletedObjective(1);
            GameAnalyticsManager.AddDesignEvent("Tutorial:OfficerTutorial:Objective1");
            SetDoorAccess(officer_firstDoor, officer_firstDoorLight, true);

            // Room 3
            do { yield return null; } while (!officer_crawlerSensor.MotionDetected);
            TriggerTutorialSegment(2);
            officer_crawler = SpawnMonster("crawler", officer_crawlerSpawnPos);
            do { yield return null; } while (!officer_crawler.IsDead);
            RemoveCompletedObjective(2);
            GameAnalyticsManager.AddDesignEvent("Tutorial:OfficerTutorial:Objective2");
            Heal(officer);
            yield return new WaitForSeconds(1f, false);
            GameMain.GameSession?.CrewManager.AddSinglePlayerChatMessage(radioSpeakerName, TextManager.Get("Officer.Radio.CrawlerDead"), ChatMessageType.Radio, null);
            SetDoorAccess(officer_secondDoor, officer_secondDoorLight, true);

            // Room 4
            do { yield return null; } while (!officer_somethingBigSensor.MotionDetected);
            GameMain.GameSession?.CrewManager.AddSinglePlayerChatMessage(radioSpeakerName, TextManager.Get("Officer.Radio.SomethingBig"), ChatMessageType.Radio, null);
            yield return new WaitForSeconds(2f, false);
            TriggerTutorialSegment(3); // Arm coilgun
            do
            {
                SetHighlight(officer_coilgunLoader.Item, officer_coilgunLoader.Inventory.GetItemAt(0) == null || officer_coilgunLoader.Inventory.GetItemAt(0).Condition == 0);
                HighlightInventorySlot(officer_coilgunLoader.Inventory, 0, highlightColor, .5f, .5f, 0f);
                SetHighlight(officer_superCapacitor.Item, officer_superCapacitor.RechargeSpeed < superCapacitorRechargeRate);
                SetHighlight(officer_ammoShelf_1.Item, officer_coilgunLoader.Item.ExternalHighlight );
                SetHighlight(officer_ammoShelf_2.Item, officer_coilgunLoader.Item.ExternalHighlight );
                if (IsSelectedItem(officer_coilgunLoader.Item))
                {
                    HighlightInventorySlot(officer.Inventory, "coilgunammobox".ToIdentifier(), highlightColor, .5f, .5f, 0f);
                }
            yield return null;
            } while (officer_coilgunLoader.Inventory.GetItemAt(0) == null || officer_superCapacitor.RechargeSpeed < superCapacitorRechargeRate || officer_coilgunLoader.Inventory.GetItemAt(0).Condition == 0);
            SetHighlight(officer_coilgunLoader.Item, false);
            SetHighlight(officer_superCapacitor.Item, false);
            SetHighlight(officer_ammoShelf_1.Item, false);
            SetHighlight(officer_ammoShelf_2.Item, false);
            RemoveCompletedObjective(3);
            GameAnalyticsManager.AddDesignEvent("Tutorial:OfficerTutorial:Objective3");

            yield return new WaitForSeconds(2f, false);
            TriggerTutorialSegment(4, GameSettings.CurrentConfig.KeyMap.KeyBindText(InputType.Select), GameSettings.CurrentConfig.KeyMap.KeyBindText(InputType.Shoot), GameSettings.CurrentConfig.KeyMap.KeyBindText(InputType.Deselect)); // Kill hammerhead
            officer_hammerhead = SpawnMonster("hammerhead", officer_hammerheadSpawnPos);
            officer_hammerhead.Params.AI.AvoidAbyss = false;
            officer_hammerhead.Params.AI.StayInAbyss = false;
            officer_hammerhead.AIController.SelectTarget(officer.AiTarget);
            SetHighlight(officer_coilgunPeriscope, true);
            float originalDistance = Vector2.Distance(officer_coilgunPeriscope.WorldPosition, officer_hammerheadSpawnPos);
            do
            {
                float distance = Vector2.Distance(officer_coilgunPeriscope.WorldPosition, officer_hammerhead.WorldPosition);
                if (distance > originalDistance * 1.5f || officer_hammerhead.WorldPosition.Y > officer_coilgunPeriscope.WorldPosition.Y)
                {
                    // Don't let the Hammerhead go too far.
                    officer_hammerhead.TeleportTo(officer_hammerheadSpawnPos + new Vector2(0, -1000));
                }
                if (distance > originalDistance)
                {
                    // Ensure that the Hammerhead targets the player
                    officer.AiTarget.SoundRange = float.MaxValue;
                    officer.AiTarget.SightRange = float.MaxValue;
                    officer_hammerhead.AIController.SelectTarget(officer.AiTarget);
                    if ((officer_hammerhead.AIController as EnemyAIController)?.SelectedTargetingParams != null)
                    {
                        ((EnemyAIController)officer_hammerhead.AIController).SelectedTargetingParams.ReactDistance = 5000.0f;
                    }
                    /*var ai = officer_hammerhead.AIController as EnemyAIController;
                    ai.sight = 2.0f;*/
                }
                yield return null;
            }
            while(!officer_hammerhead.IsDead);
            Heal(officer);
            SetHighlight(officer_coilgunPeriscope, false);
            RemoveCompletedObjective(4);
            GameAnalyticsManager.AddDesignEvent("Tutorial:OfficerTutorial:Objective4");

            yield return new WaitForSeconds(1f, false);
            GameMain.GameSession?.CrewManager.AddSinglePlayerChatMessage(radioSpeakerName, TextManager.Get("Officer.Radio.HammerheadDead"), ChatMessageType.Radio, null);
            SetDoorAccess(officer_thirdDoor, officer_thirdDoorLight, true);

            // Room 5
            //do { yield return null; } while (!officer_rangedWeaponSensor.MotionDetected);
            do { yield return null; } while (!officer_thirdDoor.IsOpen);
            yield return new WaitForSeconds(3f, false);
            TriggerTutorialSegment(5, GameSettings.CurrentConfig.KeyMap.KeyBindText(InputType.Aim), GameSettings.CurrentConfig.KeyMap.KeyBindText(InputType.Shoot)); // Ranged weapons
            SetHighlight(officer_rangedWeaponHolder.Item, true);
            do { yield return null; } while (!officer_rangedWeaponHolder.Inventory.IsEmpty()); // Wait until looted
            SetHighlight(officer_rangedWeaponHolder.Item, false);
            do
            {
                HighlightInventorySlot(officer.Inventory, "shotgun".ToIdentifier(), highlightColor, 0.5f, 0.5f, 0f);
                yield return null;
            } while (!officer.HasEquippedItem("shotgun".ToIdentifier())); // Wait until equipped
            ItemContainer shotGunChamber = officer.Inventory.FindItemByIdentifier("shotgun".ToIdentifier()).GetComponent<ItemContainer>();
            SetHighlight(officer_rangedWeaponCabinet.Item, true);
            do
            {
                if (IsSelectedItem(officer_rangedWeaponCabinet.Item))
                {
                    if (officer_rangedWeaponCabinet.Inventory.visualSlots != null)
                    {
                        for (int i = 0; i < officer_rangedWeaponCabinet.Inventory.Capacity; i++)
                        {
                            if (officer_rangedWeaponCabinet.Inventory.GetItemAt(i)?.Prefab.Identifier == "shotgunshell")
                            {
                                HighlightInventorySlot(officer_rangedWeaponCabinet.Inventory, i, highlightColor, 0.5f, 0.5f, 0f);
                            }
                        }
                    }
                }

                for (int i = 0; i < officer.Inventory.Capacity; i++)
                {
                    if (officer.Inventory.GetItemAt(i)?.Prefab.Identifier == "shotgunshell")
                    {
                        HighlightInventorySlot(officer.Inventory, i, highlightColor, 0.5f, 0.5f, 0f);
                    }
                }

                if (officer.Inventory.FindItemByIdentifier("shotgunshell".ToIdentifier()) != null || (IsSelectedItem(officer_rangedWeaponCabinet.Item) && officer_rangedWeaponCabinet.Inventory.FindItemByIdentifier("shotgunshell".ToIdentifier()) != null))
                {
                    HighlightInventorySlot(officer.Inventory, "shotgun".ToIdentifier(), highlightColor, 0.5f, 0.5f, 0f);
                }
                yield return null;
            } while (!shotGunChamber.Inventory.IsFull(takeStacksIntoAccount: true)); // Wait until all six harpoons loaded
            RemoveCompletedObjective(5);
            GameAnalyticsManager.AddDesignEvent("Tutorial:OfficerTutorial:Objective5");
            SetHighlight(officer_rangedWeaponCabinet.Item, false);
            SetDoorAccess(officer_fourthDoor, officer_fourthDoorLight, true);

            // Room 6
            do { yield return null; } while (!officer_mudraptorObjectiveSensor.MotionDetected);
            TriggerTutorialSegment(6);
            officer_mudraptor = SpawnMonster("mudraptor", officer_mudraptorSpawnPos);
            do { yield return null; } while (!officer_mudraptor.IsDead);
            Heal(officer);
            RemoveCompletedObjective(6);
            GameAnalyticsManager.AddDesignEvent("Tutorial:OfficerTutorial:Objective6");
            SetDoorAccess(tutorial_securityFinalDoor, tutorial_securityFinalDoorLight, true);

            // Submarine
            do { yield return null; } while (!tutorial_enteredSubmarineSensor.MotionDetected);
            TriggerTutorialSegment(7);
            while (ContentRunning) yield return null;
            officer.AddActiveObjectiveEntity(officer_subAmmoBox_1, officer_gunIcon, officer_gunIconColor);
            officer.AddActiveObjectiveEntity(officer_subAmmoBox_2, officer_gunIcon, officer_gunIconColor);
            officer.AddActiveObjectiveEntity(officer_subSuperCapacitor_1.Item, officer_gunIcon, officer_gunIconColor);
            officer.AddActiveObjectiveEntity(officer_subSuperCapacitor_2.Item, officer_gunIcon, officer_gunIconColor);
            SetHighlight(officer_subSuperCapacitor_1.Item, true);
            SetHighlight(officer_subSuperCapacitor_2.Item, true);
            GameMain.GameSession?.CrewManager.AddSinglePlayerChatMessage(radioSpeakerName, TextManager.Get("Officer.Radio.Submarine"), ChatMessageType.Radio, null);
            do
            {
                SetHighlight(officer_subLoader_1.Item, officer_subLoader_1.Inventory.GetItemAt(0) == null || officer_subLoader_1.Inventory.GetItemAt(0).Condition == 0);
                SetHighlight(officer_subLoader_2.Item, officer_subLoader_2.Inventory.GetItemAt(0) == null || officer_subLoader_2.Inventory.GetItemAt(0).Condition == 0);
                HighlightInventorySlot(officer_subLoader_1.Inventory, 0, highlightColor, .5f, .5f, 0f);
                HighlightInventorySlot(officer_subLoader_2.Inventory, 0, highlightColor, .5f, .5f, 0f);

                if (officer_subSuperCapacitor_1.Item.ExternalHighlight && officer_subSuperCapacitor_1.RechargeSpeed >= superCapacitorRechargeRate)
                {
                    SetHighlight(officer_subSuperCapacitor_1.Item, false);
                    officer.RemoveActiveObjectiveEntity(officer_subSuperCapacitor_1.Item);
                }

                if (officer_subSuperCapacitor_2.Item.ExternalHighlight && officer_subSuperCapacitor_2.RechargeSpeed >= superCapacitorRechargeRate)
                {
                    SetHighlight(officer_subSuperCapacitor_2.Item, false);
                    officer.RemoveActiveObjectiveEntity(officer_subSuperCapacitor_2.Item);
                }

                SetHighlight(officer_subAmmoBox_1, officer_subAmmoBox_1.ParentInventory != officer_subLoader_1.Inventory && officer_subAmmoBox_1.ParentInventory != officer_subLoader_2.Inventory);
                SetHighlight(officer_subAmmoBox_2, officer_subAmmoBox_2.ParentInventory != officer_subLoader_1.Inventory && officer_subAmmoBox_2.ParentInventory != officer_subLoader_2.Inventory);
                SetHighlight(officer_subAmmoShelf.Item, officer_subLoader_1.Item.ExternalHighlight || officer_subLoader_2.Item.ExternalHighlight);
                if (officer_subAmmoBox_1.ParentInventory == officer_subLoader_1.Inventory || officer_subAmmoBox_1.ParentInventory == officer_subLoader_2.Inventory) officer.RemoveActiveObjectiveEntity(officer_subAmmoBox_1);
                if (officer_subAmmoBox_2.ParentInventory == officer_subLoader_1.Inventory || officer_subAmmoBox_2.ParentInventory == officer_subLoader_2.Inventory) officer.RemoveActiveObjectiveEntity(officer_subAmmoBox_2);
                yield return null;
            } while (officer_subLoader_1.Item.ExternalHighlight || officer_subLoader_2.Item.ExternalHighlight || officer_subSuperCapacitor_1.Item.ExternalHighlight || officer_subSuperCapacitor_2.Item.ExternalHighlight);
            SetHighlight(officer_subLoader_1.Item, false);
            SetHighlight(officer_subLoader_2.Item, false);
            SetHighlight(officer_subSuperCapacitor_1.Item, false);
            SetHighlight(officer_subSuperCapacitor_2.Item, false);
            SetHighlight(officer_subAmmoBox_1, false);
            SetHighlight(officer_subAmmoBox_2, false);
            SetHighlight(officer_subAmmoShelf.Item, false);
            officer.RemoveActiveObjectiveEntity(officer_subSuperCapacitor_1.Item);
            officer.RemoveActiveObjectiveEntity(officer_subSuperCapacitor_2.Item);
            officer.RemoveActiveObjectiveEntity(officer_subAmmoBox_1);
            officer.RemoveActiveObjectiveEntity(officer_subAmmoBox_2);
            RemoveCompletedObjective(7);
            GameAnalyticsManager.AddDesignEvent("Tutorial:OfficerTutorial:Objective7");
            GameMain.GameSession?.CrewManager.AddSinglePlayerChatMessage(radioSpeakerName, TextManager.Get("Officer.Radio.Complete"), ChatMessageType.Radio, null);

            yield return new WaitForSeconds(4f, false);
            GameAnalyticsManager.AddDesignEvent("Tutorial:OfficerTutorial:Completed");
            CoroutineManager.StartCoroutine(TutorialCompleted());
        }

        private bool IsSelectedItem(Item item)
        {
            return officer?.SelectedConstruction == item;
        }

        private Character SpawnMonster(string speciesName, Vector2 pos)
        {
            var character = Character.Create(speciesName, pos, ToolBox.RandomSeed(8));
            var ai = character.AIController as EnemyAIController;
            ai.TargetOutposts = true;
            character.CharacterHealth.SetVitality(character.Health / 2);
            character.AnimController.Limbs.Where(l => l.attack != null).Select(l => l.attack).ForEach(a => a.AfterAttack = AIBehaviorAfterAttack.FallBack);
            return character;
        }
    }
}
