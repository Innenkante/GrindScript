using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using SoG.Modding.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoG.Modding.Patches
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

            string path = Globals.ModManager.Library.StatusEffects[en].Config.TexturePath;

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
