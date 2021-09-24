using System.Collections.Generic;
using System.Linq;

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
		public Dictionary<ItemCodex.ItemTypes, ItemEntry> Items { get; } = new Dictionary<ItemCodex.ItemTypes, ItemEntry>();

		public Dictionary<RogueLikeMode.TreatsCurses, CurseEntry> Curses { get; } = new Dictionary<RogueLikeMode.TreatsCurses, CurseEntry>();

		public Dictionary<RogueLikeMode.Perks, PerkEntry> Perks { get; } = new Dictionary<RogueLikeMode.Perks, PerkEntry>();

		public Dictionary<EnemyCodex.EnemyTypes, EnemyEntry> Enemies { get; } = new Dictionary<EnemyCodex.EnemyTypes, EnemyEntry>();
		
		public Dictionary<QuestCodex.QuestID, QuestEntry> Quests { get; } = new Dictionary<QuestCodex.QuestID, QuestEntry>();

		public Dictionary<SpellCodex.SpellTypes, SpellEntry> Spells { get; } = new Dictionary<SpellCodex.SpellTypes, SpellEntry>();

		public Dictionary<PinCodex.PinType, PinEntry> Pins { get; } = new Dictionary<PinCodex.PinType, PinEntry>();

		/// <summary>
		/// Returns a new ModLibrary which holds only objects owned by the given mod.
		/// </summary>
		public ModLibrary GetLibraryOfMod(Mod mod)
        {
			// This method is horribly inefficient...
			// TODO: Cache the results!

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

			return library;
		}

		public Dictionary<string, string> VanillaMusicRedirects { get; } = new Dictionary<string, string>();

		public Dictionary<Level.ZoneEnum, LevelEntry> Levels { get; } = new Dictionary<Level.ZoneEnum, LevelEntry>();

		public Dictionary<BaseStats.StatusEffectSource, StatusEffectEntry> StatusEffects { get; } = new Dictionary<BaseStats.StatusEffectSource, StatusEffectEntry>();
	}
}