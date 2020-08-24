using System;
using SoG.GrindScript;

namespace SoG.TrainingArenaMod
{
    public class TrainingArenaMod : BaseScript
    {
        public TrainingArenaMod()
        {
            Console.WriteLine("Are you ready to TRAIIIIIIIIIIIIN?!?!?");
        }

        public override void PostPlayerLevelUp(Player player)
        {
            if ((player.Level - 1) < 6 || (player.Level - 1) % 3 > 0)
            {
                player.SilverSkillPoints += 1;
                player.GoldSkillPoints += 1;
                player.TalentPoints += 1;
            }
            else
            {
                player.SilverSkillPoints += 2;
                player.GoldSkillPoints += 2;
                player.TalentPoints += 2;
            }
        }
    }
}
