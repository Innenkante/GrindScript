using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PerkInfo = SoG.RogueLikeMode.PerkInfo;

namespace SoG.Modding.Patches
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
            foreach (var perk in Globals.ModManager.Library.Perks.Values)
                PerkInfo.lxAllPerks.Add(new PerkInfo(perk.GameID, perk.Config.EssenceCost, perk.TextEntry));
        }
    }
}
