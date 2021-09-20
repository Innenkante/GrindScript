using System.Collections.Generic;
using System.Linq;
using SoG.Modding.API;

namespace SoG.Modding.Core
{
	using Quests;

	// I wanted to make something fancy that auto - updates when content is added
	// but just maintaining both libraries is probably better

    /// <summary>
    /// Holds all mod-relevant content. Usually composed of PersistentEntries.
    /// </summary>
	internal class ModLibrary
	{
		public Dictionary<ItemCodex.ItemTypes, ModItemEntry> Items { get; } = new Dictionary<ItemCodex.ItemTypes, ModItemEntry>();

		public Dictionary<RogueLikeMode.TreatsCurses, ModCurseEntry> Curses { get; } = new Dictionary<RogueLikeMode.TreatsCurses, ModCurseEntry>();

		public Dictionary<RogueLikeMode.Perks, ModPerkEntry> Perks { get; } = new Dictionary<RogueLikeMode.Perks, ModPerkEntry>();

		public Dictionary<EnemyCodex.EnemyTypes, ModEnemyEntry> Enemies { get; } = new Dictionary<EnemyCodex.EnemyTypes, ModEnemyEntry>();
		
		public Dictionary<QuestCodex.QuestID, ModQuestEntry> Quests { get; } = new Dictionary<QuestCodex.QuestID, ModQuestEntry>();

		public Dictionary<SpellCodex.SpellTypes, ModSpellEntry> Spells { get; } = new Dictionary<SpellCodex.SpellTypes, ModSpellEntry>();

		public Dictionary<PinCodex.PinType, ModPinEntry> Pins { get; } = new Dictionary<PinCodex.PinType, ModPinEntry>();

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

		public Dictionary<Level.ZoneEnum, ModLevelEntry> Levels { get; } = new Dictionary<Level.ZoneEnum, ModLevelEntry>();

		public Dictionary<BaseStats.StatusEffectSource, ModStatusEffectEntry> StatusEffects { get; } = new Dictionary<BaseStats.StatusEffectSource, ModStatusEffectEntry>();
	}
}