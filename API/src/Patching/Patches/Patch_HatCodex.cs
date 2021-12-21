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
            Globals.Manager.Library.GetEntry(enType, out ItemEntry entry);

            __result = null;

            if (entry != null && entry.vanillaEquip is HatInfo info)
            {
                __result = info;

                string path = entry.equipResourcePath;

                string[] directions = new string[] { "Up", "Right", "Down", "Left" };

                int index = -1;

                while (++index < 4)
                {
                    if (__result.xDefaultSet.atxTextures[index] == null)
                    {
                        if (path != null)
                        {
                            AssetUtils.TryLoadTexture(Path.Combine(path, directions[index]), Globals.Game.Content, out __result.xDefaultSet.atxTextures[index]);
                        }
                        else
                        {
                            __result.xDefaultSet.atxTextures[index] = Globals.Manager.GrindScript.ErrorTexture;
                        }
                    }
                }

                foreach (var kvp in entry.hatAltSetResourcePaths)
                {
                    index = -1;

                    while (++index < 4)
                    {
                        var altSet = __result.denxAlternateVisualSets[kvp.Key];

                        if (altSet.atxTextures[index] == null)
                        {
                            if (path != null && kvp.Value != null)
                            {
                                string altPath = Path.Combine(path, kvp.Value);
                                AssetUtils.TryLoadTexture(Path.Combine(altPath, directions[index]), Globals.Game.Content, out altSet.atxTextures[index]);
                            }
                            else
                            {
                                altSet.atxTextures[index] = Globals.Manager.GrindScript.ErrorTexture;
                            }
                        }
                    }
                }
            }

            return false;
        }
    }
}
