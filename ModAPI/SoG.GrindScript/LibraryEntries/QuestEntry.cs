using Quests;
using SoG.Modding.Configs;

namespace SoG.Modding.LibraryEntries
{
    internal class QuestEntry : IEntry<QuestCodex.QuestID>
    {
        public Mod Owner { get; set; }

        public Quests.QuestCodex.QuestID GameID { get; set; }

        public string ModID { get; set; }

        public QuestConfig Config { get; set; }

        public Quests.QuestDescription QuestData { get; set; }

        public QuestEntry(Mod owner, Quests.QuestCodex.QuestID gameID, string modID)
        {
            Owner = owner;
            GameID = gameID;
            ModID = modID;
        }
    }
}
