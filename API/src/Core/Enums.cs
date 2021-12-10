namespace SoG.Modding
{
    /// <summary>
    /// Defines all of the available equipment types.
    /// </summary>
    public enum EquipmentType
    {
        None = -1,
        Weapon = ItemCodex.ItemCategories.Weapon,
        Shield = ItemCodex.ItemCategories.Shield,
        Armor = ItemCodex.ItemCategories.Armor,
        Hat = ItemCodex.ItemCategories.Hat,
        Accessory = ItemCodex.ItemCategories.Accessory,
        Shoes = ItemCodex.ItemCategories.Shoes,
        Facegear = ItemCodex.ItemCategories.Facegear
    }

    /// <summary>
    /// Defines the directions used by the game.
    /// </summary>
    public enum Directions : byte
    {
        Up = 0,
        Right = 1,
        Down = 2,
        Left = 3
    }
}


