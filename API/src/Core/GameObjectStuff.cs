using SoG.Modding.Content;
using System;
using System.Collections.Generic;
using Quests;

namespace SoG.Modding
{
	using ItemID = ItemCodex.ItemTypes;
	using ItemEffectID = EquipmentInfo.SpecialEffect;
	using LevelID = Level.ZoneEnum;
	using WorldID = Level.WorldRegion;
	using PerkID = RogueLikeMode.Perks;
	using CurseID = RogueLikeMode.TreatsCurses;
	using EnemyID = EnemyCodex.EnemyTypes;
	using QuestID = QuestCodex.QuestID;
	using SpecialObjectiveID = Objective_SpecialObjective.UniqueID;
	using SpellID = SpellCodex.SpellTypes;
	using StatusEffectID = BaseStats.StatusEffectSource;
	using PinID = PinCodex.PinType;

	/// <summary>
	/// Defines IDs for game object entries that come from GrindScript.
	/// </summary>
	public static class GrindScriptID
	{
		public enum AudioID { }
		public enum CommandID { }
		public enum NetworkID { }
	}

	/// <summary>
	/// Provides extension methods for vanilla IDs.
	/// </summary>
	public static class IDExtension
	{
		private static IDAllocator GetID()
		{
			return Globals.ModManager.ID;
		}

		public static bool IsFromSoG<T>(this T id) where T : Enum => Enum.IsDefined(typeof(T), id);

		public static bool IsFromMod<IDType>(this IDType id)
			where IDType : struct, Enum, IComparable
		{
			return GetID().GetIDStart<IDType>().CompareTo(id) <= 0 && id.CompareTo(GetID().GetIDNext<IDType>()) < 0;
		}
	}

	/// <summary>
	/// Provides utility methods for working with game objects.
	/// </summary>
	internal static class GameObjectStuff
    {
        public static EntryType CreateEntry<IDType, EntryType>()
            where IDType : struct
            where EntryType : Entry<IDType>
        {
            if (typeof(EntryType) == typeof(AudioEntry)) return new AudioEntry() as EntryType;
            if (typeof(EntryType) == typeof(CommandEntry)) return new CommandEntry() as EntryType;
            if (typeof(EntryType) == typeof(CurseEntry)) return new CurseEntry() as EntryType;
            if (typeof(EntryType) == typeof(EnemyEntry)) return new EnemyEntry() as EntryType;
            if (typeof(EntryType) == typeof(EquipmentEffectEntry)) return new EquipmentEffectEntry() as EntryType;
            if (typeof(EntryType) == typeof(ItemEntry)) return new ItemEntry() as EntryType;
            if (typeof(EntryType) == typeof(LevelEntry)) return new LevelEntry() as EntryType;
            if (typeof(EntryType) == typeof(NetworkEntry)) return new NetworkEntry() as EntryType;
            if (typeof(EntryType) == typeof(PerkEntry)) return new PerkEntry() as EntryType;
            if (typeof(EntryType) == typeof(PinEntry)) return new PinEntry() as EntryType;
            if (typeof(EntryType) == typeof(QuestEntry)) return new QuestEntry() as EntryType;
            if (typeof(EntryType) == typeof(SpellEntry)) return new SpellEntry() as EntryType;
            if (typeof(EntryType) == typeof(StatusEffectEntry)) return new StatusEffectEntry() as EntryType;
            if (typeof(EntryType) == typeof(WorldRegionEntry)) return new WorldRegionEntry() as EntryType;
            return null;
        }

        public static Dictionary<Type, object> CreateIDStart()
        {
			return new Dictionary<Type, object>()
			{
				[typeof(GrindScriptID.AudioID)] = AudioIDStart,
				[typeof(GrindScriptID.CommandID)] = CommandIDStart,
				[typeof(CurseID)] = CurseIDStart,
				[typeof(EnemyID)] = EnemyIDStart,
				[typeof(EnemyID)] = EnemyIDStart,
				[typeof(ItemEffectID)] = ItemEffectIDStart,
				[typeof(ItemID)] = ItemIDStart,
				[typeof(LevelID)] = LevelIDStart,
				[typeof(GrindScriptID.NetworkID)] = NetworkIDStart,
				[typeof(PerkID)] = PerkIDStart,
				[typeof(PinID)] = PinIDStart,
				[typeof(QuestID)] = QuestIDStart,
				[typeof(SpellID)] = SpellIDStart,
				[typeof(StatusEffectID)] = StatusEffectIDStart,
				[typeof(WorldID)] = WorldIDStart,
				[typeof(SpecialObjectiveID)] = SpecialObjectiveIDStart,
			};
		}

		public static Dictionary<Type, object> CreateIDEnd()
        {
			return new Dictionary<Type, object>()
			{
				[typeof(GrindScriptID.AudioID)] = AudioIDEnd,
				[typeof(GrindScriptID.CommandID)] = CommandIDEnd,
				[typeof(CurseID)] = CurseIDEnd,
				[typeof(EnemyID)] = EnemyIDEnd,
				[typeof(ItemEffectID)] = ItemEffectIDEnd,
				[typeof(ItemID)] = ItemIDEnd,
				[typeof(LevelID)] = LevelIDEnd,
				[typeof(GrindScriptID.NetworkID)] = NetworkIDEnd,
				[typeof(PerkID)] = PerkIDEnd,
				[typeof(PinID)] = PinIDEnd,
				[typeof(QuestID)] = QuestIDEnd,
				[typeof(SpellID)] = SpellIDEnd,
				[typeof(StatusEffectID)] = StatusEffectIDEnd,
				[typeof(WorldID)] = WorldIDEnd,
				[typeof(SpecialObjectiveID)] = SpecialObjectiveIDEnd,
			};
		}

		public static void SetupLibrary(Library library)
        {
			library.CreateStorage<GrindScriptID.AudioID, AudioEntry>();
			library.CreateStorage<GrindScriptID.CommandID, CommandEntry>();
			library.CreateStorage<CurseID, CurseEntry>();
			library.CreateStorage<EnemyID, EnemyEntry>();
			library.CreateStorage<ItemEffectID, EquipmentEffectEntry>();
			library.CreateStorage<ItemID, ItemEntry>();
			library.CreateStorage<LevelID, LevelEntry>();
			library.CreateStorage<GrindScriptID.NetworkID, NetworkEntry>();
			library.CreateStorage<PerkID, PerkEntry>();
			library.CreateStorage<PinID, PinEntry>();
			library.CreateStorage<QuestID, QuestEntry>();
			library.CreateStorage<SpellID, SpellEntry>();
			library.CreateStorage<StatusEffectID, StatusEffectEntry>();
			library.CreateStorage<WorldID, WorldRegionEntry>();
		}

        #region ID Constants

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

		// GrindScript IDs

		public const GrindScriptID.AudioID AudioIDStart = 0;
		public const GrindScriptID.AudioID AudioIDEnd = AudioIDStart + 10000;

		public const GrindScriptID.CommandID CommandIDStart = 0;
		public const GrindScriptID.CommandID CommandIDEnd = CommandIDStart + 10000;

		public const GrindScriptID.NetworkID NetworkIDStart = 0;
		public const GrindScriptID.NetworkID NetworkIDEnd = NetworkIDStart + 10000;

        #endregion
    }
}
