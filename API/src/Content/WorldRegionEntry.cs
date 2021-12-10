using Microsoft.Xna.Framework.Content;

namespace SoG.Modding.Content
{
    /// <summary>
    /// Represents a modded world region.
    /// </summary>
    public class WorldRegionEntry : Entry<Level.WorldRegion>
    {
        #region IEntry Properties

        public override Mod Mod { get; internal set; }

        public override Level.WorldRegion GameID { get; internal set; }

        public override string ModID { get; internal set; }

        #endregion

        #region Internal Data

        // None, for now

        #endregion

        #region Public Interface

        // None, for now

        #endregion

        internal WorldRegionEntry() { }

        internal WorldRegionEntry(Mod owner, Level.WorldRegion gameID, string modID)
        {
            Mod = owner;
            GameID = gameID;
            ModID = modID;
        }

        internal override void Initialize()
        {
            var content = Globals.Game.Content;

            Globals.Game.xLevelMaster.denxRegionContent.Add(GameID, new ContentManager(content.ServiceProvider, content.RootDirectory));
        }

        internal override void Cleanup()
        {
            Globals.Game.xLevelMaster.denxRegionContent.TryGetValue(GameID, out var manager);

            manager?.Unload();

            Globals.Game.xLevelMaster.denxRegionContent.Remove(GameID);
        }
    }
}
