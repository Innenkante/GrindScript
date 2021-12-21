using HarmonyLib;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
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
            Globals.Manager.Library.GetEntry(enType, out ItemEntry entry);

            __result = entry?.vanillaEquip as FacegearInfo;

            if (__result != null)
            {
                string path = entry.equipResourcePath;

                if (entry.useVanillaResourceFormat)
                {
                    path = Path.Combine("Sprites/Equipment/Facegear/", path);
                }

                string[] directions = new string[] { "Up", "Right", "Down", "Left" };

                int index = -1;
                while (++index < 4)
                {
                    if (path != null)
                    {
                        AssetUtils.TryLoadTexture(Path.Combine(path, directions[index]), Globals.Game.Content, out __result.atxTextures[index]);
                    }
                    else if (__result.atxTextures[index] == null)
                    {
                        __result.atxTextures[index] = Globals.Manager.GrindScript.ErrorTexture;
                    }
                }
            }
            else if (entry?.vanillaEquip is HatInfo hat)
            {
                // Hacky way from Teddy to render double slotted masks
                __result = new FacegearInfo(enType)
                {
                    xItemDescription = hat.xItemDescription,
                    atxTextures = new Texture2D[]
                    {
                        RenderMaster.txNullTex,
                        RenderMaster.txNullTex,
                        RenderMaster.txNullTex,
                        RenderMaster.txNullTex,
                    }
                };
            }

            return false;
        }
    }
}
