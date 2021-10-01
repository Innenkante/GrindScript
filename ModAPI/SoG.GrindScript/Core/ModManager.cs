using HarmonyLib;
using Microsoft.Xna.Framework.Audio;
using SoG.Modding.GrindScriptMod;
using SoG.Modding.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SoG.Modding
{
    /// <summary>
    /// Provides access to the objects used for modding, and some miscellaneous functionality.
    /// </summary>
    public class ModManager
    {
        private Harmony _harmony;

        private int _launchState = 0;

        #region Public Methods

        /// <summary>
        /// Gets the cue name based on the modded ID. <para/>
        /// </summary>
        public string GetCueName(string GSID)
        {
            if (!ModUtils.SplitAudioID(GSID, out int entryID, out bool isMusic, out int cueID))
                return "";
            var entry = Mods[entryID].Audio;
            return isMusic ? entry.IndexedMusicCues[cueID] : entry.IndexedEffectCues[cueID];
        }

        /// <summary>
        /// Retrieves a new Cue for the given modded audio ID.
        /// </summary>
        public Cue GetEffectCue(string audioID)
        {
            bool success = ModUtils.SplitAudioID(audioID, out int entryID, out bool isMusic, out int cueID);
            if (!(success && !isMusic))
                return null;

            var entry = Mods[entryID].Audio;
            return entry.EffectsSB.GetCue(entry.IndexedEffectCues[cueID]);
        }

        /// <summary>
        /// Retrieves the SoundBank associated with the given modded audio ID.
        /// </summary>
        public SoundBank GetEffectSoundBank(string audioID)
        {
            bool success = ModUtils.SplitAudioID(audioID, out int entryID, out bool isMusic, out _);
            if (!(success && !isMusic))
                return null;

            var entry = Mods[entryID].Audio;

            if (entry == null)
                return null;

            return entry.EffectsSB;
        }

        /// <summary>
        /// Retrieves the WaveBank associated with the given modded audio ID.
        /// </summary>
        public WaveBank GetEffectWaveBank(string audioID)
        {
            bool success = ModUtils.SplitAudioID(audioID, out int entryID, out bool isMusic, out _);
            if (!(success && !isMusic))
                return null;

            return Mods[entryID].Audio?.EffectsWB;
        }

        /// <summary>
        /// Retrieves the SoundBank associated with the given modded audio ID.
        /// </summary>
        public SoundBank GetMusicSoundBank(string audioID)
        {
            bool success = ModUtils.SplitAudioID(audioID, out int entryID, out bool isMusic, out _);
            if (!(success && isMusic))
                return null;

            return Mods[entryID].Audio?.MusicSB;
        }

        /// <summary>
        /// Retrieves the WaveBank associated with the given modded audio ID.
        /// </summary>
        public string GetMusicWaveBank(string audioID)
        {
            bool success = ModUtils.SplitAudioID(audioID, out int entryID, out bool isMusic, out int cueID);
            if (!(success && isMusic))
                return null;

            var entry = Mods[entryID].Audio;

            if (cueID < 0 || cueID > entry.IndexedMusicBanks.Count)
                return null;

            return entry.IndexedMusicBanks[cueID];
        }

        /// <summary>
        /// Checks whenever the given name may represent a persistent WaveBank. <para/>
        /// Persistent WaveBanks are never unloaded.
        /// </summary>
        public bool IsUniversalMusicBank(string bank)
        {
            if (bank == "UniversalMusic")
                return true;

            foreach (var mod in Mods)
            {
                if (mod.NameID == bank)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Checks whenever the given WaveBank is persistent. <para/>
        /// Persistent WaveBanks are never unloaded.
        /// </summary>
        public bool IsUniversalMusicBank(WaveBank bank)
        {
            if (bank == null)
                return false;

            foreach (var mod in Mods)
            {
                if (mod.Audio.UniversalWB == bank)
                    return true;
            }

            FieldInfo universalWaveBankField = typeof(SoundSystem).GetTypeInfo().GetField("universalMusicWaveBank", BindingFlags.NonPublic | BindingFlags.Instance);
            if (bank == universalWaveBankField.GetValue(Globals.Game.xSoundSystem))
                return true;

            return false;
        }

        /// <summary>
        /// Gets a loaded mod.
        /// If the mod has disabled discovery, this method will return null.
        /// </summary>
        public Mod GetMod(string nameID)
        {
            Mod mod = Mods.First(x => x.NameID == nameID);

            return mod.AllowDiscoveryByMods ? mod : null;
        }

        #endregion

        internal ILogger Logger { get; }

        internal ModSaving Saving { get; private set; }

        internal ModLoader Loader { get; private set; }

        internal GrindScript GrindScript { get; private set; }

        internal ModLibrary Library { get; } = new ModLibrary();

        internal ID ID { get; } = new ID();

        internal List<Mod> Mods { get; } = new List<Mod>();

        internal ModManager()
        {
            Logger = Globals.Logger;

            Loader = new ModLoader(this);
            Saving = new ModSaving(this);

            GrindScript = new GrindScript();

            Logger.Debug("GrindScript instantiated!");
        }

        /// <remarks> This method contains code that must run before SoG's Main() method, such as Harmony patching. </remarks>
        internal void Setup()
        {
            Debug.Assert(_launchState == 0, $"Expected status 0 (= not set up) in {nameof(Setup)}, but got {_launchState}!");

            _launchState = 1;

            ReadConfig();

            if (AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "Secrets Of Grindea") == null)
            {
                throw new InvalidOperationException("Couldn't find Secrets of Grindea.exe in current AppDomain!");
            }

            ModUtils.TryCreateDirectory("Mods");
            ModUtils.TryCreateDirectory("Content/ModContent");

            _harmony = new Harmony("GrindScriptPatcher");

            Logger.Info("Applying Patches...");

            try
            {
                _harmony.PatchAll(typeof(ModManager).Assembly);
            }
            catch
            {
                Logger.Fatal("Harmony crashed during patching!");
                throw;
            }

            Logger.Info($"Patched {_harmony.GetPatchedMethods().Count()} methods!");
        }

        /// <remarks> This method contains code that must run before any "critical" game code (such as content and save loading). </remarks>
        internal void Start()
        {
            Debug.Assert(_launchState == 1, $"Expected status 1 (= set up, not started) in {nameof(Start)}, but got {_launchState}!");

            _launchState = 2;

            Assembly gameAssembly = AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "Secrets Of Grindea");

            Globals.Game = (Game1)gameAssembly.GetType("SoG.Program").GetField("game").GetValue(null);
            Globals.Game.sAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/GrindScript/";
            Globals.Game.xGameSessionData.xRogueLikeSession.bTemporaryHighScoreBlock = true;

            //Loader.LoadAssemblies(GetLoadableMods(), GrindScript);
        }

        private List<string> ReadIgnoredMods()
        {
            List<string> ignoredMods = new List<string>();

            var dir = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "Mods"));

            const string IgnoredModsName = "IgnoredMods.txt";
            string listPath = Path.Combine(dir, IgnoredModsName);

            if (!File.Exists(listPath))
            {
                StreamWriter writer = null;
                try
                {
                    writer = new StreamWriter(new FileStream(listPath, FileMode.Create, FileAccess.Write));

                    writer.WriteLine("# A list of mods to ignore when loadng GrindScript");
                    writer.WriteLine("# Put the names of files that should be ignored on separate lines");
                    writer.WriteLine("# Lines that start with '#' act as comments");
                }
                catch { }
                finally
                {
                    writer?.Close();
                }
            }

            StreamReader reader = null;
            try
            {
                reader = new StreamReader(new FileStream(listPath, FileMode.Open, FileAccess.Read));

                string mod;
                while ((mod = reader.ReadLine()) != null)
                {
                    if (!string.IsNullOrWhiteSpace(mod) && !mod.TrimStart().StartsWith("#"))
                    {
                        ignoredMods.Add(mod);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error($"Encountered an exception while reading config file {IgnoredModsName}! Exception: {e.Message}");
                ignoredMods.Clear();
            }
            finally
            {
                reader?.Close();
            }

            return ignoredMods;
        }

        private void ReadConfig()
        {
            const string ConfigName = "GrindScriptConfig.txt";
            const string DefaultConfigName = "GrindScriptConfig_Default.txt";

            string currentPath = Directory.GetCurrentDirectory();
            string configPath = Path.Combine(currentPath, ConfigName);

            if (!File.Exists(configPath))
            {
                try
                {
                    File.Copy(Path.Combine(currentPath, DefaultConfigName), configPath);
                }
                catch { }
            }

            StreamReader reader = null;
            try
            {
                reader = new StreamReader(new FileStream(configPath, FileMode.Open, FileAccess.Read));

                string config;
                while ((config = reader.ReadLine()) != null)
                {
                    if (config.TrimStart().StartsWith("#"))
                    {
                        continue;
                    }

                    string[] tokens = config.Split('=');

                    if (tokens.Length != 2) continue;

                    tokens[0] = tokens[0].Trim();
                    tokens[1] = tokens[1].Trim();

                    switch (tokens[0].ToLowerInvariant())
                    {
                        case "log_level":
                            if (Enum.TryParse<LogLevels>(tokens[1], out var level))
                            {
                                Logger.LogLevel = level;
                            }
                            break;
                        case "harmony_debug":
                            if (bool.TryParse(tokens[1], out bool debugMode))
                            {
                                Harmony.DEBUG = debugMode;
                                Logger.Debug("Harmony DEBUG mode is " + (debugMode ? "enabled" : "disabled"));
                            }
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error($"Encountered an exception while reading config file {ConfigName}! Exception: {e.Message}");
            }
            finally
            {
                reader?.Close();
            }
        }

        internal List<string> GetLoadableMods()
        {
            var ignoredMods = ReadIgnoredMods();

            var currentDir = Directory.GetCurrentDirectory();

            var dir = Path.Combine(currentDir, "Mods");

            List<string> fullPathIgnored = ignoredMods.Select(x => Path.Combine(dir, x)).ToList();

            var candidates = Directory.GetFiles(dir)
                .Where(x => x.EndsWith(".dll"))
                .ToList();

            var selected = candidates
                .Where(x => !fullPathIgnored.Contains(x))
                .ToList();

            int totalCount = candidates.Count;
            int selectedCount = selected.Count;
            int ignoreCount = totalCount - selectedCount;

            Globals.Logger.Info($"Found {ignoreCount} mods that are present in the ignore list.");
            Globals.Logger.Info($"Selecting {selectedCount} other mods for loading.");

            return selected;
        }
    }
}
