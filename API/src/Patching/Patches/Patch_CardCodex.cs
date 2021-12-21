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
            var storage = Globals.Manager.Library.GetAllEntries<EnemyCodex.EnemyTypes, EnemyEntry>();

            if (storage.TryGetValue(enEnemy, out EnemyEntry entry))
            {
                __result = entry.cardIllustrationPath;
            }
            else
            {
                __result = "";
            }

            return false;
        }

    }
}
