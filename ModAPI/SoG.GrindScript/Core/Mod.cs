using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using SoG.Modding.Utils;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Quests;
using Lidgren.Network;
using HarmonyLib;
using SoG.Modding.Configs;
using SoG.Modding.LibraryEntries;

namespace SoG.Modding
{
    /// <summary>
    /// Represents the base class for all mods.
    /// Mod DLLs should have one class that subclasses from BaseScript to be usable. <para/>
    /// All mods need to implement the LoadContent method. GrindScript will call this method when modded content should be loaded. <para/>
    /// BaseScript has a few callbacks that can be overriden to add extra behavior to the game for certain events.
    /// </summary>
    public abstract class Mod
    {
        public class ModPacket
        {
            /// <summary>
            /// Called when the server receives the mod packet from clients.
            /// The connection ID can be used to retrieve the client that sent the message.
            /// </summary>
            public ClientMessageParser ParseOnServer { get; set; }

            /// <summary>
            /// Called when the client receives the mod packet from servers.
            /// </summary>
            public ServerMessageParser ParseOnClient { get; set; }
        }

        internal class ModAudio
        {
            public bool Initialized = false;

            public SoundBank EffectsSB; // "<Mod>Effects.xsb"

            public WaveBank EffectsWB; // "<Mod>Music.xwb"

            public SoundBank MusicSB; //"<Mod>Music.xsb"

            public WaveBank UniversalWB; // "<Mod>.xwb", never unloaded

            public List<string> IndexedEffectCues = new List<string>();

            public List<string> IndexedMusicCues = new List<string>();

            public List<string> IndexedMusicBanks = new List<string>();

            public void Cleanup()
            {
                EffectsSB?.Dispose();

                EffectsWB?.Dispose();

                MusicSB?.Dispose();

                UniversalWB?.Dispose();
            }
        }

        public Mod()
        {
            var time = Launcher.LaunchTime;

            Logger = new ConsoleLogger(Globals.Logger?.LogLevel ?? LogLevels.Debug, NameID)
            {
                SourceColor = ConsoleColor.Yellow,
                NextLogger = new FileLogger(Globals.Logger?.LogLevel ?? LogLevels.Debug, NameID)
                {
                    FilePath = Path.Combine("Logs", $"ConsoleLog_{time.Year}.{time.Month}.{time.Day}_{time.Hour}.{time.Minute}.{time.Second}.txt")
                }
            };
        }

        /// <summary>
        /// The name of the mod. This acts as an identifier, and should be unique between mods.
        /// The default value is GetType().Name.
        /// </summary>
        public virtual string NameID => GetType().Name;

        /// <summary>
        /// The default Logger for this mod.
        /// </summary>
        public ILogger Logger { get; protected set; }

        /// <summary>
        /// The default ContentManager for this mod.
        /// </summary>
        public ContentManager Content { get; internal set; }

        /// <summary>
        /// The path to the mod's assets, relative to the "ModContent" folder.
        /// The default value is "ModContent/{NameID}".
        /// </summary>
        public string AssetPath => Path.Combine("ModContent", NameID) + "/";

        /// <summary>
        /// A reference to the registry, for ease of use.
        /// </summary>
        internal ModManager Manager { get; set; }

        /// <summary>
        /// Index assigned to mod by GrindScript.
        /// </summary>
        internal int ModIndex { get; set; }

        /// <summary>
        /// The packets parsable by this mod.
        /// </summary>
        internal Dictionary<ushort, ModPacket> ModPackets { get; } = new Dictionary<ushort, ModPacket>();

        /// <summary>
        /// The commands defined by this mod.
        /// </summary>
        internal Dictionary<string, CommandParser> ModCommands { get; } = new Dictionary<string, CommandParser>();

        /// <summary>
        /// Stores modded audio defined by this mod.
        /// </summary>
        internal ModAudio Audio { get; } = new ModAudio();

        private bool InLoad => Manager.Loader.ModInLoad == this;

        /// <summary>
        /// Gets a collection of the modded game objects for this mod.
        /// </summary>
        private ModLibrary GetLibrary()
        {
            return Manager.Library.GetLibraryOfMod(this);
        }

        #region Virtual Methods

        /// <summary>
        /// Use this method to create or load your modded game content.
        /// </summary>
        public abstract void Load();

        /// <summary>
        /// Use this method to unload any content that is not automatically unloaded by GrindScript.
        /// Unload is guaranteed to happen in the reverse order that mods were loaded.
        /// </summary>
        public abstract void Unload();

        /// <summary>
        /// Called when character files (".cha") need saving.
        /// You can write your Story Mode character-related information into the given stream.
        /// </summary>
        public virtual void SaveCharacterData(BinaryWriter stream) { }

        /// <summary>
        /// Called when character files (".cha") need loading.
        /// You can load your previously written data from this stream.
        /// The method must also handle cases where the stream is empty, or incomplete.
        /// </summary>
        public virtual void LoadCharacterData(BinaryReader stream) { }

        /// <summary>
        /// Called when world files (".wld") need saving.
        /// You can write your Story Mode world-related information into the given stream.
        /// </summary>
        public virtual void SaveWorldData(BinaryWriter stream) { }

        /// <summary>
        /// Called when world files (".wld") need loading.
        /// You can load your previously written data from this stream.
        /// The method must also handle cases where the stream is empty, or incomplete.
        /// </summary>
        public virtual void LoadWorldData(BinaryReader stream) { }

        /// <summary>
        /// Called when arcade files (".sav") need saving.
        /// You can write your Arcade-related information into the given stream.
        /// </summary>
        public virtual void SaveArcadeData(BinaryWriter stream) { }

        /// <summary>
        /// Called when arcade files (".sav") need loading.
        /// You can load your previously written data from this stream.
        /// The method must also handle cases where the stream is empty, or incomplete.
        /// </summary>
        public virtual void LoadArcadeData(BinaryReader stream) { }

        #endregion

        #region Game Logic Callbacks

        /// <summary>
        /// Called before Game1.Draw().
        /// </summary>
        public virtual void OnDraw() { }

        /// <summary>
        /// Called when a player is damaged by something.
        /// </summary>
        public virtual void OnPlayerDamaged(PlayerView player, ref int damage, ref byte type) { }

        /// <summary>
        /// Called when a player dies.
        /// </summary>
        public virtual void OnPlayerKilled(PlayerView player) { }

        /// <summary>
        /// Called after a player levels up.
        /// During save file loading, this method is called multiple times to initialize the player's stats to their level.
        /// </summary>
        public virtual void PostPlayerLevelUp(PlayerView player) { }

        /// <summary>
        /// Called when an enemy is damaged by something.
        /// </summary>
        public virtual void OnEnemyDamaged(Enemy enemy, ref int damage, ref byte type) { }

        /// <summary>
        /// Called when an Enemy dies.
        /// </summary>
        public virtual void PostEnemyKilled(Enemy enemy, AttackPhase killer) { }

        /// <summary>
        /// Called when an NPC is damaged by something.
        /// </summary>
        public virtual void OnNPCDamaged(NPC enemy, ref int damage, ref byte type) { }

        /// <summary>
        /// Called when a player interacts with an NPC
        /// </summary>
        public virtual void OnNPCInteraction(NPC npc) { }

        /// <summary>
        /// Called when the game loads Arcadia's level.
        /// </summary>
        public virtual void OnArcadiaLoad() { }

        /// <summary>
        /// Called when a new Arcade room is entered, after it has been prepared by the game
        /// (i.e. enemies have been spawned, traps laid out, etc.)
        /// </summary>
        public virtual void PostArcadeRoomStart() { }

        /// <summary>
        /// Called when an Arcade room has completed (if applicable).
        /// </summary>
        public virtual void PostArcadeRoomComplete() { }

        /// <summary>
        /// Called when an Enemy has been spawned as part of an Arcade Gauntlet.
        /// </summary>
        public virtual void PostArcadeGauntletEnemySpawned(Enemy enemy) { }

        /// <summary>
        /// Called when a player uses an item. This method can be used to implement behavior for usable items.
        /// Items can be used if they have the "Usable" item category.
        /// </summary>
        public virtual void OnItemUse(ItemCodex.ItemTypes enItem, PlayerView xView, ref bool bSend) { }

        /// <summary>
        /// Called after a spell charge started.
        /// </summary>
        public virtual void PostSpellActivation(PlayerView xView, ISpellActivation xact, SpellCodex.SpellTypes enType, int iBoostState) { }

        /// <summary>
        /// Called after a level was loaded.
        /// </summary>
        public virtual void PostLevelLoad(Level.ZoneEnum level, Level.WorldRegion region, bool staticOnly) { }

        #endregion

        #region Game Object Creation Methods

        /// <summary>
        /// Creates a new quest from the given QuestConfig.
        /// The quest must have a reward defined for it to be valid.
        /// </summary>
        public QuestCodex.QuestID CreateQuest(QuestConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            if (config.Reward == null)
                throw new ArgumentException("config's Reward must not be null!");

            if (!InLoad)
                throw new InvalidOperationException(ErrorCodex.UseOnlyDuringLoad);

            bool duplicateID = GetLibrary().Quests.Any(x => x.Value.ModID == config.ModID);

            if (duplicateID)
                throw new InvalidOperationException(ErrorCodex.DuplicateModID);

            QuestCodex.QuestID gameID = Manager.ID.QuestIDNext++;

            QuestDescription questData = new QuestDescription()
            {
                sQuestNameReference = $"Quest_{(int)gameID}_Name",
                sSummaryReference = $"Quest_{(int)gameID}_Summary",
                sDescriptionReference = $"Quest_{(int)gameID}_Description",
                iIntendedLevel = config.RecommendedPlayerLevel,
                enType = config.Type,
                xReward = config.Reward
            };

            QuestEntry entry = new QuestEntry(this, gameID, config.ModID)
            {
                Config = config.DeepCopy(),
                QuestData = questData
            };

            Manager.Library.Quests[gameID] = entry;

            entry.Initialize();

            return gameID;
        }

        public Objective_SpecialObjective.UniqueID CreateSpecialObjective()
        {
            return Manager.ID.SpecialObjectiveIDNext++;
        }

        public SpellCodex.SpellTypes CreateSpell(SpellConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            if (!InLoad)
                throw new InvalidOperationException(ErrorCodex.UseOnlyDuringLoad);

            bool duplicateID = GetLibrary().Spells.Any(x => x.Value.ModID == config.ModID);

            if (duplicateID)
                throw new InvalidOperationException(ErrorCodex.DuplicateModID);

            SpellCodex.SpellTypes gameID = Manager.ID.SpellIDNext++;

            Manager.Library.Spells[gameID] = new SpellEntry(this, gameID, config.ModID)
            {
                Config = config.DeepCopy()
            };

            return gameID;
        }

        /// <summary>
        /// Creates a new world region, and returns its ID.
        /// </summary>
        public Level.WorldRegion CreateWorldRegion(WorldRegionConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            if (!InLoad)
                throw new InvalidOperationException(ErrorCodex.UseOnlyDuringLoad);

            bool duplicateID = GetLibrary().WorldRegions.Any(x => x.Value.ModID == config.ModID);

            if (duplicateID)
                throw new InvalidOperationException(ErrorCodex.DuplicateModID);

            var gameID = Manager.ID.WorldIDNext++;

            WorldRegionEntry entry = new WorldRegionEntry(this, gameID, config.ModID)
            {
                Config = config.DeepCopy()
            };

            entry.Initialize();

            return gameID;
        }

        /// <summary>
        /// Creates a new level from the given LevelConfig.
        /// </summary>
        public Level.ZoneEnum CreateLevel(LevelConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            if (!InLoad)
                throw new InvalidOperationException(ErrorCodex.UseOnlyDuringLoad);

            bool duplicateID = GetLibrary().Levels.Any(x => x.Value.ModID == config.ModID);

            if (duplicateID)
                throw new InvalidOperationException(ErrorCodex.DuplicateModID);

            Level.ZoneEnum gameID = Manager.ID.LevelIDNext++;

            LevelEntry entry = new LevelEntry(this, gameID, config.ModID)
            {
                Config = config.DeepCopy()
            };

            Manager.Library.Levels[gameID] = entry;

            entry.Initialize();

            return gameID;
        }

        /// <summary> 
        /// Creates an item from the given ItemConfig.
        /// </summary>
        public ItemCodex.ItemTypes CreateItem(ItemConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            if (!InLoad)
                throw new InvalidOperationException(ErrorCodex.UseOnlyDuringLoad);

            bool duplicateID = GetLibrary().Items.Any(x => x.Value.ModID == config.ModID);

            if (duplicateID)
                throw new InvalidOperationException(ErrorCodex.DuplicateModID);

            ItemCodex.ItemTypes gameID = Manager.ID.ItemIDNext++;

            ItemEntry entry = new ItemEntry(this, gameID, config.ModID)
            {
                Config = config.DeepCopy()
            };

            Manager.Library.Items[gameID] = entry;

            ItemDescription itemData = entry.ItemData = new ItemDescription()
            {
                enType = gameID,
                sFullName = config.Name,
                sDescription = config.Description,
                sNameLibraryHandle = $"Item_{(int)gameID}_Name",
                sDescriptionLibraryHandle = $"Item_{(int)gameID}_Description",
                sCategory = "",
                iInternalLevel = config.SortingValue,
                byFancyness = Math.Min((byte)1, Math.Max(config.Fancyness, (byte)3)),
                iValue = config.Value,
                iOverrideBloodValue = config.BloodValue,
                fArcadeModeCostModifier = config.ArcadeValueModifier,
                lenCategory = new HashSet<ItemCodex.ItemCategories>(config.Categories)
            };

            EquipmentType typeToUse = Enum.IsDefined(typeof(EquipmentType), config.EquipType) ? config.EquipType : EquipmentType.None;

            EquipmentInfo equipData = null;
            switch (typeToUse)
            {
                case EquipmentType.None:
                    break;
                case EquipmentType.Facegear:
                    FacegearInfo faceData = (equipData = new FacegearInfo(gameID)) as FacegearInfo;

                    Array.Copy(config.FacegearOverHair, faceData.abOverHair, 4);
                    Array.Copy(config.FacegearOverHat, faceData.abOverHat, 4);
                    Array.Copy(config.FacegearOffsets, faceData.av2RenderOffsets, 4);

                    break;
                case EquipmentType.Hat:
                    HatInfo hatData = (equipData = new HatInfo(gameID) { bDoubleSlot = config.HatDoubleSlot }) as HatInfo;

                    InitializeSet(hatData.xDefaultSet, config.DefaultSet);
                    foreach (var kvp in config.AltSets)
                        InitializeSet(hatData.denxAlternateVisualSets[kvp.Key] = new HatInfo.VisualSet(), kvp.Value);

                    break;
                case EquipmentType.Weapon:
                    WeaponInfo weaponData = new WeaponInfo(config.EquipResourcePath, gameID, config.WeaponType)
                    {
                        enWeaponCategory = config.WeaponType,
                        enAutoAttackSpell = WeaponInfo.AutoAttackSpell.None
                    };
                    equipData = weaponData;

                    if (config.WeaponType == WeaponInfo.WeaponCategory.OneHanded)
                    {
                        weaponData.iDamageMultiplier = 90;
                        if (config.MagicWeapon)
                            weaponData.enAutoAttackSpell = WeaponInfo.AutoAttackSpell.Generic1H;
                    }
                    else if (config.WeaponType == WeaponInfo.WeaponCategory.TwoHanded)
                    {
                        weaponData.iDamageMultiplier = 125;
                        if (config.MagicWeapon)
                            weaponData.enAutoAttackSpell = WeaponInfo.AutoAttackSpell.Generic2H;
                    }
                    break;
                default:
                    equipData = new EquipmentInfo(config.EquipResourcePath, gameID);
                    break;
            }

            if (config.EquipType != EquipmentType.None)
            {
                equipData.deniStatChanges = new Dictionary<EquipmentInfo.StatEnum, int>(config.AllStats);
                equipData.lenSpecialEffects.AddRange(config.Effects);

                if (config.EquipType == EquipmentType.Hat)
                {
                    var altResources = entry.HatAltSetResourcePaths = new Dictionary<ItemCodex.ItemTypes, string>();
                    foreach (var set in config.AltSets)
                        altResources.Add(set.Key, set.Value.Resource);
                }
            }

            entry.EquipData = equipData;

            HashSet<ItemCodex.ItemCategories> toSanitize = new HashSet<ItemCodex.ItemCategories>
            {
                ItemCodex.ItemCategories.OneHandedWeapon,
                ItemCodex.ItemCategories.TwoHandedWeapon,
                ItemCodex.ItemCategories.Weapon,
                ItemCodex.ItemCategories.Shield,
                ItemCodex.ItemCategories.Armor,
                ItemCodex.ItemCategories.Hat,
                ItemCodex.ItemCategories.Accessory,
                ItemCodex.ItemCategories.Shoes,
                ItemCodex.ItemCategories.Facegear
            };

            itemData.lenCategory.ExceptWith(toSanitize);

            if (equipData != null)
            {
                ItemCodex.ItemCategories type = (ItemCodex.ItemCategories)config.EquipType;
                itemData.lenCategory.Add(type);

                if (type == ItemCodex.ItemCategories.Weapon)
                {
                    switch ((equipData as WeaponInfo).enWeaponCategory)
                    {
                        case WeaponInfo.WeaponCategory.OneHanded:
                            itemData.lenCategory.Add(ItemCodex.ItemCategories.OneHandedWeapon); break;
                        case WeaponInfo.WeaponCategory.TwoHanded:
                            itemData.lenCategory.Add(ItemCodex.ItemCategories.TwoHandedWeapon); break;
                    }
                }
            }

            entry.Initialize();

            return gameID;
        }

        /// <summary>
        /// Returns a SpecialEffect ID that you can use for your own equipment.
        /// </summary>
        public EquipmentInfo.SpecialEffect CreateSpecialEffect()
        {
            return Manager.ID.ItemEffectIDNext++;
        }

        public EnemyCodex.EnemyTypes CreateEnemy(EnemyConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            if (config.Constructor == null)
                throw new ArgumentException("config's Constructor must not be null!");

            if (!InLoad)
                throw new InvalidOperationException(ErrorCodex.UseOnlyDuringLoad);

            bool duplicateID = GetLibrary().Enemies.Any(x => x.Value.ModID == config.ModID);

            if (duplicateID)
                throw new InvalidOperationException(ErrorCodex.DuplicateModID);

            EnemyCodex.EnemyTypes gameID = Manager.ID.EnemyIDNext++;

            EnemyDescription enemyData = new EnemyDescription(gameID, config.Category, $"{gameID}_Name", config.Level, config.BaseHealth)
            {
                sOnHitSound = config.OnHitSound,
                sOnDeathSound = config.OnDeathSound,
                sFullName = config.Name,
                sCardDescriptionLibraryHandle = $"{gameID}_Card",
                sDetailedDescriptionLibraryHandle = $"{gameID}_Description",
                sFlavorLibraryHandle = $"{gameID}_Flavor",
                sCardDescription = config.CardInfo,
                sDetailedDescription = config.LongDescription,
                sFlavorText = config.ShortDescription,
                iCardDropChance = (int)(100f / config.CardDropChance),
            };

            EnemyEntry entry = new EnemyEntry(this, gameID, config.ModID)
            {
                Config = config.DeepCopy(),
                EnemyData = enemyData
            };

            Manager.Library.Enemies[gameID] = entry;

            config.LootTable.ForEach(x => enemyData.lxLootTable.Add(new DropChance((int)(1000 * x.Chance), x.Item)));

            entry.Initialize();

            return gameID;
        }

        /// <summary>
        /// Adds a new command that executes the given parser when called.
        /// The command can be executed by typing in chat "/(ModName):(command) (argList)". <para/>
        /// The command must not have whitespace in it.
        /// </summary>
        public void CreateCommand(string command, CommandParser parser)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            if (command.Any(char.IsWhiteSpace))
                throw new ArgumentException(ErrorCodex.NoWhiteSpaceInCommand);

            if (!InLoad)
                throw new InvalidOperationException(ErrorCodex.UseOnlyDuringLoad);

            ModCommands[command] = parser ?? throw new ArgumentNullException(nameof(parser));
        }

        public BaseStats.StatusEffectSource CreateStatusEffect(StatusEffectConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            if (!InLoad)
                throw new InvalidOperationException(ErrorCodex.UseOnlyDuringLoad);

            bool duplicateID = GetLibrary().StatusEffects.Any(x => x.Value.ModID == config.ModID);

            if (duplicateID)
                throw new InvalidOperationException(ErrorCodex.DuplicateModID);

            BaseStats.StatusEffectSource gameID = Manager.ID.StatusEffectIDNext++;

            StatusEffectEntry entry = new StatusEffectEntry(this, gameID, config.ModID)
            {
                Config = config.DeepCopy()
            };

            Manager.Library.StatusEffects[gameID] = entry;

            entry.Initialize();

            return gameID;
        }

        /// <summary>
        /// Configures custom audio for the current mod, using the config provided. <para/>
        /// Config must not be null.
        /// </summary>
        public void CreateAudio(AudioConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            if (!InLoad)
                throw new InvalidOperationException(ErrorCodex.UseOnlyDuringLoad);

            if (Audio.Initialized)
                throw new InvalidOperationException(ErrorCodex.AudioAlreadyInitialized);

            Audio.Initialized = true;

            AudioEngine audioEngine = AccessTools.Field(typeof(SoundSystem), "audioEngine").GetValue(Globals.Game.xSoundSystem) as AudioEngine;

            Audio.IndexedEffectCues.AddRange(config.EffectCueNames);

            foreach (var kvp in config.MusicCueNames)
            {
                string bankName = kvp.Key;

                foreach (var music in kvp.Value)
                {
                    Audio.IndexedMusicBanks.Add(bankName);
                    Audio.IndexedMusicCues.Add(music);
                }
            }

            string root = Path.Combine(Content.RootDirectory, AssetPath);

            ModUtils.TryLoadWaveBank(Path.Combine(root, "Sound", NameID + "Effects.xwb"), audioEngine, out Audio.EffectsWB);
            ModUtils.TryLoadSoundBank(Path.Combine(root, "Sound", NameID + "Effects.xsb"), audioEngine, out Audio.EffectsSB);
            ModUtils.TryLoadSoundBank(Path.Combine(root, "Sound", NameID + "Music.xsb"), audioEngine, out Audio.MusicSB);
            ModUtils.TryLoadWaveBank(Path.Combine(root, "Sound", NameID + ".xwb"), audioEngine, out Audio.UniversalWB);
        }

        public RogueLikeMode.TreatsCurses CreateTreatOrCurse(TreatCurseConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            if (!InLoad)
                throw new InvalidOperationException(ErrorCodex.UseOnlyDuringLoad);

            bool duplicateID = GetLibrary().Curses.Any(x => x.Value.ModID == config.ModID);

            if (duplicateID)
                throw new InvalidOperationException(ErrorCodex.DuplicateModID);

            RogueLikeMode.TreatsCurses gameID = Manager.ID.CurseIDNext++;

            CurseEntry entry = new CurseEntry(this, gameID, config.ModID)
            {
                Config = config.DeepCopy(),
                NameHandle = $"TreatCurse_{(int)gameID}_Name",
                DescriptionHandle = $"TreatCurse_{(int)gameID}_Description"
            };

            Manager.Library.Curses[gameID] = entry;

            entry.Initialize();

            return gameID;
        }

        public RogueLikeMode.Perks CreatePerk(PerkConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            if (!InLoad)
                throw new InvalidOperationException(ErrorCodex.UseOnlyDuringLoad);

            bool duplicateID = GetLibrary().Perks.Any(x => x.Value.ModID == config.ModID);

            if (duplicateID)
                throw new InvalidOperationException(ErrorCodex.DuplicateModID);

            RogueLikeMode.Perks gameID = Manager.ID.PerkIDNext++;

            PerkEntry entry = new PerkEntry(this, gameID, config.ModID)
            {
                Config = config.DeepCopy(),
                TextEntry = $"{(int)gameID}"
            };

            Manager.Library.Perks[gameID] = entry;

            entry.Initialize();

            return gameID;
        }

        public PinCodex.PinType CreatePin(PinConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            if (!InLoad)
                throw new InvalidOperationException(ErrorCodex.UseOnlyDuringLoad);

            bool duplicateID = GetLibrary().Pins.Any(x => x.Value.ModID == config.ModID);

            if (duplicateID)
                throw new InvalidOperationException(ErrorCodex.DuplicateModID);

            PinCodex.PinType gameID = Manager.ID.PinIDNext++;

            PinEntry entry = new PinEntry(this, gameID, config.ModID)
            {
                Config = config.DeepCopy()
            };

            Manager.Library.Pins[gameID] = entry;

            entry.Initialize();

            return gameID;
        }

        #endregion

        #region Other Methods

        /// <summary>
        /// Adds parsers for a modded packet.
        /// The parsers are called when a packet is received, allowing you to trigger certain actions.
        /// The parsers can be null or empty if you don't want the server or client to process it.
        /// </summary>
        public void AddPacket(ushort packetID, ModPacket packet)
        {
            ModPackets[packetID] = packet;
        }

        /// <summary>
        /// Adds a new crafting recipe.
        /// </summary>
        public void AddRecipe(ItemCodex.ItemTypes result, Dictionary<ItemCodex.ItemTypes, ushort> ingredients)
        {
            if (ingredients == null)
                throw new ArgumentNullException(nameof(ingredients));

            if (!Crafting.CraftSystem.RecipeCollection.ContainsKey(result))
            {
                var kvps = new KeyValuePair<ItemDescription, ushort>[ingredients.Count];

                int index = 0;
                foreach (var kvp in ingredients)
                    kvps[index++] = new KeyValuePair<ItemDescription, ushort>(ItemCodex.GetItemDescription(kvp.Key), kvp.Value);

                ItemDescription description = ItemCodex.GetItemDescription(result);
                Crafting.CraftSystem.RecipeCollection.Add(result, new Crafting.CraftSystem.CraftingRecipe(description, kvps));
            }

            Globals.Logger.Info($"Added recipe for item {result}!");
        }

        /// <summary>
        /// Gets an ItemType previously defined by a mod.
        /// If nothing is found, ItemCodex.ItemTypes.Null is returned.
        /// </summary>
        public ItemCodex.ItemTypes GetItemType(Mod owner, string uniqueID)
        {
            var entry = Manager.Library.Items.Values.FirstOrDefault(x => x.Owner == owner && x.ModID == uniqueID);

            return entry?.GameID ?? ItemCodex.ItemTypes.Null;
        }

        /// <summary>
        /// Instructs the SoundSystem to play the target modded music instead of the vanilla music. <para/>
        /// If redirect is the empty string, any existing redirects are cleared.
        /// </summary>
        public void RedirectVanillaMusic(string vanilla, string redirect)
        {
            var songRegionMap = AccessTools.Field(typeof(SoundSystem), "dssSongRegionMap").GetValue(Globals.Game.xSoundSystem) as Dictionary<string, string>;

            if (!songRegionMap.ContainsKey(vanilla))
            {
                Globals.Logger.Warn($"Redirecting {vanilla} to {redirect} is not possible since {vanilla} is not a vanilla music!");
                return;
            }

            bool isModded = ModUtils.SplitAudioID(redirect, out int entryID, out bool isMusic, out int cueID);
            var entry = Manager.Mods[entryID].Audio;

            string cueName = entry != null && cueID >= 0 && cueID < entry.IndexedMusicCues.Count ? entry.IndexedMusicCues[cueID] : null;

            if ((!isModded || !isMusic || cueName == null) && !(redirect == ""))
            {
                Globals.Logger.Warn($"Redirecting {vanilla} to {redirect} is not possible since {redirect} is not a modded music!");
                return;
            }

            var redirectedSongs = Manager.Library.VanillaMusicRedirects;
            bool replacing = redirectedSongs.ContainsKey(vanilla);

            if (redirect == "")
            {
                Globals.Logger.Info($"Song {vanilla} has been cleared of any redirects.");
                redirectedSongs.Remove(vanilla);
            }
            else
            {
                Globals.Logger.Info($"Song {vanilla} is now redirected to {redirect} ({cueName}). {(replacing ? $"Previous redirect was {redirectedSongs[vanilla]}" : "")}");
                redirectedSongs[vanilla] = redirect;
            }
        }

        /// <summary>
        /// Gets the ID of the effect that has the given cue name. <para/>
        /// This ID can be used to play effects with methods such as SoundSystem.PlayCue.
        /// </summary>

        public string GetEffectID(string cueName)
        {
            var effects = Audio.IndexedEffectCues;

            for (int i = 0; i < effects.Count; i++)
            {
                if (effects[i] == cueName)
                {
                    return $"GS_{ModIndex}_S{i}";
                }
            }

            return "";
        }

        /// <summary>
        /// Gets the ID of the music that has the given cue name. <para/>
        /// This ID can be used to play effects with <see cref="SoundSystem.PlaySong"/>.
        /// </summary>

        public string GetMusicID(string cueName)
        {
            var music = Audio.IndexedMusicCues;

            for (int i = 0; i < music.Count; i++)
            {
                if (music[i] == cueName)
                    return $"GS_{ModIndex}_M{i}";
            }

            return "";
        }

        /// <summary>
        /// Sends a packet to the chosen Client if currently playing as a Server; otherwise, it does nothing.
        /// </summary>
        public void SendToClient(ushort packetID, Action<BinaryWriter> data, PlayerView receiver, SequenceChannel channel = SequenceChannel.PrioritySpells_20, NetDeliveryMethod reliability = NetDeliveryMethod.ReliableOrdered)
        {
            if (!NetUtils.IsServer)
                return;

            Globals.Game._Network_SendMessage(NetUtils.WriteModData(this, packetID, data), receiver, channel, reliability);
        }

        /// <summary>
        /// Sends a packet to all Clients, if currently playing as a Server; otherwise, it does nothing.
        /// </summary>
        public void SendToAllClients(ushort packetID, Action<BinaryWriter> data, SequenceChannel channel = SequenceChannel.PrioritySpells_20, NetDeliveryMethod reliability = NetDeliveryMethod.ReliableOrdered)
        {
            if (!NetUtils.IsServer)
                return;

            Globals.Game._Network_SendMessage(NetUtils.WriteModData(this, packetID, data), channel, reliability);
        }

        /// <summary>
        /// Sends a packet to all Clients, except one, if currently playing as a Server; otherwise, it does nothing.
        /// </summary>
        public void SendToAllClientsExcept(ushort packetID, Action<BinaryWriter> data, PlayerView excluded, SequenceChannel channel = SequenceChannel.PrioritySpells_20, NetDeliveryMethod reliability = NetDeliveryMethod.ReliableOrdered)
        {
            if (!NetUtils.IsServer)
                return;

            foreach (PlayerView view in Globals.Game.dixPlayers.Values)
            {
                if (view == excluded)
                    continue;

                Globals.Game._Network_SendMessage(NetUtils.WriteModData(this, packetID, data), channel, reliability);
            }
        }

        /// <summary>
        /// Sends a packet to the Server if currently playing as a Client; otherwise, it does nothing.
        /// </summary>
        public void SendToServer(ushort packetID, Action<BinaryWriter> data, SequenceChannel channel = SequenceChannel.PrioritySpells_20, NetDeliveryMethod reliability = NetDeliveryMethod.ReliableOrdered)
        {
            if (!NetUtils.IsClient)
                return;

            Globals.Game._Network_SendMessage(NetUtils.WriteModData(this, packetID, data), channel, reliability);
        }

        #endregion

        private void InitializeSet(HatInfo.VisualSet set, ItemConfig.VSetInfo desc)
        {
            Array.Copy(desc.HatUnderHair, set.abUnderHair, 4);
            Array.Copy(desc.HatBehindPlayer, set.abBehindCharacter, 4);
            Array.Copy(desc.HatOffsets, set.av2RenderOffsets, 4);

            set.bObstructsSides = desc.ObstructHairSides;
            set.bObstructsTop = desc.ObstructHairTop;
            set.bObstructsBottom = desc.ObstructHairBottom;
        }
    }
}