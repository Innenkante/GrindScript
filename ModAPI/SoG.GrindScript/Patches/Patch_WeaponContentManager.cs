using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using HarmonyLib;
using SoG.Modding.LibraryEntries;
using WeaponAssets;
using SoG.Modding.Utils;

namespace SoG.Modding.Patches
{
    [HarmonyPatch(typeof(WeaponContentManager))]
    internal static class Patch_WeaponContentManager
    {
        [HarmonyPrefix]
        [HarmonyPatch("LoadBatch", typeof(Dictionary<ushort, string>))] // Protected Method
        internal static bool LoadBatch_Prefix(ref Dictionary<ushort, string> dis, WeaponContentManager __instance)
        {
            ItemCodex.ItemTypes type = __instance.enType;

            if (!type.IsFromMod())
                return true;

            ItemEntry entry = Globals.ModManager.Library.Items[type];
            bool oneHanded = (entry.EquipData as WeaponInfo).enWeaponCategory == WeaponInfo.WeaponCategory.OneHanded;

            foreach (KeyValuePair<ushort, string> kvp in dis)
            {
                string resourcePath = Globals.ModManager.Library.Items[type].Config.EquipResourcePath;
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

                AssetUtils.TryLoadTexture(texPath, __instance.contWeaponContent, out Texture2D tex);
                __instance.ditxWeaponTextures.Add(kvp.Key, tex);
            }

            return false;
        }
    }
}
