using System.Collections.Generic;
using System.Linq;

namespace SoG.Modding.Core
{
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
		
		public Dictionary<Quests.QuestCodex.QuestID, ModQuestEntry> Quests { get; } = new Dictionary<Quests.QuestCodex.QuestID, ModQuestEntry>();
	}

	/// <summary>
	/// Holds all content, for all mods, including non-persistent things.
	/// </summary>
	internal class GlobalLibrary : ModLibrary
	{
		public Dictionary<string, Dictionary<string, CommandParser>> Commands { get; } = new Dictionary<string, Dictionary<string, CommandParser>>();

		public Dictionary<int, ModAudioEntry> Audio { get; } = new Dictionary<int, ModAudioEntry>();

		public Dictionary<string, string> VanillaMusicRedirects { get; } = new Dictionary<string, string>();

		public Dictionary<Level.ZoneEnum, ModLevelEntry> Levels { get; } = new Dictionary<Level.ZoneEnum, ModLevelEntry>();
	}
}