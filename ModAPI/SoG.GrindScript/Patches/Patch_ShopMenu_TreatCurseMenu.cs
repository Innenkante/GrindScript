using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreatCurseMenu = SoG.ShopMenu.TreatCurseMenu;

namespace SoG.Modding.Patches
{
    [HarmonyPatch(typeof(TreatCurseMenu))]
    internal static class Patch_ShopMenu_TreatCurseMenu
    {
        /// <summary>
        /// Inserts custom curses in the Curse shop menu.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(nameof(TreatCurseMenu.FillCurseList))]
        internal static void FillCurseList_Postfix(TreatCurseMenu __instance)
        {
            foreach (var kvp in Globals.ModManager.Library.Curses)
            {
                if (!kvp.Value.Config.IsTreat)
                    __instance.lenTreatCursesAvailable.Add(kvp.Value.GameID);
            }
        }

        /// <summary>
        /// Inserts custom curses in the Treat shop menu.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(nameof(TreatCurseMenu.FillTreatList))]
        internal static void FillTreatList_Postfix(TreatCurseMenu __instance)
        {
            foreach (var kvp in Globals.ModManager.Library.Curses)
            {
                if (kvp.Value.Config.IsTreat)
                    __instance.lenTreatCursesAvailable.Add(kvp.Value.GameID);
            }
        }
    }
}
