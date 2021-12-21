using HarmonyLib;
using Quests;
using SoG.Modding.Content;

namespace SoG.Modding.Patching.Patches
{
    [HarmonyPatch(typeof(QuestCodex))]
    internal static class Patch_QuestCodex
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(QuestCodex.GetQuestDescription))]
        public static bool GetQuestDescription_Prefix(ref QuestDescription __result, QuestCodex.QuestID p_enID)
        {
            Globals.Manager.Library.GetEntry(p_enID, out QuestEntry entry);

            if (entry == null)
            {
                return true;  // Unknown mod entry?!
            }

            __result = entry.vanilla;

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(QuestCodex.GetQuestInstance))]
        public static bool GetQuestInstance_Prefix(ref Quest __result, QuestCodex.QuestID p_enID)
        {
            Globals.Manager.Library.GetEntry(p_enID, out QuestEntry entry);

            if (entry == null)
            {
                return true;  // Unknown mod entry?!
            }

            if (entry.constructor == null && entry.IsVanilla)
            {
                __result = OriginalMethods.GetQuestInstance(p_enID);
                return false;
            }

            __result = new Quest() { enQuestID = p_enID };
            __result.xDescription = entry.vanilla;

            entry.constructor.Invoke(__result);

            __result.xReward = entry.vanilla.xReward;

            return false;
        }

    }
}
