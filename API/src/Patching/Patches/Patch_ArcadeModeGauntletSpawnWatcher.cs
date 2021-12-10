using HarmonyLib;
using SoG.Modding.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Watchers;
using CodeList = System.Collections.Generic.IEnumerable<HarmonyLib.CodeInstruction>;

namespace SoG.Modding.Patching.Patches
{
    [HarmonyPatch(typeof(ArcadeModeGauntletSpawnWatcher))]
    internal static class Patch_ArcadeModeGauntletSpawnWatcher
    {
        [HarmonyPatch(nameof(ArcadeModeGauntletSpawnWatcher.Update))]
        [HarmonyTranspiler]
        internal static CodeList Update_Transpiler(CodeList code, ILGenerator gen)
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

            var insert = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldloc_S, 5),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PatchHelper), nameof(PatchHelper.GauntletEnemySpawned))),
            };

            return codeList.InsertAt(position, insert);
        }
    }
}
