using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoG.Modding.Core;
using SoG.Modding.API.Configs;
using SoG.Modding.ModUtils;

namespace SoG.Modding.API
{
    public abstract partial class Mod
    {
        public EnemyCodex.EnemyTypes CreateEnemy(EnemyConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            if (config.Constructor == null)
            {
                throw new ArgumentException("config's Constructor must not be null!");
            }

            if (!InLoad)
            {
                Globals.Logger.Error("Can not create objects outside of a load context.", source: nameof(CreateEnemy));
                return EnemyCodex.EnemyTypes.Null;
            }

            if (GetLibrary().Enemies.Any(x => x.Value.ModID == config.ModID))
            {
                Globals.Logger.Error($"An enemy with the ModID {config.ModID} already exists.", source: nameof(CreateEnemy));
                return EnemyCodex.EnemyTypes.Null;
            }

            EnemyCodex.EnemyTypes gameID = Registry.ID.EnemyIDNext++;

            EnemyDescription enemyData = new EnemyDescription(gameID, config.Category, $"{gameID}_Name", config.Level, config.BaseHealth)
            {
                sOnHitSound = config.OnHitSound,
                sOnDeathSound = config.OnDeathSound,
                sFullName = config.Name,
                sCardDescriptionLibraryHandle = $"{gameID}_Card",
                sDetailedDescriptionLibraryHandle = $"{gameID}_Description",
                sFlavorLibraryHandle = $"{gameID}_Flavor",
                sCardDescription = config.CardInfo,
                sDetailedDescription = config.LongDescription,
                sFlavorText = config.ShortDescription,
                iCardDropChance = (int)(100f / config.CardDropChance),
            };

            ModEnemyEntry entry = new ModEnemyEntry(this, gameID, config.ModID)
            {
                Config = config.DeepCopy(),
                EnemyData = enemyData
            };

            Registry.Library.Enemies[gameID] = entry;
            
            config.LootTable.ForEach(x => enemyData.lxLootTable.Add(new DropChance((int)(1000 * x.Chance), x.Item)));

            Globals.Game.EXT_AddMiscText("Enemies", enemyData.sNameLibraryHandle, enemyData.sFullName);
            Globals.Game.EXT_AddMiscText("Enemies", enemyData.sFlavorLibraryHandle, enemyData.sFlavorText);
            Globals.Game.EXT_AddMiscText("Enemies", enemyData.sCardDescriptionLibraryHandle, enemyData.sCardDescription);
            Globals.Game.EXT_AddMiscText("Enemies", enemyData.sDetailedDescriptionLibraryHandle, enemyData.sDetailedDescription);

            if (config.CardDropChance != 0 && config.CardDropOverride == EnemyCodex.EnemyTypes.Null)
            {
                // Add a Card entry in the Journal
                EnemyCodex.lxSortedCardEntries.Add(enemyData);
            }

            if (config.CreateJournalEntry)
            {
                // Add an Enemy entry in the Journal
                EnemyCodex.lxSortedDescriptions.Add(enemyData);
            }

            return gameID;
        }
    }
}
