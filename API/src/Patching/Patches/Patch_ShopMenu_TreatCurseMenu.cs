using HarmonyLib;
using SoG.Modding.Content;
using TreatCurseMenu = SoG.ShopMenu.TreatCurseMenu;

namespace SoG.Modding.Patching.Patches
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
            var storage = Globals.ModManager.Library.GetStorage<RogueLikeMode.TreatsCurses, CurseEntry>();
            foreach (var kvp in storage)
            {
                if (!kvp.Value.isTreat)
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
            var storage = Globals.ModManager.Library.GetStorage<RogueLikeMode.TreatsCurses, CurseEntry>();
            foreach (var kvp in storage)
            {
                if (kvp.Value.isTreat)
                    __instance.lenTreatCursesAvailable.Add(kvp.Value.GameID);
            }
        }
    }
}
