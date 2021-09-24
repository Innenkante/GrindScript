using SoG.Modding.Configs;

namespace SoG.Modding.LibraryEntries
{
    internal class EnemyEntry : IEntry<EnemyCodex.EnemyTypes>
    {
        public Mod Owner { get; set; }

        public EnemyCodex.EnemyTypes GameID { get; set; }

        public string ModID { get; set; }

        public EnemyConfig Config { get; set; }

        public EnemyDescription EnemyData { get; set; }

        public EnemyEntry(Mod owner, EnemyCodex.EnemyTypes gameID, string modID)
        {
            Owner = owner;
            GameID = gameID;
            ModID = modID;
        }
    }
}
