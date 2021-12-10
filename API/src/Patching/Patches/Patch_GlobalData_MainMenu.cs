using HarmonyLib;

namespace SoG.Modding.Patching.Patches
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
                PatchHelper.MainMenuWorker.AnalyzeStorySavesForCompatibility();
            }
            else if (enTarget == GlobalData.MainMenu.MenuLevel.TopMenu)
            {
                PatchHelper.MainMenuWorker.AnalyzeArcadeSavesForCompatibility();
            }
        }
    }
}
