using SoG.Modding.Configs;

namespace SoG.Modding.LibraryEntries
{
    /// <summary>
    /// Represents a modded level in the ModLibrary.
    /// </summary>
    internal class LevelEntry : IEntry<Level.ZoneEnum>
    {
        public Mod Owner { get; set; }

        public Level.ZoneEnum GameID { get; set; }

        public string ModID { get; set; }

        public LevelConfig Config { get; set; }

        public LevelEntry(Mod owner, Level.ZoneEnum gameID, string modID)
        {
            Owner = owner;
            GameID = gameID;
            ModID = modID;
        }

        public void Initialize()
        {
            // Nothing for now
        }

        public void Cleanup()
        {
            // Nothing for now
        }
    }
}
