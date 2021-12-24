using HarmonyLib;
using SoG.Modding.Content;
using System;

namespace SoG.Modding.Patching.Patches
{
    [HarmonyPatch(typeof(WeaponCodex))]
    internal static class Patch_WeaponCodex
    {
        /// <summary>
        /// Retrieves the WeaponInfo of an entry.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(nameof(WeaponCodex.GetWeaponInfo))]
        internal static bool GetWeaponInfo_Prefix(ref WeaponInfo __result, ItemCodex.ItemTypes enType)
        {
            Globals.Manager.Library.GetEntry(enType, out ItemEntry entry);

            __result = entry.vanillaEquip as WeaponInfo;

            return false;
        }
    }
}
