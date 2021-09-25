using SoG.Modding.Configs;
using SoG.Modding.Utils;

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

        public void Initialize()
        {
            // Add a Card entry in the Journal
            if (Config.CardDropChance != 0 && Config.CardDropOverride == EnemyCodex.EnemyTypes.Null)
            {
                EnemyCodex.lxSortedCardEntries.Add(EnemyData);
            }

            // Add an Enemy entry in the Journal
            if (Config.CreateJournalEntry)
            {
                EnemyCodex.lxSortedDescriptions.Add(EnemyData);
            }

            Globals.Game.EXT_AddMiscText("Enemies", EnemyData.sNameLibraryHandle, EnemyData.sFullName);
            Globals.Game.EXT_AddMiscText("Enemies", EnemyData.sFlavorLibraryHandle, EnemyData.sFlavorText);
            Globals.Game.EXT_AddMiscText("Enemies", EnemyData.sCardDescriptionLibraryHandle, EnemyData.sCardDescription);
            Globals.Game.EXT_AddMiscText("Enemies", EnemyData.sDetailedDescriptionLibraryHandle, EnemyData.sDetailedDescription);
        }

        public void Cleanup()
        {
            // Enemy instances have their assets cleared due to using the world region content manager

            EnemyCodex.lxSortedCardEntries.Remove(EnemyData);
            EnemyCodex.lxSortedDescriptions.Remove(EnemyData);

            Globals.Game.EXT_RemoveMiscText("Enemies", EnemyData.sNameLibraryHandle);
            Globals.Game.EXT_RemoveMiscText("Enemies", EnemyData.sFlavorLibraryHandle);
            Globals.Game.EXT_RemoveMiscText("Enemies", EnemyData.sCardDescriptionLibraryHandle);
            Globals.Game.EXT_RemoveMiscText("Enemies", EnemyData.sDetailedDescriptionLibraryHandle);

            // Enemy Codex textures only load into InGameMenu.contTempAssetManager
            // We unload contTempAssetManager as part of mod reloading procedure
        }
    }
}
