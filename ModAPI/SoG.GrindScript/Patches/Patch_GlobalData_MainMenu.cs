using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoG.Modding.Patches
{
    [HarmonyPatch(typeof(GlobalData.MainMenu))]
    internal static class Patch_GlobalData_MainMenu
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(GlobalData.MainMenu.Transition))]
        internal static void Transition_Prefix(GlobalData.MainMenu.MenuLevel enTarget)
        {
            if (enTarget == GlobalData.MainMenu.MenuLevel.CharacterSelect)
            {
                HelperCallbacks.MainMenuWorker.AnalyzeStorySavesForCompatibility();
            }
        }
    }
}
