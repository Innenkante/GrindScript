using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CodeEnumerable = System.Collections.Generic.IEnumerable<HarmonyLib.CodeInstruction>;
using System.IO;

namespace SoG.Modding.Utils
{
    /// <summary>
    /// Provides various helper methods for transpiling the game.
    /// Throughout the code, CodeList is a shortcut for IEnumerable of CodeInstructions.
    /// </summary>
    public static class PatchTools
    {
        // How the stack changes based on a StackBehavior (in terms of objects)
        private static readonly Dictionary<StackBehaviour, int> __stackDeltas = new Dictionary<StackBehaviour, int>
        {
            { StackBehaviour.Pop0, 0 },
            { StackBehaviour.Pop1, -1 },
            { StackBehaviour.Pop1_pop1, -2 },
            { StackBehaviour.Popi, -1 },
            { StackBehaviour.Popi_pop1, -2 },
            { StackBehaviour.Popi_popi, -2 },
            { StackBehaviour.Popi_popi_popi, -3 },
            { StackBehaviour.Popi_popi8, -2 },
            { StackBehaviour.Popi_popr4, -2 },
            { StackBehaviour.Popi_popr8, -2 },
            { StackBehaviour.Popref, -1 },
            { StackBehaviour.Popref_pop1, -2 },
            { StackBehaviour.Popref_popi, -2 },
            { StackBehaviour.Popref_popi_pop1, -3 },
            { StackBehaviour.Popref_popi_popi, -3 },
            { StackBehaviour.Popref_popi_popi8, -3 },
            { StackBehaviour.Popref_popi_popr4, -3 },
            { StackBehaviour.Popref_popi_popr8, -3 },
            { StackBehaviour.Popref_popi_popref, -3 },
            { StackBehaviour.Push0, 0 },
            { StackBehaviour.Push1, 1 },
            { StackBehaviour.Push1_push1, 2 },
            { StackBehaviour.Pushi, 1 },
            { StackBehaviour.Pushi8, 1 },
            { StackBehaviour.Pushr4, 1 },
            { StackBehaviour.Pushr8, 1 },
            { StackBehaviour.Pushref, 1 },
            { StackBehaviour.Varpop, -1 },
            { StackBehaviour.Varpush, 1 }
        };

        /// <summary>
        /// Transpiles the given instruction set by inserting code instructions after the target method. <para/>
        /// If there are multiple calls of the target method, you can specify which one to insert after using methodIndex (zero-indexed). <para/>
        /// If the target method has a return value that is being used by subsquent code, you can specify ignoreLackOfPop = true to force the insertion.
        /// This is useful if you also use other insertions to create a ternary operator. <para/>
        /// </summary>
        /// <returns> The modified code, with new instructions inserted as described. </returns>
        /// <exception cref="Exception"> Thrown if the transpile fails due to the described incompatibilities. </exception>
        public static CodeEnumerable InsertAfterMethod(CodeEnumerable code, MethodInfo target, CodeEnumerable insert, int methodIndex = 0, bool missingPopIsOk = false, ConsoleLogger log = null)
        {
            int counter = methodIndex + 1;
            var noReturnValue = target.ReturnType == typeof(void);
            int stage = 0;

            foreach (CodeInstruction ins in code)
            {
                if (stage == 0)
                {
                    if (ins.Calls(target) && --counter == 0) 
                        stage = 1;
                }
                else if (stage == 1)
                {
                    if (!(ins.opcode == OpCodes.Pop || noReturnValue || missingPopIsOk)) 
                        throw new Exception("Transpile failed: insert target has return value that is being used!");

                    stage = 2;

                    if (ins.opcode == OpCodes.Pop) 
                        yield return ins;
                    foreach (CodeInstruction newIns in insert) 
                        yield return newIns;
                    if (ins.opcode == OpCodes.Pop)
                        continue;
                }
                yield return ins;
            }
            if (stage != 2) throw new Exception("Transpile failed: couldn't find target!");
        }

        /// <summary> 
        /// Transpiles the given instruction set by inserting code instructions before the target method. <para/>
        /// If there are multiple calls of the target method, you can specify which one to insert before of using methodIndex (zero-indexed). <para/>
        /// </summary>
        /// <returns> The modified code, with new instructions inserted as described. </returns>
        /// <exception cref="Exception"> Thrown if the transpile fails due to failing to find the target method, or if a suitable insertion point wasn't spotted. </exception>
        public static CodeEnumerable InsertBeforeMethod(CodeEnumerable code, MethodInfo target, CodeEnumerable insert, int methodIndex = 0, ConsoleLogger log = null)
        {
            List<CodeInstruction> codeStore = new List<CodeInstruction>();
            List<CodeInstruction> leftoverCode = new List<CodeInstruction>();
            int counter = methodIndex + 1;
            int stage = 0;

            foreach (CodeInstruction ins in code)
            {
                if (stage == 0 && ins.Calls(target) && --counter == 0) 
                    stage = 1;
                if (stage == 0) codeStore.Add(ins);
                else leftoverCode.Add(ins);
            }
            if (stage != 1) throw new Exception("Transpile failed: couldn't find target!");

            
            int insertIndex = codeStore.Count;
            int stackDelta = -1 * target.GetParameters().Length;
            if ((target.CallingConvention & CallingConventions.HasThis) == CallingConventions.HasThis)
                stackDelta -= 1; // Account for "this"

            if (stackDelta != 0) 
            {
                for (insertIndex = codeStore.Count - 1; insertIndex >= 0; insertIndex--)
                {
                    var ins = codeStore[insertIndex];
                    stackDelta += __stackDeltas[ins.opcode.StackBehaviourPush] + __stackDeltas[ins.opcode.StackBehaviourPop];

                    if (stackDelta == 0)
                    {
                        stage = 2;
                        break;
                    }
                    if (stackDelta > 0)
                        throw new Exception("Transpile failed: stackDelta has positive value!");
                }
            }
            else stage = 2;
            if (stage != 2)
                throw new Exception("Transpile failed: couldn't calculate position before method!");

            for (int index = 0; index < codeStore.Count; index++)
            {
                if (index == insertIndex)
                {
                    foreach (CodeInstruction ins in insert)
                        yield return ins;
                }
                yield return codeStore[index];
            }

            if (insertIndex == codeStore.Count)
            {
                foreach (CodeInstruction ins in insert)
                    yield return ins;
            }

            foreach (CodeInstruction ins in leftoverCode)
                yield return ins;
        }

        /// <summary>
        /// Calls InsertAfterMethod and InsertBeforeMethod, in that order.
        /// </summary>
        public static CodeEnumerable InsertAroundMethod(CodeEnumerable code, MethodInfo target, CodeEnumerable before, CodeEnumerable after, int methodIndex = 0, bool missingPopIsOk = false, ConsoleLogger log = null)
        {
            code = InsertAfterMethod(code, target, after, methodIndex, missingPopIsOk, log);
            return InsertBeforeMethod(code, target, before, methodIndex, log);
        }

        /// <summary>
        /// Adds the specified list of IL code, before the instruction at the given offset.
        /// </summary>
        public static CodeEnumerable InsertAt(CodeEnumerable code, CodeEnumerable insert, int offset)
        {
            bool inserted = false;
            foreach (CodeInstruction ins in code)
            {
                if (!inserted && offset-- <= 0)
                {
                    inserted = true;
                    foreach (CodeInstruction op in insert)
                        yield return op;
                }
                yield return ins;
            }
        }

        /// <summary>
        /// Removes the specified number of IL instructions, starting with the instruction at the given offset.
        /// </summary>
        public static CodeEnumerable RemoveAt(CodeEnumerable code, int toDelete, int offset)
        {
            foreach (CodeInstruction ins in code)
            {
                if (offset-- <= 0 && toDelete-- > 0)
                    continue;
                yield return ins;
            }
        }

        /// <summary>
        /// Calls RemoveAt, followed by InsertAt, effectively replacing the removed code with the inserted one.
        /// </summary>
        public static CodeEnumerable ReplaceAt(CodeEnumerable code, int toDelete, CodeEnumerable toInsert, int offset)
        {
            code = RemoveAt(code, toDelete, offset);
            return InsertAt(code, toInsert, offset);
        }

        /// <summary>
        /// Tries to retrieve the IL at the given offset.
        /// If found, true is returned, otherwise false.
        /// </summary>
        public static bool TryILAt(CodeEnumerable code, int offset, out OpCode op)
        {
            foreach (CodeInstruction ins in code)
            {
                if (offset-- > 0)
                    continue;
                op = ins.opcode;
                return true;
            }
            op = OpCodes.Nop;
            return false;
        }
    }
}
