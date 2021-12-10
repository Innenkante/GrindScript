using HarmonyLib;
using Microsoft.Xna.Framework;
using SoG.Modding.Content;
using SoG.Modding.Utils;

namespace SoG.Modding.Patching.Patches
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

            var storage = Globals.ModManager.Library.GetStorage<ItemCodex.ItemTypes, ItemEntry>();
            ItemEntry entry = storage[enType];
            __result = entry.vanillaItem;

            if (__result.txDisplayImage == null)
            {
                AssetUtils.TryLoadTexture(entry.iconPath, Globals.Game.Content, out __result.txDisplayImage);
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(ItemCodex.GetItemInstance))]
        internal static bool GetItemInstance_Prefix(ref Item __result, ItemCodex.ItemTypes enType)
        {
            if (!enType.IsFromMod())
                return true;

            var storage = Globals.ModManager.Library.GetStorage<ItemCodex.ItemTypes, ItemEntry>();
            ItemEntry entry = storage[enType];
            string trueShadowTex = entry.shadowPath != "" ? entry.shadowPath : "Items/DropAppearance/hartass02";
            ItemDescription xDesc = entry.vanillaItem;

            __result = new Item()
            {
                enType = enType,
                sFullName = xDesc.sFullName,
                bGiveToServer = xDesc.lenCategory.Contains(ItemCodex.ItemCategories.GrantToServer)
            };

            AssetUtils.TryLoadTexture(entry.iconPath, Globals.Game.xLevelMaster.contRegionContent, out __result.xRenderComponent.txTexture);
            AssetUtils.TryLoadTexture(trueShadowTex, Globals.Game.xLevelMaster.contRegionContent, out __result.xRenderComponent.txShadowTexture);

            __result.xCollisionComponent.xMovementCollider = new SphereCollider(10f, Vector2.Zero, __result.xTransform, 1f, __result) { bCollideWithFlat = true };

            return false;
        }
    }
}
