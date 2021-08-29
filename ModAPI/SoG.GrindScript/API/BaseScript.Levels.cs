using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SoG.Modding.Core;
using System.Diagnostics;
using SoG.Modding.Utils;
using SoG.Modding.API.Configs;

namespace SoG.Modding.API
{
    public abstract partial class BaseScript
    {
        public Level.WorldRegion CreateWorldRegion()
        {
            var VanillaContent = Globals.Game.Content;

            var gameID = ModAPI.Registry.ID.WorldIDNext++;

            Globals.Game.xLevelMaster.denxRegionContent.Add(gameID, new ContentManager(VanillaContent.ServiceProvider, VanillaContent.RootDirectory));

            return gameID;
        }

        /// <summary>
        /// Creates a new level from the given LevlConfig.
        /// Config must not be null.
        /// </summary>
        public Level.ZoneEnum CreateLevel(LevelConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            BaseScript mod = ModAPI.Registry.LoadContext;

            if (mod == null)
            {
                Globals.Logger.Error("Can not create objects outside of a load context.", source: nameof(CreateLevel));
                return Level.ZoneEnum.None;
            }

            Level.ZoneEnum gameID = ModAPI.Registry.ID.LevelIDNext++;

            ModAPI.Registry.Library.Levels[gameID] = new ModLevelEntry(mod, gameID)
            {
                Config = config.DeepCopy()
            };

            return gameID;
        }
    }
}
