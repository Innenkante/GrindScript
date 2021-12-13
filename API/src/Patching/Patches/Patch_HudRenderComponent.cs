using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using SoG.Modding.Content;
using SoG.Modding.Utils;

namespace SoG.Modding.Patching.Patches
{
    [HarmonyPatch(typeof(HudRenderComponent))]
    internal static class Patch_HudRenderComponent
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(HudRenderComponent.GetBuffTexture))]
        internal static bool GetBuffTexture_Prefix(ref Texture2D __result, BaseStats.StatusEffectSource en)
        {
            Globals.Manager.Library.TryGetEntry(en, out StatusEffectEntry entry);

            if (entry == null)
            {
                __result = RenderMaster.txNullTex;  // Unknown mod entry?
                return false;
            }

            if (entry.texturePath == null)
            {
                if (entry.IsVanilla)
                {
                    __result = OriginalMethods.GetBuffTexture(Globals.Game.xHUD, en);
                    return false;
                }

                __result = Globals.Manager.GrindScript.ErrorTexture;  // Bad texture
                return false;
            }

            AssetUtils.TryLoadTexture(entry.texturePath, Globals.Game.Content, out __result);
            return false;
        }
    }
}
