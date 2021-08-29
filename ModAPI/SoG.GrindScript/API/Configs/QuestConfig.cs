using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoG.Modding.API.Configs
{
    using Quests;

    public class QuestConfig
    {
        public QuestConfig(string uniqueID)
        {
            ModID = uniqueID;
        }

        public string ModID { get; set; }

        public string Name { get; set; } = "Unknown Mod Quest";

        public string Summary { get; set; } = "Some random quest from a mod! It's probably important.";

        public string Description { get; set; } = "Dunno man, ask the modder about the quest details! He forgot to put them in, shesh.";

        public int RecommendedPlayerLevel { get; set; } = 1;

        public QuestDescription.QuestType Type { get; set; } = QuestDescription.QuestType.Collect;

        public QuestReward Reward { get; set; }

        /// <summary>
        /// A method that receives a Quest instance (based off of this config) and sets its objectives.
        /// </summary>
        public Action<Quest> Constructor { get; set; }

        public QuestConfig DeepCopy()
        {
            QuestConfig clone = (QuestConfig)MemberwiseClone();

            // TODO: Make a deep copy that properly copies QuestReward

            return clone;
        }
    }
}
