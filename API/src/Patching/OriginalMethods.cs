using HarmonyLib;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Quests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SoG.ShopMenu;

namespace SoG.Modding.Patching
{
    /// <summary>
    /// Contains a collection of reverse patches created from the original game methods.
    /// Calling method from this class is almost the same as if you called the vanilla ones.
    /// That is, no prefix / postfix / whatever patches are applied.
    /// </summary>
    [HarmonyPatch]
    internal static class OriginalMethods
    {
        #region Curse related

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Game1), nameof(Game1._RogueLike_GetTreatCurseTexture))]
        internal static Texture2D _RogueLike_GetTreatCurseTexture(Game1 __instance, RogueLikeMode.TreatsCurses enTreat)
        {
            throw new NotImplementedException("Stub method.");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Game1), nameof(Game1._RogueLike_GetTreatCurseInfo))]
        internal static void _RogueLike_GetTreatCurseInfo(Game1 __instance, RogueLikeMode.TreatsCurses enTreatCurse, out string sNameHandle, out string sDescriptionHandle, out float fScoreModifier)
        {
            throw new NotImplementedException("Stub method.");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(TreatCurseMenu), nameof(TreatCurseMenu.FillCurseList))]
        internal static void FillCurseList(TreatCurseMenu __instance)
        {
            throw new NotImplementedException("Stub method.");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(TreatCurseMenu), nameof(TreatCurseMenu.FillTreatList))]
        internal static void FillTreatList(TreatCurseMenu __instance)
        {
            throw new NotImplementedException("Stub method.");
        }

        #endregion

        #region Enemy related

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(CardCodex), nameof(CardCodex.GetIllustrationPath))]
        public static string GetIllustrationPath(EnemyCodex.EnemyTypes enEnemy)
        {
            throw new NotImplementedException("Stub method.");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(EnemyCodex), nameof(EnemyCodex.GetEnemyDescription))]
        internal static EnemyDescription GetEnemyDescription(EnemyCodex.EnemyTypes enType)
        {
            throw new NotImplementedException("Stub method.");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(EnemyCodex), nameof(EnemyCodex.GetEnemyDefaultAnimation))]
        public static Animation GetEnemyDefaultAnimation(EnemyCodex.EnemyTypes enType, ContentManager Content)
        {
            throw new NotImplementedException("Stub method.");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(EnemyCodex), nameof(EnemyCodex.GetEnemyDisplayIcon))]
        public static Texture2D GetEnemyDisplayIcon(EnemyCodex.EnemyTypes enType, ContentManager Content, bool bBigIfPossible)
        {
            throw new NotImplementedException("Stub method.");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(EnemyCodex), nameof(EnemyCodex.GetEnemyLocationPicture))]
        public static Texture2D GetEnemyLocationPicture(EnemyCodex.EnemyTypes enType, ContentManager Content)
        {
            throw new NotImplementedException("Stub method.");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Game1), nameof(Game1._Enemy_AdjustForDifficulty))]
        internal static void _Enemy_AdjustForDifficulty(Game1 __instance, Enemy xEn)
        {
            throw new NotImplementedException("Stub method.");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Game1), nameof(Game1._Enemy_MakeElite))]
        internal static bool _Enemy_MakeElite(Game1 __instance, Enemy xEn, bool bAttachEffect)
        {
            throw new NotImplementedException("Stub method.");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(EnemyCodex), nameof(EnemyCodex.GetEnemyInstance_CacuteForward))]
        internal static Enemy GetEnemyInstance_CacuteForward(EnemyCodex.EnemyTypes enType, Level.WorldRegion enOverrideContent)
        {
            throw new NotImplementedException("Stub method.");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(EnemyCodex), nameof(EnemyCodex.GetEnemyInstance))]
        internal static Enemy GetEnemyInstance(EnemyCodex.EnemyTypes enType, Level.WorldRegion enOverrideContent)
        {
            IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var original = AccessTools.Method(typeof(EnemyCodex), nameof(EnemyCodex.GetEnemyInstance_CacuteForward));
                var replacement = AccessTools.Method(typeof(OriginalMethods), nameof(OriginalMethods.GetEnemyInstance_CacuteForward));

                return instructions.MethodReplacer(original, replacement);
            }

            _ = Transpiler(null);
            throw new NotImplementedException("Stub method.");
        }

        #endregion

        #region Item related

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(ItemCodex), nameof(ItemCodex.GetItemDescription_PostSpecial))]
        internal static ItemDescription GetItemDescription_PostSpecial(ItemCodex.ItemTypes enType)
        {
            throw new NotImplementedException("Stub method.");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(ItemCodex), nameof(ItemCodex.GetItemDescription))]
        internal static ItemDescription GetItemDescription(ItemCodex.ItemTypes enType)
        {
            IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var original = AccessTools.Method(typeof(ItemCodex), nameof(ItemCodex.GetItemDescription_PostSpecial));
                var replacement = AccessTools.Method(typeof(OriginalMethods), nameof(OriginalMethods.GetItemDescription_PostSpecial));

                return instructions.MethodReplacer(original, replacement);
            }

            _ = Transpiler(null);
            throw new NotImplementedException("Stub method.");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(EquipmentCodex), nameof(EquipmentCodex.GetAccessoryInfo))]
        internal static EquipmentInfo GetAccessoryInfo(ItemCodex.ItemTypes enType)
        {
            throw new NotImplementedException("Stub method.");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(EquipmentCodex), nameof(EquipmentCodex.GetArmorInfo))]
        internal static EquipmentInfo GetArmorInfo(ItemCodex.ItemTypes enType)
        {
            throw new NotImplementedException("Stub method.");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(EquipmentCodex), nameof(EquipmentCodex.GetShieldInfo))]
        internal static EquipmentInfo GetShieldInfo(ItemCodex.ItemTypes enType)
        {
            throw new NotImplementedException("Stub method.");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(EquipmentCodex), nameof(EquipmentCodex.GetShoesInfo))]
        internal static EquipmentInfo GetShoesInfo(ItemCodex.ItemTypes enType)
        {
            throw new NotImplementedException("Stub method.");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(FacegearCodex), nameof(FacegearCodex.GetHatInfo))]
        internal static FacegearInfo GetFacegearInfo(ItemCodex.ItemTypes enType)
        {
            throw new NotImplementedException("Stub method.");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(HatCodex), nameof(HatCodex.GetHatInfo))]
        internal static HatInfo GetHatInfo(ItemCodex.ItemTypes enType)
        {
            throw new NotImplementedException("Stub method.");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(WeaponCodex), nameof(WeaponCodex.GetWeaponInfo))]
        internal static WeaponInfo GetWeaponInfo(ItemCodex.ItemTypes enType)
        {
            throw new NotImplementedException("Stub method.");
        }

        #endregion

        #region

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(LevelBlueprint), nameof(LevelBlueprint.GetBlueprint))]
        internal static LevelBlueprint GetBlueprint(Level.ZoneEnum enZoneToGet)
        {
            throw new NotImplementedException("Stub method.");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Game1), nameof(Game1._LevelLoading_DoStuff))]
        public static void _LevelLoading_DoStuff(Game1 __instance, Level.ZoneEnum enLevel, bool bStaticOnly)
        {
            throw new NotImplementedException("Stub method.");
        }

        #endregion

        #region Perks

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Game1), nameof(Game1._RogueLike_GetPerkTexture))]
        public static Texture2D _RogueLike_GetPerkTexture(Game1 __instance, RogueLikeMode.Perks enPerk)
        {
            throw new NotImplementedException("Stub method.");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(RogueLikeMode.PerkInfo), nameof(RogueLikeMode.PerkInfo.Init))]
        public static void PerkInfoInit()
        {
            throw new NotImplementedException("Stub method.");
        }

        #endregion

        #region Pins

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(PinCodex), nameof(PinCodex.GetInfo))]
        public static PinInfo GetPinInfo(RogueLikeMode.Perks enType)
        {
            throw new NotImplementedException("Stub method.");
        }

        #endregion

        #region Quests

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(QuestCodex), nameof(QuestCodex.GetQuestDescription))]
        public static QuestDescription GetQuestDescription(QuestCodex.QuestID p_enID)
        {
            throw new NotImplementedException("Stub method.");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(QuestCodex), nameof(QuestCodex.GetQuestInstance))]
        public static Quest GetQuestInstance(QuestCodex.QuestID p_enID)
        {
            throw new NotImplementedException("Stub method.");
        }

        #endregion

        #region Spells

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(SpellCodex), nameof(SpellCodex.IsMagicSkill))]
        public static bool SpellIsMagicSkill(SpellCodex.SpellTypes enType)
        {
            throw new NotImplementedException("Stub method.");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(SpellCodex), nameof(SpellCodex.IsMeleeSkill))]
        public static bool SpellIsMeleeSkill(SpellCodex.SpellTypes enType)
        {
            throw new NotImplementedException("Stub method.");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(SpellCodex), nameof(SpellCodex.IsUtilitySkill))]
        public static bool SpellIsUtilitySkill(SpellCodex.SpellTypes enType)
        {
            throw new NotImplementedException("Stub method.");
        }

        #endregion

        #region Status Effects

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(HudRenderComponent), nameof(HudRenderComponent.GetBuffTexture))]
        public static Texture2D GetBuffTexture(HudRenderComponent __instance, BaseStats.StatusEffectSource en)
        {
            throw new NotImplementedException("Stub method.");
        }

        #endregion
    }
}
