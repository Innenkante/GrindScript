using HarmonyLib;
using SoG.Modding.Content;

namespace SoG.Modding.Patching.Patches
{
    [HarmonyPatch(typeof(PinCodex))]
    internal static class Patch_PinCodex
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(PinCodex.GetInfo))]
        internal static bool GetInfo_Prefix(ref PinInfo __result, PinCodex.PinType enType)
        {
            Globals.Manager.Library.GetEntry(enType, out PinEntry entry);

            if (entry == null)
            {
                return true;  // Unknown mod entry?
            }

            string[] palettes = new string[]
            {
                "Test1",
                "Test2",
                "Test3",
                "Test4",
                "Test5"
            };

            __result = new PinInfo(
                enType,
                "None",
                entry.description,
                entry.pinShape.ToString(),
                entry.pinSymbol.ToString(),
                entry.pinColor == PinEntry.Color.White ? "TestLight" : palettes[(int)entry.pinColor],
                entry.isSticky,
                entry.isBroken,
                FontManager.GetFontByCategory("SmallTitle", FontManager.FontType.Bold8Spacing1),
                FontManager.GetFontByCategory("InMenuDescription", FontManager.FontType.Reg7)
                );

            return false;
        }
    }
}
