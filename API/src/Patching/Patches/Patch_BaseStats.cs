using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace SoG.Modding.Patching.Patches
{
    [HarmonyPatch(typeof(BaseStats))]
    internal static class Patch_BaseStats
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(BaseStats.Update))]
        public static void Update_Prefix(BaseStats __instance)
        {
            foreach (Mod mod in Globals.Manager.ActiveMods)
                mod.OnBaseStatsUpdate(__instance);
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(BaseStats.Update))]
        public static void Update_Postfix(BaseStats __instance)
        {
            foreach (Mod mod in Globals.Manager.ActiveMods)
                mod.PostBaseStatsUpdate(__instance);
        }
    }
}
