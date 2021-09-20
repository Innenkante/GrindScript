using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using SoG.Modding.API;
using SoG.Modding.API.Configs;
using System;
using System.Collections.Generic;
using Quests;

namespace SoG.Modding.Core
{
    /// <summary>
    /// ISaveableEntry represent objects that are saved by SoG in one way or another.
    /// As a result, additional information is required for ensuring consistency.
    /// </summary>
    interface IEntry<IDType> where IDType : struct
    {
        Mod Owner { get; }

        IDType GameID { get; }

        string ModID { get; }
    }

    /// <summary>
    /// Represents a modded item in the ModLibrary.
    /// The item can act as equipment if EquipData is not null.
    /// </summary>
    internal class ModItemEntry : IEntry<ItemCodex.ItemTypes>
    {
        public Mod Owner { get; set; }

        public ItemCodex.ItemTypes GameID { get; set; }

        public string ModID { get; set; }

        public ItemConfig Config { get; set; }

        public ItemDescription ItemData { get; set; }

        public EquipmentInfo EquipData { get; set; } // May be null or a subtype

        public Dictionary<ItemCodex.ItemTypes, string> HatAltSetResourcePaths { get; set; }

        public ModItemEntry(Mod owner, ItemCodex.ItemTypes gameID, string modID)
        {
            Owner = owner;
            GameID = gameID;
            ModID = modID;
        }
    }

    /// <summary>
    /// Represents a modded level in the ModLibrary.
    /// </summary>
    internal class ModLevelEntry : IEntry<Level.ZoneEnum>
    {
        public Mod Owner { get; set; }

        public Level.ZoneEnum GameID { get; set; }

        public string ModID { get; set; }

        public LevelConfig Config { get; set; }

        public ModLevelEntry(Mod owner, Level.ZoneEnum gameID, string modID)
        {
            Owner = owner;
            GameID = gameID;
            ModID = modID;
        }
    }

    /// <summary>
    /// Represents a modded treat or curse in the ModLibrary.
    /// Treats are functionally identical to Curses, but appear in a different menu.
    /// </summary>
    internal class ModCurseEntry : IEntry<RogueLikeMode.TreatsCurses>
    {
        public Mod Owner { get; set; }

        public RogueLikeMode.TreatsCurses GameID { get; set; }

        public string ModID { get; set; }

        public TreatCurseConfig Config { get; set; }

        public string NameHandle { get; set; } = "";

        public string DescriptionHandle { get; set; } = "";

        public ModCurseEntry(Mod owner, RogueLikeMode.TreatsCurses gameID, string modID)
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
        public Mod Owner { get; set; }

        public RogueLikeMode.Perks GameID { get; set; }

        public string ModID { get; set; }

        public PerkConfig Config { get; set; }

        public string TextEntry { get; set; }

        public ModPerkEntry(Mod owner, RogueLikeMode.Perks gameID, string modID)
        {
            Owner = owner;
            GameID = gameID;
            ModID = modID;
        }
    }

    internal class ModEnemyEntry : IEntry<EnemyCodex.EnemyTypes>
    {
        public Mod Owner { get; set; }

        public EnemyCodex.EnemyTypes GameID { get; set; }

        public string ModID { get; set; }

        public EnemyConfig Config { get; set; }

        public EnemyDescription EnemyData { get; set; }

        public ModEnemyEntry(Mod owner, EnemyCodex.EnemyTypes gameID, string modID)
        {
            Owner = owner;
            GameID = gameID;
            ModID = modID;
        }
    }

    internal class ModQuestEntry : IEntry<QuestCodex.QuestID>
    {
        public Mod Owner { get; set; }

        public Quests.QuestCodex.QuestID GameID { get; set; }

        public string ModID { get; set; }

        public QuestConfig Config { get; set; }

        public Quests.QuestDescription QuestData { get; set; }

        public ModQuestEntry(Mod owner, Quests.QuestCodex.QuestID gameID, string modID)
        {
            Owner = owner;
            GameID = gameID;
            ModID = modID;
        }
    }

    internal class ModSpellEntry : IEntry<SpellCodex.SpellTypes>
    {
        public Mod Owner { get; set; }

        public SpellCodex.SpellTypes GameID { get; set; }

        public string ModID { get; set; }

        public SpellConfig Config { get; set; }

        public ModSpellEntry(Mod owner, SpellCodex.SpellTypes gameID, string modID)
        {
            Owner = owner;
            GameID = gameID;
            ModID = modID;
        }
    }

    internal class ModStatusEffectEntry : IEntry<BaseStats.StatusEffectSource>
    {
        public Mod Owner { get; set; }

        public BaseStats.StatusEffectSource GameID { get; set; }

        public string ModID { get; set; }

        public StatusEffectConfig Config { get; set; }

        public ModStatusEffectEntry(Mod owner, BaseStats.StatusEffectSource gameID, string modID)
        {
            Owner = owner;
            GameID = gameID;
            ModID = modID;
        }
    }

    internal class ModPinEntry : IEntry<PinCodex.PinType>
    {
        public Mod Owner { get; set; }

        public PinCodex.PinType GameID { get; set; }

        public string ModID { get; set; }

        public PinConfig Config { get; set; }

        public ModPinEntry(Mod owner, PinCodex.PinType gameID, string modID)
        {
            Owner = owner;
            GameID = gameID;
            ModID = modID;
        }
    }
}
