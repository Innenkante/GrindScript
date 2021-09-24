using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using HarmonyLib;
using SoG.Modding.LibraryEntries;

namespace SoG.Modding.Patches
{
    /// <summary>
    /// Contains item-related patches.
    /// (ItemCodex, EquipmentCodex, etc)
    /// </summary>

    [HarmonyPatch]
    internal static class ItemLogicPatches
    {

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ItemCodex), nameof(ItemCodex.GetItemDescription))]
        internal static bool OnGetItemDescription(ref ItemDescription __result, ItemCodex.ItemTypes enType)
        {
            if (!enType.IsFromMod())
                return true;

            ItemEntry entry = Globals.API.Loader.Library.Items[enType];
            __result = entry.ItemData;
            Utils.ModUtils.TryLoadTex(entry.Config.IconPath, entry.Config.Manager, out __result.txDisplayImage);

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ItemCodex), nameof(ItemCodex.GetItemInstance))]
        internal static bool OnGetItemInstance(ref Item __result, ItemCodex.ItemTypes enType)
        {
            if (!enType.IsFromMod())
                return true;

            ItemEntry entry = Globals.API.Loader.Library.Items[enType];
            string trueShadowTex = entry.Config.ShadowPath != "" ? entry.Config.ShadowPath : "Items/DropAppearance/hartass02";
            ItemDescription xDesc = entry.ItemData;

            __result = new Item()
            {
                enType = enType,
                sFullName = xDesc.sFullName,
                bGiveToServer = xDesc.lenCategory.Contains(ItemCodex.ItemCategories.GrantToServer)
            };

            Utils.ModUtils.TryLoadTex(entry.Config.IconPath, entry.Config.Manager, out __result.xRenderComponent.txTexture);
            Utils.ModUtils.TryLoadTex(trueShadowTex, entry.Config.Manager, out __result.xRenderComponent.txShadowTexture);

            __result.xCollisionComponent.xMovementCollider = new SphereCollider(10f, Vector2.Zero, __result.xTransform, 1f, __result) { bCollideWithFlat = true };

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(EquipmentCodex), nameof(EquipmentCodex.GetArmorInfo))]
        internal static bool OnGetEquipmentInfo_0(ref EquipmentInfo __result, ItemCodex.ItemTypes enType)
        {
            if (!enType.IsFromMod())
                return true;

            __result = Globals.API.Loader.Library.Items[enType].EquipData;

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(EquipmentCodex), nameof(EquipmentCodex.GetAccessoryInfo))]
        internal static bool OnGetEquipmentInfo_1(ref EquipmentInfo __result, ItemCodex.ItemTypes enType) => OnGetEquipmentInfo_0(ref __result, enType);

        [HarmonyPrefix]
        [HarmonyPatch(typeof(EquipmentCodex), nameof(EquipmentCodex.GetShieldInfo))]
        internal static bool OnGetEquipmentInfo_2(ref EquipmentInfo __result, ItemCodex.ItemTypes enType) => OnGetEquipmentInfo_0(ref __result, enType);

        [HarmonyPrefix]
        [HarmonyPatch(typeof(EquipmentCodex), nameof(EquipmentCodex.GetShoesInfo))]
        internal static bool OnGetEquipmentInfo_3(ref EquipmentInfo __result, ItemCodex.ItemTypes enType) => OnGetEquipmentInfo_0(ref __result, enType);

        [HarmonyPrefix]
        [HarmonyPatch(typeof(FacegearCodex), nameof(FacegearCodex.GetHatInfo))]
        internal static bool OnGetFacegearInfo(ref FacegearInfo __result, ItemCodex.ItemTypes enType)
        {
            if (!enType.IsFromMod())
                return true;

            ItemEntry entry = Globals.API.Loader.Library.Items[enType];
            ContentManager manager = entry.Config.Manager;
            string path = entry.Config.EquipResourcePath;

            __result = entry.EquipData as FacegearInfo;

            Utils.ModUtils.TryLoadTex(Path.Combine(path, "Up"), manager, out __result.atxTextures[0]);
            Utils.ModUtils.TryLoadTex(Path.Combine(path, "Right"), manager, out __result.atxTextures[1]);
            Utils.ModUtils.TryLoadTex(Path.Combine(path, "Down"), manager, out __result.atxTextures[2]);
            Utils.ModUtils.TryLoadTex(Path.Combine(path, "Left"), manager, out __result.atxTextures[3]);

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HatCodex), nameof(HatCodex.GetHatInfo))]
        internal static bool OnGetHatInfo(ref HatInfo __result, ItemCodex.ItemTypes enType)
        {
            if (!enType.IsFromMod())
                return true;

            ItemEntry entry = Globals.API.Loader.Library.Items[enType];
            ContentManager manager = entry.Config.Manager;
            string path = entry.Config.EquipResourcePath;

            __result = entry.EquipData as HatInfo;

            Utils.ModUtils.TryLoadTex(Path.Combine(path, "Up"), manager, out __result.xDefaultSet.atxTextures[0]);
            Utils.ModUtils.TryLoadTex(Path.Combine(path, "Right"), manager, out __result.xDefaultSet.atxTextures[1]);
            Utils.ModUtils.TryLoadTex(Path.Combine(path, "Down"), manager, out __result.xDefaultSet.atxTextures[2]);
            Utils.ModUtils.TryLoadTex(Path.Combine(path, "Left"), manager, out __result.xDefaultSet.atxTextures[3]);

            foreach (var kvp in __result.denxAlternateVisualSets)
            {
                string altPath = Path.Combine(path, entry.HatAltSetResourcePaths[kvp.Key]);

                Utils.ModUtils.TryLoadTex(Path.Combine(altPath, "Up"), manager, out kvp.Value.atxTextures[0]);
                Utils.ModUtils.TryLoadTex(Path.Combine(altPath, "Right"), manager, out kvp.Value.atxTextures[1]);
                Utils.ModUtils.TryLoadTex(Path.Combine(altPath, "Down"), manager, out kvp.Value.atxTextures[2]);
                Utils.ModUtils.TryLoadTex(Path.Combine(altPath, "Left"), manager, out kvp.Value.atxTextures[3]);
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(WeaponCodex), nameof(WeaponCodex.GetWeaponInfo))]
        internal static bool OnGetWeaponInfo(ref WeaponInfo __result, ItemCodex.ItemTypes enType)
        {
            if (!enType.IsFromMod())
                return true;

            __result = Globals.API.Loader.Library.Items[enType].EquipData as WeaponInfo;

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(WeaponAssets.WeaponContentManager), "LoadBatch", new Type[] { typeof(Dictionary<ushort, string>) })] // Protected Method
        internal static bool OnLoadBatch(ref Dictionary<ushort, string> dis, WeaponAssets.WeaponContentManager __instance)
        {
            ItemCodex.ItemTypes type = __instance.enType;

            if (!type.IsFromMod())
                return true;

            ItemEntry entry = Globals.API.Loader.Library.Items[type];
            ContentManager manager = entry.Config.Manager;
            bool oneHanded = (entry.EquipData as WeaponInfo).enWeaponCategory == WeaponInfo.WeaponCategory.OneHanded;

            if (manager != null)
                __instance.contWeaponContent.RootDirectory = manager.RootDirectory;

            foreach (KeyValuePair<ushort, string> kvp in dis)
            {
                string resourcePath = Globals.API.Loader.Library.Items[type].Config.EquipResourcePath;
                string texPath = kvp.Value.Replace($"Weapons/{resourcePath}/", "");

                if (oneHanded)
                {
                    texPath = texPath.Replace("Sprites/Heroes/OneHanded/", resourcePath + "/");
                    texPath = texPath.Replace("Sprites/Heroes/Charge/OneHand/", resourcePath + "/1HCharge/");
                }
                else
                {
                    texPath = texPath.Replace("Sprites/Heroes/TwoHanded/", resourcePath + "/");
                    texPath = texPath.Replace("Sprites/Heroes/Charge/TwoHand/", resourcePath + "/2HCharge/");
                }

                Utils.ModUtils.TryLoadTex(texPath, __instance.contWeaponContent, out Texture2D tex);
                __instance.ditxWeaponTextures.Add(kvp.Key, tex);
            }

            return false;
        }
    }
}
