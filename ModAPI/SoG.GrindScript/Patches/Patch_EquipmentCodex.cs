﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoG.Modding.Patches
{
    [HarmonyPatch(typeof(EquipmentCodex))]
    internal static class Patch_EquipmentCodex
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(EquipmentCodex.GetArmorInfo))]
        internal static bool GetArmorInfo_Prefix(ref EquipmentInfo __result, ItemCodex.ItemTypes enType)
        {
            if (!enType.IsFromMod())
                return true;

            __result = Globals.ModManager.Library.Items[enType].EquipData;

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(EquipmentCodex.GetAccessoryInfo))]
        internal static bool GetAccessoryInfo_Prefix(ref EquipmentInfo __result, ItemCodex.ItemTypes enType)
        {
            return GetArmorInfo_Prefix(ref __result, enType);
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(EquipmentCodex.GetShieldInfo))]
        internal static bool GetShieldInf_Prefix(ref EquipmentInfo __result, ItemCodex.ItemTypes enType)
        {
            return GetArmorInfo_Prefix(ref __result, enType);
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(EquipmentCodex.GetShoesInfo))]
        internal static bool GetShoesInfo_Prefix(ref EquipmentInfo __result, ItemCodex.ItemTypes enType)
        {
            return GetArmorInfo_Prefix(ref __result, enType);
        }
    }
}
