using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SoG.Modding.Content;
using SoG.Modding.Utils;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using CodeList = System.Collections.Generic.IEnumerable<HarmonyLib.CodeInstruction>;

namespace SoG.Modding.Patching.Patches
{
    [HarmonyPatch(typeof(EnemyCodex))]
    internal static class Patch_EnemyCodex
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(EnemyCodex.GetEnemyDescription))]
        internal static bool GetEnemyDescription_Prefix(ref EnemyDescription __result, EnemyCodex.EnemyTypes enType)
        {
            var storage = Globals.Manager.Library.GetAllEntries<EnemyCodex.EnemyTypes, EnemyEntry>();

            if (storage.TryGetValue(enType, out EnemyEntry entry))
            {
                __result = entry.vanilla;
            }
            else
            {
                __result = null;  // Unknown mod item?
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(EnemyCodex.GetEnemyInstance))]
        internal static bool GetEnemyInstance_Prefix(EnemyCodex.EnemyTypes enType, Level.WorldRegion enOverrideContent, ref Enemy __result)
        {
            if (enType.IsFromSoG())
            {
                var storage = Globals.Manager.Library.GetAllEntries<EnemyCodex.EnemyTypes, EnemyEntry>();
                if (!storage.TryGetValue(enType, out EnemyEntry entry) || entry.constructor == null)
                {
                    // No replacement exists. Let's continue to the vanilla code to get the instance!
                    return true;
                }
            }
            else if (!enType.IsFromMod())
            {
                __result = null;
                return false; // Unknown mod entry?
            }

            __result = EditedMethods.GetModdedEnemyInstance(enType, enOverrideContent);
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(EnemyCodex.GetEnemyDefaultAnimation))]
        public static bool GetEnemyDefaultAnimation_Prefix(ref Animation __result, EnemyCodex.EnemyTypes enType, ContentManager Content)
        {
            var storage = Globals.Manager.Library.GetAllEntries<EnemyCodex.EnemyTypes, EnemyEntry>();

            if (storage.TryGetValue(enType, out EnemyEntry entry))
            {
                if (entry.defaultAnimation == null)
                {
                    if (entry.IsVanilla)
                    {
                        return true;
                    }

                    __result = new Animation(0, 0, Globals.Manager.GrindScript.ErrorTexture, Vector2.Zero);
                    return false;  // Animation hasn't been set...
                }

                __result = entry.defaultAnimation.Invoke(Content);
                return false;
            }

            __result = new Animation(0, 0, Globals.Manager.GrindScript.ErrorTexture, Vector2.Zero);
            return false;  // Unknown mod entry?
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(EnemyCodex.GetEnemyDisplayIcon))]
        public static bool GetEnemyDisplayIcon_Prefix(ref Texture2D __result, EnemyCodex.EnemyTypes enType, ContentManager Content)
        {
            var storage = Globals.Manager.Library.GetAllEntries<EnemyCodex.EnemyTypes, EnemyEntry>();

            if (storage.TryGetValue(enType, out EnemyEntry entry))
            {
                if (entry.displayIconPath == null)
                {
                    if (entry.IsVanilla)
                    {
                        return true;  // No replacement found, get it from vanilla
                    }

                    __result = Globals.Manager.GrindScript.ErrorTexture;
                    return false;  // Texture not set...
                }

                AssetUtils.TryLoadTexture(entry.displayIconPath, Content, out __result);
                return false;
            }

            __result = Globals.Manager.GrindScript.ErrorTexture;
            return false;  // Unknown mod entry?
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(EnemyCodex.GetEnemyLocationPicture))]
        public static bool GetEnemyLocationPicture_Prefix(ref Texture2D __result, EnemyCodex.EnemyTypes enType, ContentManager Content)
        {
            var storage = Globals.Manager.Library.GetAllEntries<EnemyCodex.EnemyTypes, EnemyEntry>();

            if (storage.TryGetValue(enType, out EnemyEntry entry))
            {
                if (entry.displayBackgroundPath == null)
                {
                    if (entry.IsVanilla)
                    {
                        return true;  // No replacement found, get it from vanilla
                    }

                    __result = Globals.Manager.GrindScript.ErrorTexture;
                    return false;  // Texture not set...
                }

                AssetUtils.TryLoadTexture(entry.displayBackgroundPath, Content, out __result);
                return false;
            }

            __result = Globals.Manager.GrindScript.ErrorTexture;
            return false;  // Unknown mod item?
        }
    }
}
