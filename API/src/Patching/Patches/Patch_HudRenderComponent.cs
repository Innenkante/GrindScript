using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using SoG.Modding.Content;
using SoG.Modding.Utils;

namespace SoG.Modding.Patching.Patches
{
    [HarmonyPatch(typeof(HudRenderComponent))]
    internal static class Patch_HudRenderComponent
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(HudRenderComponent.GetBuffTexture))]
        internal static void GetBuffTexture_Postfix(ref Texture2D __result, BaseStats.StatusEffectSource en)
        {
            if (!en.IsFromMod())
                return;

            var storage = Globals.ModManager.Library.GetStorage<BaseStats.StatusEffectSource, StatusEffectEntry>();
            string path = storage[en].texturePath;

            if (string.IsNullOrEmpty(path))
            {
                __result = null;
            }
            else
            {
                AssetUtils.TryLoadTexture(path, Globals.Game.Content, out __result);
            }
        }
    }
}
