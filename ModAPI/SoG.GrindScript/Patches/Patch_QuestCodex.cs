using HarmonyLib;
using Quests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoG.Modding.Patches
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

            __result = Globals.ModManager.Library.Quests[p_enID].QuestData;

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

            Globals.ModManager.Library.Quests[p_enID].Config.Constructor?.Invoke(__result);

            __result.xReward = Globals.ModManager.Library.Quests[p_enID].QuestData.xReward;
        }

    }
}
