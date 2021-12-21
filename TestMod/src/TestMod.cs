using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using Quests;
using SoG.Modding.Content;
using SoG.Modding.Utils;
using SoG.Modding.Extensions;
using Microsoft.Xna.Framework.Graphics;

namespace SoG.Modding.TestMod
{
    using CancelOptions = Animation.CancelOptions;
    using Criteria = AnimInsCriteria.Criteria;
    using EventType = AnimInsEvent.EventType;
    using LoopSettings = Animation.LoopSettings;

    [ModDependency("GrindScript", "0.14")]
    [ModDependency("Addons.ModGoodies", "0.14")]
    public class TestMod: Mod
    {
        int levelUpsSoFar = 0;

        string modOneHandedWeapon = "_Mod_Item0005";
        string modTwoHandedWeapon = "_Mod_Item0006";
        string modShield = "_Mod_Item0001";
        string modHat = "_Mod_Item0003";
        string modFacegear = "_Mod_Item0004";
        string modAccessory = "_Mod_Item0002";

        string modMisc1 = "_Mod_Item0007";
        string modMisc2 = "_Mod_Item0008";

        string modLevel = "_Mod_Level001";

        string modSlime = "_Mod_Enemy001";

        string modQuest = "_Mod_Quest001";

        bool buffTalents = false;

        public override string NameID => "TestMod";

        public override Version ModVersion => new Version("0.14");

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

                Mod addon = Manager.GetMod("Addons.ModGoodies");

                if (addon != null)
                {
                    (addon as Addons.ModGoodies).AddSkillEditCallback((SpellCodex.SpellTypes skill, byte level, ref int modifiedLevel) => {
                        if (buffTalents && SpellCodex.IsTalent(skill))
                        {
                            modifiedLevel += 10;
                        }
                    });
                }

            }
            catch(Exception e)
            {
                Logger.Error($"Failed to load! Exception message: {e.Message}");
                return;
            }
            Logger.Info("Loaded Successfully!");
        }

        public override void PostLoad()
        {
            // In here goes anything that can't be done during Load
            // For instance, Audio IDs are only available once Load() exits

            AddCraftingRecipe(GetItem(modOneHandedWeapon).GameID, new Dictionary<ItemCodex.ItemTypes, ushort>
            {
                [GetItem(modMisc1).GameID] = 5,
                [GetItem(modMisc2).GameID] = 10,
                [GetItem(modTwoHandedWeapon).GameID] = 1
            });

            GetAudio().RedirectVanillaMusic("BossBattle01", "Clash");
            GetAudio().RedirectVanillaMusic("BishopBattle", "Ripped");
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

            ItemEntry item;

            item = CreateItem(modShield);
            item.Name = "Shield Example";
            item.Description = "This is a custom shield!";
            item.EquipType = EquipmentType.Shield;
            item.IconPath = AssetPath + "Items/ModShield/Icon";
            item.EquipResourcePath = AssetPath + "Items/ModShield";
            item.ShldHP = 133;

            item = CreateItem(modAccessory);
            item.Name = "Accessory Example";
            item.Description = "This is a custom accessory that mimics a shield due to lazyness!";
            item.IconPath = AssetPath + "Items/Common/Icon";
            item.EquipType = EquipmentType.Accessory;
            item.ATK = 1337;

            item = CreateItem(modHat);
            item.Name = "Hat Example";
            item.Description = "This is a custom hat!";
            item.IconPath = AssetPath + "Items/ModHat/Icon";
            item.EquipType = EquipmentType.Hat;
            item.EquipResourcePath = AssetPath + "Items/ModHat";
            item.ATK = 1111;
            item.DefaultSet.SetHatOffsets(new Vector2(4f, 7f), new Vector2(5f, 5f), new Vector2(5f, 5f), new Vector2(4f, 7f));

            item = CreateItem(modFacegear);
            item.Name = "Facegear Example";
            item.Description = "This is a custom facegear!";
            item.IconPath = AssetPath + "Items/ModFacegear/Icon";
            item.EquipType = EquipmentType.Facegear;
            item.EquipResourcePath = AssetPath + "Items/ModFacegear";
            item.ATK = 1234;
            item.SetFacegearOffsets(new Vector2(2f, -1f), new Vector2(5f, -3f), new Vector2(3f, -5f), new Vector2(2f, -1f));

            item = CreateItem(modOneHandedWeapon);
            item.Name = "OneHandedMelee Example";
            item.Description = "This is a custom 1H weapon! It has cool animations for downward attacks.";
            item.IconPath = AssetPath + "Items/Mod1H/Icon";
            item.EquipType = EquipmentType.Weapon;
            item.WeaponType = WeaponInfo.WeaponCategory.OneHanded;
            item.MagicWeapon = false;
            item.EquipResourcePath = AssetPath + "Items/Mod1H";
            item.ATK = 1555;

            item = CreateItem(modTwoHandedWeapon);
            item.Name = "TwoHandedMagic Example";
            item.Description = "This is a custom 2H weapon!";
            item.IconPath = AssetPath + "Items/Mod2H/Icon";
            item.EquipType = EquipmentType.Weapon;
            item.WeaponType = WeaponInfo.WeaponCategory.TwoHanded;
            item.MagicWeapon = true;
            item.EquipResourcePath = AssetPath + "Items/Mod2H";
            item.ATK = 776;

            item = CreateItem(modMisc1);
            item.Name = "Mod Misc 1";
            item.Description = "This is a custom miscellaneous item!";
            item.IconPath = AssetPath + "Items/Common/Icon";
            item.AddCategory(ItemCodex.ItemCategories.Misc);

            item = CreateItem(modMisc2);
            item.Name = "Mod Misc 2";
            item.Description = "This is another custom miscellaneous item!";
            item.IconPath = AssetPath + "Items/Common/Icon";
            item.AddCategory(ItemCodex.ItemCategories.Misc);

            Logger.Info("Done with Creating Items!");
        }

        void SetupAudio()
        {
            Logger.Info("Building sounds...");

            AudioEntry audio = CreateAudio();

            audio.AddMusic("TestMod", "Intro", "Clash", "DeafSilence");
            audio.AddMusic("TestModStuff", "Ripped", "Destiny");

            Logger.Info("Done with sounds!");
        }

        void SetupCommands()
        {
            Logger.Info("Setting up commands...");

            CommandEntry commands = CreateCommands();

            commands.SetCommand("BuffTalents", (args, _) =>
            {
                buffTalents = !buffTalents;
                CAS.AddChatMessage("Buffed talents: " + buffTalents);
            });

            commands.SetCommand("GiveItems", (args, _) =>
            {
                if (NetUtils.IsLocalOrServer)
                {
                    PlayerView localPlayer = Globals.Game.xLocalPlayer;
                    CAS.AddChatMessage("Dropping Items!");
                    GetItem(modShield).GameID.SpawnItem(localPlayer);
                    GetItem(modAccessory).GameID.SpawnItem(localPlayer);
                    GetItem(modFacegear).GameID.SpawnItem(localPlayer);
                    GetItem(modHat).GameID.SpawnItem(localPlayer);
                    GetItem(modOneHandedWeapon).GameID.SpawnItem(localPlayer);
                    GetItem(modTwoHandedWeapon).GameID.SpawnItem(localPlayer);
                }
                else CAS.AddChatMessage("You can't do that if you're a client!");
            });

            commands.SetCommand("PlayMusic", (args, _) =>
            {
                if (args.Length != 1)
                    CAS.AddChatMessage("Usage: /PlayMusic <Audio>");

                var music = new Dictionary<string, string>
                {
                    ["Intro"] = GetAudio().GetMusicID("Intro"),
                    ["Destiny"] = GetAudio().GetMusicID("Destiny"),
                    ["Ripped"] = GetAudio().GetMusicID("Ripped"),
                    ["Clash"] = GetAudio().GetMusicID("Clash"),
                    ["DeafSilence"] = GetAudio().GetMusicID("DeafSilence")
                };

                if (music.TryGetValue(args[0], out string ID))
                    Globals.Game.xSoundSystem.PlaySong(ID, true);
                else CAS.AddChatMessage("Unknown mod music!");
            });

            commands.SetCommand("GibCraft", (args, _) =>
            {
                PlayerView localPlayer = Globals.Game.xLocalPlayer;
                CAS.AddChatMessage("Dropping Items!");

                int amount = 10;
                while (amount-- > 0)
                {
                    GetItem(modMisc1).GameID.SpawnItem(localPlayer);
                    GetItem(modMisc2).GameID.SpawnItem(localPlayer);
                }
            });

            commands.SetCommand("Yeet", (args, _) =>
            {
                Globals.Game._Level_PrepareSwitchAuto(LevelBlueprint.GetBlueprint(GetLevel(modLevel).GameID), 0);
            });

            commands.SetCommand("SpawnModSlime", (_1, _2) =>
            {
                PlayerView localPlayer = Globals.Game.xLocalPlayer;
                Globals.Game._EntityMaster_AddEnemy(GetEnemy(modSlime).GameID, localPlayer.xEntity.xTransform.v2Pos + Utility.RandomizeVector2Direction(new Random()) * 100);

                CAS.AddChatMessage("Spawned Mod Slime near you!");
            });

            commands.SetCommand("RestartModQuest", (_1, _2) =>
            {
                Globals.Game.EXT_ForgetQuest(GetQuest(modQuest).GameID);
                Globals.Game.EXT_AddQuest(GetQuest(modQuest).GameID);
            });

            commands.SetCommand("SendPacket", (args, _2) =>
            {
                CAS.AddChatMessage("Sending message!");

                Action<BinaryWriter> data = (writer) =>
                {
                    writer.Write("Lmao message");
                };

                // Only one of these will run at a time, depending on role!
                SendToAllClients(0, data);
                SendToServer(0, data);
            });

            Logger.Info("Commands set up successfully!");
        }

        void SetupLevels()
        {
            Logger.Info("Setting up levels...");

            LevelEntry level = CreateLevel(modLevel);
            level.WorldRegion = Level.WorldRegion.PillarMountains;
            level.Builder = CaveLevelStuff.Build;
            level.Loader = null;

            Logger.Info("Levels set up successfully!");
        }

        void SetupRoguelike()
        {
            Logger.Info("Doing Roguelike stuff...");

            PerkEntry perk01 = CreatePerk("_Mod_Perk001");
            perk01.Name = "Soul Booster";
            perk01.Description = "Gain 10 extra EP.";
            perk01.EssenceCost = 15;
            perk01.RunStartActivator = (player) =>
            {
                player.xEntity.xBaseStats.iMaxEP += 10;
                player.xEntity.xBaseStats._ichkBaseMaxEP += 10 * 2;
            };
            perk01.TexturePath = AssetPath + "RogueLike/SoulBooster";

            CurseEntry curse01 = CreateCurse("_Mod_Curse001");
            curse01.Name = "Placeholder Curse 01";
            curse01.Description = "Placeholder-y stuff!";
            curse01.IsCurse = true;
            curse01.ScoreModifier = 0.4f;
            curse01.TexturePath = "";

            CurseEntry curse02 = CreateCurse("_Mod_Treat001");
            curse02.Name = "Placeholder Treat 01";
            curse02.Description = "Placeholder-y stuff!";
            curse02.IsCurse = false;
            curse02.ScoreModifier = -0.15f;
            curse02.TexturePath = "";

            PinEntry pin01 = CreatePin("_Mod_Pin001");
            pin01.CreateCollectionEntry = true;
            pin01.Description = "Omg it's a P I N";
            pin01.EquipAction = (player) =>
            {
                player.xEntity.xBaseStats.iBaseMaxHP += 150;
                player.xEntity.xBaseStats._ichkHPBalance += 150 * 2;
            };
            pin01.UnequipAction = (player) =>
            {
                player.xEntity.xBaseStats.iBaseMaxHP -= 150;
                player.xEntity.xBaseStats._ichkHPBalance -= 150 * 2;
            };

            Logger.Info("Done with Roguelike stuff!");
        }

        void SetupEnemies()
        {
            // Textures are loaded in ModSlime's instance builder, so here we set them to null

            ModSlimeAI.Mod = this;

            EnemyEntry enemy = CreateEnemy(modSlime);
            enemy.Name = "Modded Slime";
            enemy.BaseHealth = 1600;
            enemy.Constructor = ModSlimeAI.InstanceBuilder;
            enemy.DifficultyScaler = ModSlimeAI.DifficultyScaler;
            enemy.EliteScaler = null;
            enemy.Category = EnemyDescription.Category.Regular;
            enemy.CardDropChance = 50f;
            enemy.CardInfo = "Boosts your bamboozling by 420%";
            enemy.DefaultAnimation = (Content) => new Animation(1, 0, Content.Load<Texture2D>("Sprites/Monster/Pillar Mountains/Slime/Run/Up"), new Vector2(14f, 20f), 4, 14, 25, 23, 0, 0, 14, LoopSettings.Looping, CancelOptions.IgnoreIfPlaying, true, true);
            enemy.CardIllustrationPath = "GUI/InGameMenu/Journal/CardAlbum/Cards/slime_green";
            enemy.DisplayBackgroundPath = "GUI/InGameMenu/Journal/journalBG_FlyingFortress";
            enemy.OnHitSound = "LoodDamage";
            enemy.OnHitSound = "Slime_Death";

            enemy.LootTable.Add(new EnemyEntry.Drop(100f, ItemCodex.ItemTypes._Misc_GiftBox_Consumable));
            enemy.LootTable.Add(new EnemyEntry.Drop(75f, ItemCodex.ItemTypes._Misc_GiftBox_Consumable));
            enemy.LootTable.Add(new EnemyEntry.Drop(50f, ItemCodex.ItemTypes._Misc_GiftBox_Consumable));
            enemy.LootTable.Add(new EnemyEntry.Drop(25f, ItemCodex.ItemTypes._Misc_GiftBox_Consumable));

            var Animations = ModSlimeAI.ClassAnimationPrototypes;

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

            QuestEntry basicQuest = CreateQuest(modQuest);
            basicQuest.Name = "Carrot Collettor";
            basicQuest.Summary = "Collect 375 carrots for Mr. Morcov's big carrot festival!";
            basicQuest.Description = "<insert stuff>";
            basicQuest.RecommendedPlayerLevel = 69;
            basicQuest.Constructor = x =>
            {
                x.lxObjectives.Add(new Objective_FindItems(ItemCodex.ItemTypes.Carrot, 500, "", false));
                x.liObjectiveGroups.Add(1);
            };
            basicQuest.Reward = reward;
        }

        void SetupNetworking()
        {
            NetworkEntry network = CreateNetwork();

            network.SetClientSideParser(0, (BinaryReader x) =>
            {
                string message = x.ReadString();
                CAS.AddChatMessage("Mod Message from Server: " + message);
            });

            network.SetServerSideParser(0, (BinaryReader x, long connectionID) =>
            {
                PlayerView view = Globals.Game.dixPlayers[connectionID];

                string message = x.ReadString();
                CAS.AddChatMessage($"Mod Message from Client {view.sNetworkNickname}: " + message);
            });
        }
    }
}
