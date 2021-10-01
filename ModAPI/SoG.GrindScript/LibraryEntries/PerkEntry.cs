using SoG.Modding.Configs;
using SoG.Modding.Utils;

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

        public void Initialize()
        {
            Globals.Game.EXT_AddMiscText("Menus", "Perks_Name_" + TextEntry, Config.Name);
            Globals.Game.EXT_AddMiscText("Menus", "Perks_Description_" + TextEntry, Config.Description);

            // Texture on demand
        }

        public void Cleanup()
        {
            Globals.Game.EXT_RemoveMiscText("Menus", "Perks_Name_" + TextEntry);
            Globals.Game.EXT_RemoveMiscText("Menus", "Perks_Description_" + TextEntry);

            if (ModUtils.IsModContentPath(Config.TexturePath))
            {
                AssetUtils.UnloadAsset(Globals.Game.Content, Config.TexturePath);
            }
        }
    }
}
