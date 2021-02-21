using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace SoG.GrindScript
{

    /// <summary>
    /// Helper class that stores additional information for modded enemies.
    /// Used internally by GrindScript.
    /// </summary>
    public class ModEnemyData
    {
        public EnemyCodex.EnemyTypes enType;

        public EnemyBuilderPrototype InstanceBuilder { get; set; }

        public EnemyBuilderPrototype DifficultyBuilder { get; set; }

        public EnemyBuilderPrototype EliteBuilder { get; set; }

        public bool bGrantEliteBonuses = true;

        public ModEnemyData(EnemyCodex.EnemyTypes enType)
        {
            this.enType = enType;
        }
    }

    public static class EnemyHelper
    {
        // Static setup methods

        /// <summary> Creates a new EnemyDescription that can be used by the game. </summary>
        public static EnemyDescription CreateEnemyDescription(string sName, int iLevel, int iBaseHealth)
        {
            string sBaseEntryName = sName.Replace(" ", "");

            EnemyDescription xDesc = new EnemyDescription(ModLibrary.EnemyTypesNext, sBaseEntryName + "_Name", iLevel, iBaseHealth)
            {
                sOnDeathSound = "",
                sOnHitSound = "",
                sFullName = sName
            };

            EnemyCodex.denxDescriptionDict[xDesc.enType] = xDesc;
            ModLibrary.EnemyDetails.Add(xDesc.enType, new ModEnemyData(xDesc.enType));

            Ui.AddMiscText("Enemies", sBaseEntryName + "_Name", sName);

            return xDesc;
        }

        // EnemyTypes extensions

        public static bool IsSoGEnemy(this EnemyCodex.EnemyTypes enType)
        {
            return Enum.IsDefined(typeof(EnemyCodex.EnemyTypes), enType);
        }

        public static bool IsModEnemy(this EnemyCodex.EnemyTypes enType)
        {
            return enType >= ModLibrary.EnemyTypesStart && enType < ModLibrary.EnemyTypesNext;
        }

        // EnemyDescription extensions

        /// <summary> Spawns an enemy at the target player's position. </summary>
        /// <remarks> Use this for testing purposes; spawning enemies next to players isn't a great idea.</remarks>
        public static Enemy SpawnEnemy(this EnemyDescription xDesc, PlayerView xTarget)
        {
            PlayerEntity xEntity = xTarget.xEntity;

            return Utils.GetTheGame()._EntityMaster_AddEnemy(xDesc.enType, xEntity.xTransform.v2Pos, xEntity.xCollisionComponent.ibitCurrentColliderLayer, xEntity.xRenderComponent.fVirtualHeight);
        }

        /// <summary> Spawns an enemy at the target location. </summary>
        public static Enemy SpawnEnemy(this EnemyDescription xDesc, Vector2 v2Pos, float fVirtualHeight, int iColliderLayer)
        {
            return Utils.GetTheGame()._EntityMaster_AddEnemy(xDesc.enType, v2Pos, iColliderLayer, fVirtualHeight);
        }

        /// <summary> Sets the sound cues for hitting and killing an enemy. </summary>
        public static void SetCommonSounds(this EnemyDescription xDesc, string sOnHitSound, string sOnDeathSound)
        {
            xDesc.sOnHitSound = sOnHitSound;
            xDesc.sOnDeathSound = sOnDeathSound;
        }

        /// <summary> 
        /// Sets the size of an enemy, and approximately where the enemy's "center" is. 
        /// These are used by some game objects (such as Guardian Shields).
        /// </summary>
        public static void SetSizeAndOffset(this EnemyDescription xDesc, Vector2 v2Size, Vector2 v2OffsetToMid)
        {
            xDesc.v2ApproximateOffsetToMid = v2OffsetToMid;
            xDesc.v2ApproximateSize = v2Size;
        }

        /// <summary> Adds an item that can be dropped when the enemy is killed. </summary>
        /// <remarks> This method can be called multiple times to have multiple item drops of the same type, but with different drop chance or roll count. </remarks>
        /// <param name="fDropChance"> Chance for an item to drop. 100.0f translates to 100% chance, and 0f to 0% respectively. </param>
        /// <param name="iRolls"> Number of items to roll for. Setting this to 4 would mean up to 4 enType items can drop, each with fDropChance chance. </param>
        public static void AddLoot(this EnemyDescription xDesc, ItemCodex.ItemTypes enType, float fDropChance = 100.0f, int iRolls = 1)
        {
            xDesc.lxLootTable.Add(new DropChance((int)(fDropChance * 1000.0f), enType, iRolls));
        }

        /// <summary>
        /// Sets whenever the elite enemy benefits from generic elite bonuses.
        /// <para> If true, elites will gain HP scaling based on player count, more money drops, will grant more EXP, etc. </para>
        /// <remarks> An example of enemies with no elite "bonus" are empowered Season Knights and Season Mages. </remarks>
        /// </summary>
        public static void SetEliteBonuses(this EnemyDescription xDesc, bool bGrantBonuses)
        {
            if(!xDesc.enType.IsModEnemy())
            {
                throw new ArgumentException("Provided enType is not a mod enemy.");
            }

            ModLibrary.EnemyDetails[xDesc.enType].bGrantEliteBonuses = bGrantBonuses;
        }

        /// <summary>
        /// Sets the delegate used to create new enemy instances of type xDesc.enType.
        /// This delegate will be called every time a new enemy instance is created.
        /// <para> 
        /// The delegate is where you should set stats that don't scale with Game Difficulty, setup animations, and stuff like that. 
        /// Also, the delegate is where you must set the enemy's xBehaviour to a new Behaviour object. Typically, that would be the new AI you created for the modded enemy.
        /// </para>
        /// </summary>
        public static void SetInstanceBuilder(this EnemyDescription xDesc, EnemyBuilderPrototype xProto)
        {
            if (!xDesc.enType.IsModEnemy())
            {
                throw new ArgumentException("Provided enType is not a mod enemy.");
            }

            ModLibrary.EnemyDetails[xDesc.enType].InstanceBuilder = xProto;
        }

        /// <summary>
        /// Sets the delegate used to scale stats for enemy instances based on Game Difficulty.
        /// This delegate is usually called when a new enemy is created. It's also called for existing enemies when Game Difficulty is changed in Story Mode.
        /// <para> 
        /// The delegate is where you should set stats that scale with Game Difficulty.
        /// You can also modify the enemy's Animations to be faster or whatever, like what SoG does for some enemies.
        /// </para>
        /// </summary>
        public static void SetDifficultyBuilder(this EnemyDescription xDesc, EnemyBuilderPrototype xProto)
        {
            if (!xDesc.enType.IsModEnemy())
            {
                throw new ArgumentException("Provided enType is not a mod enemy.");
            }

            ModLibrary.EnemyDetails[xDesc.enType].DifficultyBuilder = xProto;
        }

        /// <summary>
        /// Sets the delegate used to scale stats for enemy instances when they're turned into Elites.
        /// This delegate is usually called when a new enemy is created as Elite. It's also called whenever an existing enemy is promoted to Elite status (Skeleton Mages, Bishop in Arcade, etc.)
        /// <para> 
        /// The delegate is where you should modify the enemy's stats so that Elite enemies are more dangerous.
        /// You can also modify the Elite enemy's Animations to be faster or whatever, like what SoG does for some enemies.
        /// </para>
        /// <para> Unlike the other two delegates, you can choose to skip this one if you wish. Doing so will cause the enemy to have no elite variant. </para>
        /// </summary>
        public static void SetEliteBuilder(this EnemyDescription xDesc, EnemyBuilderPrototype xProto)
        {
            if (!xDesc.enType.IsModEnemy())
            {
                throw new ArgumentException("Provided enType is not a mod enemy.");
            }

            ModLibrary.EnemyDetails[xDesc.enType].EliteBuilder = xProto;
        }

        // 
        // Harmony Library Patches
        // 

        /// <summary> Patches GetEnemyInstance so that SoG can create modded enemy instances. </summary>
        private static bool GetEnemyInstance_PrefixPatch(ref Enemy __result, EnemyCodex.EnemyTypes enType, Level.WorldRegion enOverrideContent)
        {
            if(!enType.IsModEnemy())
            {
                return true; // Executes original method
            }

            EnemyDescription xDesc = EnemyCodex.denxDescriptionDict[enType];

            __result = new Enemy()
            {
                xEnemyDescription = xDesc,
                enType = enType
            };

            __result.xRenderComponent.xOwnerObject = __result;

            ModEnemyData xModData = ModLibrary.EnemyDetails[enType];

            xModData.InstanceBuilder?.Invoke(__result);

            __result.xBaseStats.iLevel = __result.xEnemyDescription.iLevel;
            __result.xBaseStats.iHP = (__result.xBaseStats.iBaseMaxHP = __result.xEnemyDescription.iMaxHealth);
            if (__result.xEnemyDescription.enCategory == EnemyDescription.Category.Regular)
            {
                __result.rcRegularHPRenderComponent = new RegularEnemyHPRenderComponent(__result);
            }
            __result.xRenderComponent.bReSortHeight = true;
            __result.xRenderComponent.GetCurrentAnimation().Reset();
            foreach (DropChance xDrop in __result.xEnemyDescription.lxLootTable)
            {
                __result.lxLootTable.Add(new DropChance(xDrop.iChance, xDrop.enItemToDrop, xDrop.iRolls));
                ItemCodex.GetItemDescription(xDrop.enItemToDrop);
            }

            return false; // Skips original method
        }

        /// <summary> Patches _Enemy_AdjustForDifficulty so that SoG can scale modded enemy instances based on difficulty. </summary>
        private static bool _Enemy_AdjustForDifficulty_PrefixPatch(Enemy xEn)
        {
            if (!xEn.enType.IsModEnemy())
            {
                return true; // Executes original method
            }

            if (xEn.xEnemyDescription.enCategory != 0)
            {
                xEn.xBehaviour.bIgnorePetsDuringTargetSearch = true;
            }

            ModLibrary.EnemyDetails[xEn.enType].DifficultyBuilder?.Invoke(xEn);

            return false; // Skips original method
        }


        // TO DO: Consider transforming this into a transpile

        /// <summary> Patches _Enemy_MakeElite so that SoG can scale modded enemy instances if they're elite. </summary>
        private static bool _Enemy_MakeElite_PrefixPatch(ref bool __result, Enemy xEn, bool bAttachEffect)
        {
            __result = false;

            if (!xEn.enType.IsModEnemy())
            {
                return true; // Executes original method
            }

            if (xEn.xBehaviour.bIsElite)
            {
                return false; // Skips original method (No changes needed)
            }

            ModEnemyData xModData = ModLibrary.EnemyDetails[xEn.enType];

            if (xModData.EliteBuilder != null)
            {
                xModData.EliteBuilder(xEn);
                __result = true;
            }

            if (__result)
            {
                Game1 xGame = Utils.GetTheGame();
                xEn.xBehaviour.bIsElite = true;
                if (xGame.xStateMaster.enGameMode == StateMaster.GameModes.RogueLike && RogueLikeMode.randLevelLoadStuff.Next(Math.Max(20 - xGame.xGameSessionData.xRogueLikeSession.iCurrentFloor * 2, 5)) == 0)
                {
                    xEn.lxLootTable.Add(new DropChance(100000, ItemCodex.ItemTypes._KeyItem_RoguelikeEssence));
                }
                if (xModData.bGrantEliteBonuses)
                {
                    xEn.xBaseStats.iBaseMaxHP = (int)((float)xEn.xBaseStats.iBaseMaxHP * (0.75f + 0.25f * (float)CAS.NumberOfPlayers));
                    xEn.xBaseStats.iHP = xEn.xBaseStats.iMaxHP;
                    foreach (PlayerView xView in xGame.dixPlayers.Values)
                    {
                        if (xView.xEquipment.HasBadge(PinCodex.PinType.ElitesIncreaseATKMATKPermanentlyButAlsoStronger))
                        {
                            xView.xEntity.xBaseStats.dxfModifiedResistanceVsTarget[xEn] = 1.5f;
                        }
                    }
                }
                if (CAS.GameMode == StateMaster.GameModes.Story)
                {
                    if (xModData.bGrantEliteBonuses && !xEn.bSuppressEliteDropBonus)
                    {
                        xEn.iBonusMoney = xEn.xBaseStats.iLevel * 20;
                        foreach (DropChance x in xEn.lxLootTable)
                        {
                            x.iChance *= 5;
                        }
                        xEn.xBaseStats.iLevel += 3;
                    }
                    xEn.sOverrideName = RogueLikeMode.GetSuperSuffix(xEn.xEnemyDescription.sFullName, xEn.enType);
                }
                if (bAttachEffect)
                {
                    SortedAnimated xEffe = xGame._EffectMaster_AddEffect(new SortedAnimated(xEn.xTransform.v2Pos, SortedAnimated.SortedAnimatedEffects.BuffEffectEliteEnemy)) as SortedAnimated;
                    xEffe.xRenderComponent.xTransform = xEn.xRenderComponent.xTransform;
                    xEn.xEliteEffect = xEffe;
                    if (xEn.enType == EnemyCodex.EnemyTypes.Desert_Solem)
                    {
                        xEffe.xRenderComponent.fScale = 1.5f;
                    }
                    xGame._EntityMaster_AddWatcher(new Watchers.EnemyAttachedEffectWatcher(xEn, xEffe));
                }
            }

            return false; // Skips original method
        }
    }
}
