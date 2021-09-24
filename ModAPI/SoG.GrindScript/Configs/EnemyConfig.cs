using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace SoG.Modding.Configs
{
    public class EnemyConfig
    {
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

        public EnemyConfig(string uniqueID)
        {
            ModID = uniqueID;
        }

        public string ModID { get; set; } = "";

        public string Name { get; set; } = "Weird Mod Thingy";

        public string ShortDescription { get; set; } = "Some random enemy from a mod!";

        public string LongDescription { get; set; } = "The modder forgot to add a LongDescription, sheesh.";

        public int BaseHealth { get; set; } = 100;

        public int Level { get; set; } = 1;

        public string OnHitSound { get; set; } = "Slime_DamageSolo";

        public string OnDeathSound { get; set; } = "Slime_Death";

        public bool CreateJournalEntry { get; set; } = true;

        public Func<ContentManager, Animation> DefaultAnimation { get; set; } = null;

        public Func<ContentManager, Texture2D> DisplayBackground { get; set; } = null;

        public Func<ContentManager, Texture2D> DisplayIcon { get; set; } = null;

        public Vector2 ApproximateCenter { get; set; } = new Vector2(0, -8);

        public Vector2 ApproximateSize { get; set; } = new Vector2(16, 16);

        /// <summary>
        /// Sets the actual card to be dropped.
        /// This is useful if you have enemy variations that are conceptually similar.
        /// If you set this to something other than Null, you don't need to specify CardInfo or CardIllustrationPath (they're not used).
        /// </summary>
        public EnemyCodex.EnemyTypes CardDropOverride { get; set; } = EnemyCodex.EnemyTypes.Null;

        public float CardDropChance { get; set; } = 0f;

        public string CardInfo { get; set; } = "Card effect not yet set by the modder!";

        public string CardIllustrationPath { get; set; } = "GUI/InGameMenu/Journal/CardAlbum/Cards/placeholder";

        public List<Drop> LootTable { get; private set; } = new List<Drop>();

        public EnemyBuilder Constructor { get; set; } = null;

        public EnemyBuilder DifficultyScaler { get; set; } = null;

        public EnemyBuilder EliteScaler { get; set; } = null;

        public EnemyDescription.Category Category { get; set; } = EnemyDescription.Category.Regular;

        public EnemyConfig DeepCopy()
        {
            EnemyConfig clone = (EnemyConfig) MemberwiseClone();

            clone.LootTable = new List<Drop>(LootTable);

            return clone;
        }
    }
}
