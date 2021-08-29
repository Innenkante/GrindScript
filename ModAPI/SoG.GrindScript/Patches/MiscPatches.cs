using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using LevelLoading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SoG.Modding.Core;
using SoG.Modding.Extensions;
using SoG.Modding.Utils;

namespace SoG.Modding.Patches
{
    using CodeList = System.Collections.Generic.IEnumerable<HarmonyLib.CodeInstruction>;

    /// <summary>
    /// Contains patches for miscellaneous patches.
    /// Remark: If you see a class with a ton of patches, 
    /// it should be moved to its own patch collection class.
    /// </summary>

    [HarmonyPatch]
    internal static class MiscPatches
    {
        /// <summary>
        /// Inserts custom curses in the Curse shop menu.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ShopMenu.TreatCurseMenu), "FillCurseList")]
        internal static void PostFillCurseList(ShopMenu.TreatCurseMenu __instance)
        {
            foreach (var kvp in Globals.API.Registry.Library.Curses)
            {
                if (!kvp.Value.Config.IsTreat)
                    __instance.lenTreatCursesAvailable.Add(kvp.Value.GameID);
            }
        }

        /// <summary>
        /// Inserts custom curses in the Treat shop menu.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ShopMenu.TreatCurseMenu), "FillTreatList")]
        internal static void PostFillTreatList(ShopMenu.TreatCurseMenu __instance)
        {
            foreach (var kvp in Globals.API.Registry.Library.Curses)
            {
                if (kvp.Value.Config.IsTreat)
                    __instance.lenTreatCursesAvailable.Add(kvp.Value.GameID);
            }
        }


        /// <summary>
        /// Inserts custom perks in the Perk shop menu.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(RogueLikeMode.PerkInfo), "Init")]
        internal static void PostPerkListInit()
        {
            foreach (var perk in Globals.API.Registry.Library.Perks.Values)
                RogueLikeMode.PerkInfo.lxAllPerks.Add(new RogueLikeMode.PerkInfo(perk.GameID, perk.Config.EssenceCost, perk.TextEntry));
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ItemCodex), "GetItemDescription")]
        internal static bool OnGetItemDescription(ref ItemDescription __result, ItemCodex.ItemTypes enType)
        {
            if (!enType.IsFromMod())
                return true;

            ModItemEntry entry = Globals.API.Registry.Library.Items[enType];
            __result = entry.ItemData;
            Tools.TryLoadTex(entry.Config.IconPath, entry.Config.Manager, out __result.txDisplayImage);

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ItemCodex), "GetItemInstance")]
        internal static bool OnGetItemInstance(ref Item __result, ItemCodex.ItemTypes enType)
        {
            if (!enType.IsFromMod())
                return true;

            ModItemEntry entry = Globals.API.Registry.Library.Items[enType];
            string trueShadowTex = entry.Config.ShadowPath != "" ? entry.Config.ShadowPath : "Items/DropAppearance/hartass02";
            ItemDescription xDesc = entry.ItemData;

            __result = new Item()
            {
                enType = enType,
                sFullName = xDesc.sFullName,
                bGiveToServer = xDesc.lenCategory.Contains(ItemCodex.ItemCategories.GrantToServer)
            };

            Tools.TryLoadTex(entry.Config.IconPath, entry.Config.Manager, out __result.xRenderComponent.txTexture);
            Tools.TryLoadTex(trueShadowTex, entry.Config.Manager, out __result.xRenderComponent.txShadowTexture);

            __result.xCollisionComponent.xMovementCollider = new SphereCollider(10f, Vector2.Zero, __result.xTransform, 1f, __result) { bCollideWithFlat = true };

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(EquipmentCodex), "GetArmorInfo")]
        internal static bool OnGetEquipmentInfo_0(ref EquipmentInfo __result, ItemCodex.ItemTypes enType)
        {
            if (!enType.IsFromMod())
                return true;

            __result = Globals.API.Registry.Library.Items[enType].EquipData;

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(EquipmentCodex), "GetAccessoryInfo")]
        internal static bool OnGetEquipmentInfo_1(ref EquipmentInfo __result, ItemCodex.ItemTypes enType) => OnGetEquipmentInfo_0(ref __result, enType);

        [HarmonyPrefix]
        [HarmonyPatch(typeof(EquipmentCodex), "GetShieldInfo")]
        internal static bool OnGetEquipmentInfo_2(ref EquipmentInfo __result, ItemCodex.ItemTypes enType) => OnGetEquipmentInfo_0(ref __result, enType);

        [HarmonyPrefix]
        [HarmonyPatch(typeof(EquipmentCodex), "GetShoesInfo")]
        internal static bool OnGetEquipmentInfo_3(ref EquipmentInfo __result, ItemCodex.ItemTypes enType) => OnGetEquipmentInfo_0(ref __result, enType);

        [HarmonyPrefix]
        [HarmonyPatch(typeof(FacegearCodex), "GetHatInfo")]
        internal static bool OnGetFacegearInfo(ref FacegearInfo __result, ItemCodex.ItemTypes enType)
        {
            if (!enType.IsFromMod())
                return true;

            ModItemEntry entry = Globals.API.Registry.Library.Items[enType];
            ContentManager manager = entry.Config.Manager;
            string path = entry.Config.EquipResourcePath;

            __result = entry.EquipData as FacegearInfo;

            Tools.TryLoadTex(Path.Combine(path, "Up"), manager, out __result.atxTextures[0]);
            Tools.TryLoadTex(Path.Combine(path, "Right"), manager, out __result.atxTextures[1]);
            Tools.TryLoadTex(Path.Combine(path, "Down"), manager, out __result.atxTextures[2]);
            Tools.TryLoadTex(Path.Combine(path, "Left"), manager, out __result.atxTextures[3]);

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HatCodex), "GetHatInfo")]
        internal static bool OnGetHatInfo(ref HatInfo __result, ItemCodex.ItemTypes enType)
        {
            if (!enType.IsFromMod())
                return true;

            ModItemEntry entry = Globals.API.Registry.Library.Items[enType];
            ContentManager manager = entry.Config.Manager;
            string path = entry.Config.EquipResourcePath;

            __result = entry.EquipData as HatInfo;

            Tools.TryLoadTex(Path.Combine(path, "Up"), manager, out __result.xDefaultSet.atxTextures[0]);
            Tools.TryLoadTex(Path.Combine(path, "Right"), manager, out __result.xDefaultSet.atxTextures[1]);
            Tools.TryLoadTex(Path.Combine(path, "Down"), manager, out __result.xDefaultSet.atxTextures[2]);
            Tools.TryLoadTex(Path.Combine(path, "Left"), manager, out __result.xDefaultSet.atxTextures[3]);

            foreach (var kvp in __result.denxAlternateVisualSets)
            {
                string altPath = Path.Combine(path, entry.HatAltSetResourcePaths[kvp.Key]);

                Tools.TryLoadTex(Path.Combine(altPath, "Up"), manager, out kvp.Value.atxTextures[0]);
                Tools.TryLoadTex(Path.Combine(altPath, "Right"), manager, out kvp.Value.atxTextures[1]);
                Tools.TryLoadTex(Path.Combine(altPath, "Down"), manager, out kvp.Value.atxTextures[2]);
                Tools.TryLoadTex(Path.Combine(altPath, "Left"), manager, out kvp.Value.atxTextures[3]);
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(WeaponCodex), "GetWeaponInfo")]
        internal static bool OnGetWeaponInfo(ref WeaponInfo __result, ItemCodex.ItemTypes enType)
        {
            if (!enType.IsFromMod())
                return true;

            __result = Globals.API.Registry.Library.Items[enType].EquipData as WeaponInfo;

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(WeaponAssets.WeaponContentManager), "LoadBatch", new Type[] { typeof(Dictionary<ushort, string>) })]
        internal static bool OnLoadBatch(ref Dictionary<ushort, string> dis, WeaponAssets.WeaponContentManager __instance)
        {
            ItemCodex.ItemTypes type = __instance.enType;

            if (!type.IsFromMod())
                return true;

            ModItemEntry entry = Globals.API.Registry.Library.Items[type];
            ContentManager manager = entry.Config.Manager;
            bool oneHanded = (entry.EquipData as WeaponInfo).enWeaponCategory == WeaponInfo.WeaponCategory.OneHanded;

            if (manager != null)
                __instance.contWeaponContent.RootDirectory = manager.RootDirectory;

            foreach (KeyValuePair<ushort, string> kvp in dis)
            {
                string resourcePath = Globals.API.Registry.Library.Items[type].Config.EquipResourcePath;
                string texPath = kvp.Value.Replace($"Weapons/{resourcePath}/", "");

                if (oneHanded)
                {
                    texPath = texPath.Replace("Sprites/Heroes/OneHanded/", resourcePath + "/");
                    texPath = texPath.Replace("Sprites/Heroes/Charge/OneHand/", resourcePath + "/1HCharge/");
                }
                else
                {
                    texPath = texPath.Replace("Sprites/Heroes/TwoHanded/", resourcePath + "/");
                    texPath = texPath.Replace("Sprites/Heroes/Charge/TwoHand/", resourcePath + "/2HCharge/");
                }

                Tools.TryLoadTex(texPath, __instance.contWeaponContent, out Texture2D tex);
                __instance.ditxWeaponTextures.Add(kvp.Key, tex);
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(LevelBlueprint), "GetBlueprint")]
        internal static bool OnGetLevelBlueprint(ref LevelBlueprint __result, Level.ZoneEnum enZoneToGet)
        {
            if (!enZoneToGet.IsFromMod())
                return true;

            LevelBlueprint bprint = new LevelBlueprint();

            bprint.CheckForConsistency();

            ModLevelEntry entry = Globals.API.Registry.Library.Levels[enZoneToGet];

            try
            {
                entry.Config.Builder?.Invoke(bprint);
            }
            catch (Exception e)
            {
                Globals.Logger.Error($"Builder threw an exception for level {enZoneToGet}! Exception: {e}");
                bprint = new LevelBlueprint();
            }

            bprint.CheckForConsistency(true);

            // Enforce certain values

            bprint.enRegion = entry.Config.WorldRegion;
            bprint.enZone = entry.GameID;
            bprint.sDefaultMusic = ""; // TODO Custom music
            bprint.sDialogueFiles = ""; // TODO Dialogue Files
            bprint.sMenuBackground = "bg01_mountainvillage"; // TODO Proper custom backgrounds. Transpiling _Level_Load is a good idea.
            bprint.sZoneName = ""; // TODO Zone titles


            // Loader setup

            Loader.afCurrentHeightLayers = new float[bprint.aiLayerDefaultHeight.Length];
            for (int i = 0; i < bprint.aiLayerDefaultHeight.Length; i++)
                Loader.afCurrentHeightLayers[i] = bprint.aiLayerDefaultHeight[i];

            Loader.lxCurrentSC = bprint.lxInvisibleWalls;

            // Return from method

            __result = bprint;
            return false;
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(EnemyCodex), "GetEnemyDescription")]
        internal static bool OnGetEnemyDescription(ref EnemyDescription __result, EnemyCodex.EnemyTypes enType)
        {
            if (!enType.IsFromMod())
                return true;

            __result = Globals.API.Registry.Library.Enemies[enType].EnemyData;

            return false;
        }

        /// <summary>
        /// Implements custom enemy construction by transpiling the second part of GetEnemyInstance.
        /// (Note that our IDs will always trigger the condition for "CacuteForward" version to be called)
        /// </summary>
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(EnemyCodex), "GetEnemyInstance_CacuteForward")]
        internal static CodeList GetEnemyInstanceTranspiler(CodeList code, ILGenerator gen)
        {
            // Assert to check if underlying method hasn't shifted heavily
            OpCode op = OpCodes.Nop;
            Debug.Assert(PatchTools.TryILAt(code, 20, out op) && op == OpCodes.Ldstr, "GetEnemyInstance transpiler is invalid!");

            var insert = new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Call, typeof(HelperCallbacks).GetPrivateStaticMethod(nameof(HelperCallbacks.InGetEnemyInstance))),
                new CodeInstruction(OpCodes.Stloc_0) // Store returned enemy
            };

            return PatchTools.InsertAt(code, insert, 20 + 2);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CardCodex), "GetIllustrationPath")]
        public static bool OnGetIllustrationPatch(ref string __result, EnemyCodex.EnemyTypes enEnemy)
        {
            if (!enEnemy.IsFromMod())
            {
                return true;
            }

            __result = Globals.API.Registry.Library.Enemies[enEnemy].Config.CardIllustrationPath;

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(EnemyCodex), "GetEnemyDefaultAnimation")]
        public static bool OnGetEnemyDefaultAnimation(ref Animation __result, EnemyCodex.EnemyTypes enType, ContentManager Content)
        {
            if (!enType.IsFromMod())
            {
                return true;
            }

            __result = Globals.API.Registry.Library.Enemies[enType].Config.DefaultAnimation?.Invoke(Content);

            if (__result == null)
            {
                __result = new Animation(1, 0, RenderMaster.txNullTex, Vector2.Zero);
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(EnemyCodex), "GetEnemyDisplayIcon")]
        public static bool OnGetEnemyDisplayIcon(ref Texture2D __result, EnemyCodex.EnemyTypes enType, ContentManager Content)
        {
            if (!enType.IsFromMod())
            {
                return true;
            }

            __result = Globals.API.Registry.Library.Enemies[enType].Config.DisplayIcon?.Invoke(Content);

            if (__result == null)
            {
                __result = RenderMaster.txNullTex;
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(EnemyCodex), "GetEnemyLocationPicture")]
        public static bool OnGetEnemyLocationPicture(ref Texture2D __result, EnemyCodex.EnemyTypes enType, ContentManager Content)
        {
            if (!enType.IsFromMod())
            {
                return true;
            }

            __result = Globals.API.Registry.Library.Enemies[enType].Config.DisplayBackground?.Invoke(Content);

            if (__result == null)
            {
                __result = RenderMaster.txNullTex;
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Quests.QuestCodex), "GetQuestDescription")]
        public static bool OnGetQuestDescription(ref Quests.QuestDescription __result, Quests.QuestCodex.QuestID p_enID)
        {
            if (!p_enID.IsFromMod())
            {
                return true;
            }

            __result = Globals.API.Registry.Library.Quests[p_enID].QuestData;

            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Quests.QuestCodex), "GetQuestInstance")]
        public static void PostGetQuestInstance(ref Quests.Quest __result, Quests.QuestCodex.QuestID p_enID)
        {
            if (!p_enID.IsFromMod())
            {
                return;
            }

            Globals.API.Registry.Library.Quests[p_enID].Config.Constructor?.Invoke(__result);

            __result.xReward = Globals.API.Registry.Library.Quests[p_enID].QuestData.xReward;
        }
    }
}
