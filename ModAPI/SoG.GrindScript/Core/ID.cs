using System.Collections.Generic;
using System;

namespace SoG.Modding.Core
{
	using ItemID = ItemCodex.ItemTypes;
	using ItemEffectID = EquipmentInfo.SpecialEffect;
	using LevelID = Level.ZoneEnum;
	using WorldID = Level.WorldRegion;
	using PerkID = RogueLikeMode.Perks;
	using CurseID = RogueLikeMode.TreatsCurses;
	using EnemyID = EnemyCodex.EnemyTypes;
	using QuestID = Quests.QuestCodex.QuestID;
	using SpecialObjectiveID = Quests.Objective_SpecialObjective.UniqueID;
	using SpellID = SpellCodex.SpellTypes;

	/// <summary>
	/// Bookkeeping class for IDs.
	/// </summary>
	internal class ID
	{
        #region Ranges

        public const ItemID ItemIDStart = (ItemID)700000;
		public const ItemID ItemIDEnd = ItemIDStart + 100000;

		public const ItemEffectID ItemEffectIDStart = (ItemEffectID)700;
		public const ItemEffectID ItemEffectIDEnd = ItemEffectIDStart + 5000;

		public const LevelID LevelIDStart = (LevelID)5600;
		public const LevelID LevelIDEnd = LevelIDStart + 10000;

		public const WorldID WorldIDStart = (WorldID)650;
		public const WorldID WorldIDEnd = WorldIDStart + 10000;

		public const PerkID PerkIDStart = (PerkID)3500;
		public const PerkID PerkIDEnd = PerkIDStart + 1000;

		public const CurseID CurseIDStart = (CurseID)3500;
		public const CurseID CurseIDEnd = CurseIDStart + 1000;

		public const EnemyID EnemyIDStart = (EnemyID)400000;
		public const EnemyID EnemyIDEnd = EnemyIDStart + 1000;

		public const QuestID QuestIDStart = (QuestID)45000;
		public const QuestID QuestIDEnd = QuestIDStart + 1000;

		public const SpecialObjectiveID SpecialObjectiveIDStart = (SpecialObjectiveID)5000;
		public const SpecialObjectiveID SpecialObjectiveIDEnd = SpecialObjectiveIDStart + 10000;

		public const SpellID SpellIDStart = (SpellID)55000;
		public const SpellID SpellIDEnd = SpellIDStart + 10000;

        #endregion

        #region Next ID Values

        public ItemID ItemIDNext { get; set; } = ItemIDStart;

		public ItemEffectID ItemEffectIDNext { get; set; } = ItemEffectIDStart;

		public LevelID LevelIDNext { get; set; } = LevelIDStart;

		public WorldID WorldIDNext { get; set; } = WorldIDStart;

		public PerkID PerkIDNext { get; set; } = PerkIDStart;

		public CurseID CurseIDNext { get; set; } = CurseIDStart;

		public EnemyID EnemyIDNext { get; set; } = EnemyIDStart;

		public QuestID QuestIDNext { get; set; } = QuestIDStart;

		public SpecialObjectiveID SpecialObjectiveIDNext { get; set; } = SpecialObjectiveIDStart;

		public SpellID SpellIDNext { get; set; } = SpellIDStart;

		#endregion
	}

	public static class IDExtension
	{
		public static bool IsFromSoG<T>(this T id) where T : Enum => Enum.IsDefined(typeof(T), id);

		public static bool IsFromMod(this ItemCodex.ItemTypes id) => id >= ID.ItemIDStart && id < Globals.API.Loader.ID.ItemIDNext;

		public static bool IsFromMod(this Level.WorldRegion id) => ID.WorldIDStart <= id && id < Globals.API.Loader.ID.WorldIDNext;

		public static bool IsFromMod(this Level.ZoneEnum id) => ID.LevelIDStart <= id && id < Globals.API.Loader.ID.LevelIDNext;

		public static bool IsFromMod(this RogueLikeMode.TreatsCurses id) => id >= ID.CurseIDStart && id < Globals.API.Loader.ID.CurseIDNext;

		public static bool IsFromMod(this RogueLikeMode.Perks id) => id >= ID.PerkIDStart && id < Globals.API.Loader.ID.PerkIDNext;

		public static bool IsFromMod(this EnemyCodex.EnemyTypes id) => id >= ID.EnemyIDStart && id < Globals.API.Loader.ID.EnemyIDNext;

		public static bool IsFromMod(this Quests.QuestCodex.QuestID id) => id >= ID.QuestIDStart && id < Globals.API.Loader.ID.QuestIDNext;

		public static bool IsFromMod(this SpellCodex.SpellTypes id) => id >= ID.SpellIDStart && id < Globals.API.Loader.ID.SpellIDNext;
	}
}