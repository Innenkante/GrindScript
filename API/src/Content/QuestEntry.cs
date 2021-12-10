using Quests;
using System;

namespace SoG.Modding.Content
{
    /// <summary>
    /// Represents a modded quest.
    /// </summary>
    /// <remarks> 
    /// Most of the methods in this class can only be used while a mod is loading, that is, inside <see cref="Mod.Load"/>.
    /// </remarks>
    public class QuestEntry : Entry<QuestCodex.QuestID>
    {
        #region IEntry Properties

        public override Mod Mod { get; internal set; }

        public override Quests.QuestCodex.QuestID GameID { get; internal set; }

        public override string ModID { get; internal set; }

        #endregion

        #region Internal Data

        internal string name = "Unknown Mod Quest";

        internal string summary = "Some random quest from a mod! It's probably important.";

        internal string description = "Dunno man, ask the modder about the quest details! He forgot to put them in, shesh.";

        internal Action<Quest> constructor;

        internal QuestDescription vanilla = new QuestDescription();

        #endregion

        #region Public Interface

        /// <summary>
        /// Gets or sets the display name of this quest.
        /// </summary>
        public string Name
        {
            get => name;
            set
            {
                ErrorHelper.ThrowIfNotLoading(Mod);
                name = value;
            }
        }

        /// <summary>
        /// Gets or sets the description of this quest's entry in the journal.
        /// </summary>
        public string Summary
        {
            get => summary;
            set
            {
                ErrorHelper.ThrowIfNotLoading(Mod);
                summary = value;
            }
        }

        /// <summary>
        /// Gets or sets the full description of this quest. <para/>
        /// The full description is displayed whenever you start a quest.
        /// </summary>
        public string Description
        {
            get => description;
            set
            {
                ErrorHelper.ThrowIfNotLoading(Mod);
                description = value;
            }
        }

        /// <summary>
        /// Gets or sets the recommended player level.
        /// The difficulty star count in the journal adapts based on the difference between
        /// the player level and the recommended level of the quest.
        /// </summary>
        public int RecommendedPlayerLevel
        {
            get => vanilla.iIntendedLevel;
            set
            {
                ErrorHelper.ThrowIfNotLoading(Mod);
                vanilla.iIntendedLevel = value;
            }
        }

        /// <summary>
        /// Gets or sets the quest's type.
        /// </summary>
        public QuestDescription.QuestType Type
        {
            get => vanilla.enType;
            set
            {
                ErrorHelper.ThrowIfNotLoading(Mod);
                vanilla.enType = value;
            }
        }

        /// <summary>
        /// Gets or sets the quest's reward.
        /// </summary>
        public QuestReward Reward
        {
            get => vanilla.xReward;
            set
            {
                ErrorHelper.ThrowIfNotLoading(Mod);
                vanilla.xReward = value;
            }
        }

        /// <summary>
        /// Gets or sets the constructor for the quest instance. <para/>
        /// The constructor is called wheneever a new quest of this type is started.
        /// You can use it to setup objectives and other things.
        /// </summary>
        public Action<Quest> Constructor
        {
            get => constructor;
            set
            {
                ErrorHelper.ThrowIfNotLoading(Mod);
                constructor = value;
            }
        }

        #endregion

        internal QuestEntry() { }

        internal QuestEntry(Mod owner, Quests.QuestCodex.QuestID gameID, string modID)
        {
            Mod = owner;
            GameID = gameID;
            ModID = modID;

            SymbolicItemFlagReward noReward = new SymbolicItemFlagReward();
            noReward.AddItem(ItemCodex.ItemTypes._Misc_BagLol, 1);

            vanilla.xReward = noReward;
        }

        internal override void Initialize()
        {
            vanilla.sQuestNameReference = $"Quest_{(int)GameID}_Name";
            vanilla.sSummaryReference = $"Quest_{(int)GameID}_Summary";
            vanilla.sDescriptionReference = $"Quest_{(int)GameID}_Description";

            Globals.Game.EXT_AddMiscText("Quests", vanilla.sQuestNameReference, name);
            Globals.Game.EXT_AddMiscText("Quests", vanilla.sSummaryReference, summary);
            Globals.Game.EXT_AddMiscText("Quests", vanilla.sDescriptionReference, description);
        }

        internal override void Cleanup()
        {
            Globals.Game.EXT_RemoveMiscText("Quests", vanilla.sQuestNameReference);
            Globals.Game.EXT_RemoveMiscText("Quests", vanilla.sSummaryReference);
            Globals.Game.EXT_RemoveMiscText("Quests", vanilla.sDescriptionReference);
        }
    }
}
