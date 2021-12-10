using Microsoft.Xna.Framework.Content;
using SoG.Modding.Utils;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Stat = SoG.EquipmentInfo.StatEnum;
using System;

namespace SoG.Modding.Content
{
    /// <summary>
    /// Represents a modded enemy, and defines ways to create it.
    /// </summary>
    /// <remarks> 
    /// Equipment data is only used if the item acts as an equipment.
    /// For example, setting weapon data isn't needed for facegear. <para/>
    /// Most of the methods in this class can only be used while a mod is loading, that is, inside <see cref="Mod.Load"/>.
    /// </remarks>
    public class ItemEntry : Entry<ItemCodex.ItemTypes>
    {
        /// <summary>
        /// Provides methods to configure a hat's visual set.
        /// The arrays in this class are arranged based on direction: Up, Right, Down and Left.
        /// </summary>
        public class VisualSetConfig
        {
            internal ItemEntry entry;

            internal ItemCodex.ItemTypes comboItem;

            internal HatInfo.VisualSet visualSet;

            /// <summary>
            /// Gets an array of booleans that toggle hair overlap for each direction.
            /// </summary>
            public bool[] HatUnderHair
            {
                get
                {
                    ErrorHelper.ThrowIfNotLoading(entry.Mod);
                    return visualSet.abUnderHair;
                }
            }

            /// <summary>
            /// Gets an array of booleans that toggle player overlap for each direction.
            /// </summary>
            public bool[] HatBehindPlayer
            {
                get
                {
                    ErrorHelper.ThrowIfNotLoading(entry.Mod);
                    return visualSet.abBehindCharacter;
                }
            }

            /// <summary>
            /// Gets an array of four vectors that define the sprite displacement for each direction.
            /// </summary>
            public Vector2[] HatOffsets
            {
                get
                {
                    ErrorHelper.ThrowIfNotLoading(entry.Mod);
                    return visualSet.av2RenderOffsets;
                }
            }

            /// <summary>
            /// Gets an array of four booleans that toggle hat overlap for hair "sides", for each direction.
            /// </summary>
            public bool ObstructHairSides
            {
                get => visualSet.bObstructsSides;
                set
                {
                    ErrorHelper.ThrowIfNotLoading(entry.Mod);
                    visualSet.bObstructsSides = value;
                }
            }

            /// <summary>
            /// Gets an array of four booleans that toggle hat overlap for hair "tops", for each direction.
            /// </summary>
            public bool ObstructHairTop
            {
                get => visualSet.bObstructsSides;
                set
                {
                    ErrorHelper.ThrowIfNotLoading(entry.Mod);
                    visualSet.bObstructsSides = value;
                }
            }

            /// <summary>
            /// Gets an array of four booleans that toggle hat overlap for hair "bottoms", for each direction.
            /// </summary>
            public bool ObstructHairBottom
            {
                get => visualSet.bObstructsBottom;
                set
                {
                    ErrorHelper.ThrowIfNotLoading(entry.Mod);
                    visualSet.bObstructsBottom = value;
                }
            }

            /// <summary>
            /// Gets or sets the resource path of the visual set. <para/>
            /// For alternate sets, the path is relative to <see cref="ItemEntry.EquipResourcePath"/>.
            /// </summary>
            public string Resource
            {
                get => entry.hatAltSetResourcePaths[comboItem];
                set
                {
                    ErrorHelper.ThrowIfNotLoading(entry.Mod);
                    entry.hatAltSetResourcePaths[comboItem] = value;
                }
            }

            /// <summary>
            /// Helper method to set hair overlaps for each direction.
            /// </summary>
            public void SetHatUnderHair(bool up, bool right, bool down, bool left)
            {
                var array = HatUnderHair;
                array[0] = up;
                array[1] = right;
                array[2] = down;
                array[3] = left;
            }

            /// <summary>
            /// Helper method to set player overlaps for each direction.
            /// </summary>
            public void SetHatBehindPlayer(bool up, bool right, bool down, bool left)
            {
                var array = HatBehindPlayer;
                array[0] = up;
                array[1] = right;
                array[2] = down;
                array[3] = left;
            }

            /// <summary>
            /// Helper method to set sprite offsets for each direction.
            /// </summary>
            public void SetHatOffsets(Vector2 up, Vector2 right, Vector2 down, Vector2 left)
            {
                var array = HatOffsets;
                array[0] = up;
                array[1] = right;
                array[2] = down;
                array[3] = left;
            }

            /// <summary>
            /// Helper method to set hair obstructions for each direction.
            /// </summary>
            public void SetHatHairObstruction(bool sides, bool top, bool bottom)
            {
                ObstructHairSides = sides;
                ObstructHairTop = top;
                ObstructHairBottom = bottom;
            }
        }

        #region IEntry Properties

        public override Mod Mod { get; internal set; }

        public override ItemCodex.ItemTypes GameID { get; internal set; }

        public override string ModID { get; internal set; }

        #endregion

        #region Internal Data

        internal string iconPath = "";

        internal string shadowPath = "";

        internal string equipResourcePath = "";

        internal Dictionary<Stat, int> stats  = new Dictionary<Stat, int>();

        internal EquipmentType equipType = EquipmentType.None;

        internal HashSet<EquipmentInfo.SpecialEffect> effects = new HashSet<EquipmentInfo.SpecialEffect>();

        internal bool[] facegearOverHair = new bool[] { true, true, true, true };

        internal bool[] facegearOverHat = new bool[] { true, true, true, true };

        internal Vector2[] facegearOffsets = new Vector2[] { Vector2.Zero, Vector2.Zero, Vector2.Zero, Vector2.Zero };

        internal HatInfo.VisualSet defaultSet = new HatInfo.VisualSet();

        internal Dictionary<ItemCodex.ItemTypes, HatInfo.VisualSet> altSets = new Dictionary<ItemCodex.ItemTypes, HatInfo.VisualSet>();

        internal Dictionary<ItemCodex.ItemTypes, string> hatAltSetResourcePaths = new Dictionary<ItemCodex.ItemTypes, string>();

        internal bool hatDoubleSlot = false;

        internal WeaponInfo.WeaponCategory weaponType = WeaponInfo.WeaponCategory.OneHanded;

        internal bool magicWeapon = false;

        internal ItemDescription vanillaItem = new ItemDescription();

        internal EquipmentInfo vanillaEquip;

        #endregion

        #region Public Interface

        /// <summary>
        /// Gets or sets the display name of the item.
        /// </summary>
        public string Name
        {
            get => vanillaItem.sFullName;
            set
            {
                ErrorHelper.ThrowIfNotLoading(Mod);
                vanillaItem.sFullName = value;
            }
        }

        /// <summary>
        /// Gets or sets the description of the item.
        /// </summary>
        public string Description
        {
            get => vanillaItem.sDescription;
            set
            {
                ErrorHelper.ThrowIfNotLoading(Mod);
                vanillaItem.sDescription = value;
            }
        }

        /// <summary> 
        /// Gets or sets the path to the item's icon. The texture path is relative to "Content/".
        /// </summary>
        public string IconPath
        {
            get => iconPath;
            set
            {
                ErrorHelper.ThrowIfNotLoading(Mod);
                iconPath = value;
            }
        }

        /// <summary> 
        /// Gets or sets the path to the item's shadow texture. The texture path is relative to "Content/".
        /// </summary>
        public string ShadowPath
        {
            get => shadowPath;
            set
            {
                ErrorHelper.ThrowIfNotLoading(Mod);
                shadowPath = value;
            }
        }

        /// <summary>
        /// Gets or sets the gold value of the item.
        /// Buy price is equal to the gold value, the sell price is halved, and buyback is doubled.
        /// </summary>
        public int Value
        {
            get => vanillaItem.iValue;
            set
            {
                ErrorHelper.ThrowIfNotLoading(Mod);
                vanillaItem.iValue = value;
            }
        }

        /// <summary>
        /// Gets or sets the health cost of this item, when bought from the Shadier Merchant in Arcade.
        /// A value of 0 will cause the game to calculate the blood cost from the item's gold price.
        /// </summary>
        public int BloodValue
        {
            get => vanillaItem.iOverrideBloodValue;
            set
            {
                ErrorHelper.ThrowIfNotLoading(Mod);
                vanillaItem.iOverrideBloodValue = value;
            }
        }

        /// <summary>
        /// Gets or sets the value modifier for this item in Arcade Mode.
        /// A value of 0.5 would make the item have half the gold value in Arcade Mode.
        /// </summary>
        public float ArcadeValueModifier
        {
            get => vanillaItem.fArcadeModeCostModifier;
            set
            {
                ErrorHelper.ThrowIfNotLoading(Mod);
                vanillaItem.fArcadeModeCostModifier = value;
            }
        }

        /// <summary>
        /// Gets or sets the sort value of this item.
        /// Items with a higher value will appear first when sorting the inventory using the "Best" filter.
        /// </summary>
        public ushort SortingValue
        {
            get => vanillaItem.iInternalLevel;
            set
            {
                ErrorHelper.ThrowIfNotLoading(Mod);
                vanillaItem.iInternalLevel = value;
            }
        }

        /// <summary> 
        /// Gets or sets the special effect around item drops of this type.
        /// Valid values are 1 (none), 2 (silver ring) and 3 (gold ring). 
        /// </summary>
        public byte Fancyness
        {
            get => vanillaItem.byFancyness;
            set
            {
                ErrorHelper.ThrowIfNotLoading(Mod);
                vanillaItem.byFancyness = (byte)MathHelper.Clamp(value, 1, 3);
            }
        }

        /// <summary>
        /// Adds a category for this item. <para/>
        /// Some categories have a special meaning.
        /// For example, items with <see cref="ItemCodex.ItemCategories.Usable"/> can be quickslotted and activated.
        /// GrindScript automatically assigns certain categories for items that are also equipments. 
        /// </summary>
        /// <param name="category"> The category to add. </param>
        public void AddCategory(ItemCodex.ItemCategories category)
        {
            ErrorHelper.ThrowIfNotLoading(Mod);
            vanillaItem.lenCategory.Add(category);
        }

        /// <summary>
        /// Removes a category for this item.
        /// </summary>
        /// <param name="category"> The category to remove. </param>
        public void RemoveCategory(ItemCodex.ItemCategories category)
        {
            ErrorHelper.ThrowIfNotLoading(Mod);
            vanillaItem.lenCategory.Add(category);
        }

        /// <summary>
        /// Gets or sets the equipment's resource path. The resource path is relative to "Content/".
        /// For equipment, textures are loaded using specific file names, all relative to this resource path.
        /// </summary>
        public string EquipResourcePath
        {
            get => equipResourcePath;
            set
            {
                ErrorHelper.ThrowIfNotLoading(Mod);
                equipResourcePath = value;
            }
        }

        /// <summary>
        /// Gets or sets the MaxHP provided by the equipment.
        /// </summary>
        public int HP 
        { 
            get => stats[Stat.HP]; 
            set => ErrorHelper.AssertLoading(Mod, () => stats[Stat.HP] = value); 
        }

        /// <summary>
        /// Gets or sets the MaxEP provided by the equipment.
        /// </summary>
        public int EP
        {
            get => stats[Stat.EP];
            set => ErrorHelper.AssertLoading(Mod, () => stats[Stat.EP] = value);
        }

        /// <summary>
        /// Gets or sets the ATK provided by the equipment.
        /// </summary>
        public int ATK
        {
            get => stats[Stat.ATK];
            set => ErrorHelper.AssertLoading(Mod, () => stats[Stat.ATK] = value);
        }

        /// <summary>
        /// Gets or sets the MATK provided by the equipment.
        /// </summary>
        public int MATK
        {
            get => stats[Stat.MATK];
            set => ErrorHelper.AssertLoading(Mod, () => stats[Stat.MATK] = value);
        }

        /// <summary>
        /// Gets or sets the DEF provided by the equipment.
        /// </summary>
        public int DEF
        {
            get => stats[Stat.DEF];
            set => ErrorHelper.AssertLoading(Mod, () => stats[Stat.DEF] = value);
        }

        /// <summary>
        /// Gets or sets the ASPD provided by the equipment.
        /// </summary>
        public int ASPD
        {
            get => stats[Stat.ASPD];
            set => ErrorHelper.AssertLoading(Mod, () => stats[Stat.ASPD] = value);
        }

        /// <summary>
        /// Gets or sets the CSPD provided by the equipment.
        /// </summary>
        public int CSPD
        {
            get => stats[Stat.CSPD];
            set => ErrorHelper.AssertLoading(Mod, () => stats[Stat.CSPD] = value);
        }

        /// <summary>
        /// Gets or sets the Crit provided by the equipment.
        /// </summary>
        public int Crit
        {
            get => stats[Stat.Crit];
            set => ErrorHelper.AssertLoading(Mod, () => stats[Stat.Crit] = value);
        }

        /// <summary>
        /// Gets or sets the CritDMG provided by the equipment.
        /// </summary>
        public int CritDMG
        {
            get => stats[Stat.CritDMG];
            set => ErrorHelper.AssertLoading(Mod, () => stats[Stat.CritDMG] = value);
        }

        /// <summary>
        /// Gets or sets the ShldHP provided by the equipment. <para/>
        /// This works even if it's not on a shield!
        /// </summary>
        public int ShldHP
        {
            get => stats[Stat.ShldHP];
            set => ErrorHelper.AssertLoading(Mod, () => stats[Stat.ShldHP] = value);
        }

        /// <summary>
        /// Gets or sets the EP Regen provided by the equipment.
        /// </summary>
        public int EPRegen
        {
            get => stats[Stat.EPRegen];
            set => ErrorHelper.AssertLoading(Mod, () => stats[Stat.EPRegen] = value);
        }

        /// <summary>
        /// Gets or sets the Shield Regen provided by the equipment. <para/>
        /// This stat isn't visible in the equipment display, however it still works,
        /// and will boost up the shield's health regen amount.
        /// </summary>
        public int ShldRegen
        {
            get => stats[Stat.ShldRegen];
            set => ErrorHelper.AssertLoading(Mod, () => stats[Stat.ShldRegen] = value);
        }

        /// <summary>
        /// Gets or sets what equipment this item acts as (if any).
        /// Setting this to values other than <see cref="EquipmentType.None"/>
        /// will make this item into an equipment of that type.
        /// </summary>
        public EquipmentType EquipType
        {
            get => equipType;
            set
            {
                ErrorHelper.ThrowIfNotLoading(Mod);
                equipType = value;
            }
        }

        /// <summary>
        /// Adds a special effect to this equipment.
        /// Multiple effects (of different types) can be added;
        /// each of them will show up as "Special Effect" in-game.
        /// </summary>
        /// <param name="effect"> The effect to add. This can also be a modded effect. </param>
        public void AddSpecialEffect(EquipmentInfo.SpecialEffect effect)
        {
            ErrorHelper.ThrowIfNotLoading(Mod);
            effects.Add(effect);
        }

        /// <summary>
        /// Removes a special effect from this equipment.
        /// </summary>
        /// <param name="effect"> The effect to add. This can also be a modded effect. </param>
        public void RemoveSpecialEffect(EquipmentInfo.SpecialEffect effect)
        {
            ErrorHelper.ThrowIfNotLoading(Mod);
            effects.Remove(effect);
        }

        /// <summary>
        /// Gets an array of four booleans that toggle facegear overlap for hair, for each direction.
        /// </summary>
        public bool[] FacegearOverHair
        {
            get
            {
                ErrorHelper.ThrowIfNotLoading(Mod);
                return facegearOverHair;
            }
        }

        /// <summary>
        /// Gets an array of four booleans that toggle facegear overlap for hat, for each direction.
        /// </summary>
        public bool[] FacegearOverHat
        {
            get
            {
                ErrorHelper.ThrowIfNotLoading(Mod);
                return facegearOverHat;
            }
        }

        /// <summary>
        /// Gets an array of four sprite render offsets for facegear, for each direction.
        /// </summary>
        public Vector2[] FacegearOffsets
        {
            get
            {
                ErrorHelper.ThrowIfNotLoading(Mod);
                return facegearOffsets;
            }
        }

        private VisualSetConfig _defaultSetConfig;

        /// <summary>
        /// Gets the default visual set of a hat. This set is used for most hair types.
        /// </summary>
        public VisualSetConfig DefaultSet
        {
            get
            {
                if (_defaultSetConfig == null)
                {
                    _defaultSetConfig = new VisualSetConfig()
                    {
                        entry = this,
                        comboItem = ItemCodex.ItemTypes.Null,
                        visualSet = defaultSet
                    };
                }

                return _defaultSetConfig;
            }
        }

        /// <summary>
        /// Creates a configuration for an alternate visual set. <para/>
        /// Alternate visual sets are used when the player has certain hair styles.
        /// Keep in mind that the hair styles are defined as items in <see cref="ItemCodex.ItemTypes"/>.
        /// </summary>
        /// <param name="comboItem"> The hair to create the alternate visual set for. </param>
        /// <returns> The config for the alternate set. </returns>
        public VisualSetConfig ConfigureAltSet(ItemCodex.ItemTypes comboItem)
        {
            if (!altSets.TryGetValue(comboItem, out HatInfo.VisualSet set))
            {
                set = altSets[comboItem] = new HatInfo.VisualSet();
            }

            return new VisualSetConfig()
            {
                entry = this,
                comboItem = comboItem,
                visualSet = set
            };
        }

        /// <summary>
        /// Gets or sets whenever hats occupy one slot (hat) or two (hat + facegear => mask).
        /// </summary>
        public bool HatDoubleSlot
        {
            get => hatDoubleSlot;
            set
            {
                ErrorHelper.ThrowIfNotLoading(Mod);
                hatDoubleSlot = value;
            }
        }

        /// <summary>
        /// Gets or sets the equipment's weapon type.
        /// </summary>
        public WeaponInfo.WeaponCategory WeaponType
        {
            get => weaponType;
            set
            {
                ErrorHelper.ThrowIfNotLoading(Mod);
                weaponType = value;
            }
        }

        /// <summary>
        /// Gets or sets whenever the weapon is magical or not. <para/>
        /// Basic attacks with physical weapons deal 100% ATK as damage. <para/>
        /// For magic weapons, the damage is equal to 40% (ATK + MATK).
        /// Additionally, magic weapons can fire a projectile that deals 40% MATK as damage.
        /// </summary>
        public bool MagicWeapon
        {
            get => magicWeapon;
            set
            {
                ErrorHelper.ThrowIfNotLoading(Mod);
                magicWeapon = value;
            }
        }

        // Methods specific to FacegearInfo

        /// <summary>
        /// Helper method for setting facegear hair overlaps for all directions.
        /// </summary>
        public void SetFacegearOverHair(bool up, bool right, bool down, bool left)
        {
            var array = FacegearOverHair;
            array[0] = up;
            array[1] = right;
            array[2] = down;
            array[3] = left;
        }

        /// <summary>
        /// Helper method for setting facegear hat overlaps for all directions.
        /// </summary>
        public void SetFacegearOverHat(bool up, bool right, bool down, bool left)
        {
            var array = FacegearOverHat;
            array[0] = up;
            array[1] = right;
            array[2] = down;
            array[3] = left;
        }

        /// <summary>
        /// Helper method for setting facegear offsets for all directions.
        /// </summary>
        public void SetFacegearOffsets(Vector2 up, Vector2 right, Vector2 down, Vector2 left)
        {
            var array = FacegearOffsets;
            array[0] = up;
            array[1] = right;
            array[2] = down;
            array[3] = left;
        }

        #endregion

        internal ItemEntry() { }

        internal ItemEntry(Mod owner, ItemCodex.ItemTypes gameID, string modID)
        {
            Mod = owner;
            GameID = gameID;
            ModID = modID;
        }

        internal override void Initialize()
        {
            vanillaItem.enType = GameID;
            
            if (vanillaEquip != null)
            {
                vanillaEquip.enItemType = GameID;
                vanillaEquip.xItemDescription = vanillaItem;
            }

            vanillaItem.sNameLibraryHandle = $"Item_{(int)GameID}_Name";
            vanillaItem.sDescriptionLibraryHandle = $"Item_{(int)GameID}_Description";
            vanillaItem.sCategory = "";

            EquipmentType typeToUse = Enum.IsDefined(typeof(EquipmentType), equipType) ? equipType : EquipmentType.None;

            EquipmentInfo equipData = null;
            switch (typeToUse)
            {
                case EquipmentType.None:
                    break;
                case EquipmentType.Facegear:
                    FacegearInfo faceData = (equipData = new FacegearInfo(GameID)) as FacegearInfo;

                    Array.Copy(facegearOverHair, faceData.abOverHair, 4);
                    Array.Copy(facegearOverHat, faceData.abOverHat, 4);
                    Array.Copy(facegearOffsets, faceData.av2RenderOffsets, 4);

                    break;
                case EquipmentType.Hat:
                    HatInfo hatData = (equipData = new HatInfo(GameID) { bDoubleSlot = hatDoubleSlot }) as HatInfo;

                    hatData.xDefaultSet = defaultSet;
                    hatData.denxAlternateVisualSets = altSets;

                    break;
                case EquipmentType.Weapon:
                    WeaponInfo weaponData = new WeaponInfo(equipResourcePath, GameID, weaponType)
                    {
                        enWeaponCategory = weaponType,
                        enAutoAttackSpell = WeaponInfo.AutoAttackSpell.None
                    };
                    equipData = weaponData;

                    if (weaponType == WeaponInfo.WeaponCategory.OneHanded)
                    {
                        weaponData.iDamageMultiplier = 90;
                        if (magicWeapon)
                            weaponData.enAutoAttackSpell = WeaponInfo.AutoAttackSpell.Generic1H;
                    }
                    else if (weaponType == WeaponInfo.WeaponCategory.TwoHanded)
                    {
                        weaponData.iDamageMultiplier = 125;
                        if (magicWeapon)
                            weaponData.enAutoAttackSpell = WeaponInfo.AutoAttackSpell.Generic2H;
                    }
                    break;
                default:
                    equipData = new EquipmentInfo(equipResourcePath, GameID);
                    break;
            }

            if (equipType != EquipmentType.None)
            {
                equipData.deniStatChanges = new Dictionary<Stat, int>(stats);
                equipData.lenSpecialEffects.AddRange(effects);
            }

            vanillaEquip = equipData;

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

            vanillaItem.lenCategory.ExceptWith(toSanitize);

            if (equipData != null)
            {
                ItemCodex.ItemCategories type = (ItemCodex.ItemCategories)equipType;
                vanillaItem.lenCategory.Add(type);

                if (type == ItemCodex.ItemCategories.Weapon)
                {
                    switch ((equipData as WeaponInfo).enWeaponCategory)
                    {
                        case WeaponInfo.WeaponCategory.OneHanded:
                            vanillaItem.lenCategory.Add(ItemCodex.ItemCategories.OneHandedWeapon); break;
                        case WeaponInfo.WeaponCategory.TwoHanded:
                            vanillaItem.lenCategory.Add(ItemCodex.ItemCategories.TwoHandedWeapon); break;
                    }
                }
            }

            Globals.Game.EXT_AddMiscText("Items", vanillaItem.sNameLibraryHandle, vanillaItem.sFullName);
            Globals.Game.EXT_AddMiscText("Items", vanillaItem.sDescriptionLibraryHandle, vanillaItem.sDescription);

            // Textures are loaded on demand
        }

        internal override void Cleanup()
        {
            Globals.Game.EXT_RemoveMiscText("Items", vanillaItem.sNameLibraryHandle);
            Globals.Game.EXT_RemoveMiscText("Items", vanillaItem.sDescriptionLibraryHandle);

            // TODO: Set texture references to null since they're disposed

            // Weapon assets are automatically disposed of by the game
            // Same goes for dropped item's textures

            ContentManager manager = Globals.Game.Content;

            if (ModUtils.IsModContentPath(iconPath))
            {
                AssetUtils.UnloadAsset(Globals.Game.Content, iconPath);
            }

            string[] directions = new string[]
            {
                "Up", "Right", "Down", "Left"
            };

            if (vanillaEquip is HatInfo hatData)
            {
                string basePath = equipResourcePath;
                int index = -1;

                while (++index < 4)
                {
                    string texPath = Path.Combine(basePath, directions[index]);

                    if (ModUtils.IsModContentPath(texPath))
                    {
                        AssetUtils.UnloadAsset(manager, texPath);
                    }
                }

                foreach (var kvp in hatData.denxAlternateVisualSets)
                {
                    string altPath = Path.Combine(basePath, hatAltSetResourcePaths[kvp.Key]);

                    index = -1;

                    while (++index < 4)
                    {
                        string texPath = Path.Combine(altPath, directions[index]);

                        if (ModUtils.IsModContentPath(texPath))
                        {
                            AssetUtils.UnloadAsset(manager, texPath);
                        }
                    }
                }
            }
            else if (vanillaEquip is FacegearInfo)
            {
                string path = equipResourcePath;
                int index = -1;

                while (++index < 4)
                {
                    AssetUtils.UnloadAsset(manager, Path.Combine(path, directions[index]));
                }
            }
        }
    }
}
