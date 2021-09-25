using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Xna.Framework;
using SoG.Modding.LibraryEntries;
using SoG.Modding.Utils;

namespace SoG.Modding.Patches
{
    [HarmonyPatch(typeof(ItemCodex))]
    class Patch_ItemCodex
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(ItemCodex.GetItemDescription))]
        internal static bool GetItemDescription_Prefix(ref ItemDescription __result, ItemCodex.ItemTypes enType)
        {
            if (!enType.IsFromMod())
                return true;

            ItemEntry entry = Globals.ModManager.Library.Items[enType];
            __result = entry.ItemData;

            if (__result.txDisplayImage == null)
            {
                ModUtils.TryLoadTex(entry.Config.IconPath, Globals.Game.Content, out __result.txDisplayImage);
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(ItemCodex.GetItemInstance))]
        internal static bool GetItemInstance_Prefix(ref Item __result, ItemCodex.ItemTypes enType)
        {
            if (!enType.IsFromMod())
                return true;

            ItemEntry entry = Globals.ModManager.Library.Items[enType];
            string trueShadowTex = entry.Config.ShadowPath != "" ? entry.Config.ShadowPath : "Items/DropAppearance/hartass02";
            ItemDescription xDesc = entry.ItemData;

            __result = new Item()
            {
                enType = enType,
                sFullName = xDesc.sFullName,
                bGiveToServer = xDesc.lenCategory.Contains(ItemCodex.ItemCategories.GrantToServer)
            };

            ModUtils.TryLoadTex(entry.Config.IconPath, Globals.Game.xLevelMaster.contRegionContent, out __result.xRenderComponent.txTexture);
            ModUtils.TryLoadTex(trueShadowTex, Globals.Game.xLevelMaster.contRegionContent, out __result.xRenderComponent.txShadowTexture);

            __result.xCollisionComponent.xMovementCollider = new SphereCollider(10f, Vector2.Zero, __result.xTransform, 1f, __result) { bCollideWithFlat = true };

            return false;
        }
    }
}
