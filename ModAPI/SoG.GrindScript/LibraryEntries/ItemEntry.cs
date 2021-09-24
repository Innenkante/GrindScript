using System.Collections.Generic;
using SoG.Modding.Configs;

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
    }
}
