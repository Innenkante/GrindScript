using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;
using System.Linq;
using System.Collections.Generic;

namespace SoG.Modding.Content
{
    /// <summary>
    /// Represents a modded enemy, and defines ways to create it.
    /// </summary>
    /// <remarks> 
    /// Most of the methods in this class can only be used while a mod is loading, that is, inside <see cref="Mod.Load"/>.
    /// </remarks>
    public class EnemyEntry : Entry<EnemyCodex.EnemyTypes>
    {
        /// <summary>
        /// Holds the item and drop chance pair inside enemy loot tables.
        /// </summary>
        public struct Drop
        {
            public float Chance;

            public ItemCodex.ItemTypes Item;

            public Drop(float chance, ItemCodex.ItemTypes item)
            {
                Chance = chance;
                Item = item;
            }
        }

        #region IEntry Properties

        public override Mod Mod { get; internal set; }

        public override EnemyCodex.EnemyTypes GameID { get; internal set; }

        public override string ModID { get; internal set; }

        #endregion

        #region Internal Data

        internal EnemyDescription vanilla = new EnemyDescription(EnemyCodex.EnemyTypes.Null, "", 1, 100);

        internal bool createJournalEntry = true;

        internal Func<ContentManager, Animation> defaultAnimation = null;

        internal string displayBackgroundPath = null;

        internal string displayIconPath = null;

        internal string cardIllustrationPath = "GUI/InGameMenu/Journal/CardAlbum/Cards/placeholder";

        internal List<Drop> lootTable = new List<Drop>();

        internal EnemyBuilder constructor = null;

        internal EnemyBuilder difficultyScaler = null;

        internal EnemyBuilder eliteScaler = null;

        #endregion

        #region Public Interface

        /// <summary>
        /// Gets or sets the enemy's display name.
        /// </summary>
        public string Name
        {
            get => vanilla.sFullName;
            set
            {
                ErrorHelper.ThrowIfNotLoading(Mod);
                vanilla.sFullName = value;
            }
        }

        /// <summary>
        /// Gets or sets the enemy's short description.
        /// This is displayed when selecting an enemy description in the enemy menu.
        /// </summary>
        public string ShortDescription
        {
            get => vanilla.sFlavorText;
            set
            {
                ErrorHelper.ThrowIfNotLoading(Mod);
                vanilla.sFlavorText = value;
            }
        }

        /// <summary>
        /// Gets or sets the enemy's long description.
        /// This is displayed when reading an enemy's details from the enemy menu.
        /// </summary>
        public string LongDescription
        {
            get => vanilla.sDetailedDescription;
            set
            {
                ErrorHelper.ThrowIfNotLoading(Mod);
                vanilla.sDetailedDescription = value;
            }
        }

        /// <summary>
        /// Gets or sets the enemy's base health. <para/>
        /// The base health represents the health of the monster on difficulty 0
        /// (Story's Normal, Arcade's 0 Catalyst), as a non-elite enemy.
        /// </summary>
        public int BaseHealth
        {
            get => vanilla.iMaxHealth;
            set
            {
                ErrorHelper.ThrowIfNotLoading(Mod);
                vanilla.iMaxHealth = value;
            }
        }

        /// <summary>
        /// Gets or sets the enemy's level.<para/>
        /// Players gain experience based on the player - enemy level difference.
        /// This is also displayed in the enemy codex when viewing an enemy's details.
        /// </summary>
        public int Level
        {
            get => vanilla.iLevel;
            set
            {
                ErrorHelper.ThrowIfNotLoading(Mod);
                vanilla.iLevel = value;
            }
        }

        /// <summary>
        /// Gets or sets the effect name of the audio (if any) that this enemy emits on hit.
        /// </summary>
        public string OnHitSound
        {
            get => vanilla.sOnHitSound;
            set
            {
                ErrorHelper.ThrowIfNotLoading(Mod);
                vanilla.sOnHitSound = value;
            }
        }

        /// <summary>
        /// Gets or sets the effect name of the audio (if any) that this enemy emits on death.
        /// </summary>
        public string OnDeathSound
        {
            get => vanilla.sOnDeathSound;
            set
            {
                ErrorHelper.ThrowIfNotLoading(Mod);
                vanilla.sOnDeathSound = value;
            }
        }

        /// <summary>
        /// Gets or sets whenever to make a journal enemy or not.
        /// If this is set to true, an entry will appear for this monster inside the enemy menu.
        /// </summary>
        public bool CreateJournalEntry
        {
            get => createJournalEntry;
            set
            {
                ErrorHelper.ThrowIfNotLoading(Mod);
                createJournalEntry = value;
            }
        }

        /// <summary>
        /// Gets or sets the default animation callback. <para/>
        /// The callback should return an animation to use when viewing the enemy's details.
        /// </summary>
        public Func<ContentManager, Animation> DefaultAnimation
        {
            get => defaultAnimation;
            set
            {
                ErrorHelper.ThrowIfNotLoading(Mod);
                defaultAnimation = value;
            }
        }

        /// <summary>
        /// Gets or sets the display background path. The texture path is relative to "Content/". <para/>
        /// The texture is used as background for the enemy animation when reading the enemy's details.
        /// </summary>
        public string DisplayBackgroundPath
        {
            get => displayBackgroundPath;
            set
            {
                ErrorHelper.ThrowIfNotLoading(Mod);
                displayBackgroundPath = value;
            }
        }

        /// <summary>
        /// Gets or sets the display icon path. The texture path is relative to "Content/". <para/>
        /// The texture is used as the icon for the enemy entry.
        /// </summary>
        public string DisplayIconPath
        {
            get => displayIconPath;
            set
            {
                ErrorHelper.ThrowIfNotLoading(Mod);
                displayIconPath = value;
            }
        }

        /// <summary>
        /// Gets or sets the approximate center of the enemy. <para/>
        /// Some game elements (such as Guardian shields) use this field to position themselves relative to
        /// the enemy center.
        /// </summary>
        public Vector2 ApproximateCenter
        {
            get => vanilla.v2ApproximateOffsetToMid;
            set
            {
                ErrorHelper.ThrowIfNotLoading(Mod);
                vanilla.v2ApproximateOffsetToMid = value;
            }
        }

        /// <summary>
        /// Gets or sets the approximate size of the enemy. <para/>
        /// Some game elements (such as Guardian shields) use this field to position themselves based on the
        /// enemy's size.
        /// </summary>
        public Vector2 ApproximateSize
        {
            get => vanilla.v2ApproximateSize;
            set
            {
                ErrorHelper.ThrowIfNotLoading(Mod);
                vanilla.v2ApproximateSize = value;
            }
        }

        /// <summary>
        /// Gets or sets the card to drop from this enemy. <para/>
        /// If this is set to <see cref="EnemyCodex.EnemyTypes.Null"/>, the enemy will drop its own card.
        /// Other IDs will cause the enemy to drop that card instead, and will remove the enemy's entry from the card menu.
        /// </summary>
        public EnemyCodex.EnemyTypes CardDropOverride
        {
            get => vanilla.enCardTypeOverride;
            set
            {
                ErrorHelper.ThrowIfNotLoading(Mod);
                vanilla.enCardTypeOverride = value;
            }
        }

        /// <summary>
        /// Gets or sets the card drop chance.
        /// A value of 100f corresponds to 100%.
        /// A value o 0f disables card drops. It will also remove the enemy's entry from the card menu.
        /// </summary>
        public float CardDropChance
        {
            get => vanilla.iCardDropChance != 0 ? (int)(100f / vanilla.iCardDropChance) : 0f;
            set
            {
                ErrorHelper.ThrowIfNotLoading(Mod);
                vanilla.iCardDropChance = value != 0f ? (int)(100f / value) : 0;
            }
        }

        /// <summary>
        /// Gets or sets the description of the card's effects.
        /// </summary>
        public string CardInfo
        {
            get => vanilla.sCardDescription;
            set
            {
                ErrorHelper.ThrowIfNotLoading(Mod);
                vanilla.sCardDescription = value;
            }
        }

        /// <summary>
        /// Gets or sets the card's illustration path. The texture path is relative to "Content/".
        /// </summary>
        public string CardIllustrationPath
        {
            get => cardIllustrationPath;
            set
            {
                ErrorHelper.ThrowIfNotLoading(Mod);
                cardIllustrationPath = value;
            }
        }

        /// <summary>
        /// Gets the loot table of this enemy. You can edit the list to add new drops.
        /// </summary>
        /// <remarks>
        /// Using this method after <see cref="Mod.Load"/> will return a copy of the table.
        /// </remarks>
        public List<Drop> LootTable
        {
            get
            {
                if (!Mod.InLoad)
                {
                    return new List<Drop>(lootTable);
                }

                return lootTable;
            }
        }

        /// <summary>
        /// Gets or sets the enemy's constructor.
        /// The constructor is used to initialize an enemy once it has been created.
        /// </summary>
        public EnemyBuilder Constructor
        {
            get => constructor;
            set
            {
                ErrorHelper.ThrowIfNotLoading(Mod);
                constructor = value;
            }
        }

        /// <summary>
        /// Gets or sets the enemy's difficulty scaler.
        /// The constructor is used to update the enemy's stats or behavior when the difficulty changes.
        /// This method is also called after the enemy is spawned.
        /// </summary>
        public EnemyBuilder DifficultyScaler
        {
            get => difficultyScaler;
            set
            {
                ErrorHelper.ThrowIfNotLoading(Mod);
                difficultyScaler = value;
            }
        }

        /// <summary>
        /// Gets or sets the enemy's elite scaler.
        /// The constructor is used to update the enemy's stats or behavior when it becomes elite.
        /// This method is also called after the enemy is spawned.
        /// </summary>
        public EnemyBuilder EliteScaler
        {
            get => eliteScaler;
            set
            {
                ErrorHelper.ThrowIfNotLoading(Mod);
                eliteScaler = value;
            }
        }

        /// <summary>
        /// Gets or sets the enemy's category.
        /// </summary>
        public EnemyDescription.Category Category
        {
            get => vanilla.enCategory;
            set
            {
                ErrorHelper.ThrowIfNotLoading(Mod);
                vanilla.enCategory = value;
            }
        }

        #endregion

        internal EnemyEntry() { }

        internal override void Initialize()
        {
            vanilla.enType = GameID;

            // Add a Card entry in the Journal
            if (vanilla.iCardDropChance != 0 && vanilla.enCardTypeOverride == EnemyCodex.EnemyTypes.Null)
            {
                EnemyCodex.lxSortedCardEntries.Add(vanilla);
            }

            // Add an Enemy entry in the Journal
            if (createJournalEntry)
            {
                EnemyCodex.lxSortedDescriptions.Add(vanilla);
            }

            // Add drops
            vanilla.lxLootTable.AddRange(lootTable.Select(x => new DropChance((int)(x.Chance * 1000f), x.Item, 1)));

            Globals.Game.EXT_AddMiscText("Enemies", vanilla.sNameLibraryHandle, vanilla.sFullName);
            Globals.Game.EXT_AddMiscText("Enemies", vanilla.sFlavorLibraryHandle, vanilla.sFlavorText);
            Globals.Game.EXT_AddMiscText("Enemies", vanilla.sCardDescriptionLibraryHandle, vanilla.sCardDescription);
            Globals.Game.EXT_AddMiscText("Enemies", vanilla.sDetailedDescriptionLibraryHandle, vanilla.sDetailedDescription);
        }

        internal override void Cleanup()
        {
            // Enemy instances have their assets cleared due to using the world region content manager

            EnemyCodex.lxSortedCardEntries.Remove(vanilla);
            EnemyCodex.lxSortedDescriptions.Remove(vanilla);

            Globals.Game.EXT_RemoveMiscText("Enemies", vanilla.sNameLibraryHandle);
            Globals.Game.EXT_RemoveMiscText("Enemies", vanilla.sFlavorLibraryHandle);
            Globals.Game.EXT_RemoveMiscText("Enemies", vanilla.sCardDescriptionLibraryHandle);
            Globals.Game.EXT_RemoveMiscText("Enemies", vanilla.sDetailedDescriptionLibraryHandle);

            // Enemy Codex textures only load into InGameMenu.contTempAssetManager
            // We unload contTempAssetManager as part of mod reloading procedure
        }
    }
}
