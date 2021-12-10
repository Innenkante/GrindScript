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
            if (!p_enID.IsFromMod())
            {
                return true;
            }

            var storage = Globals.ModManager.Library.GetStorage<QuestCodex.QuestID, QuestEntry>();

            __result = storage[p_enID].vanilla;

            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(QuestCodex.GetQuestInstance))]
        public static void GetQuestInstance_Postfix(ref Quest __result, QuestCodex.QuestID p_enID)
        {
            if (!p_enID.IsFromMod())
            {
                return;
            }

            var storage = Globals.ModManager.Library.GetStorage<QuestCodex.QuestID, QuestEntry>();

            storage[p_enID].constructor?.Invoke(__result);

            __result.xReward = storage[p_enID].vanilla.xReward;
        }

    }
}
