using SoG.Modding.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoG.Modding.Extensions;
using Quests;
using SoG.Modding.API.Configs;

namespace SoG.Modding.API
{
    public abstract partial class Mod
    {
        /// <summary>
        /// Creates a new quest from the given QuestConfig.
        /// The quest must have a reward defined for it to be valid.
        /// </summary>
        public QuestCodex.QuestID CreateQuest(QuestConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            if (config.Reward == null)
            {
                throw new ArgumentException("config's Reward must not be null!");
            }

            if (!InLoad)
            {
                Globals.Logger.Error("Can not create objects outside of a load context.", source: nameof(CreateQuest));
                return QuestCodex.QuestID.None;
            }

            QuestCodex.QuestID gameID = Registry.ID.QuestIDNext++;

            // TODO: Write remaining quest creation code, plus patches

            QuestDescription questData = new QuestDescription()
            {
                sQuestNameReference = $"Quest_{(int)gameID}_Name",
                sSummaryReference = $"Quest_{(int)gameID}_Summary",
                sDescriptionReference = $"Quest_{(int)gameID}_Description",
                iIntendedLevel = config.RecommendedPlayerLevel,
                enType = config.Type,
                xReward = config.Reward
            };

            ModQuestEntry entry = new ModQuestEntry(this, gameID, config.ModID)
            {
                Config = config.DeepCopy(),
                QuestData = questData
            };

            Registry.Library.Quests[gameID] = entry;

            Globals.Game.EXT_AddMiscText("Quests", questData.sQuestNameReference, config.Name);
            Globals.Game.EXT_AddMiscText("Quests", questData.sSummaryReference, config.Summary);
            Globals.Game.EXT_AddMiscText("Quests", questData.sDescriptionReference, config.Description);

            return gameID;
        }

        public Objective_SpecialObjective.UniqueID CreateSpecialObjective()
        {
            return Registry.ID.SpecialObjectiveIDNext++;
        }
    }
}
