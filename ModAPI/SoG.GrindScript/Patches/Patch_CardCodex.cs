using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace SoG.Modding.Patches
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

            __result = Globals.ModManager.Library.Enemies[enEnemy].Config.CardIllustrationPath;

            return false;
        }

    }
}
