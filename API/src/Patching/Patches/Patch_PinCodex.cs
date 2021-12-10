using HarmonyLib;
using SoG.Modding.Content;

namespace SoG.Modding.Patching.Patches
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

            var storage = Globals.ModManager.Library.GetStorage<PinCodex.PinType, PinEntry>();
            var entry = storage[enType];

            __result = new PinInfo(
                enType,
                "None",
                entry.description,
                entry.pinShape.ToString(),
                entry.pinSymbol.ToString(),
                entry.isSticky ? "TestLight" : palettes[(int)entry.pinColor],
                entry.isSticky,
                entry.isBroken,
                FontManager.GetFontByCategory("SmallTitle", FontManager.FontType.Bold8Spacing1),
                FontManager.GetFontByCategory("InMenuDescription", FontManager.FontType.Reg7)
                );
        }
    }
}
