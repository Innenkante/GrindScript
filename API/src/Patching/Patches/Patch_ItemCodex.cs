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
            Globals.Manager.Library.TryGetEntry(enType, out ItemEntry entry);

            if (entry != null)
            {
                __result = entry.vanillaItem;

                if (entry.iconPath != null)
                {
                    AssetUtils.TryLoadTexture(entry.iconPath, Globals.Game.Content, out __result.txDisplayImage);
                }
                else
                {
                    if (__result.txDisplayImage == null)
                    {
                        __result.txDisplayImage = Globals.Manager.GrindScript.ErrorTexture;
                    }
                }
            }
            else
            {
                __result = new ItemDescription() { enType = enType };
            }

            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(ItemCodex.GetItemInstance))]
        internal static void GetItemInstance_Postfix(ref Item __result, ItemCodex.ItemTypes enType)
        {
            Globals.Manager.Library.TryGetEntry(enType, out ItemEntry entry);

            __result.enType = entry.vanillaItem.enType;
            __result.sFullName = entry.vanillaItem.sFullName;
            __result.bGiveToServer = entry.vanillaItem.lenCategory.Contains(ItemCodex.ItemCategories.GrantToServer);

            if (entry.iconPath != null)
            {
                AssetUtils.TryLoadTexture(entry.iconPath, Globals.Game.xLevelMaster.contRegionContent, out __result.xRenderComponent.txTexture);
            }
            else
            {
                if (__result.xRenderComponent.txTexture == null)
                {
                    __result.xRenderComponent.txTexture = entry.vanillaItem.txDisplayImage;
                }
            }

            if (entry.shadowPath != null)
            {
                AssetUtils.TryLoadTexture(entry.shadowPath, Globals.Game.xLevelMaster.contRegionContent, out __result.xRenderComponent.txShadowTexture);
            }
            else
            {
                if (__result.xRenderComponent.txShadowTexture == null)
                {
                    AssetUtils.TryLoadTexture("Items/DropAppearance/hartass02", Globals.Game.xLevelMaster.contRegionContent, out __result.xRenderComponent.txShadowTexture);
                }
            }
        }
    }
}
