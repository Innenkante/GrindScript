using Microsoft.Xna.Framework;
using SoG.Modding.Extensions;
using SoG.Modding.Utils;
using System;
using System.Collections.Generic;
using System.IO;

namespace SoG.FeatureExample
{
    using Quests;
    using SoG.Modding;
    using SoG.Modding.Configs;
    using CancelOptions = Animation.CancelOptions;
    using Criteria = AnimInsCriteria.Criteria;
    using EventType = AnimInsEvent.EventType;
    using LoopSettings = Animation.LoopSettings;

    public class FeatureExample: Mod
    {
        int levelUpsSoFar = 0;

        ItemCodex.ItemTypes modOneHandedWeapon = ItemCodex.ItemTypes.Null;
        ItemCodex.ItemTypes modTwoHandedWeapon = ItemCodex.ItemTypes.Null;
        ItemCodex.ItemTypes modShield = ItemCodex.ItemTypes.Null;
        ItemCodex.ItemTypes modHat = ItemCodex.ItemTypes.Null;
        ItemCodex.ItemTypes modFacegear = ItemCodex.ItemTypes.Null;
        ItemCodex.ItemTypes modAccessory = ItemCodex.ItemTypes.Null;

        ItemCodex.ItemTypes modMisc1 = ItemCodex.ItemTypes.Null;
        ItemCodex.ItemTypes modMisc2 = ItemCodex.ItemTypes.Null;

        Level.ZoneEnum modLevel = Level.ZoneEnum.None;

        EnemyCodex.EnemyTypes modSlime = EnemyCodex.EnemyTypes.Null;

        QuestCodex.QuestID modQuest = QuestCodex.QuestID.None;

        string audioIntro = "";
        string audioRipped = "";
        string audioDestiny = "";
        string audioClash = "";
        string audioDeafSilence = "";

        public override string NameID => base.NameID;

        public override void Load()
        {
            Logger.Info("Loading FeatureExample mod....");
            try
            {
                SetupItems();

                SetupAudio();

                SetupCommands();

                SetupLevels();

                SetupRoguelike();

                SetupEnemies();

                SetupQuests();

                SetupNetworking();
            }
            catch(Exception e)
            {
                Logger.Error($"Failed to load! Exception message: {e.Message}");
                return;
            }
            Logger.Info("Loaded Successfully!");
        }

        public override void Unload()
        {
            Logger.Info($"Calling Unload!");
        }

        public override void OnPlayerDamaged(PlayerView view, ref int damage, ref byte type)
        {
            Logger.Info("OnPlayerDamaged() called!");
        }

        public override void OnPlayerKilled(PlayerView player)
        {
            Logger.Info("OnPlayerKilled() called!");
        }

        public override void PostPlayerLevelUp(PlayerView player)
        {
            levelUpsSoFar++;
            if (levelUpsSoFar == 50 || levelUpsSoFar < 50 && (levelUpsSoFar + 7) % 8 == 0)
            {
                Logger.Info("PostPlayerLevelUp() called! Cumulative count: " + levelUpsSoFar);
            }
            if (levelUpsSoFar == 50)
            {
                Logger.Info("Won't bother outputting any more levelups since there's too many!");
            }
        }

        public override void OnEnemyDamaged(Enemy enemy, ref int damage, ref byte type)
        {
            Logger.Info("OnEnemyDamaged() called!");
        }

        public override void OnNPCDamaged(NPC enemy, ref int damage, ref byte type)
        {
            Logger.Info("OnNPCDamaged() called!");
        }

        public override void OnNPCInteraction(NPC npc)
        {
            Logger.Info("OnNPCInteraction() called!");
        }

        public override void OnArcadiaLoad()
        {
            Logger.Info("OnArcadiaLoad() called!");
        }

        public override void OnItemUse(ItemCodex.ItemTypes enItem, PlayerView xView, ref bool bSend)
        {
            Logger.Info("OnItemUse() called!");
        }

        public override void SaveArcadeData(BinaryWriter stream)
        {
            stream.Write("<Arcade Data here or sumthin'>");
        }

        public override void SaveCharacterData(BinaryWriter stream)
        {
            stream.Write("<Char Data here or sumthin'>");
        }

        public override void SaveWorldData(BinaryWriter stream)
        {
            stream.Write("<World Data here or sumthin'>");
        }

        public override void LoadArcadeData(BinaryReader stream)
        {
            try
            {
                Logger.Info("Arcade data: " + stream.ReadString());
            }
            catch
            {
                Logger.Info("No arcade data");
            }
        }

        public override void LoadCharacterData(BinaryReader stream)
        {
            try
            {
                Logger.Info("Char data: " + stream.ReadString());
            }
            catch
            {
                Logger.Info("No char data");
            }
        }

        public override void LoadWorldData(BinaryReader stream)
        {
            try
            {
                Logger.Info("World data: " + stream.ReadString());
            }
            catch
            {
                Logger.Info("No world data");
            }
        }

        void SetupItems()
        {
            Logger.Info("Creating Items...");

            Dictionary<string, ItemConfig> ItemLibrary = new Dictionary<string, ItemConfig>
            {
                ["_Mod_Item0001"] = new ItemConfig("_Mod_Item0001")
                {
                    Name = "Shield Example",
                    Description = "This is a custom shield!",
                    EquipType = EquipmentType.Shield,
                    IconPath = AssetPath + "Items/ModShield/Icon",
                    EquipResourcePath = AssetPath + "Items/ModShield",
                    ShldHP = 1337
                },

                ["_Mod_Item0002"] = new ItemConfig("_Mod_Item0002")
                {
                    Name = "Accessory Example",
                    Description = "This is a custom accessory that mimics a shield due to lazyness!",
                    IconPath = AssetPath + "Items/Common/Icon",
                    EquipType = EquipmentType.Accessory,
                    ATK = 1337
                },

                ["_Mod_Item0003"] = new ItemConfig("_Mod_Item0003")
                {
                    Name = "Hat Example",
                    Description = "This is a custom hat!",
                    IconPath = AssetPath + "Items/ModHat/Icon",
                    EquipType = EquipmentType.Hat,
                    EquipResourcePath = AssetPath + "Items/ModHat",
                    ATK = 1111
                }
                    .SetHatOffsets(new Vector2(4f, 7f), new Vector2(5f, 5f), new Vector2(5f, 5f), new Vector2(4f, 7f)),

                ["_Mod_Item0004"] = new ItemConfig("_Mod_Item0004")
                {
                    Name = "Facegear Example",
                    Description = "This is a custom facegear!",
                    IconPath = AssetPath + "Items/ModFacegear/Icon",
                    EquipType = EquipmentType.Facegear,
                    EquipResourcePath = AssetPath + "Items/ModFacegear",
                    ATK = 1234
                }
                    .SetFacegearOffsets(new Vector2(2f, -1f), new Vector2(5f, -3f), new Vector2(3f, -5f), new Vector2(2f, -1f)),

                ["_Mod_Item0005"] = new ItemConfig("_Mod_Item0005")
                {
                    Name = "OneHandedMelee Example",
                    Description = "This is a custom 1H weapon! It has cool animations for downward attacks.",
                    IconPath = AssetPath + "Items/Mod1H/Icon",
                    EquipType = EquipmentType.Weapon,
                    WeaponType = WeaponInfo.WeaponCategory.OneHanded,
                    MagicWeapon = false,
                    EquipResourcePath = AssetPath + "Items/Mod1H",
                    ATK = 1555
                },

                ["_Mod_Item0006"] = new ItemConfig("_Mod_Item0006")
                {
                    Name = "TwoHandedMagic Example",
                    Description = "This is a custom 2H weapon!",
                    IconPath = AssetPath + "Items/Mod2H/Icon",
                    EquipType = EquipmentType.Weapon,
                    WeaponType = WeaponInfo.WeaponCategory.TwoHanded,
                    MagicWeapon = true,
                    EquipResourcePath = AssetPath + "Items/Mod2H",
                    ATK = 776
                },

                ["_Mod_Item0007"] = new ItemConfig("_Mod_Item0007")
                {
                    Name = "Mod Misc 1",
                    Description = "This is a custom miscellaneous item!",
                    IconPath = AssetPath + "Items/Common/Icon",
                    Categories = { ItemCodex.ItemCategories.Misc }
                },

                ["_Mod_Item0008"] = new ItemConfig("_Mod_Item0008")
                {
                    Name = "Mod Misc 2",
                    Description = "This is another custom miscellaneous item!",
                    IconPath = AssetPath + "Items/Common/Icon",
                    Categories = { ItemCodex.ItemCategories.Misc }
                },
            };

            foreach (var item in ItemLibrary)
                CreateItem(item.Value);

            modShield = GetItemType(this, "_Mod_Item0001");
            modAccessory = GetItemType(this, "_Mod_Item0002");
            modHat = GetItemType(this, "_Mod_Item0003");
            modFacegear = GetItemType(this, "_Mod_Item0004");
            modOneHandedWeapon = GetItemType(this, "_Mod_Item0005");
            modTwoHandedWeapon = GetItemType(this, "_Mod_Item0006");
            modMisc1 = GetItemType(this, "_Mod_Item0007");
            modMisc2 = GetItemType(this, "_Mod_Item0008");

            AddRecipe(modOneHandedWeapon, new Dictionary<ItemCodex.ItemTypes, ushort>
            {
                [modMisc1] = 5,
                [modMisc2] = 10,
                [modTwoHandedWeapon] = 1
            });

            Logger.Info("Done with Creating Items!");
        }

        void SetupAudio()
        {
            Logger.Info("Building sounds...");

            CreateAudio(new AudioConfig().AddMusic("FeatureExample", "Intro", "Clash", "DeafSilence").AddMusic("FeatureExampleStuff", "Ripped", "Destiny"));

            audioIntro = GetMusicID("Intro");
            audioDestiny = GetMusicID("Destiny");
            audioRipped = GetMusicID("Ripped");
            audioClash = GetMusicID("Clash");
            audioDeafSilence = GetMusicID("DeafSilence");

            RedirectVanillaMusic("BossBattle01", audioClash);
            RedirectVanillaMusic("BishopBattle", audioRipped);

            Logger.Info("Done with sounds!");
        }

        void SetupCommands()
        {
            Logger.Info("Setting up commands...");

            var parsers = new Dictionary<string, CommandParser>
            {
                ["GiveItems"] = (argList, _) =>
                {
                    string[] args = argList.Split(' ');
                    if (NetUtils.IsLocal)
                    {
                        PlayerView localPlayer = Globals.Game.xLocalPlayer;
                        CAS.AddChatMessage("Dropping Items!");
                        modShield.SpawnItem(localPlayer);
                        modAccessory.SpawnItem(localPlayer);
                        modFacegear.SpawnItem(localPlayer);
                        modHat.SpawnItem(localPlayer);
                        modOneHandedWeapon.SpawnItem(localPlayer);
                        modTwoHandedWeapon.SpawnItem(localPlayer);
                    }
                    else CAS.AddChatMessage("You can't do that if you're a client!");
                },

                ["PlayMusic"] = (argList, _) =>
                {
                    string[] args = argList.Split(' ');
                    if (args.Length != 1)
                        CAS.AddChatMessage("Usage: /PlayMusic <Audio>");

                    var music = new Dictionary<string, string>
                    {
                        ["Intro"] = audioIntro,
                        ["Destiny"] = audioDestiny,
                        ["Ripped"] = audioRipped,
                        ["Clash"] = audioClash,
                        ["DeafSilence"] = audioDeafSilence
                    };

                    if (music.TryGetValue(args[0], out string ID))
                        Globals.Game.xSoundSystem.PlaySong(ID, true);
                    else CAS.AddChatMessage("Unknown mod music!");
                },

                ["TellIDs"] = (argList, _) =>
                {
                    Inventory inv = Globals.Game.xLocalPlayer.xInventory;
                    CAS.AddChatMessage("Shield:" + (int)modShield + ", count: " + inv.GetAmount(modShield));
                    CAS.AddChatMessage("Accessory:" + (int)modAccessory + ", count: " + inv.GetAmount(modAccessory));
                    CAS.AddChatMessage("Hat:" + (int)modHat + ", count: " + inv.GetAmount(modHat));
                    CAS.AddChatMessage("Facegear:" + (int)modFacegear + ", count: " + inv.GetAmount(modFacegear));
                    CAS.AddChatMessage("One Handed:" + (int)modOneHandedWeapon + ", count: " + inv.GetAmount(modOneHandedWeapon));
                    CAS.AddChatMessage("Two Handed:" + (int)modTwoHandedWeapon + ", count: " + inv.GetAmount(modTwoHandedWeapon));
                },

                ["GibCraft"] = (argList, _) =>
                {
                    PlayerView localPlayer = Globals.Game.xLocalPlayer;
                    CAS.AddChatMessage("Dropping Items!");

                    int amount = 10;
                    while (amount-- > 0)
                    {
                        modMisc1.SpawnItem(localPlayer);
                        modMisc2.SpawnItem(localPlayer);
                    }
                },

                ["Yeet"] = (argList, _) =>
                {
                    Globals.Game._Level_PrepareSwitchAuto(LevelBlueprint.GetBlueprint(modLevel), 0);
                },

                ["SpawnModSlime"] = (_1, _2) =>
                {
                    PlayerView localPlayer = Globals.Game.xLocalPlayer;
                    Globals.Game._EntityMaster_AddEnemy(modSlime, localPlayer.xEntity.xTransform.v2Pos + Utility.RandomizeVector2Direction(new Random()) * 100);

                    CAS.AddChatMessage("Spawned Mod Slime near you!");
                },

                ["RestartModQuest"] = (_1, _2) =>
                {
                    Globals.Game.EXT_ForgetQuest(modQuest);
                    Globals.Game.EXT_AddQuest(modQuest);
                },

                ["SendPacket"] = (argList, _2) => 
                {
                    Action<BinaryWriter> data = (writer) =>
                    {
                        writer.Write("Lmao message");
                    };

                    // Only one of these will run at a time, depending on role!
                    SendToAllClients(0, data);
                    SendToServer(0, data);
                },
            };

            foreach (var command in parsers)
            {
                CreateCommand(command.Key, command.Value);
            }

            Logger.Info("Commands set up successfully!");
        }

        void SetupLevels()
        {
            Logger.Info("Setting up levels...");

            LevelConfig cfg = new LevelConfig("_Mod_Level001")
            {
                WorldRegion = Level.WorldRegion.PillarMountains,
                Builder = CaveLevelStuff.Build,
                Loader = null
            };

            modLevel = CreateLevel(cfg);

            Logger.Info("Levels set up successfully!");
        }

        void SetupRoguelike()
        {
            Logger.Info("Doing Roguelike stuff...");

            PerkConfig perk01 = new PerkConfig("_Mod_Perk001")
            {
                Name = "Soul Booster",
                Description = "Gain 10 extra EP.",
                EssenceCost = 15,
                RunStartActivator = (player) =>
                {
                    player.xEntity.xBaseStats.iMaxEP += 10;
                    player.xEntity.xBaseStats._ichkBaseMaxEP += 10 * 2;
                },
                TexturePath = AssetPath + "RogueLike/SoulBooster"
            };

            CreatePerk(perk01);

            TreatCurseConfig curse01 = new TreatCurseConfig("_Mod_Curse001")
            {
                Name = "Placeholder 01",
                Description = "Placeholder-y stuff!",
                IsCurse = true,
                ScoreModifier = 5,
                TexturePath = ""
            };

            TreatCurseConfig curse02 = new TreatCurseConfig("_Mod_Curse002")
            {
                Name = "Placeholder 02",
                Description = "Placeholder-y stuff!",
                IsCurse = true,
                ScoreModifier = 5,
                TexturePath = ""
            };

            TreatCurseConfig curse03 = new TreatCurseConfig("_Mod_Curse003")
            {
                Name = "Placeholder 03",
                Description = "Placeholder-y stuff!",
                IsCurse = true,
                ScoreModifier = 5,
                TexturePath = ""
            };

            TreatCurseConfig treat01 = new TreatCurseConfig("_Mod_Treat001")
            {
                Name = "Placeholder 01",
                Description = "Placeholder-y stuff!",
                IsCurse = false,
                ScoreModifier = -5,
                TexturePath = ""
            };

            CreateTreatOrCurse(treat01);
            CreateTreatOrCurse(curse01);
            CreateTreatOrCurse(curse02);
            CreateTreatOrCurse(curse03);

            Logger.Info("Done with Roguelike stuff!");
        }

        void SetupEnemies()
        {
            // Textures are loaded in ModSlime's instance builder, so here we set them to null

            ModSlimeAI.Mod = this;
            var Animations = ModSlimeAI.ClassAnimationPrototypes;

            EnemyConfig modSlimeCfg = new EnemyConfig("_Mod_Enemy001")
            {
                Name = "Modded Slime",
                BaseHealth = 1600,
                Constructor = ModSlimeAI.InstanceBuilder,
                DifficultyScaler = ModSlimeAI.DifficultyScaler,
                EliteScaler = null,
                Category = EnemyDescription.Category.Regular,
                CardDropChance = 50f,
                CardInfo = "Boosts your bamboozling by 420%"
            };

            modSlimeCfg.LootTable.Add(new EnemyConfig.Drop(100f, ItemCodex.ItemTypes._Misc_GiftBox_Consumable));
            modSlimeCfg.LootTable.Add(new EnemyConfig.Drop(75f, ItemCodex.ItemTypes._Misc_GiftBox_Consumable));
            modSlimeCfg.LootTable.Add(new EnemyConfig.Drop(50f, ItemCodex.ItemTypes._Misc_GiftBox_Consumable));
            modSlimeCfg.LootTable.Add(new EnemyConfig.Drop(25f, ItemCodex.ItemTypes._Misc_GiftBox_Consumable));

            modSlime = CreateEnemy(modSlimeCfg);

            Animations[0] = new Animation(0, 0, null, new Vector2(14f, 14f), 4, 8, 25, 18, 0, 0, 8, LoopSettings.Looping, CancelOptions.IgnoreIfPlaying, true, true) { bIgnoreSentTicks = true };

            Animations[1] = new Animation(1, 0, null, new Vector2(14f, 19f), 4, 14, 25, 23, 0, 0, 14, LoopSettings.Looping, CancelOptions.IgnoreIfPlaying, true, true);
            Animations[1].SetInstructions(
                new AnimationInstruction(new AnimInsCriteria(Criteria.TriggerAtFrameX, 2f), new AnimInsEvent(EventType.PlaySound, "Slime_Jump", 0f))
                );

            Animations[2] = new Animation(2, 0, null, new Vector2(14f, 10f), 4, 2, 27, 14, 0, 0, 2, LoopSettings.Clamp, CancelOptions.IgnoreIfPlaying, false, false);
            Animations[2].SetInstructions(
                new AnimationInstruction(new AnimInsCriteria(Criteria.TriggerAtFrameX, 2f), new AnimInsEvent(EventType.FreezeFrame, 10f, 0f)),
                new AnimationInstruction(new AnimInsCriteria(Criteria.TriggerAtEnd), new AnimInsEvent(EventType.PlayAnimation, 3f))
                );

            Animations[3] = new Animation(3, 0, null, new Vector2(14f, 10f), 4, 2, 27, 14, 0, 0, 2, LoopSettings.Clamp, CancelOptions.IgnoreIfPlaying, false, false);
            Animations[3].SetInstructions(
                new AnimationInstruction(new AnimInsCriteria(Criteria.TriggerAtEnd), new AnimInsEvent(EventType.PlayAnimation, 0f))
                );

            Animations[4] = new Animation(4, 0, null, new Vector2(12f, 17f), 4, 18, 21, 26, 0, 0, 18, LoopSettings.Clamp, CancelOptions.IgnoreIfPlaying, false, true);
            Animations[4].SetInstructions(
                new AnimationInstruction(new AnimInsCriteria(Criteria.TriggerAtFrameX, 8f), new AnimInsEvent(EventType.PlaySound, "Slime_Attack", 1f)),
                new AnimationInstruction(new AnimInsCriteria(Criteria.TriggerAtFrameX, 9f), new AnimInsEvent(EventType.FreezeFrame, 10f, 0f)),
                new AnimationInstruction(new AnimInsCriteria(Criteria.TriggerAtFrameX, 11f), new AnimInsEvent(EventType.AddSmoothPush, 0f, -3f)),
                new AnimationInstruction(new AnimInsCriteria(Criteria.TriggerAtFrameX, 11f), new AnimInsEvent(EventType.FreezeFrame, 10f, 0f)),
                new AnimationInstruction(new AnimInsCriteria(Criteria.TriggerAtEnd), new AnimInsEvent(EventType.PlayAnimation, 0f))
                );

            Animations[5] = new Animation(5, 1, null, new Vector2(17f, 12f), 4, 18, 29, 16, 0, 0, 18, LoopSettings.Clamp, CancelOptions.IgnoreIfPlaying, false, true);
            Animations[5].SetInstructions(
                new AnimationInstruction(new AnimInsCriteria(Criteria.TriggerAtFrameX, 8f), new AnimInsEvent(EventType.PlaySound, "Slime_Attack", 1f)),
                new AnimationInstruction(new AnimInsCriteria(Criteria.TriggerAtFrameX, 9f), new AnimInsEvent(EventType.FreezeFrame, 10f, 0f)),
                new AnimationInstruction(new AnimInsCriteria(Criteria.TriggerAtFrameX, 11f), new AnimInsEvent(EventType.AddSmoothPush, 3f, 0f)),
                new AnimationInstruction(new AnimInsCriteria(Criteria.TriggerAtFrameX, 11f), new AnimInsEvent(EventType.FreezeFrame, 10f, 0f)),
                new AnimationInstruction(new AnimInsCriteria(Criteria.TriggerAtEnd), new AnimInsEvent(EventType.PlayAnimation, 0f))
                );

            Animations[6] = new Animation(6, 2, null, new Vector2(12f, 18f), 4, 18, 21, 26, 0, 0, 18, LoopSettings.Clamp, CancelOptions.IgnoreIfPlaying, false, true);
            Animations[6].SetInstructions(
                new AnimationInstruction(new AnimInsCriteria(Criteria.TriggerAtFrameX, 8f), new AnimInsEvent(EventType.PlaySound, "Slime_Attack", 1f)),
                new AnimationInstruction(new AnimInsCriteria(Criteria.TriggerAtFrameX, 9f), new AnimInsEvent(EventType.FreezeFrame, 10f, 0f)),
                new AnimationInstruction(new AnimInsCriteria(Criteria.TriggerAtFrameX, 11f), new AnimInsEvent(EventType.AddSmoothPush, 0f, 3f)),
                new AnimationInstruction(new AnimInsCriteria(Criteria.TriggerAtFrameX, 11f), new AnimInsEvent(EventType.FreezeFrame, 10f, 0f)),
                new AnimationInstruction(new AnimInsCriteria(Criteria.TriggerAtEnd), new AnimInsEvent(EventType.PlayAnimation, 0f))
                );

            Animations[7] = new Animation(7, 3, null, new Vector2(17f, 12f), 4, 18, 29, 16, 0, 0, 18, LoopSettings.Clamp, CancelOptions.IgnoreIfPlaying, false, true);
            Animations[7].SetInstructions(
            new AnimationInstruction(new AnimInsCriteria(Criteria.TriggerAtFrameX, 8f), new AnimInsEvent(EventType.PlaySound, "Slime_Attack", 1f)),
                new AnimationInstruction(new AnimInsCriteria(Criteria.TriggerAtFrameX, 9f), new AnimInsEvent(EventType.FreezeFrame, 10f, 0f)),
                new AnimationInstruction(new AnimInsCriteria(Criteria.TriggerAtFrameX, 11f), new AnimInsEvent(EventType.AddSmoothPush, -3f, 0f)),
                new AnimationInstruction(new AnimInsCriteria(Criteria.TriggerAtFrameX, 11f), new AnimInsEvent(EventType.FreezeFrame, 10f, 0f)),
                new AnimationInstruction(new AnimInsCriteria(Criteria.TriggerAtEnd), new AnimInsEvent(EventType.PlayAnimation, 0f))
                );

            Animations[8] = new Animation(8, 0, null, new Vector2(17f, 12f), 4, 5, 32, 20, 0, 0, 5, LoopSettings.Clamp, CancelOptions.IgnoreIfPlaying, false, false);
            Animations[8].SetInstructions(
                new AnimationInstruction(new AnimInsCriteria(Criteria.TriggerAtFrameX, 5f), new AnimInsEvent(EventType.CallBackAnimation, 0f)),
                new AnimationInstruction(new AnimInsCriteria(Criteria.TriggerAtEnd), new AnimInsEvent(EventType.PlayAnimation, 0f))
                );

            Animations[9] = new Animation(40000, 0, null, new Vector2(14f, 10f), 4, 1, 27, 14, 0, 0, 2, LoopSettings.Clamp, CancelOptions.IgnoreIfPlaying, false, false);
            Animations[10] = new Animation(40001, 1, null, new Vector2(14f, 10f), 4, 1, 27, 14, 0, 0, 2, LoopSettings.Clamp, CancelOptions.IgnoreIfPlaying, false, false);
            Animations[11] = new Animation(40002, 2, null, new Vector2(14f, 10f), 4, 1, 27, 14, 0, 0, 2, LoopSettings.Clamp, CancelOptions.IgnoreIfPlaying, false, false);
            Animations[12] = new Animation(40003, 3, null, new Vector2(14f, 10f), 4, 1, 27, 14, 0, 0, 2, LoopSettings.Clamp, CancelOptions.IgnoreIfPlaying, false, false);

            Animations[13] = new Animation(40004, 0, null, new Vector2(14f, 10f), 4, 2, 27, 14, 0, 0, 2, LoopSettings.Clamp, CancelOptions.IgnoreIfPlaying, false, false);
            Animations[13].SetInstructions(
                new AnimationInstruction(new AnimInsCriteria(Criteria.TriggerAtEnd), new AnimInsEvent(EventType.PlayAnimation, 0f))
                );

            Animations[14] = new Animation(40005, 0, null, new Vector2(14f, 10f), 4, 2, 27, 14, 0, 0, 2, LoopSettings.Clamp, CancelOptions.IgnoreIfPlaying, false, false);
            Animations[14].SetInstructions(
                new AnimationInstruction(new AnimInsCriteria(Criteria.TriggerAtEnd), new AnimInsEvent(EventType.PlayAnimation, 0f))
                );

            Animations[15] = new Animation(40006, 0, null, new Vector2(14f, 10f), 4, 2, 27, 14, 0, 0, 2, LoopSettings.Clamp, CancelOptions.IgnoreIfPlaying, false, false);
            Animations[15].SetInstructions(
                new AnimationInstruction(new AnimInsCriteria(Criteria.TriggerAtEnd), new AnimInsEvent(EventType.PlayAnimation, 0f))
                );

            Animations[16] = new Animation(40007, 0, null, new Vector2(14f, 10f), 4, 2, 27, 14, 0, 0, 2, LoopSettings.Clamp, CancelOptions.IgnoreIfPlaying, false, false);
            Animations[16].SetInstructions(
                new AnimationInstruction(new AnimInsCriteria(Criteria.TriggerAtEnd), new AnimInsEvent(EventType.PlayAnimation, 0f))
                );

        }

        void SetupQuests()
        {
            SymbolicItemFlagReward reward = new SymbolicItemFlagReward();
            reward.AddItem(ItemCodex.ItemTypes._KeyItem_GoldenCarrot, 1);

            QuestConfig basicQuest = new QuestConfig("_Mod_Quest001")
            {
                Name = "Carrot Collettor",
                Summary = "Collect 375 carrots for Mr. Morcov's big carrot festival!",
                Description = "<insert stuff>",
                RecommendedPlayerLevel = 69,
                Constructor = x =>
                {
                    x.lxObjectives.Add(new Objective_FindItems(ItemCodex.ItemTypes.Carrot, 500, "", false));
                    x.liObjectiveGroups.Add(1);
                },
                Reward = reward
            };

            modQuest = CreateQuest(basicQuest);
        }

        void SetupNetworking()
        {
            ModPacket pinger = new ModPacket()
            {
                ParseOnClient = (BinaryReader x) =>
                {
                    string message = x.ReadString();
                    CAS.AddChatMessage("Mod Message from Server: " + message);
                },
                ParseOnServer = (BinaryReader x, long connectionID) =>
                {
                    PlayerView view = Globals.Game.dixPlayers[connectionID];

                    string message = x.ReadString();
                    CAS.AddChatMessage($"Mod Message from Client {view.sNetworkNickname}: " + message);
                }
            };

            AddPacket(0, pinger);
        }
    }
}
