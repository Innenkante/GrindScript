using HarmonyLib;
using SoG.Modding.Content;

namespace SoG.Modding.Patching.Patches
{
    [HarmonyPatch(typeof(WeaponCodex))]
    internal static class Patch_WeaponCodex
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(WeaponCodex.GetWeaponInfo))]
        internal static bool GetWeaponInfo_Prefix(ref WeaponInfo __result, ItemCodex.ItemTypes enType)
        {
            if (!enType.IsFromMod())
                return true;

            Globals.ModManager.Library.TryGetEntry(enType, out ItemEntry entry);
            __result = entry.vanillaEquip as WeaponInfo;

            return false;
        }
    }
}
