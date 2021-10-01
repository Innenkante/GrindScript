using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Xna.Framework.Content;
using SoG.Modding.LibraryEntries;
using SoG.Modding.Utils;

namespace SoG.Modding.Patches
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

            ItemEntry entry = Globals.ModManager.Library.Items[enType];
            ContentManager manager = Globals.Game.Content;
            string path = entry.Config.EquipResourcePath;

            __result = entry.EquipData as FacegearInfo;

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
