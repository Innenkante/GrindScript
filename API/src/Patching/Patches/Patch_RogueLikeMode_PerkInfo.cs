using HarmonyLib;
using SoG.Modding.Content;
using PerkInfo = SoG.RogueLikeMode.PerkInfo;

namespace SoG.Modding.Patching.Patches
{
    [HarmonyPatch(typeof(PerkInfo))]
    internal static class Patch_RogueLikeMode_PerkInfo
    {
        /// <summary>
        /// Inserts custom perks in the Perk shop menu.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(nameof(PerkInfo.Init))]
        internal static void Init_Postfix()
        {
            var storage = Globals.ModManager.Library.GetStorage<RogueLikeMode.Perks, PerkEntry>();
            foreach (var perk in storage.Values)
                PerkInfo.lxAllPerks.Add(new PerkInfo(perk.GameID, perk.essenceCost, perk.textEntry));
        }
    }
}
