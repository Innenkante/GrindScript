using HarmonyLib;
using Microsoft.Xna.Framework.Content;
using SoG.Modding.Content;
using SoG.Modding.Utils;
using System.IO;

namespace SoG.Modding.Patching.Patches
{
    [HarmonyPatch(typeof(FacegearCodex))]
    internal static class Patch_FacegearCodex
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(FacegearCodex.GetHatInfo))]
        internal static bool GetHatInfo_Prefix(ref FacegearInfo __result, ItemCodex.ItemTypes enType)
        {
            if (!enType.IsFromMod())
                return true;

            var storage = Globals.ModManager.Library.GetStorage<ItemCodex.ItemTypes, ItemEntry>();
            ItemEntry entry = storage[enType];
            ContentManager manager = Globals.Game.Content;
            string path = entry.equipResourcePath;

            __result = entry.vanillaEquip as FacegearInfo;

            string[] directions = new string[]
            {
                "Up", "Right", "Down", "Left"
            };

            int index = -1;
            while (++index < 4)
            {
                if (__result.atxTextures[index] == null)
                {
                    AssetUtils.TryLoadTexture(Path.Combine(path, directions[index]), manager, out __result.atxTextures[index]);
                }
            }

            return false;
        }
    }
}
