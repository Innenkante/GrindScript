using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

	/// <summary>
	/// Contains constants used for ID Allocation.
	/// ID Start is chosen as a value that greatly exceeds the value of the biggest vanilla ID of that type.
	/// ID Count (the value added to ID Start) is chosen as a value that should greatly exceed even the most content-intensive mod collections.
	/// ID End is represented as ID Start + ID Count. We assume that IDs after ID End are not used by anyone (i.e. they can be used as temporary IDs).
	/// </summary>
	public static class IDRange
	{
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
	}
}
