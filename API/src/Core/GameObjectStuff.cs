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
		private static Dictionary<Type, object> s_vanillaIDs = new Dictionary<Type, object>();

		static IDExtension()
        {
			AddVanillaIDs<ItemID>();
			AddVanillaIDs<ItemEffectID>();
			AddVanillaIDs<LevelID>();
			AddVanillaIDs<WorldID>();
			AddVanillaIDs<PerkID>();
			AddVanillaIDs<CurseID>();
			AddVanillaIDs<EnemyID>();
			AddVanillaIDs<QuestID>();
			AddVanillaIDs<SpecialObjectiveID>();
			AddVanillaIDs<SpellID>();
			AddVanillaIDs<StatusEffectID>();
			AddVanillaIDs<PinID>();

			// If it is needed, manually remove problematic IDs

			RemoveVanillaIDs(
				CurseID.None, CurseID.Curse_Hard, CurseID.Treat_Easy
				);

			// Do not remove card entries, or else kaboom!
			RemoveVanillaIDs(
				EnemyID.Null, EnemyID.YellowSlime, EnemyID.SeasonWarden, 
				EnemyID.MtBloom_Troll, EnemyID.GhostShip_Placeholder4, 
				EnemyID.Lood_Placeholder, EnemyID.Lood_Placeholder2, EnemyID.Lood_Placeholder3
				);

			RemoveVanillaIDs(
				ItemID.Null, ItemID._Furniture_Decoration_CompletedPlant_OneHandWeaponPlant_Empty,
				ItemID._ChaosModeUpgrade_HPUp,
				ItemID._ChaosModeUpgrade_ATKUp,
				ItemID._ChaosModeUpgrade_CSPDUp,
				ItemID._ChaosModeUpgrade_EPRegUp,
				ItemID._ChaosModeUpgrade_MaxEPUp,
				ItemID._ChaosModeUpgrade_TalentPoints,
				ItemID._ChaosModeUpgrade_LastDroppableGeneric,
				ItemID._ChaosModeUpgrade_SpellStart,
				ItemID._ChaosModeUpgrade_SpellEnd
				);

			RemoveVanillaIDs(
				LevelID.SeasonChange_F4_LAST,
				LevelID.TimeTown_ENDOFREGION,
				LevelID.Desert_ENDOFREGION,
				LevelID.GhostShip_ENDOFREGION,
				LevelID.Endgame_ENDOFREGION,
				LevelID.MarinoMansion_Cellar,  // Not used
				LevelID.WinterLand_ToyFactory_PreBossRoom,  // Not used
				LevelID.MountBloom_PoisonObstacleRoom,  // Not used
				LevelID.TimeTown_Map03_BossRoom,  // Not used
				LevelID.GhostShip_F1OutsideEntrance,  // Not used
				LevelID.Lobby,
				LevelID.None
				);

			RemoveVanillaIDs(PerkID.None,
				PerkID.MoreNormalItems,  // Not implemented
				PerkID.StartAtLvlTwo,  // Not implemented fully
				PerkID.MoreRegenAfterFloors,  // Not implemented
				PerkID.RegenAfterRooms  // Not implemented
				);

			RemoveVanillaIDs(
				QuestID._SideQuest_Knark,
				QuestID.None,
				QuestID._SideQuest_TheSpectralBall_OBSOLETE,
				QuestID._SideQuest_TheSpectralBall_MK2_OBSOLETE,
				QuestID._RogueLikeQuest_GrindeaChallenge03,
				QuestID._RogueLikeQuest_GrindeaChallenge04
				);
		}

		private static void AddVanillaIDs<T>()
			where T : struct, Enum
		{
			s_vanillaIDs[typeof(T)] = new HashSet<T>((T[])Enum.GetValues(typeof(T)));
        }

		private static void RemoveVanillaIDs<T>(params T[] ids)
			where T : struct, Enum
		{
			if (!s_vanillaIDs.TryGetValue(typeof(T), out _))
            {
				s_vanillaIDs[typeof(T)] = new HashSet<T>();
            }

			((HashSet<T>)s_vanillaIDs[typeof(T)]).ExceptWith(ids);
		}

		private static IDAllocator GetID()
		{
			return Globals.Manager.ID;
		}

		public static bool IsFromSoG<T>(this T id)
			where T : struct, Enum
		{
			if (s_vanillaIDs.ContainsKey(typeof(T)))
            {
				return ((HashSet<T>)s_vanillaIDs[typeof(T)]).Contains(id);
            }

			return false;
		}

		public static IEnumerable<T> GetAllSoGIDs<T>()
        {
			if (s_vanillaIDs.ContainsKey(typeof(T)))
			{
				return (IEnumerable<T>)s_vanillaIDs[typeof(T)];
			}

			return Array.Empty<T>();
		}

		public static bool IsFromMod<IDType>(this IDType id)
			where IDType : struct, Enum
		{
			return GetID().GetIDStart<IDType>().CompareTo(id) <= 0 && id.CompareTo(GetID().GetIDNext<IDType>()) < 0;
		}
	}

	/// <summary>
	/// Provides utility methods for working with game objects.
	/// </summary>
	internal static class GameObjectStuff
    {
		static GameObjectStuff()
        {
			_originalPinCollection = new List<PinID>(PinCodex.SortedPinEntries);
        }

        public static EntryType CreateEntry<IDType, EntryType>()
            where IDType : struct, Enum
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

		private static List<PinID> _originalPinCollection;

		public static List<PinID> GetOriginalPinCollection()
        {
			return _originalPinCollection;
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

		public const PinID PinIDStart = (PinID)35000;
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
