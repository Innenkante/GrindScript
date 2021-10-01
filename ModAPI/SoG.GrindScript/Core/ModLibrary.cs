using System.Collections.Generic;
using System.Linq;
using System;

namespace SoG.Modding
{
    using Quests;
    using SoG.Modding.LibraryEntries;

    // I wanted to make something fancy that auto - updates when content is added
    // but just maintaining both libraries is probably better

    /// <summary>
    /// Holds all mod-relevant content. Usually composed of PersistentEntries.
    /// </summary>
    internal class ModLibrary
	{
        #region Mod Content

        public Dictionary<ItemCodex.ItemTypes, ItemEntry> Items { get; } = new Dictionary<ItemCodex.ItemTypes, ItemEntry>();

		public Dictionary<RogueLikeMode.TreatsCurses, CurseEntry> Curses { get; } = new Dictionary<RogueLikeMode.TreatsCurses, CurseEntry>();

		public Dictionary<RogueLikeMode.Perks, PerkEntry> Perks { get; } = new Dictionary<RogueLikeMode.Perks, PerkEntry>();

		public Dictionary<EnemyCodex.EnemyTypes, EnemyEntry> Enemies { get; } = new Dictionary<EnemyCodex.EnemyTypes, EnemyEntry>();
		
		public Dictionary<QuestCodex.QuestID, QuestEntry> Quests { get; } = new Dictionary<QuestCodex.QuestID, QuestEntry>();

		public Dictionary<SpellCodex.SpellTypes, SpellEntry> Spells { get; } = new Dictionary<SpellCodex.SpellTypes, SpellEntry>();

		public Dictionary<PinCodex.PinType, PinEntry> Pins { get; } = new Dictionary<PinCodex.PinType, PinEntry>();

		public Dictionary<BaseStats.StatusEffectSource, StatusEffectEntry> StatusEffects { get; } = new Dictionary<BaseStats.StatusEffectSource, StatusEffectEntry>();

		public Dictionary<Level.ZoneEnum, LevelEntry> Levels { get; } = new Dictionary<Level.ZoneEnum, LevelEntry>();

		public Dictionary<Level.WorldRegion, WorldRegionEntry> WorldRegions { get; } = new Dictionary<Level.WorldRegion, WorldRegionEntry>();

		#endregion

		#region Global Content

		public Dictionary<string, string> VanillaMusicRedirects { get; } = new Dictionary<string, string>();

		#endregion

		/// <summary>
		/// Returns a new ModLibrary which holds only objects owned by the given mod.
		/// </summary>
		public ModLibrary GetLibraryOfMod(Mod mod)
		{
			// TODO: Improve this method

			ModLibrary library = new ModLibrary();

			foreach (var pair in Items.Where(x => x.Value.Owner == mod))
				library.Items[pair.Key] = pair.Value;

			foreach (var pair in Curses.Where(x => x.Value.Owner == mod))
				library.Curses[pair.Key] = pair.Value;

			foreach (var pair in Perks.Where(x => x.Value.Owner == mod))
				library.Perks[pair.Key] = pair.Value;

			foreach (var pair in Enemies.Where(x => x.Value.Owner == mod))
				library.Enemies[pair.Key] = pair.Value;

			foreach (var pair in Quests.Where(x => x.Value.Owner == mod))
				library.Quests[pair.Key] = pair.Value;

			foreach (var pair in Spells.Where(x => x.Value.Owner == mod))
				library.Spells[pair.Key] = pair.Value;

			foreach (var pair in Pins.Where(x => x.Value.Owner == mod))
				library.Pins[pair.Key] = pair.Value;

			foreach (var pair in StatusEffects.Where(x => x.Value.Owner == mod))
				library.StatusEffects[pair.Key] = pair.Value;

			foreach (var pair in Levels.Where(x => x.Value.Owner == mod))
				library.Levels[pair.Key] = pair.Value;

			foreach (var pair in WorldRegions.Where(x => x.Value.Owner == mod))
				library.WorldRegions[pair.Key] = pair.Value;

			return library;
		}

		/// <summary>
		/// Clears all content. Should be used during unloading only.
		/// </summary>
		public void Unload()
        {
			foreach (var gameObject in Items.Values)
				gameObject.Cleanup();

			foreach (var gameObject in Curses.Values)
				gameObject.Cleanup();

			foreach (var gameObject in Perks.Values)
				gameObject.Cleanup();

			foreach (var gameObject in Enemies.Values)
				gameObject.Cleanup();

			foreach (var gameObject in Quests.Values)
				gameObject.Cleanup();

			foreach (var gameObject in Spells.Values)
				gameObject.Cleanup();

			foreach (var gameObject in Pins.Values)
				gameObject.Cleanup();

			foreach (var gameObject in StatusEffects.Values)
				gameObject.Cleanup();

			foreach (var gameObject in Levels.Values)
				gameObject.Cleanup();

			foreach (var gameObject in WorldRegions.Values)
				gameObject.Cleanup();

			Items.Clear();
			Curses.Clear();
			Perks.Clear();
			Enemies.Clear();
			Quests.Clear();
			Spells.Clear();
			Pins.Clear();
			StatusEffects.Clear();
			Levels.Clear();
			VanillaMusicRedirects.Clear();
			WorldRegions.Clear();
        }

		public Dictionary<IDType, Entry> GetEntries<IDType, Entry>() where Entry : IEntry<IDType> where IDType : struct
        {
			object[] containers = new object[]
			{
				Items, Curses, Perks, Enemies, Quests, Spells, Pins, StatusEffects, Levels, WorldRegions
			};

			foreach (var con in containers)
            {
				if (con is Dictionary<IDType, Entry> entries)
                {
					return entries;
                }
            }

			return null;
        }
	}
}