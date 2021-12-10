using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace SoG.Modding.Addons
{
    /// <summary>
    /// Delegate that edits the perceived level of a skill.
    /// </summary>
    /// <param name="skill"> The skill being edited. </param>
    /// <param name="originalLevel"> The original (true) level of the skill. </param>
    /// <param name="currentLevel"> The current modified value. </param>
    public delegate void SkillEditCallback(SpellCodex.SpellTypes skill, byte originalLevel, ref int currentLevel);

    /// <summary>
    /// An addon that provides special mod functionality.
    /// </summary>
    public class ModGoodies : Mod
    {
        private List<SkillEditCallback> _callbacks = new List<SkillEditCallback>();

        private Harmony _harmony = new Harmony(typeof(ModGoodies).FullName);

        private Dictionary<PlayerView, Dictionary<SpellCodex.SpellTypes, byte>> _bonusLevelTracker = new Dictionary<PlayerView, Dictionary<SpellCodex.SpellTypes, byte>>();

        private HashSet<SpellCodex.SpellTypes> _trackedSkills = new HashSet<SpellCodex.SpellTypes>();

        internal static ModGoodies TheMod;

        public override string NameID => "Addons.ModGoodies";

        public override bool AllowDiscoveryByMods => true;

        public override Version ModVersion => new Version("0.14");

        public override void Load()
        {
            Logger.Info("Patching addons...");

            _harmony.PatchAll(typeof(ModGoodies).Assembly);

            Logger.Info("Done!");

            foreach (var talent in (Enum.GetValues(typeof(SpellCodex.SpellTypes)) as SpellCodex.SpellTypes[]).Where(x => SpellCodex.IsTalent(x)))
            {
                AddTrackedSkill(talent);
            }

            RemoveTrackedSkill(SpellCodex.SpellTypes._Talent_StaticField);
            RemoveTrackedSkill(SpellCodex.SpellTypes._Talent_OBSOLETE1);

            TheMod = this;
        }

        public override void Unload()
        {
            Logger.Info("Unpatching addons...");

            _harmony.UnpatchAll(_harmony.Id);

            Logger.Info("Done!");

            TheMod = null;
        }

        public override void OnBaseStatsUpdate(BaseStats stats)
        {
            if (!(stats.xOwner is PlayerEntity entity))
            {
                return;
            }

            PlayerView view = entity.Owner;

            if (!_bonusLevelTracker.ContainsKey(view))
            {
                _bonusLevelTracker[view] = new Dictionary<SpellCodex.SpellTypes, byte>();
            }

            foreach (var skill in _trackedSkills)
            {
                if (!_bonusLevelTracker[view].ContainsKey(skill))
                {
                    _bonusLevelTracker[view][skill] = 0;
                }

                byte modifiedLevel = GetModifiedSkillLevel(view.xViewStats, skill);
                byte trueLevel = GetTrueSkillLevel(view.xViewStats, skill);

                int extraPoints = modifiedLevel - trueLevel;

                int delta = extraPoints - _bonusLevelTracker[view][skill];
                _bonusLevelTracker[view][skill] = (byte)extraPoints;

                ApplyStatChanges(view, skill, delta);
            }
        }

        #region Public API

        /// <summary>
        /// Adds a callback that can change the perceived level of a skill.
        /// For skills that have effects that are applied on level up or refund,
        /// you must register the skill using <see cref="AddTrackedSkill(SpellCodex.SpellTypes)"/>
        /// for level changes tp update stats automatically.
        /// By default, all vanilla talents are tracked.
        /// </summary>
        /// <param name="callback"> The callback to add </param>
        public void AddSkillEditCallback(SkillEditCallback callback)
        {
            _callbacks.Add(callback);
        }

        /// <summary>
        /// Removes a previously added callback.
        /// </summary>
        /// <param name="callback"> The callback to remove </param>
        public void RemoveSkillLevelCallback(SkillEditCallback callback)
        {
            _callbacks.Remove(callback);
        }

        /// <summary>
        /// Tracks a talent. Tracked talents have their level up / down stats updated automatically as they gain or lose bonus levels.
        /// </summary>
        /// <param name="types"> The skill to keep track of </param>
        public void AddTrackedSkill(SpellCodex.SpellTypes type)
        {
            if (!_trackedSkills.Add(type))
            {
                return;
            }

            foreach (var bonusLevels in _bonusLevelTracker.Values)
            {
                bonusLevels[type] = 0;
            }
        }

        /// <summary>
        /// Untracks the talent. Before untracking, any stats from bonus levels are removed.
        /// </summary>
        /// <param name="types"> The skill to stop tracking </param>
        public void RemoveTrackedSkill(SpellCodex.SpellTypes type)
        {
            if (!_trackedSkills.Remove(type))
            {
                return;
            }

            foreach (var player in _bonusLevelTracker)
            {
                byte bonus = player.Value[type];
                player.Value.Remove(type);

                ApplyStatChanges(player.Key, type, bonus * -1);
            }
        }

        /// <summary>
        /// Gets the true skill level of a skill, as it would normally appear in vanilla.
        /// </summary>
        /// <param name="stats"> The player view's stats </param>
        /// <param name="skill"> The skill to get the level from </param>
        /// <returns> The true level of the skill. If the player doesn't have the skill, 0 is returned. </returns>
        public byte GetTrueSkillLevel(PlayerViewStats stats, SpellCodex.SpellTypes skill)
        {
            byte level = 0;

            if (stats._denbySkills.ContainsKey(skill))
            {
                level = stats._denbySkills[skill];
            }

            return level;
        }

        /// <summary>
        /// Gets the modified skill level of a skill. This level depends on the output of all skill edit callbacks.
        /// </summary>
        /// <param name="stats"> The player view's stats </param>
        /// <param name="skill"> The skill to get the level from </param>
        /// <returns> The perceived level of the skill </returns>
        public byte GetModifiedSkillLevel(PlayerViewStats stats, SpellCodex.SpellTypes spell)
        {
            byte level = GetTrueSkillLevel(stats, spell);
            int modifiedLevel = level;

            foreach (var callback in _callbacks)
            {
                callback(spell, level, ref modifiedLevel);

                // Levels must be between 0 and 255
                modifiedLevel = Math.Min(modifiedLevel, byte.MaxValue);
                modifiedLevel = Math.Max(modifiedLevel, byte.MinValue);
            }

            return (byte)modifiedLevel;
        }

        #endregion

        private void ApplyStatChanges(PlayerView player, SpellCodex.SpellTypes spell, int levels)
        {
            while (levels > 0)
            {
                levels -= 1;

                // We need to avoid using a multiplier of 1, since that would level up the skill.
                // We only want to apply the stat changes!
                Globals.Game._Skill_LevelUp(player, spell, false, 3);
                Globals.Game._Skill_LevelUp(player, spell, false, -2);
            }
            while (levels < 0)
            {
                levels += 1;

                Globals.Game._Skill_LevelUp(player, spell, false, -1);
            }
        }
    }
}
