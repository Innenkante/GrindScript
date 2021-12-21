using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using SoG.Modding.Content;
using SoG.Modding.Utils;
using System.Collections.Generic;
using WeaponAssets;
using System;

namespace SoG.Modding.Patching.Patches
{
    [HarmonyPatch(typeof(WeaponContentManager))]
    internal static class Patch_WeaponContentManager
    {
        /// <summary>
        /// Loads weapon assets for a mod entry.
        /// For entries that do not use the vanilal resource format, a shortened folder structure is used.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch("LoadBatch", typeof(Dictionary<ushort, string>))] // Protected Method
        internal static bool LoadBatch_Prefix(ref Dictionary<ushort, string> dis, WeaponContentManager __instance)
        {
            Globals.Manager.Library.GetEntry(__instance.enType, out ItemEntry entry);

            ErrorHelper.Assert(entry != null, ErrorHelper.UnknownEntry);

            bool oneHanded = (entry.vanillaEquip as WeaponInfo).enWeaponCategory == WeaponInfo.WeaponCategory.OneHanded;

            string resourcePath = entry.equipResourcePath;

            foreach (KeyValuePair<ushort, string> kvp in dis)
            {
                string texPath = kvp.Value;

                if (!entry.useVanillaResourceFormat)
                {
                    texPath = texPath.Replace($"Weapons/{resourcePath}/", "");

                    if (oneHanded)
                    {
                        texPath = texPath
                            .Replace("Sprites/Heroes/OneHanded/", resourcePath + "/")
                            .Replace("Sprites/Heroes/Charge/OneHand/", resourcePath + "/1HCharge/");
                    }
                    else
                    {
                        texPath = texPath
                            .Replace("Sprites/Heroes/TwoHanded/", resourcePath + "/")
                            .Replace("Sprites/Heroes/Charge/TwoHand/", resourcePath + "/2HCharge/");
                    }
                }

                AssetUtils.TryLoadTexture(texPath, __instance.contWeaponContent, out Texture2D tex);
                __instance.ditxWeaponTextures.Add(kvp.Key, tex);
            }

            return false;
        }
    }
}
