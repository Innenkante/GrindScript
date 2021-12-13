using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace SoG.Modding.Patching
{
    [HarmonyPatch]
    internal static class EditedMethods
    {
        [HarmonyReversePatch]
        [HarmonyPatch(typeof(EnemyCodex), nameof(EnemyCodex.GetEnemyInstance_CacuteForward))]
        public static Enemy GetModdedEnemyInstance(EnemyCodex.EnemyTypes enType, Level.WorldRegion enOverrideContent)
        {
            IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                List<CodeInstruction> codeList = instructions.ToList();

                int start = codeList.FindIndex(x => x.opcode == OpCodes.Ldstr && x.operand.Equals("Sprites/Monster/")) + 2;

                int end = codeList.FindIndex(x => x.opcode == OpCodes.Call && x.operand.Equals(AccessTools.Method(typeof(EnemyCodex), nameof(EnemyCodex.GetEnemyDescription)))) - 2;

                codeList[end].labels.Clear();

                codeList.RemoveRange(start, end - start);
                codeList.InsertRange(start, new List<CodeInstruction>()
                {
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldarg_1),
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PatchHelper), nameof(PatchHelper.InGetEnemyInstance))),
                    new CodeInstruction(OpCodes.Stloc_0) // Store returned enemy
                });

                return codeList;
            }

            _ = Transpiler(null);
            throw new InvalidOperationException("Stub method.");
        }

        // This method doesn't apply a new name and doesn't grant bonuses (yet)
        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Game1), nameof(Game1._Enemy_MakeElite))]
        public static Enemy _Enemy_MakeElite(Game1 __instance, EnemyCodex.EnemyTypes enType, Level.WorldRegion enOverrideContent)
        {
            IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                List<CodeInstruction> codeList = instructions.ToList();

                int start = codeList.FindIndex(x => x.opcode == OpCodes.Ldstr && x.operand.Equals("Sprites/Monster/")) + 8;

                int end = codeList.FindIndex(x => x.opcode == OpCodes.Ldloc_1);

                codeList.RemoveRange(start, end - start);
                codeList.InsertRange(start, new List<CodeInstruction>()
                {
                    new CodeInstruction(OpCodes.Ldarg_1),
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PatchHelper), nameof(PatchHelper.InEnemyMakeElite))),
                    new CodeInstruction(OpCodes.Stloc_1), // Store elite status in bRet
                });

                return codeList;
            }

            _ = Transpiler(null);
            throw new InvalidOperationException("Stub method.");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Game1), nameof(Game1._LevelLoading_DoStuff))]
        public static void _LevelLoading_DoStuff(Game1 __instance, Level.ZoneEnum enLevel, bool bStaticOnly)
        {
            IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                List<CodeInstruction> codeList = instructions.ToList();

                int firstLdarg = codeList.FindIndex(x => x.opcode == OpCodes.Ldarg_1);

                int start = codeList.FindIndex(firstLdarg + 1, x => x.opcode == OpCodes.Ldarg_1);

                MethodInfo target = AccessTools.Method(typeof(Quests.QuestLog), nameof(Quests.QuestLog.UpdateCheck_PlaceVisited));

                int end = codeList.FindIndex(x => x.opcode == OpCodes.Callvirt && x.operand.Equals(target)) - 5;

                List<CodeInstruction> inserted = new List<CodeInstruction>()
                {
                    new CodeInstruction(OpCodes.Ldarg_S, 1).WithLabels(codeList[start].labels).WithBlocks(codeList[start].blocks),
                    new CodeInstruction(OpCodes.Ldarg_S, 2),
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PatchHelper), nameof(PatchHelper.InLevelLoadDoStuff)))
                };

                codeList[end].labels.Clear();
                codeList[end].blocks.Clear();

                codeList.RemoveRange(start, end - start);

                codeList.InsertRange(start, inserted);

                return codeList;
            }

            _ = Transpiler(null);
            throw new InvalidOperationException("Stub method.");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Game1), nameof(Game1._RogueLike_ActivatePin))]
        public static void ApplyPinEffect(Game1 __instance, PlayerView xView, PinCodex.PinType enEffect, bool bSend)
        {
            IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                List<CodeInstruction> codeList = instructions.ToList();

                int ldarg2_pos = 3;
                int start = -1;
                while (ldarg2_pos-- > 0)
                {
                    start = codeList.FindIndex(start + 1, x => x.opcode == OpCodes.Ldarg_2);
                }

                int end = codeList.FindLastIndex(x => x.opcode == OpCodes.Ret);

                codeList[start].labels.Clear();

                codeList.RemoveRange(0, start);

                return codeList;
            }

            _ = Transpiler(null);
            throw new InvalidOperationException("Stub method.");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Game1), nameof(Game1._RogueLike_ActivatePin))]
        public static void SendPinActivation(Game1 __instance, PlayerView xView, PinCodex.PinType enEffect, bool bSend)
        {
            IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                List<CodeInstruction> codeList = instructions.ToList();

                int ldarg2_pos = 3;
                int start = -1;
                while (ldarg2_pos-- > 0)
                {
                    start = codeList.FindIndex(start + 1, x => x.opcode == OpCodes.Ldarg_2);
                }

                int end = codeList.FindLastIndex(x => x.opcode == OpCodes.Ret);

                codeList[end].labels.Clear();
                codeList[end].WithLabels(codeList[start].labels);

                codeList.RemoveRange(start, end - start);

                return codeList;
            }

            _ = Transpiler(null);
            throw new InvalidOperationException("Stub method.");
        }



        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Game1), nameof(Game1._RogueLike_DeactivatePin))]
        public static void RemovePinEffect(Game1 __instance, PlayerView xView, PinCodex.PinType enEffect, bool bSend)
        {
            IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                List<CodeInstruction> codeList = instructions.ToList();

                int ldarg2_pos = 3;
                int start = -1;
                while (ldarg2_pos-- > 0)
                {
                    start = codeList.FindIndex(start + 1, x => x.opcode == OpCodes.Ldarg_2);
                }

                int end = codeList.FindLastIndex(x => x.opcode == OpCodes.Ret);

                codeList[start].labels.Clear();

                codeList.RemoveRange(0, start);

                return codeList;
            }

            _ = Transpiler(null);
            throw new InvalidOperationException("Stub method.");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Game1), nameof(Game1._RogueLike_DeactivatePin))]
        public static void SendPinDeactivation(Game1 __instance, PlayerView xView, PinCodex.PinType enEffect, bool bSend)
        {
            IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                List<CodeInstruction> codeList = instructions.ToList();

                int ldarg2_pos = 3;
                int start = -1;
                while (ldarg2_pos-- > 0)
                {
                    start = codeList.FindIndex(start + 1, x => x.opcode == OpCodes.Ldarg_2);
                }

                int end = codeList.FindLastIndex(x => x.opcode == OpCodes.Ret);

                codeList[end].labels.Clear();
                codeList[end].WithLabels(codeList[start].labels);

                codeList.RemoveRange(start, end - start);

                return codeList;
            }

            _ = Transpiler(null);
            throw new InvalidOperationException("Stub method.");
        }
    }
}
