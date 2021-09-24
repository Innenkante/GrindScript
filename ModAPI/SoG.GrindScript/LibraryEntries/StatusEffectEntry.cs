using SoG.Modding.Configs;

namespace SoG.Modding.LibraryEntries
{
    internal class StatusEffectEntry : IEntry<BaseStats.StatusEffectSource>
    {
        public Mod Owner { get; set; }

        public BaseStats.StatusEffectSource GameID { get; set; }

        public string ModID { get; set; }

        public StatusEffectConfig Config { get; set; }

        public StatusEffectEntry(Mod owner, BaseStats.StatusEffectSource gameID, string modID)
        {
            Owner = owner;
            GameID = gameID;
            ModID = modID;
        }
    }
}
