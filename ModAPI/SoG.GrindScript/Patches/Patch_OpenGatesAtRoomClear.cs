using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bagmen;

namespace SoG.Modding.Patches
{
    [HarmonyPatch(typeof(OpenGatesAtRoomClear))]
    internal static class Patch_OpenGatesAtRoomClear
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(OpenGatesAtRoomClear.OpenBlockades))]
        internal static void OpenBlockades_Postfix()
        {
            foreach (Mod mod in Globals.ModManager.Mods)
                mod.PostArcadeRoomComplete();
        }
    }
}
