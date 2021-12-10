using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SoG.InGameMenu;

namespace SoG.Modding.Extensions
{
    public static class SpellExtension
    {
        public static bool IsGeneralTalent(this SpellCodex.SpellTypes skill)
        {
            return new SkillView().lenGeneralTalentDisplayOrder.Contains(skill);
        }

        public static bool IsMeleeTalent(this SpellCodex.SpellTypes skill)
        {
            return new SkillView().lenMeleeTalentDisplayOrder.Contains(skill);
        }

        public static bool IsMagicTalent(this SpellCodex.SpellTypes skill)
        {
            return new SkillView().lenMagicTalentDisplayOrder.Contains(skill);
        }

        public static bool IsTalent(this SpellCodex.SpellTypes skill)
        {
            return SpellCodex.IsTalent(skill);
        }
    }
}
