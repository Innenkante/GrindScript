using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Content;
using SoG.Modding.Configs;
using SoG.Modding.Utils;

namespace SoG.Modding.LibraryEntries
{
    /// <summary>
    /// Represents a modded item in the ModLibrary.
    /// The item can act as equipment if EquipData is not null.
    /// </summary>
    internal class ItemEntry : IEntry<ItemCodex.ItemTypes>
    {
        public Mod Owner { get; set; }

        public ItemCodex.ItemTypes GameID { get; set; }

        public string ModID { get; set; }

        public ItemConfig Config { get; set; }

        public ItemDescription ItemData { get; set; }

        public EquipmentInfo EquipData { get; set; } // May be null or a subtype

        public Dictionary<ItemCodex.ItemTypes, string> HatAltSetResourcePaths { get; set; }

        public ItemEntry(Mod owner, ItemCodex.ItemTypes gameID, string modID)
        {
            Owner = owner;
            GameID = gameID;
            ModID = modID;
        }

        public void Initialize()
        {
            Globals.Game.EXT_AddMiscText("Items", ItemData.sNameLibraryHandle, ItemData.sFullName);
            Globals.Game.EXT_AddMiscText("Items", ItemData.sDescriptionLibraryHandle, ItemData.sDescription);

            // Textures are loaded on demand
        }

        public void Cleanup()
        {
            Globals.Game.EXT_RemoveMiscText("Items", ItemData.sNameLibraryHandle);
            Globals.Game.EXT_RemoveMiscText("Items", ItemData.sDescriptionLibraryHandle);

            // TODO: Set texture references to null since they're disposed

            // Weapon assets are automatically disposed of by the game
            // Same goes for dropped item's textures

            ContentManager manager = Globals.Game.Content;

            if (ModUtils.IsModContentPath(Config.IconPath))
            {
                AssetUtils.UnloadAsset(Globals.Game.Content, Config.IconPath);
            }

            string[] directions = new string[]
            {
                "Up", "Right", "Down", "Left"
            };

            if (EquipData is HatInfo hatData)
            {
                string basePath = Config.EquipResourcePath;
                int index = -1;

                while (++index < 4)
                {
                    string texPath = Path.Combine(basePath, directions[index]);

                    if (ModUtils.IsModContentPath(texPath))
                    {
                        AssetUtils.UnloadAsset(manager, texPath);
                    }
                }

                foreach (var kvp in hatData.denxAlternateVisualSets)
                {
                    string altPath = Path.Combine(basePath, HatAltSetResourcePaths[kvp.Key]);

                    index = -1;

                    while (++index < 4)
                    {
                        string texPath = Path.Combine(altPath, directions[index]);

                        if (ModUtils.IsModContentPath(texPath))
                        {
                            AssetUtils.UnloadAsset(manager, texPath);
                        }
                    }
                }
            }
            else if (EquipData is FacegearInfo)
            {
                string path = Config.EquipResourcePath;
                int index = -1;

                while (++index < 4)
                {
                    AssetUtils.UnloadAsset(manager, Path.Combine(path, directions[index]));
                }
            }
        }
    }
}
