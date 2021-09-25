using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoG.Modding.Patches
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

            __result = Globals.ModManager.Library.Items[enType].EquipData as WeaponInfo;

            return false;
        }
    }
}
