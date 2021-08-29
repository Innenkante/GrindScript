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

	/// <summary>
	/// Bookkeeping class for IDs.
	/// </summary>
	internal class IDHolder
	{
		public ItemID ItemIDNext { get; set; } = IDRange.ItemIDStart;

		public ItemEffectID ItemEffectIDNext { get; set; } = IDRange.ItemEffectIDStart;

		public LevelID LevelIDNext { get; set; } = IDRange.LevelIDStart;

		public WorldID WorldIDNext { get; set; } = IDRange.WorldIDStart;

		public PerkID PerkIDNext { get; set; } = IDRange.PerkIDStart;

		public CurseID CurseIDNext { get; set; } = IDRange.CurseIDStart;

		public EnemyID EnemyIDNext { get; set; } = IDRange.EnemyIDStart;

		public QuestID QuestIDNext { get; set; } = IDRange.QuestIDStart;

		public SpecialObjectiveID SpecialObjectiveIDNext { get; set; } = IDRange.SpecialObjectiveIDStart;
	}
}