using SoG.Modding.Configs;

namespace SoG.Modding.LibraryEntries
{
    /// <summary>
    /// Represents a modded treat or curse in the ModLibrary.
    /// Treats are functionally identical to Curses, but appear in a different menu.
    /// </summary>
    internal class CurseEntry : IEntry<RogueLikeMode.TreatsCurses>
    {
        public Mod Owner { get; set; }

        public RogueLikeMode.TreatsCurses GameID { get; set; }

        public string ModID { get; set; }

        public TreatCurseConfig Config { get; set; }

        public string NameHandle { get; set; } = "";

        public string DescriptionHandle { get; set; } = "";

        public CurseEntry(Mod owner, RogueLikeMode.TreatsCurses gameID, string modID)
        {
            Owner = owner;
            GameID = gameID;
            ModID = modID;
        }
    }
}
