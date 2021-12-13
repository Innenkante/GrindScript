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
        [HarmonyPrefix]
        [HarmonyPatch(nameof(TreatCurseMenu.FillCurseList))]
        internal static bool FillCurseList_Prefix(TreatCurseMenu __instance)
        {
            var storage = Globals.Manager.Library.GetStorage<RogueLikeMode.TreatsCurses, CurseEntry>();

            __instance.lenTreatCursesAvailable.Clear();

            foreach (var kvp in storage)
            {
                if (!kvp.Value.isTreat)
                    __instance.lenTreatCursesAvailable.Add(kvp.Value.GameID);
            }

            return false;
        }

        /// <summary>
        /// Inserts custom curses in the Treat shop menu.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(nameof(TreatCurseMenu.FillTreatList))]
        internal static bool FillTreatList_Prefix(TreatCurseMenu __instance)
        {
            var storage = Globals.Manager.Library.GetStorage<RogueLikeMode.TreatsCurses, CurseEntry>();

            __instance.lenTreatCursesAvailable.Clear();

            foreach (var kvp in storage)
            {
                if (kvp.Value.isTreat)
                    __instance.lenTreatCursesAvailable.Add(kvp.Value.GameID);
            }

            return false;
        }
    }
}
