using Microsoft.Xna.Framework;
using System;
using SoG.Modding.Core;

namespace SoG.Modding.Extensions
{
    /// <summary>
    /// Following extensions check if a certain game ID is currently allocated to a mod
    /// </summary>
    public static class IDExtension
    {
        public static bool IsFromSoG<T>(this T id) where T : Enum => Enum.IsDefined(typeof(T), id);

        public static bool IsFromMod(this ItemCodex.ItemTypes id) => id >= IDRange.ItemIDStart && id < Globals.API.Registry.ID.ItemIDNext;

        public static bool IsFromMod(this Level.WorldRegion id) => IDRange.WorldIDStart <= id && id < Globals.API.Registry.ID.WorldIDNext;

        public static bool IsFromMod(this Level.ZoneEnum id) => IDRange.LevelIDStart <= id && id < Globals.API.Registry.ID.LevelIDNext;

        public static bool IsFromMod(this RogueLikeMode.TreatsCurses id) => id >= IDRange.CurseIDStart && id < Globals.API.Registry.ID.CurseIDNext;

        public static bool IsFromMod(this RogueLikeMode.Perks id) => id >= IDRange.PerkIDStart && id < Globals.API.Registry.ID.PerkIDNext;
        
        public static bool IsFromMod(this EnemyCodex.EnemyTypes id) => id >= IDRange.EnemyIDStart && id < Globals.API.Registry.ID.EnemyIDNext;
        
        public static bool IsFromMod(this Quests.QuestCodex.QuestID id) => id >= IDRange.QuestIDStart && id < Globals.API.Registry.ID.QuestIDNext;
    }
}
