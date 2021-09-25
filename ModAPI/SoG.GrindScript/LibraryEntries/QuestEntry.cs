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

        public void Initialize()
        {
            Globals.Game.EXT_AddMiscText("Quests", QuestData.sQuestNameReference, Config.Name);
            Globals.Game.EXT_AddMiscText("Quests", QuestData.sSummaryReference, Config.Summary);
            Globals.Game.EXT_AddMiscText("Quests", QuestData.sDescriptionReference, Config.Description);

        }

        public void Cleanup()
        {
            Globals.Game.EXT_RemoveMiscText("Quests", QuestData.sQuestNameReference);
            Globals.Game.EXT_RemoveMiscText("Quests", QuestData.sSummaryReference);
            Globals.Game.EXT_RemoveMiscText("Quests", QuestData.sDescriptionReference);
        }
    }
}
