using HarmonyLib;
using Microsoft.Xna.Framework.Content;
using SoG.Modding.Content;
using SoG.Modding.Utils;
using System.IO;

namespace SoG.Modding.Patching.Patches
{
    [HarmonyPatch(typeof(HatCodex))]
    internal static class Patch_HatCodex
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(HatCodex.GetHatInfo))]
        internal static bool GetHatInfo_Prefix(ref HatInfo __result, ItemCodex.ItemTypes enType)
        {
            if (!enType.IsFromMod())
                return true;

            var storage = Globals.ModManager.Library.GetStorage<ItemCodex.ItemTypes, ItemEntry>();
            ItemEntry entry = storage[enType];
            ContentManager manager = Globals.Game.Content;
            string path = entry.equipResourcePath;

            __result = entry.vanillaEquip as HatInfo;

            string[] directions = new string[]
            {
                "Up", "Right", "Down", "Left"
            };

            int index = -1;

            while (++index < 4)
            {
                if (__result.xDefaultSet.atxTextures[index] == null)
                {
                    AssetUtils.TryLoadTexture(Path.Combine(path, directions[index]), manager, out __result.xDefaultSet.atxTextures[index]);
                }
            }

            foreach (var kvp in __result.denxAlternateVisualSets)
            {
                string altPath = Path.Combine(path, entry.hatAltSetResourcePaths[kvp.Key]);

                index = -1;

                while (++index < 4)
                {
                    if (kvp.Value.atxTextures[index] == null)
                    {
                        AssetUtils.TryLoadTexture(Path.Combine(altPath, directions[index]), manager, out kvp.Value.atxTextures[index]);
                    }
                }
            }

            return false;
        }
    }
}
