using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SoG.Modding.Utils;
using CodeList = System.Collections.Generic.IEnumerable<HarmonyLib.CodeInstruction>;

namespace SoG.Modding.Patches
{
    [HarmonyPatch(typeof(EnemyCodex))]
    internal static class Patch_EnemyCodex
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(EnemyCodex.GetEnemyDescription))]
        internal static bool GetEnemyDescription_Prefix(ref EnemyDescription __result, EnemyCodex.EnemyTypes enType)
        {
            if (!enType.IsFromMod())
                return true;

            __result = Globals.ModManager.Library.Enemies[enType].EnemyData;

            return false;
        }

        /// <summary>
        /// Implements custom enemy construction by transpiling the second part of GetEnemyInstance.
        /// (Note that our IDs will always trigger the condition for "CacuteForward" version to be called)
        /// </summary>
        [HarmonyTranspiler]
        [HarmonyPatch(nameof(EnemyCodex.GetEnemyInstance_CacuteForward))]
        internal static CodeList GetEnemyInstance_CacuteForward_Transpiler(CodeList code, ILGenerator gen)
        {
            // Assert to check if underlying method hasn't shifted heavily
            OpCode op = OpCodes.Nop;
            Debug.Assert(PatchUtils.TryILAt(code, 20, out op) && op == OpCodes.Ldstr, "GetEnemyInstance transpiler is invalid!");

            var insert = new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HelperCallbacks), nameof(HelperCallbacks.InGetEnemyInstance))),
                new CodeInstruction(OpCodes.Stloc_0) // Store returned enemy
            };

            return PatchUtils.InsertAt(code, insert, 20 + 2);
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(EnemyCodex.GetEnemyDefaultAnimation))]
        public static bool GetEnemyDefaultAnimation_Prefix(ref Animation __result, EnemyCodex.EnemyTypes enType, ContentManager Content)
        {
            if (!enType.IsFromMod())
            {
                return true;
            }

            __result = Globals.ModManager.Library.Enemies[enType].Config.DefaultAnimation?.Invoke(Content);

            if (__result == null)
            {
                __result = new Animation(1, 0, RenderMaster.txNullTex, Vector2.Zero);
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(EnemyCodex.GetEnemyDisplayIcon))]
        public static bool GetEnemyDisplayIcon_Prefix(ref Texture2D __result, EnemyCodex.EnemyTypes enType, ContentManager Content)
        {
            if (!enType.IsFromMod())
            {
                return true;
            }

            AssetUtils.TryLoadTexture(Globals.ModManager.Library.Enemies[enType].Config.DisplayIconPath, Content, out __result);

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(EnemyCodex.GetEnemyLocationPicture))]
        public static bool GetEnemyLocationPicture_Prefix(ref Texture2D __result, EnemyCodex.EnemyTypes enType, ContentManager Content)
        {
            if (!enType.IsFromMod())
            {
                return true;
            }

            AssetUtils.TryLoadTexture(Globals.ModManager.Library.Enemies[enType].Config.DisplayBackgroundPath, Content, out __result);

            return false;
        }

    }
}
