using Microsoft.Xna.Framework.Content;
using SoG.Modding.Configs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoG.Modding.LibraryEntries
{
    internal class WorldRegionEntry : IEntry<Level.WorldRegion>
    {
        public Mod Owner { get; set; }

        public Level.WorldRegion GameID { get; set; }

        public string ModID { get; set; }

        public WorldRegionConfig Config { get; set; }

        public WorldRegionEntry(Mod owner, Level.WorldRegion gameID, string modID)
        {
            Owner = owner;
            GameID = gameID;
            ModID = modID;
        }

        public void Initialize()
        {
            var content = Globals.Game.Content;

            Globals.Game.xLevelMaster.denxRegionContent.Add(GameID, new ContentManager(content.ServiceProvider, content.RootDirectory));
        }

        public void Cleanup()
        {
            Globals.Game.xLevelMaster.denxRegionContent.TryGetValue(GameID, out var manager);

            manager?.Unload();

            Globals.Game.xLevelMaster.denxRegionContent.Remove(GameID);
        }
    }
}
