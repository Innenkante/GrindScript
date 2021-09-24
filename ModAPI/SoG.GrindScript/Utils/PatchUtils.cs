using HarmonyLib;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CodeEnumerable = System.Collections.Generic.IEnumerable<HarmonyLib.CodeInstruction>;

namespace SoG.Modding.Utils
{
    /// <summary>
    /// Provides helper methods for transpiling IL lists.
    /// </summary>
    public static class PatchUtils
    {
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
        /// Inserts IL instructions after the target method call (zero-indexed, counting from the start index provided). <para/>
        /// If the method returns an object, either the first IL instruction inserted must be OpCodes.Pop, or usesReturnValue must be true.
        /// </summary>
        /// <returns> The original code with the IL instructions inserted. </returns>
        /// <exception cref="InvalidOperationException"> 
        /// Thrown if the method was not found, or if it returns a value, 
        /// the first IL instruction is not OpCodes.Pop, and usesReturnValue is false.
        /// </exception>
        public static CodeEnumerable InsertAfterMethod(CodeEnumerable code, MethodInfo target, CodeEnumerable insert, int methodIndex = 0, int startIndex = 0, bool usesReturnValue = false)
        {
            int counter = methodIndex + 1;
            var noReturnValue = target.ReturnType == typeof(void);
            int stage = 0;
            int index = -1;

            foreach (CodeInstruction ins in code)
            {
                index++;

                if (stage == 0)
                {
                    if (index >= startIndex && ins.Calls(target) && --counter == 0) 
                        stage = 1;
                }
                else if (stage == 1)
                {
                    if (!(ins.opcode == OpCodes.Pop || noReturnValue || usesReturnValue))
                    {
                        throw new InvalidOperationException($"The method returns a value, but {nameof(usesReturnValue)} is false, and no OpCodes.Pop was found.");
                    }

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

            if (stage != 2)
            {
                throw new InvalidOperationException("Could not find the target method call.");
            }
        }

        /// <summary> 
        /// Inserts IL instructions before the target method call (zero-indexed, counting from the start index provided).
        /// If the method call's first IL instruction has any jump labels, these are shifted to the first instruction in the insert list.
        /// </summary>
        /// <returns> The original code with the IL instructions inserted.  </returns>
        /// <exception cref="Exception"> Thrown if the method was not found. </exception>
        public static CodeEnumerable InsertBeforeMethod(CodeEnumerable code, MethodInfo target, CodeEnumerable insert, int methodIndex = 0, int startIndex = 0)
        {
            List<CodeInstruction> codeStore = new List<CodeInstruction>();
            List<CodeInstruction> leftoverCode = new List<CodeInstruction>();
            int counter = methodIndex + 1;
            int stage = 0;
            int index = -1;
            CodeInstruction firstMethodCallInstruction = null;

            foreach (CodeInstruction ins in code)
            {
                index++;

                if (index >= startIndex && stage == 0 && ins.Calls(target) && --counter == 0)
                {
                    stage = 1;
                    firstMethodCallInstruction = ins;
                }

                if (stage == 0)
                {
                    codeStore.Add(ins);
                }
                else
                {
                    leftoverCode.Add(ins);
                }
            }

            if (stage != 1)
            {
                throw new InvalidOperationException("Could not find the target method call.");
            }
            
            int insertIndex = codeStore.Count;
            int stackDelta = -1 * target.GetParameters().Length;

            if ((target.CallingConvention & CallingConventions.HasThis) == CallingConventions.HasThis)
            {
                stackDelta -= 1;
            }

            if (stackDelta != 0) 
            {
                for (insertIndex = codeStore.Count - 1; insertIndex >= 0; insertIndex--)
                {
                    var ins = codeStore[insertIndex];
                    stackDelta += __stackDeltas[ins.opcode.StackBehaviourPush] + __stackDeltas[ins.opcode.StackBehaviourPop];

                    if (stackDelta == 0)
                    {
                        stage = 2;
                        firstMethodCallInstruction = codeStore[insertIndex];
                        break;
                    }
                    if (stackDelta > 0)
                        throw new InvalidOperationException("Instructions preceding the method have an invalid state.");
                }
            }
            else
            {
                stage = 2;
            }

            if (stage != 2)
            {
                throw new InvalidOperationException("Could not calculate insert position.");
            }

            for (index = 0; index < codeStore.Count; index++)
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

            CodeInstruction firstInserted = insert.First();

            // This is done due to methods that come right after scopes

            firstInserted.WithLabels(firstMethodCallInstruction.labels.ToArray());
            firstMethodCallInstruction.labels.Clear();
        }

        /// <summary>
        /// Calls InsertAfterMethod and InsertBeforeMethod, in that order, with the given arguments.
        /// </summary>
        public static CodeEnumerable InsertAroundMethod(CodeEnumerable code, MethodInfo target, CodeEnumerable before, CodeEnumerable after, int methodIndex = 0, int startIndex = 0, bool usesReturnValue = false)
        {
            code = InsertAfterMethod(code, target, after, methodIndex, startIndex, usesReturnValue);
            return InsertBeforeMethod(code, target, before, methodIndex, startIndex);
        }

        /// <summary>
        /// Adds IL instructions in the code section, before the given offset.
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
        /// Removes IL instructions in the code section, starting with the instruction at the given offset.
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
        /// Calls RemoveAt and InsertAt, in that order, replacing the deleted instructions with new ones.
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

        /// <summary>
        /// Returns the first position in the code list where the find criteria provided returns true.
        /// If not found, returns -1.
        /// </summary>
        public static int FindPosition(List<CodeInstruction> code, Func<List<CodeInstruction>, int, bool> findCriteria)
        {
            for (int index = 0; index < code.Count; index++)
            {
                if (findCriteria(code, index))
                {
                    return index;
                }
            }

            return -1;
        }
    }
}
