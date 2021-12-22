using Quests;
using SoG.Modding.Content;
using SoG.Modding.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoG.Modding.GrindScriptMod
{
    internal static class VanillaParser
    {
        public static CurseEntry ParseCurse(RogueLikeMode.TreatsCurses gameID)
        {
            CurseEntry entry = new CurseEntry()
            {
                Mod = Globals.Manager.VanillaMod,
                GameID = gameID,
                ModID = gameID.ToString()
            };

            OriginalMethods._RogueLike_GetTreatCurseInfo(Globals.Game, gameID, out entry.nameHandle, out entry.descriptionHandle, out entry.scoreModifier);

            entry.texturePath = null;

            entry.name = Globals.Game.EXT_GetMiscText("Menus", entry.nameHandle).sUnparsedFullLine;
            entry.description = Globals.Game.EXT_GetMiscText("Menus", entry.descriptionHandle).sUnparsedFullLine;


            // Hacky way to detemine if it's a curse or treat in vanilla

            var treatCurseMenu = Globals.Game.xShopMenu.xTreatCurseMenu;
            entry.isTreat = treatCurseMenu.lenTreatCursesAvailable.Contains(gameID);

            if (gameID == RogueLikeMode.TreatsCurses.Treat_MoreLoods)
            {
                // Special case
                entry.isTreat = true;
            }

            // End of hacky method

            return entry;
        }

        public static EnemyEntry ParseEnemy(EnemyCodex.EnemyTypes gameID)
        {
            EnemyEntry entry = new EnemyEntry()
            {
                Mod = Globals.Manager.VanillaMod,
                GameID = gameID,
                ModID = gameID.ToString()
            };

            if (gameID == EnemyCodex.EnemyTypes.TimeTemple_GiantWorm_Recolor)
            {
                var normal = OriginalMethods.GetEnemyDescription(EnemyCodex.EnemyTypes.TimeTemple_GiantWorm);

                // Recolored worm borrows its enemy description!
                entry.vanilla = new EnemyDescription(gameID, normal.sNameLibraryHandle, normal.iLevel, normal.iMaxHealth)
                {
                    sOnHitSound = normal.sOnHitSound,
                    sOnDeathSound = normal.sOnDeathSound,
                    lxLootTable = new List<DropChance>(normal.lxLootTable),
                    sFullName = normal.sFullName,
                    sFlavorText = normal.sFlavorText,
                    sFlavorLibraryHandle = normal.sFlavorLibraryHandle,
                    sDetailedDescription = normal.sDetailedDescription,
                    sDetailedDescriptionLibraryHandle = normal.sDetailedDescriptionLibraryHandle,
                    iCardDropChance = normal.iCardDropChance,
                    v2ApproximateOffsetToMid = normal.v2ApproximateOffsetToMid,
                    v2ApproximateSize = normal.v2ApproximateSize,
                };

                entry.createJournalEntry = false;
            }
            else
            {
                entry.vanilla = OriginalMethods.GetEnemyDescription(gameID);
                entry.createJournalEntry = EnemyCodex.lxSortedDescriptions.Contains(entry.vanilla);
            }

            entry.defaultAnimation = null;
            entry.displayBackgroundPath = null;
            entry.displayIconPath = null;

            entry.cardIllustrationPath = OriginalMethods.GetIllustrationPath(gameID);

            entry.constructor = null;
            entry.difficultyScaler = null;
            entry.eliteScaler = null;

            List<EnemyCodex.EnemyTypes> resetCardChance = new List<EnemyCodex.EnemyTypes>()
            {
                EnemyCodex.EnemyTypes.Special_ElderBoar,
                EnemyCodex.EnemyTypes.Pumpking,
                EnemyCodex.EnemyTypes.Marino,
                EnemyCodex.EnemyTypes.Boss_MotherPlant,
                EnemyCodex.EnemyTypes.TwilightBoar

            };

            if (resetCardChance.Contains(entry.GameID))
            {
                // These don't have cards, but a drop chance higher than 0 would generate a card entry
                entry.vanilla.iCardDropChance = 0;
            }

            return entry;
        }

        public static EquipmentEffectEntry ParseEquipmentEffect(EquipmentInfo.SpecialEffect gameID)
        {
            EquipmentEffectEntry entry = new EquipmentEffectEntry()
            {
                Mod = Globals.Manager.VanillaMod,
                GameID = gameID,
                ModID = gameID.ToString()
            };

            // Nothing to do for now!

            return entry;
        }

        public static ItemEntry ParseItem(ItemCodex.ItemTypes gameID)
        {
            ItemEntry entry = new ItemEntry()
            {
                Mod = Globals.Manager.VanillaMod,
                GameID = gameID,
                ModID = gameID.ToString()
            };

            entry.vanillaItem = OriginalMethods.GetItemDescription(gameID);

            EquipmentInfo equip = null;

            if (entry.vanillaItem.lenCategory.Contains(ItemCodex.ItemCategories.Weapon))
            {
                var weapon = OriginalMethods.GetWeaponInfo(gameID);
                equip = weapon;
                entry.equipType = EquipmentType.Weapon;

                entry.weaponType = weapon.enWeaponCategory;
                entry.magicWeapon = weapon.enAutoAttackSpell != WeaponInfo.AutoAttackSpell.None;
            }
            else if (entry.vanillaItem.lenCategory.Contains(ItemCodex.ItemCategories.Shield))
            {
                equip = OriginalMethods.GetShieldInfo(gameID);
                entry.equipType = EquipmentType.Shield;
            }
            else if (entry.vanillaItem.lenCategory.Contains(ItemCodex.ItemCategories.Armor))
            {
                equip = OriginalMethods.GetArmorInfo(gameID);
                entry.equipType = EquipmentType.Armor;
            }
            else if (entry.vanillaItem.lenCategory.Contains(ItemCodex.ItemCategories.Hat))
            {
                var hat = OriginalMethods.GetHatInfo(gameID);
                equip = hat;
                entry.equipType = EquipmentType.Hat;

                entry.defaultSet = hat.xDefaultSet;

                foreach (var pair in hat.denxAlternateVisualSets)
                {
                    entry.altSets[pair.Key] = pair.Value;
                    entry.hatAltSetResourcePaths[pair.Key] = null;
                }

                entry.hatDoubleSlot = hat.bDoubleSlot;
            }
            else if (entry.vanillaItem.lenCategory.Contains(ItemCodex.ItemCategories.Facegear))
            {
                var facegear = OriginalMethods.GetFacegearInfo(gameID);
                equip = facegear;
                entry.equipType = EquipmentType.Facegear;

                entry.facegearOverHair = facegear.abOverHat;
                entry.facegearOverHat = facegear.abOverHat;
                entry.facegearOffsets = facegear.av2RenderOffsets;
            }
            else if (entry.vanillaItem.lenCategory.Contains(ItemCodex.ItemCategories.Shoes))
            {
                equip = OriginalMethods.GetShoesInfo(gameID);
                entry.equipType = EquipmentType.Shoes;
            }
            else if (entry.vanillaItem.lenCategory.Contains(ItemCodex.ItemCategories.Accessory))
            {
                equip = OriginalMethods.GetAccessoryInfo(gameID);
                entry.equipType = EquipmentType.Accessory;
            }

            entry.iconPath = null;
            entry.shadowPath = null;
            entry.equipResourcePath = null;

            if (equip != null)
            {
                entry.stats = new Dictionary<EquipmentInfo.StatEnum, int>(equip.deniStatChanges);
                entry.effects = new HashSet<EquipmentInfo.SpecialEffect>(equip.lenSpecialEffects);
                entry.equipResourcePath = equip.sResourceName;
            }

            // Obviously we're not gonna use the modded format to load vanilla assets
            entry.useVanillaResourceFormat = true;

            return entry;
        }

        public static LevelEntry ParseLevel(Level.ZoneEnum gameID)
        {
            LevelEntry entry = new LevelEntry()
            {
                Mod = Globals.Manager.VanillaMod,
                GameID = gameID,
                ModID = gameID.ToString()
            };

            try
            {
                LevelBlueprint vanilla = OriginalMethods.GetBlueprint(gameID);

                entry.worldRegion = vanilla.enRegion;
            }
            catch
            {
                Globals.Logger.Trace($"Auto-guessing region for {gameID} as {Level.WorldRegion.PillarMountains}.");

                entry.worldRegion = Level.WorldRegion.PillarMountains;
            }

            return entry;
        }

        public static PerkEntry ParsePerk(RogueLikeMode.Perks gameID)
        {
            PerkEntry entry = new PerkEntry()
            {
                Mod = Globals.Manager.VanillaMod,
                GameID = gameID,
                ModID = gameID.ToString()
            };

            entry.unlockCondition = null;

            var perkInfo = RogueLikeMode.PerkInfo.lxAllPerks.FirstOrDefault(x => x.enPerk == gameID);

            if (perkInfo == null)
            {
                // These perks are dumb and have a special unlock condition.

                if (gameID == RogueLikeMode.Perks.PetWhisperer)
                {
                    perkInfo = new RogueLikeMode.PerkInfo(RogueLikeMode.Perks.PetWhisperer, 20, "PetWhisperer");

                    entry.unlockCondition = () => CAS.WorldRogueLikeData.henActiveFlags.Contains(FlagCodex.FlagID._Roguelike_TalkedToWeivForTheFirstTime);
                }
                else if (gameID == RogueLikeMode.Perks.MoreFishingRooms)
                {
                    perkInfo = new RogueLikeMode.PerkInfo(RogueLikeMode.Perks.MoreFishingRooms, 25, "MoreFishingRooms");

                    entry.unlockCondition = () => CAS.WorldRogueLikeData.henActiveFlags.Contains(FlagCodex.FlagID._Roguelike_Improvement_Aquarium);
                }
                else if (gameID == RogueLikeMode.Perks.OnlyPinsAfterChallenges)
                {
                    perkInfo = new RogueLikeMode.PerkInfo(RogueLikeMode.Perks.OnlyPinsAfterChallenges, 30, "OnlyPinsAfterChallenges");

                    entry.unlockCondition = () => CAS.WorldRogueLikeData.henActiveFlags.Contains(FlagCodex.FlagID._Roguelike_PinsUnlocked);
                }
                else if (gameID == RogueLikeMode.Perks.ChanceAtPinAfterBattleRoom)
                {
                    perkInfo = new RogueLikeMode.PerkInfo(RogueLikeMode.Perks.ChanceAtPinAfterBattleRoom, 30, "ChanceAtPinAfterBattleRoom");

                    entry.unlockCondition = () => CAS.WorldRogueLikeData.henActiveFlags.Contains(FlagCodex.FlagID._Roguelike_PinsUnlocked);
                }
                else if (gameID == RogueLikeMode.Perks.MoreLoods)
                {
                    perkInfo = new RogueLikeMode.PerkInfo(RogueLikeMode.Perks.MoreLoods, 25, "MoreLoods");

                    entry.unlockCondition = () => CAS.WorldRogueLikeData.henActiveFlags.Contains(FlagCodex.FlagID._Roguelike_HasSeenLood);
                }
                else
                {
                    throw new Exception("Perk description unavailable.");
                }
            }

            entry.essenceCost = perkInfo.iEssenceCost;

            entry.textEntry = perkInfo.sNameHandle;
            entry.name = Globals.Game.EXT_GetMiscText("Menus", "Perks_Name_" + perkInfo.sNameHandle)?.sUnparsedFullLine;
            entry.description = Globals.Game.EXT_GetMiscText("Menus", "Perks_Description_" + perkInfo.sNameHandle)?.sUnparsedFullLine;

            entry.texturePath = null;

            return entry;
        }

        public static PinEntry ParsePin(PinCodex.PinType gameID)
        {
            PinEntry entry = new PinEntry()
            {
                Mod = Globals.Manager.VanillaMod,
                GameID = gameID,
                ModID = gameID.ToString()
            };

            PinInfo info = PinCodex.GetInfo(gameID);

            Enum.TryParse(info.sSymbol, out entry.pinSymbol);
            Enum.TryParse(info.sShape, out entry.pinShape);

            switch (info.sPalette)
            {
                case "Test1":
                    entry.pinColor = PinEntry.Color.YellowOrange;
                    break;
                case "Test2":
                    entry.pinColor = PinEntry.Color.Seagull;
                    break;
                case "Test3":
                    entry.pinColor = PinEntry.Color.Coral;
                    break;
                case "Test4":
                    entry.pinColor = PinEntry.Color.Conifer;
                    break;
                case "Test5":
                    entry.pinColor = PinEntry.Color.BilobaFlower;
                    break;
                case "TestLight":
                    entry.pinColor = PinEntry.Color.White;
                    break;
            }

            entry.isSticky = info.bSticky;
            entry.isBroken = info.bBroken;
            entry.description = info.sDescription;

            entry.conditionToDrop = null;

            // We have to manually set this, unfortunately.
            switch (gameID)
            {
                case PinCodex.PinType.VoodooDoll:
                    entry.conditionToDrop = () => CAS.NumberOfPlayers == 1;
                    break;
                case PinCodex.PinType.ThreeRedSmashBallsAutoSpawn:
                    entry.conditionToDrop = () =>
                    {
                        return
                            CAS.NumberOfPlayers == 1 &&
                            CAS.LocalPlayer.xEquipment.xWeapon != null &&
                            CAS.LocalPlayer.xEquipment.xWeapon.enWeaponCategory == WeaponInfo.WeaponCategory.TwoHanded;
                    };
                    break;
                case PinCodex.PinType.PotionDrinkFreezesClosestEnemy:
                case PinCodex.PinType.RefillAPotionAtStartEveryRoom:
                case PinCodex.PinType.DrinkingPotionSpawnsArrows:
                case PinCodex.PinType.DrinkingPotionGuaranteesCritNextSpell:
                case PinCodex.PinType.DrinkingAPotionGrantsLightningConduits:
                case PinCodex.PinType.PotionEffectsHaveDoubledDuration:
                    entry.conditionToDrop = () => CAS.WorldRogueLikeData.henActiveFlags.Contains(FlagCodex.FlagID._Roguelike_Improvement_Alchemist);
                    break;
                case PinCodex.PinType.DoubleLoods_LoodsHaveMoreHealth:
                    entry.conditionToDrop = () => CAS.WorldRogueLikeData.henActiveFlags.Contains(FlagCodex.FlagID._Roguelike_HasSeenLood) && CAS.NumberOfPlayers == 1;
                    break;
                case PinCodex.PinType.GainPowerIn_EvergrindFields:
                    entry.conditionToDrop = () => CAS.CurrentRegion == Level.WorldRegion.PillarMountains;
                    break;
                case PinCodex.PinType.GainPowerIn_PumpkinWoods:
                    entry.conditionToDrop = () => CAS.CurrentRegion == Level.WorldRegion.EvergrindEast;
                    break;
                case PinCodex.PinType.GainPowerIn_FlyingFortress:
                    entry.conditionToDrop = () => CAS.CurrentRegion == Level.WorldRegion.HalloweenForest;
                    break;
                case PinCodex.PinType.GainPowerIn_Seasonne:
                    entry.conditionToDrop = () => CAS.CurrentRegion == Level.WorldRegion.FlyingFortress;
                    break;
                case PinCodex.PinType.GainPowerIn_SeasonTemple:
                    entry.conditionToDrop = () => CAS.CurrentRegion == Level.WorldRegion.Winterland;
                    break;
                case PinCodex.PinType.GainPowerIn_MtBloom:
                    entry.conditionToDrop = () => CAS.CurrentRegion == Level.WorldRegion.SeasonTemple;
                    break;
                case PinCodex.PinType.GainPowerIn_TaiMing:
                    entry.conditionToDrop = () => CAS.CurrentRegion == Level.WorldRegion.MtBloom;
                    break;
                case PinCodex.PinType.GainPowerIn_Desert:
                    entry.conditionToDrop = () => CAS.CurrentRegion == Level.WorldRegion.TimeTemple;
                    break;
                case PinCodex.PinType.GainPowerIn_LostShip:
                    entry.conditionToDrop = () => CAS.CurrentRegion == Level.WorldRegion.Desert;
                    break;
            }

            entry.createCollectionEntry = GameObjectStuff.GetOriginalPinCollection().Contains(gameID);

            return entry;
        }

        public static QuestEntry ParseQuest(QuestCodex.QuestID gameID)
        {
            QuestEntry entry = new QuestEntry()
            {
                Mod = Globals.Manager.VanillaMod,
                GameID = gameID,
                ModID = gameID.ToString()
            };

            entry.vanilla = OriginalMethods.GetQuestDescription(gameID);

            entry.name = Globals.Game.EXT_GetMiscText("Quests", entry.vanilla.sQuestNameReference)?.sUnparsedFullLine;
            entry.description = Globals.Game.EXT_GetMiscText("Quests", entry.vanilla.sDescriptionReference)?.sUnparsedFullLine;
            entry.summary = Globals.Game.EXT_GetMiscText("Quests", entry.vanilla.sSummaryReference)?.sUnparsedFullLine;

            entry.constructor = null;  // As usual, for vanilla entries, will construct from vanilla methods if no replacement is set

            return entry;
        }

        public static SpellEntry ParseSpell(SpellCodex.SpellTypes gameID)
        {
            SpellEntry entry = new SpellEntry()
            {
                Mod = Globals.Manager.VanillaMod,
                GameID = gameID,
                ModID = gameID.ToString()
            };

            entry.isMagicSkill = OriginalMethods.SpellIsMagicSkill(gameID);
            entry.isMeleeSkill = OriginalMethods.SpellIsMeleeSkill(gameID);
            entry.isUtilitySkill = OriginalMethods.SpellIsUtilitySkill(gameID);

            entry.builder = null;

            return entry;
        }

        public static StatusEffectEntry ParseStatusEffect(BaseStats.StatusEffectSource gameID)
        {
            StatusEffectEntry entry = new StatusEffectEntry()
            {
                Mod = Globals.Manager.VanillaMod,
                GameID = gameID,
                ModID = gameID.ToString()
            };

            entry.texturePath = null;

            return entry;
        }

        public static WorldRegionEntry ParseWorldRegion(Level.WorldRegion gameID)
        {
            WorldRegionEntry entry = new WorldRegionEntry()
            {
                Mod = Globals.Manager.VanillaMod,
                GameID = gameID,
                ModID = gameID.ToString()
            };

            // Currently has no significant code.

            return entry;
        }
    }
}
