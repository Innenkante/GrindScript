using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoG.Modding.Core;
using SoG.Modding.ModUtils;
using SoG.Modding.API.Configs;

namespace SoG.Modding.API
{
    public abstract partial class Mod
    {
        /// <summary> 
        /// Creates an item for each of the ItemConfigs provided. 
        /// </summary>

        public void CreateItems(IEnumerable<ItemConfig> cfgs)
        {
            foreach (var cfg in cfgs)
                CreateItem(cfg);
        }

        /// <summary> 
        /// Creates an item from the given ItemConfig.
        /// </summary>

        public ItemCodex.ItemTypes CreateItem(ItemConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }
            if (!InLoad)
            {
                Globals.Logger.Error("Can not create objects outside of a load context.", source: nameof(CreateItem));
                return ItemCodex.ItemTypes.Null;
            }

            if (GetLibrary().Items.Any(x => x.Value.ModID == config.ModID))
            {
                Globals.Logger.Error($"An item with the ModID {config.ModID} already exists.", source: nameof(CreateItem));
                return ItemCodex.ItemTypes.Null;
            }

            ItemCodex.ItemTypes gameID = Registry.ID.ItemIDNext++;

            ModItemEntry entry = new ModItemEntry(this, gameID, config.ModID)
            {
                Config = config.DeepCopy()
            };

            Registry.Library.Items[gameID] = entry;

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

            Globals.Game.EXT_AddMiscText("Items", itemData.sNameLibraryHandle, itemData.sFullName);
            Globals.Game.EXT_AddMiscText("Items", itemData.sDescriptionLibraryHandle, itemData.sDescription);

            return gameID;
        }

        /// <summary>
        /// Adds a new crafting recipe.
        /// </summary>

        public void AddRecipe(ItemCodex.ItemTypes result, Dictionary<ItemCodex.ItemTypes, ushort> ingredients)
        {
            if (ingredients == null)
            {
                Globals.Logger.Warn("Can't add recipe because ingredient dictionary is null!");
                return;
            }

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
        /// Returns a SpecialEffect ID that you can use for your own equipment.
        /// </summary>
        
        public EquipmentInfo.SpecialEffect CreateSpecialEffect()
        {
            return Registry.ID.ItemEffectIDNext++;
        }

        /// <summary>
        /// Gets an ItemType previously defined by a mod.
        /// If nothing is found, ItemCodex.ItemTypes.Null is returned.
        /// </summary>

        public ItemCodex.ItemTypes GetItemType(Mod owner, string uniqueID)
        {
            var entry = Registry.Library.Items.Values.FirstOrDefault(x => x.Owner == owner && x.ModID == uniqueID);

            return entry?.GameID ?? ItemCodex.ItemTypes.Null;
        }

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
