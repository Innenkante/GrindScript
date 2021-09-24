using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using Stat = SoG.EquipmentInfo.StatEnum;

namespace SoG.Modding.Configs
{
    /// <summary>
    /// Used to define a custom item's basic properties, such as its name, value, item categories, and other things.
    /// </summary>

    public class ItemConfig
    {
        public class VSetInfo
        {
            public bool[] HatUnderHair { get; private set; } = new bool[] { false, false, false, false };

            public bool[] HatBehindPlayer { get; private set; } = new bool[] { false, false, false, false };

            public Vector2[] HatOffsets { get; private set; } = new Vector2[] { Vector2.Zero, Vector2.Zero, Vector2.Zero, Vector2.Zero };

            public bool ObstructHairSides { get; set; } = true;

            public bool ObstructHairTop { get; set; } = true;

            public bool ObstructHairBottom { get; set; } = false;

            /// <summary> The subfolder where the textures are for this style, relative to the hat textures folder. </summary>
            public string Resource { get; set; } = "";

            public VSetInfo DeepCopy()
            {
                VSetInfo clone = (VSetInfo)MemberwiseClone();

                clone.HatUnderHair = new bool[4];
                clone.HatBehindPlayer = new bool[4];
                clone.HatOffsets = new Vector2[4];

                Array.Copy(HatUnderHair, clone.HatUnderHair, HatUnderHair.Length);
                Array.Copy(HatBehindPlayer, clone.HatBehindPlayer, HatBehindPlayer.Length);
                Array.Copy(HatOffsets, clone.HatOffsets, HatOffsets.Length);

                return clone;
            }
        }

        /// <summary>
        /// Creates a new ItemConfig. UniqueID must be unique between all other items in the same mod.
        /// </summary>

        public ItemConfig(string uniqueID)
        {
            ModID = uniqueID;
        }

        // ItemDescription

        /// <summary> An identifier that must be unique between all other items in the same mod. </summary>
        public string ModID { get; set; } = "";

        public string Name { get; set; } = "A Mod Item";

        public string Description { get; set; } = "Description pending!";

        /// <summary> The path to the drop appearance texture of an item, relative to "Content/", and without the ".xnb" extension. </summary>
        public string IconPath { get; set; } = "";

        /// <summary> The path to the drop shadow texture of an item, relative to "Content/", and without the ".xnb" extension. </summary>
        public string ShadowPath { get; set; } = "";

        /// <summary> Gold value of an item. For fresh players, this is identical to the buy price, and twice the sell price. </summary>
        public int Value { get; set; } = 1;

        /// <summary> Overrides the health cost of this item when bought from the Shadier Merchant. If set to 0, the game calculates it from the gold cost. </summary>
        public int BloodValue { get; set; } = 0;

        /// <summary> Gold value modifier for items when playing in Arcade Mode. </summary>
        public float ArcadeValueModifier { get; set; } = 1f;

        /// <summary> Items with higher sorting values appear first when sorting by "Best" in inventory. </summary>
        public ushort SortingValue { get; set; } = 1;

        /// <summary> Determines the special visual effect used for item drops. Valid values are 1 (none), 2 (silver ring) and 3 (gold ring). </summary>
        public byte Fancyness { get; set; } = 1;

        /// <summary> The ContentManager to use for this item's textures. Default manager is Game1.Content. </summary>
        public ContentManager Manager { get; set; } = Globals.Game.Content;

        /// <summary> Item Categories to assign to this item. GrindScript automatically assigns certain categories for equipments. </summary>
        public HashSet<ItemCodex.ItemCategories> Categories { get; private set; } = new HashSet<ItemCodex.ItemCategories>();

        public ItemConfig SetCategories(params ItemCodex.ItemCategories[] categories)
        {
            Categories.Clear();
            Categories.UnionWith(categories);
            return this;
        }

        // EquipmentInfo - shared

        /// <summary> The path to the folder containing the textures needed for the equipment. </summary>
        public string EquipResourcePath { get; set; } = "";

        public Dictionary<Stat, int> AllStats { get; private set; } = new Dictionary<Stat, int>();

        public int HP { get => AllStats[Stat.HP]; set => AllStats[Stat.HP] = value; }

        public int EP { get => AllStats[Stat.EP]; set => AllStats[Stat.EP] = value; }

        public int ATK { get => AllStats[Stat.ATK]; set => AllStats[Stat.ATK] = value; }

        public int MATK { get => AllStats[Stat.MATK]; set => AllStats[Stat.MATK] = value; }

        public int DEF { get => AllStats[Stat.DEF]; set => AllStats[Stat.DEF] = value; }

        public int ASPD { get => AllStats[Stat.ASPD]; set => AllStats[Stat.ASPD] = value; }

        public int CSPD { get => AllStats[Stat.CSPD]; set => AllStats[Stat.CSPD] = value; }

        public int Crit { get => AllStats[Stat.Crit]; set => AllStats[Stat.Crit] = value; }

        public int CritDMG { get => AllStats[Stat.CritDMG]; set => AllStats[Stat.CritDMG] = value; }

        public int ShldHP { get => AllStats[Stat.ShldHP]; set => AllStats[Stat.ShldHP] = value; }

        public int EPRegen { get => AllStats[Stat.EPRegen]; set => AllStats[Stat.EPRegen] = value; }

        public int ShldRegen { get => AllStats[Stat.ShldRegen]; set => AllStats[Stat.ShldRegen] = value; }

        /// <summary> 
        /// The equipment type of this item.
        /// Certain config settings are ignored depending on the EquipType (for example, hat appearance settings for weapons). <para/>
        /// A value of EquipType.None will create a non-equipment item.
        /// </summary>
        public EquipmentType EquipType { get; set; } = EquipmentType.None;

        /// <summary> The Special Effects that this equipment has. Multiple special effects can be added. </summary>
        public HashSet<EquipmentInfo.SpecialEffect> Effects { get; private set; } = new HashSet<EquipmentInfo.SpecialEffect>();

        // FacegearInfo

        public bool[] FacegearOverHair { get; private set; } = new bool[] { true, true, true, true };

        public bool[] FacegearOverHat { get; private set; } = new bool[] { true, true, true, true };

        public Vector2[] FacegearOffsets { get; private set; } = new Vector2[] { Vector2.Zero, Vector2.Zero, Vector2.Zero, Vector2.Zero };

        // HatInfo

        /// <summary> For hats, sets the default visual appearance. </summary>
        public VSetInfo DefaultSet { get; private set; } = new VSetInfo();

        /// <summary> For hats, sets the visual appearance when the player has a certain hair style. </summary>
        public Dictionary<ItemCodex.ItemTypes, VSetInfo> AltSets { get; private set; } = new Dictionary<ItemCodex.ItemTypes, VSetInfo>();

        /// <summary> For hats, sets whenever it occupies both the hat and the facegear slot (i.e. it is not possible to equip a facegear alongside this hat). </summary>
        public bool HatDoubleSlot { get; set; } = false;

        // WeaponInfo

        public WeaponInfo.WeaponCategory WeaponType { get; set; } = WeaponInfo.WeaponCategory.OneHanded;

        /// <summary> For weapons, causes it to act as a magic weapon (i.e. basic attacks shoot projectiles and draw their damage from both ATK and MATK). </summary>
        public bool MagicWeapon { get; set; } = false;

        // Helper Methods

        private VSetInfo GetOrCreateSet(ItemCodex.ItemTypes altSet)
        {
            if (altSet == ItemCodex.ItemTypes.Null)
                return DefaultSet;

            return AltSets.TryGetValue(altSet, out VSetInfo exists) ? exists : AltSets[altSet] = new VSetInfo();
        }

        // Methods specific to FacegearInfo

        public ItemConfig SetFacegearOverHair(bool up, bool right, bool down, bool left)
        {
            Array.Copy(new bool[] { up, right, down, left }, FacegearOverHair, 4);
            return this;
        }

        public ItemConfig SetFacegearOverHat(bool up, bool right, bool down, bool left)
        {
            Array.Copy(new bool[] { up, right, down, left }, FacegearOverHat, 4);
            return this;
        }

        public ItemConfig SetFacegearOffsets(Vector2 up, Vector2 right, Vector2 down, Vector2 left)
        {
            Array.Copy(new Vector2[] { up, right, down, left }, FacegearOffsets, 4);
            return this;
        }

        // Methods specific to HatInfo

        public ItemConfig SetHatUnderHair(bool up, bool right, bool down, bool left, ItemCodex.ItemTypes targetHairStyle = ItemCodex.ItemTypes.Null)
        {
            VSetInfo setToUse = GetOrCreateSet(targetHairStyle);
            Array.Copy(new bool[] { up, right, down, left }, setToUse.HatUnderHair, 4);
            return this;
        }

        public ItemConfig SetHatBehindPlayer(bool up, bool right, bool down, bool left, ItemCodex.ItemTypes targetHairStyle = ItemCodex.ItemTypes.Null)
        {
            VSetInfo setToUse = GetOrCreateSet(targetHairStyle);
            Array.Copy(new bool[] { up, right, down, left }, setToUse.HatBehindPlayer, 4);
            return this;
        }

        public ItemConfig SetHatOffsets(Vector2 up, Vector2 right, Vector2 down, Vector2 left, ItemCodex.ItemTypes targetHairStyle = ItemCodex.ItemTypes.Null)
        {
            VSetInfo setToUse = GetOrCreateSet(targetHairStyle);
            Array.Copy(new Vector2[] { up, right, down, left }, setToUse.HatOffsets, 4);
            return this;
        }

        public ItemConfig SetHatHairObstruction(bool sides, bool top, bool bottom, ItemCodex.ItemTypes targetHairStyle = ItemCodex.ItemTypes.Null)
        {
            VSetInfo setToUse = GetOrCreateSet(targetHairStyle);
            setToUse.ObstructHairSides = sides;
            setToUse.ObstructHairTop = top;
            setToUse.ObstructHairBottom = bottom;
            return this;
        }

        public ItemConfig SetHatAltSetResource(ItemCodex.ItemTypes hairdoAltSet, string resource)
        {
            VSetInfo setToUse = GetOrCreateSet(hairdoAltSet);
            if (setToUse != DefaultSet)
                setToUse.Resource = resource;
            return this;
        }

        public ItemConfig DeepCopy()
        {
            ItemConfig clone = (ItemConfig)MemberwiseClone();

            clone.Categories = new HashSet<ItemCodex.ItemCategories>(Categories);
            clone.AllStats = new Dictionary<Stat, int>(AllStats);
            clone.Effects = new HashSet<EquipmentInfo.SpecialEffect>(Effects);

            clone.FacegearOverHair = new bool[4];
            clone.FacegearOverHat = new bool[4];
            clone.FacegearOffsets = new Vector2[4];

            Array.Copy(FacegearOverHair, clone.FacegearOverHair, FacegearOverHair.Length);
            Array.Copy(FacegearOverHat, clone.FacegearOverHat, FacegearOverHat.Length);
            Array.Copy(FacegearOffsets, clone.FacegearOffsets, FacegearOffsets.Length);

            clone.DefaultSet = DefaultSet.DeepCopy();

            clone.AltSets = new Dictionary<ItemCodex.ItemTypes, VSetInfo>();

            foreach (var kvp in AltSets)
            {
                clone.AltSets.Add(kvp.Key, kvp.Value.DeepCopy());
            }

            return clone;
        }
    }
}
