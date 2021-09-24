using SoG.Modding.Configs;

namespace SoG.Modding.LibraryEntries
{
    /// <summary>
    /// Represents a modded perk in the ModLibrary.
    /// </summary>
    internal class PerkEntry : IEntry<RogueLikeMode.Perks>
    {
        public Mod Owner { get; set; }

        public RogueLikeMode.Perks GameID { get; set; }

        public string ModID { get; set; }

        public PerkConfig Config { get; set; }

        public string TextEntry { get; set; }

        public PerkEntry(Mod owner, RogueLikeMode.Perks gameID, string modID)
        {
            Owner = owner;
            GameID = gameID;
            ModID = modID;
        }
    }
}
