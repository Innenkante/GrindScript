using Bagmen;
using HarmonyLib;

namespace SoG.Modding.Patching.Patches
{
    [HarmonyPatch(typeof(OpenGatesAtRoomClear))]
    internal static class Patch_OpenGatesAtRoomClear
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(OpenGatesAtRoomClear.OpenBlockades))]
        internal static void OpenBlockades_Postfix()
        {
            foreach (Mod mod in Globals.Manager.ActiveMods)
                mod.PostArcadeRoomComplete();
        }
    }
}
