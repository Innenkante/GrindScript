using HarmonyLib;
using SoG.Modding.Content;
using PerkInfo = SoG.RogueLikeMode.PerkInfo;

namespace SoG.Modding.Patching.Patches
{
    [HarmonyPatch(typeof(PerkInfo))]
    internal static class Patch_RogueLikeMode_PerkInfo
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(PerkInfo.Init))]
        internal static bool InitPrefix()
        {
            PerkInfo.lxAllPerks.Clear();

            foreach (var pair in Globals.Manager.Library.GetAllEntries<RogueLikeMode.Perks, PerkEntry>())
            {
                if (pair.Value.unlockCondition == null || pair.Value.unlockCondition.Invoke())
                {
                    PerkInfo.lxAllPerks.Add(new PerkInfo(pair.Key, pair.Value.essenceCost, pair.Value.textEntry));
                }
            }

            return false;
        }
    }
}
