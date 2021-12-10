using HarmonyLib;
using SoG.Modding.Content;

namespace SoG.Modding.Patching.Patches
{
    [HarmonyPatch(typeof(CardCodex))]
    internal static class Patch_CardCodex
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(CardCodex.GetIllustrationPath))]
        public static bool GetIllustrationPath_Prefix(ref string __result, EnemyCodex.EnemyTypes enEnemy)
        {
            if (!enEnemy.IsFromMod())
            {
                return true;
            }

            var storage = Globals.ModManager.Library.GetStorage<EnemyCodex.EnemyTypes, EnemyEntry>();
            __result = storage[enEnemy].cardIllustrationPath;

            return false;
        }

    }
}
