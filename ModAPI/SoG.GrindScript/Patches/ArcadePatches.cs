using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using SoG.Modding.Core;
using SoG.Modding.Extensions;
using SoG.Modding.ModUtils;
using SoG.Modding.API;

namespace SoG.Modding.Patches
{
    using CodeList = System.Collections.Generic.IEnumerable<HarmonyLib.CodeInstruction>;

    /// <summary>
    /// Contains arcade-related patches.
    /// </summary>

    [HarmonyPatch]
    internal static class ArcadePatches
    {
        /// <summary>
        /// Inserts custom curses in the Curse shop menu.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ShopMenu.TreatCurseMenu), "FillCurseList")]
        internal static void PostFillCurseList(ShopMenu.TreatCurseMenu __instance)
        {
            foreach (var kvp in Globals.API.Loader.Library.Curses)
            {
                if (!kvp.Value.Config.IsTreat)
                    __instance.lenTreatCursesAvailable.Add(kvp.Value.GameID);
            }
        }

        /// <summary>
        /// Inserts custom curses in the Treat shop menu.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ShopMenu.TreatCurseMenu), "FillTreatList")]
        internal static void PostFillTreatList(ShopMenu.TreatCurseMenu __instance)
        {
            foreach (var kvp in Globals.API.Loader.Library.Curses)
            {
                if (kvp.Value.Config.IsTreat)
                    __instance.lenTreatCursesAvailable.Add(kvp.Value.GameID);
            }
        }

        /// <summary>
        /// Inserts custom perks in the Perk shop menu.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(RogueLikeMode.PerkInfo), "Init")]
        internal static void PostPerkListInit()
        {
            foreach (var perk in Globals.API.Loader.Library.Perks.Values)
                RogueLikeMode.PerkInfo.lxAllPerks.Add(new RogueLikeMode.PerkInfo(perk.GameID, perk.Config.EssenceCost, perk.TextEntry));
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Bagmen.OpenGatesAtRoomClear), "OpenBlockades")]
        internal static void PostArcadeRoomEnd()
        {
            foreach (Mod mod in Globals.API.Loader.Mods)
                mod.PostArcadeRoomComplete();
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(Watchers.ArcadeModeGauntletSpawnWatcher), "Update")]
        internal static CodeList GauntletSpawnWatcherUpdateTranspiler(CodeList code, ILGenerator gen)
        {
            List<CodeInstruction> codeList = code.ToList();

            int position = -1;

            for (int index = 0; index + 1 < codeList.Count; index++)
            {
                bool found =
                    codeList[index].opcode == OpCodes.Stfld &&
                    codeList[index + 1].opcode == OpCodes.Ret;

                if (found)
                {
                    position = index + 1;
                    break;
                }
            }

            var insert = new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Ldloc_S, 5),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HelperCallbacks), nameof(HelperCallbacks.GauntletEnemySpawned))),
            };

            return PatchUtils.InsertAt(codeList, insert, position);
        }
    }
}
