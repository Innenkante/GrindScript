using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Xna.Framework;

namespace SoG.Modding.Addons
{
    [HarmonyPatch]
    [HarmonyPriority(Priority.LowerThanNormal)]
    public static class Patch_MiscStuff
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerViewStats), nameof(PlayerViewStats.GetSkillLevel))]
        public static bool GetSkillLevelPrefix(PlayerViewStats __instance, SpellCodex.SpellTypes enType, ref byte __result)
        {
            __result = ModGoodies.TheMod.GetModifiedSkillLevel(__instance, enType);

            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Game1), nameof(Game1._InGameMenu_RenderSkills_RenderTalent))]
        public static void RenderTalentPostfix(Vector2 v2Pos, Color cColor, float fAlpha, float fScale, SpellCodex.SpellTypes enType)
        {
            int delta = ModGoodies.TheMod.GetModifiedSkillLevel(Globals.Game.xLocalPlayer.xViewStats, enType) - ModGoodies.TheMod.GetTrueSkillLevel(Globals.Game.xLocalPlayer.xViewStats, enType);

            Color color = Color.GreenYellow;
            if (delta == 0)
            {
                color = Color.Silver;
            }
            else if (delta < 0)
            {
                color = Color.OrangeRed;
            }

            string sign = "+";
            if (delta == 0)
            {
                sign = "";
            }
            else if (delta < 0)
            {
                sign = "-";
            }

            Globals.Game._RenderMaster_RenderTextWithOutline(FontManager.GetFont(FontManager.FontType.Reg7), sign + Math.Abs(delta).ToString(), v2Pos + new Vector2(-8f, 18f), Vector2.Zero, fScale, color, Color.Black);
        }
    }
}
