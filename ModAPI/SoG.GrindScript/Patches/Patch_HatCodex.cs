using HarmonyLib;
using Microsoft.Xna.Framework.Content;
using SoG.Modding.LibraryEntries;
using SoG.Modding.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoG.Modding.Patches
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

            ItemEntry entry = Globals.ModManager.Library.Items[enType];
            ContentManager manager = Globals.Game.Content;
            string path = entry.Config.EquipResourcePath;

            __result = entry.EquipData as HatInfo;

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
                string altPath = Path.Combine(path, entry.HatAltSetResourcePaths[kvp.Key]);

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
