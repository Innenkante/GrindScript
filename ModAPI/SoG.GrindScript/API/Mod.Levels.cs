using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SoG.Modding.Core;
using System.Diagnostics;
using SoG.Modding.ModUtils;
using SoG.Modding.API.Configs;

namespace SoG.Modding.API
{
    public abstract partial class Mod
    {
        /// <summary>
        /// Creates a new world region, and returns its ID.
        /// </summary>
        public Level.WorldRegion CreateWorldRegion()
        {
            var VanillaContent = Globals.Game.Content;

            var gameID = Registry.ID.WorldIDNext++;

            Globals.Game.xLevelMaster.denxRegionContent.Add(gameID, new ContentManager(VanillaContent.ServiceProvider, VanillaContent.RootDirectory));

            return gameID;
        }

        /// <summary>
        /// Creates a new level from the given LevelConfig.
        /// </summary>
        public Level.ZoneEnum CreateLevel(LevelConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            if (!InLoad)
            {
                Globals.Logger.Error("Can not create objects outside of a load context.", source: nameof(CreateLevel));
                return Level.ZoneEnum.None;
            }

            Level.ZoneEnum gameID = Registry.ID.LevelIDNext++;

            Registry.Library.Levels[gameID] = new ModLevelEntry(this, gameID, config.ModID)
            {
                Config = config.DeepCopy()
            };

            return gameID;
        }
    }
}
