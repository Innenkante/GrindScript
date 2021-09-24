using SoG.Modding.Configs;

namespace SoG.Modding.LibraryEntries
{
    internal class PinEntry : IEntry<PinCodex.PinType>
    {
        public Mod Owner { get; set; }

        public PinCodex.PinType GameID { get; set; }

        public string ModID { get; set; }

        public PinConfig Config { get; set; }

        public PinEntry(Mod owner, PinCodex.PinType gameID, string modID)
        {
            Owner = owner;
            GameID = gameID;
            ModID = modID;
        }
    }
}
