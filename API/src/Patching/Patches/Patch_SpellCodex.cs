using HarmonyLib;
using Microsoft.Xna.Framework;
using SoG.Modding.Content;

namespace SoG.Modding.Patching.Patches
{
    [HarmonyPatch(typeof(SpellCodex))]
    internal static class Patch_SpellCodex
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(SpellCodex.GetSpellInstance), typeof(SpellCodex.SpellTypes), typeof(int), typeof(Level.WorldRegion))]
        internal static bool GetSpellInstance_Prefix(ref ISpellInstance __result, SpellCodex.SpellTypes enType, int iPowerLevel, Level.WorldRegion enOverrideRegion)
        {
            Globals.Manager.Library.TryGetEntry(enType, out SpellEntry entry);

            if (entry == null)
            {
                __result = null;
                return true;  // Unknown mod item?
            }

            if (entry.builder == null && entry.IsVanilla)
            {
                return true;  // Get from vanilla
            }

            __result = entry.builder(iPowerLevel, enOverrideRegion);

            if (__result.xRenderComponent == null)
            {
                __result.xRenderComponent = new AnimatedRenderComponent(__result)
                {
                    xTransform = __result.xTransform
                };
            }

            __result.xRenderComponent.xOwnerObject = __result;

            if (__result.xRenderComponent is AnimatedRenderComponent arc && arc.dixAnimations.Count == 0)
            {
                arc.dixAnimations.Add(0, new Animation(0, 0, RenderMaster.txNullTex, new Vector2(8f, 6f), 4, 1, 17, 32, 0, 0, 6, Animation.LoopSettings.Looping, Animation.CancelOptions.IgnoreIfPlaying, true, true));
            }

            return false;
        }

        /*
        [HarmonyPostfix]
        [HarmonyPatch(nameof(SpellCodex.IsEPBlocking))]
        internal static void IsEPBlocking_Postfix(SpellCodex.SpellTypes enType, ref bool __result)
        {
            if (!enType.IsFromMod())
                return;

            __result = false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(SpellCodex.IsHidden))]
        internal static void IsHidden_Postfix(SpellCodex.SpellTypes enType, ref bool __result)
        {
            if (!enType.IsFromMod())
                return;

            __result = false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(SpellCodex.IsTalent))]
        internal static void IsTalent_Postfix(SpellCodex.SpellTypes enType, ref bool __result)
        {
            if (!enType.IsFromMod())
                return;

            __result = false;
        }
        */

        [HarmonyPrefix]
        [HarmonyPatch(nameof(SpellCodex.IsMagicSkill))]
        internal static bool IsMagicSkill_Prefix(SpellCodex.SpellTypes enType, ref bool __result)
        {
            Globals.Manager.Library.TryGetEntry(enType, out SpellEntry entry);

            if (entry == null)
            {
                __result = false;
            }
            else
            {
                __result = entry.isMagicSkill;
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(SpellCodex.IsMeleeSkill))]
        internal static bool IsMeleeSkill_Prefix(SpellCodex.SpellTypes enType, ref bool __result)
        {
            Globals.Manager.Library.TryGetEntry(enType, out SpellEntry entry);

            if (entry == null)
            {
                __result = false;
            }
            else
            {
                __result = entry.isMeleeSkill;
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(SpellCodex.IsUtilitySkill))]
        internal static bool IsUtilitySkill_Prefix(SpellCodex.SpellTypes enType, ref bool __result)
        {
            Globals.Manager.Library.TryGetEntry(enType, out SpellEntry entry);

            if (entry == null)
            {
                __result = false;
            }
            else
            {
                __result = entry.isUtilitySkill;
            }

            return false;
        }
    }
}
