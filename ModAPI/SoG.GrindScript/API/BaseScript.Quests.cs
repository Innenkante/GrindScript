using SoG.Modding.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoG.Modding.Extensions;

namespace SoG.Modding.API
{
    using Quests;
    using SoG.Modding.API.Configs;

    public abstract partial class BaseScript
    {
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

            BaseScript mod = ModAPI.Registry.LoadContext;

            if (mod == null)
            {
                Globals.Logger.Error("Can not create objects outside of a load context.", source: nameof(CreateQuest));
                return QuestCodex.QuestID.None;
            }

            QuestCodex.QuestID gameID = ModAPI.Registry.ID.QuestIDNext++;

            // TODO: Write remaining quest creation code, plus patches

            QuestDescription questData = new QuestDescription()
            {
                sQuestNameReference = $"Quest_{config.ModID}_Name",
                sSummaryReference = $"Quest_{config.ModID}_Summary",
                sDescriptionReference = $"Quest_{config.ModID}_Description",
                iIntendedLevel = config.RecommendedPlayerLevel,
                enType = config.Type,
                xReward = config.Reward
            };

            ModQuestEntry entry = new ModQuestEntry(mod, gameID, config.ModID)
            {
                Config = config.DeepCopy(),
                QuestData = questData
            };

            ModAPI.Registry.Library.Quests[gameID] = entry;
            mod.Library.Quests[gameID] = entry;

            Globals.Game.EXT_AddMiscText("Quests", questData.sQuestNameReference, config.Name);
            Globals.Game.EXT_AddMiscText("Quests", questData.sSummaryReference, config.Summary);
            Globals.Game.EXT_AddMiscText("Quests", questData.sDescriptionReference, config.Description);

            return gameID;
        }

        public Objective_SpecialObjective.UniqueID CreateSpecialObjective()
        {
            return ModAPI.Registry.ID.SpecialObjectiveIDNext++;
        }
    }
}
