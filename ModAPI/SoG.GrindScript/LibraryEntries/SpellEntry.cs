using SoG.Modding.Configs;

namespace SoG.Modding.LibraryEntries
{
    internal class SpellEntry : IEntry<SpellCodex.SpellTypes>
    {
        public Mod Owner { get; set; }

        public SpellCodex.SpellTypes GameID { get; set; }

        public string ModID { get; set; }

        public SpellConfig Config { get; set; }

        public SpellEntry(Mod owner, SpellCodex.SpellTypes gameID, string modID)
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
