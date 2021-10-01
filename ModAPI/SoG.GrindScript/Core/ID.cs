using System;

namespace SoG.Modding
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
    using StatusEffectID = BaseStats.StatusEffectSource;
    using PinID = PinCodex.PinType;

    /// <summary>
    /// Bookkeeping class for IDs.
    /// </summary>
    internal class ID
	{
		public ID()
        {
			Reset();
        }

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

		public const StatusEffectID StatusEffectIDStart = (StatusEffectID)10000;
		public const StatusEffectID StatusEffectIDEnd = StatusEffectIDStart + 10000;

		public const PinID PinIDStart = (PinID)10000;
		public const PinID PinIDEnd = PinIDStart + 10000;

		#endregion

		#region Next ID Values

		public ItemID ItemIDNext { get; set; }

		public ItemEffectID ItemEffectIDNext { get; set; }

		public LevelID LevelIDNext { get; set; }

		public WorldID WorldIDNext { get; set; }

		public PerkID PerkIDNext { get; set; }

		public CurseID CurseIDNext { get; set; }

		public EnemyID EnemyIDNext { get; set; }

		public QuestID QuestIDNext { get; set; }

		public SpecialObjectiveID SpecialObjectiveIDNext { get; set; }

		public SpellID SpellIDNext { get; set; }

		public StatusEffectID StatusEffectIDNext { get; set; }

		public PinID PinIDNext { get; set; }

		#endregion

		/// <summary>
		/// Resets all IDs. This should be used during unloading only.
		/// </summary>
		public void Reset()
        {
			ItemIDNext = ItemIDStart;

			ItemEffectIDNext = ItemEffectIDStart;

			LevelIDNext = LevelIDStart;

			WorldIDNext = WorldIDStart;

			PerkIDNext = PerkIDStart;

			CurseIDNext = CurseIDStart;

			EnemyIDNext = EnemyIDStart;

			QuestIDNext = QuestIDStart;

			SpecialObjectiveIDNext = SpecialObjectiveIDStart;

			SpellIDNext = SpellIDStart;

			StatusEffectIDNext = StatusEffectIDStart;

			PinIDNext = PinIDStart;
		}
	}

	public static class IDExtension
	{
		public static bool IsFromSoG<T>(this T id) where T : Enum => Enum.IsDefined(typeof(T), id);

		public static bool IsFromMod(this ItemID id) => id >= ID.ItemIDStart && id < Globals.ModManager.ID.ItemIDNext;

		public static bool IsFromMod(this WorldID id) => ID.WorldIDStart <= id && id < Globals.ModManager.ID.WorldIDNext;

		public static bool IsFromMod(this LevelID id) => ID.LevelIDStart <= id && id < Globals.ModManager.ID.LevelIDNext;

		public static bool IsFromMod(this CurseID id) => id >= ID.CurseIDStart && id < Globals.ModManager.ID.CurseIDNext;

		public static bool IsFromMod(this PerkID id) => id >= ID.PerkIDStart && id < Globals.ModManager.ID.PerkIDNext;

		public static bool IsFromMod(this EnemyID id) => id >= ID.EnemyIDStart && id < Globals.ModManager.ID.EnemyIDNext;

		public static bool IsFromMod(this QuestID id) => id >= ID.QuestIDStart && id < Globals.ModManager.ID.QuestIDNext;

		public static bool IsFromMod(this SpellID id) => id >= ID.SpellIDStart && id < Globals.ModManager.ID.SpellIDNext;

		public static bool IsFromMod(this StatusEffectID id) => id >= ID.StatusEffectIDStart && id < Globals.ModManager.ID.StatusEffectIDNext;

		public static bool IsFromMod(this PinID id) => id >= ID.PinIDStart && id < Globals.ModManager.ID.PinIDNext;
	}
}