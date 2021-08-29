using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using SoG.Modding.API;
using SoG.Modding.API.Configs;
using System;
using System.Collections.Generic;

namespace SoG.Modding.Core
{
    /// <summary>
    /// ISaveableEntry represent objects that are saved by SoG in one way or another.
    /// As a result, additional information is required for ensuring consistency.
    /// </summary>
    interface IEntry<IDType> where IDType : struct
    {
        BaseScript Owner { get; }

        IDType GameID { get; }

        string ModID { get; }
    }

    /// <summary>
    /// Represents a modded item in the ModLibrary.
    /// The item can act as equipment if EquipData is not null.
    /// </summary>
    internal class ModItemEntry : IEntry<ItemCodex.ItemTypes>
    {
        public BaseScript Owner { get; set; }

        public ItemCodex.ItemTypes GameID { get; set; }

        public string ModID { get; set; }

        public ItemConfig Config;

        public ItemDescription ItemData;

        public EquipmentInfo EquipData; // May be null or a subtype

        public Dictionary<ItemCodex.ItemTypes, string> HatAltSetResourcePaths;

        public ModItemEntry(BaseScript owner, ItemCodex.ItemTypes gameID, string modID)
        {
            Owner = owner;
            GameID = gameID;
            ModID = modID;
        }
    }

    /// <summary>
    /// Stores modded audio for a mod - an entry is created for each mod upon its creation,
    /// and initialized by the mod if needed.
    /// </summary>
    internal class ModAudioEntry
    {
        public BaseScript Owner { get; set; }

        public int GameID { get; set; }

        public bool IsReady = false;

        public SoundBank EffectsSB; // "<Mod>Effects.xsb"

        public WaveBank EffectsWB; // "<Mod>Music.xwb"

        public SoundBank MusicSB; //"<Mod>Music.xsb"

        public WaveBank UniversalWB; // "<Mod>.xwb", never unloaded

        public Dictionary<int, string> EffectNames = new Dictionary<int, string>();

        public Dictionary<int, string> MusicNames = new Dictionary<int, string>();

        public Dictionary<string, string> MusicBankNames = new Dictionary<string, string>();

        public ModAudioEntry(BaseScript owner, int audioID)
        {
            Owner = owner;
            GameID = audioID;
        }
    }
    
    /// <summary>
    /// Represents a modded level in the ModLibrary.
    /// </summary>
    internal class ModLevelEntry
    {
        public BaseScript Owner { get; set; }

        public Level.ZoneEnum GameID { get; set; }

        public LevelConfig Config;

        public ModLevelEntry(BaseScript owner, Level.ZoneEnum gameID)
        {
            Owner = owner;
            GameID = gameID;
        }
    }

    /// <summary>
    /// Represents a modded treat or curse in the ModLibrary.
    /// Treats are functionally identical to Curses, but appear in a different menu.
    /// </summary>
    internal class ModCurseEntry : IEntry<RogueLikeMode.TreatsCurses>
    {
        public BaseScript Owner { get; set; }

        public RogueLikeMode.TreatsCurses GameID { get; set; }

        public string ModID { get; set; }

        public TreatCurseConfig Config;

        public string NameHandle = "";

        public string DescriptionHandle = "";

        public ModCurseEntry(BaseScript owner, RogueLikeMode.TreatsCurses gameID, string modID)
        {
            Owner = owner;
            GameID = gameID;
            ModID = modID;
        }
    }

    /// <summary>
    /// Represents a modded perk in the ModLibrary.
    /// </summary>
    internal class ModPerkEntry : IEntry<RogueLikeMode.Perks>
    {
        public BaseScript Owner { get; set; }

        public RogueLikeMode.Perks GameID { get; set; }

        public string ModID { get; set; }

        public PerkConfig Config;

        public string TextEntry;

        public ModPerkEntry(BaseScript owner, RogueLikeMode.Perks gameID, string modID)
        {
            Owner = owner;
            GameID = gameID;
            ModID = modID;
        }
    }

    internal class ModEnemyEntry : IEntry<EnemyCodex.EnemyTypes>
    {
        public BaseScript Owner { get; set; }

        public EnemyCodex.EnemyTypes GameID { get; set; }

        public string ModID { get; set; }

        public EnemyConfig Config;

        public EnemyDescription EnemyData;

        public ModEnemyEntry(BaseScript owner, EnemyCodex.EnemyTypes gameID, string modID)
        {
            Owner = owner;
            GameID = gameID;
            ModID = modID;
        }
    }

    internal class ModQuestEntry : IEntry<Quests.QuestCodex.QuestID>
    {
        public BaseScript Owner { get; set; }

        public Quests.QuestCodex.QuestID GameID { get; set; }

        public string ModID { get; set; }

        public QuestConfig Config;

        public Quests.QuestDescription QuestData;

        public ModQuestEntry(BaseScript owner, Quests.QuestCodex.QuestID gameID, string modID)
        {
            Owner = owner;
            GameID = gameID;
            ModID = modID;
        }
    }
}
