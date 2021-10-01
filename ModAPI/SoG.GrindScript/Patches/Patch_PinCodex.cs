using HarmonyLib;
using SoG.Modding.Configs;
using SoG.Modding.LibraryEntries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoG.Modding.Patches
{
    [HarmonyPatch(typeof(PinCodex))]
    internal static class Patch_PinCodex
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(PinCodex.GetInfo))]
        internal static void GetInfo_Postfix(ref PinInfo __result, PinCodex.PinType enType)
        {
            if (!enType.IsFromMod())
                return;

            string[] palettes = new string[]
            {
                "Test1",
                "Test2",
                "Test3",
                "Test4",
                "Test5"
            };

            PinEntry entry = Globals.ModManager.Library.Pins[enType];
            PinConfig config = entry.Config;

            __result = new PinInfo(
                enType,
                "None",
                config.Description,
                config.PinShape.ToString(),
                config.PinSymbol.ToString(),
                config.IsSticky ? "TestLight" : palettes[(int)config.PinColor],
                config.IsSticky,
                config.IsBroken,
                FontManager.GetFontByCategory("SmallTitle", FontManager.FontType.Bold8Spacing1),
                FontManager.GetFontByCategory("InMenuDescription", FontManager.FontType.Reg7)
                );
        }
    }
}
