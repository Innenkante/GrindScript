using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private static readonly Dictionary<StackBehaviour, int> s_stackDeltas;

        static PatchUtils()
        {
            s_stackDeltas = new Dictionary<StackBehaviour, int>()
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
        }

        public static List<CodeInstruction> InsertAfterMethod(this List<CodeInstruction> code, MethodInfo target, List<CodeInstruction> insertedCode, int methodIndex = 0, int startOffset = 0, bool editsReturnValue = false)
        {
            int counter = methodIndex + 1;
            bool noReturnValue = target.ReturnType == typeof(void);

            int index = 0;
            int codeCount = code.Count;

            // Search for the method
            while (!(index >= startOffset && code[index].Calls(target) && --counter == 0))
            {
                if (index == codeCount)
                {
                    throw new InvalidOperationException("Could not find the target method call.");
                }

                index += 1;
            }

            int stackDelta = noReturnValue || editsReturnValue ? 0 : 1;

            int firstIndex = index;

            // Find method end

            while (stackDelta > 0 && firstIndex < code.Count)
            {
                firstIndex += 1;
                stackDelta += s_stackDeltas[code[firstIndex].opcode.StackBehaviourPush];
                stackDelta += s_stackDeltas[code[firstIndex].opcode.StackBehaviourPop];

                if (stackDelta < 0)
                {
                    throw new InvalidOperationException("Instructions after the method have an invalid state.");
                }
            }

            if (stackDelta != 0)
            {
                throw new InvalidOperationException("Could not calculate insert position.");
            }

            // For methods that come right before scopes, shift labels and stuff

            insertedCode[insertedCode.Count - 1].WithLabels(code[firstIndex + 1].labels.ToArray());
            code[firstIndex + 1].labels.Clear();

            code.InsertRange(firstIndex + 1, insertedCode);

            return code;
        }

        public static List<CodeInstruction> InsertBeforeMethod(this List<CodeInstruction> code, MethodInfo target, List<CodeInstruction> insertedCode, int methodIndex = 0, int startOffset = 0)
        {
            int counter = methodIndex + 1;

            int index = -1;
            int codeCount = code.Count;

            int stackDelta = -1 * target.GetParameters().Length;

            if ((target.CallingConvention & CallingConventions.HasThis) == CallingConventions.HasThis)
            {
                stackDelta -= 1;
            }

            // Search for method
            while (!(index >= startOffset && code[index].Calls(target) && --counter == 0))
            {
                if (index == codeCount)
                {
                    throw new InvalidOperationException("Could not find the target method call.");
                }

                index += 1;
            }

            // Throw if we couldn't find the method


            int firstIndex = index;

            // Find method start

            while (stackDelta < 0 && firstIndex > 0)
            {
                firstIndex -= 1;
                stackDelta += s_stackDeltas[code[firstIndex].opcode.StackBehaviourPush];
                stackDelta += s_stackDeltas[code[firstIndex].opcode.StackBehaviourPop];

                if (stackDelta > 0)
                {
                    throw new InvalidOperationException("Instructions preceding the method have an invalid state.");
                }
            }

            if (stackDelta != 0)
            {
                throw new InvalidOperationException("Could not calculate insert position.");
            }

            // For methods that come right after scopes, shift labels and stuff

            insertedCode[0].WithLabels(code[firstIndex].labels.ToArray());
            code[firstIndex].labels.Clear();

            code.InsertRange(firstIndex, insertedCode);

            return code;
        }

        public static List<CodeInstruction> InsertAroundMethod(this List<CodeInstruction> code, MethodInfo target, List<CodeInstruction> before, List<CodeInstruction> after, int methodIndex = 0, int startOffset = 0, bool editsReturnValue = false)
        {
            InsertAfterMethod(code, target, after, methodIndex, startOffset, editsReturnValue);
            InsertBeforeMethod(code, target, before, methodIndex, startOffset);

            return code;
        }

        public static List<CodeInstruction> InsertAt(this List<CodeInstruction> code, int position, List<CodeInstruction> insertedCode)
        {
            code.InsertRange(position, insertedCode);
            return code;
        }

        public static List<CodeInstruction> RemoveAt(this List<CodeInstruction> code, int position, int count)
        {
            code.RemoveRange(position, count);
            return code;
        }

        public static List<CodeInstruction> ReplaceAt(this List<CodeInstruction> code, int position, int count, List<CodeInstruction> insertedCode)
        {
            code.RemoveRange(position, count);
            code.InsertRange(position, insertedCode);
            return code;
        }

        public static List<CodeInstruction> ReplaceMethod(this List<CodeInstruction> code, MethodInfo target, MethodInfo replacement, int methodIndex = 0, int startOffset = 0)
        {
            var targetParams = target.GetParameters();
            var replacementParams = replacement.GetParameters();

            if (targetParams.Length != replacementParams.Length)
            {
                throw new InvalidOperationException("The target and the replacement have incompatible parameter lists");
            }

            for (int index = 0; index < targetParams.Length; index++)
            {
                var targetParam = targetParams[index];
                var replacementParam = replacementParams[index];

                bool notCompatible = targetParam.ParameterType != replacementParam.ParameterType ||
                    targetParam.IsIn != replacementParam.IsIn ||
                    targetParam.IsOut != replacementParam.IsOut ||
                    targetParam.IsRetval != replacementParam.IsRetval;

                if (notCompatible)
                {
                    throw new InvalidOperationException("The target and the replacement have incompatible parameter lists");
                }
            }

            for (int index = 0; index < code.Count; index++)
            {
                if (code[index].Calls(target))
                {
                    code[index].operand = replacement;
                }
            }

            return code;
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
