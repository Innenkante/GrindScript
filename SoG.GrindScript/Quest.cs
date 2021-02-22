using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoG.GrindScript
{
    public class QuestObject
    {
        public bool Finished { get; set; }
    }

    public class SingleKillQuestObject : QuestObject
    {

    }

    public class MultipleKillQuestObject : QuestObject
    {

    }


    public class Quest
    {
        public string Name { get; }
        public string Summary { get; }
        public int Level { get; }
        public Action<Player> Reward { get; }

        public Quest(string name, string summary, int level, Action<Player> reward)
        {
            Name = name;
            Summary = summary;
            Level = level;
            Reward = reward;
        }
    }

    public class SingleKillQuest : Quest
    {
        public EnemyCodex.EnemyTypes EnemyToKill { get; }

        public SingleKillQuest(string name, string summary, int level, Action<Player> reward, EnemyCodex.EnemyTypes enemyToKill) : base(name, summary, level, reward)
        {
            EnemyToKill = enemyToKill;
        }
    }

    public class MultipleKillQuest : Quest
    {
        public EnemyCodex.EnemyTypes EnemiesToKill { get; }

        public MultipleKillQuest(string name, string summary, int level, Action<Player> reward, EnemyCodex.EnemyTypes enemiesToKill) : base(name, summary, level, reward)
        {
            EnemiesToKill = enemiesToKill;
        }
    }


}
