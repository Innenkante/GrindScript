using HarmonyLib;
using Microsoft.Xna.Framework.Audio;
using SoG.Modding.Content;
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
        private static FieldInfo s_songRegionMap = AccessTools.Field(typeof(SoundSystem), "dssSongRegionMap");

        private Harmony _harmony;

        private int _launchState = 0;

        internal ILogger Logger { get; }

        internal ModSaving Saving { get; private set; }

        internal ModLoader Loader { get; private set; }

        internal GrindScript GrindScript { get; private set; }

        internal VanillaMod VanillaMod { get; private set; }

        internal Library Library { get; } = new Library();

        internal IDAllocator ID { get; } = new IDAllocator();

        internal List<Mod> SaveableMods => ActiveMods.Where(x => !x.DisableObjectCreation).ToList();

        internal List<Mod> ActiveMods => Mods.Where(x => !x.Disabled).ToList();

        internal List<Mod> Mods { get; } = new List<Mod>();

        internal ModManager()
        {
            Logger = Globals.Logger;

            Loader = new ModLoader(this);
            Saving = new ModSaving(this);

            GrindScript = new GrindScript();
            VanillaMod = new VanillaMod();

            Logger.Debug("GrindScript instantiated!");
        }

        #region Public Methods

        /// <summary>
        /// Gets a loaded mod.
        /// You can only query for mods that have AllowDiscoveryByMods set to true.
        /// </summary>
        public Mod GetMod(string nameID)
        {
            Mod mod = ActiveMods.FirstOrDefault(x => x.NameID == nameID);

            return mod.AllowDiscoveryByMods ? mod : null;
        }

        #endregion

        internal void RedirectVanillaMusic(string vanillaName, string modID)
        {
            var songRegionMap = s_songRegionMap.GetValue(Globals.Game.xSoundSystem) as Dictionary<string, string>;

            if (!songRegionMap.ContainsKey(vanillaName))
            {
                Globals.Logger.Warn($"Invalid music redirect {vanillaName} -> {modID}.");
                return;
            }

            bool isModded = ModUtils.SplitAudioID(modID, out int entryID, out bool isMusic, out int cueID);

            Library.TryGetEntry<GrindScriptID.AudioID, AudioEntry>((GrindScriptID.AudioID)entryID, out var entry);

            string cueName = null;

            if (entry != null && cueID >= 0 && cueID < entry.indexedMusicCues.Count)
            {
                cueName = entry.indexedMusicCues[cueID];
            }

            if (!(modID == "" || isModded && isMusic && cueName != null))
            {
                Globals.Logger.Warn($"Invalid music redirect {vanillaName} -> {modID}.");
                return;
            }

            if (modID == "")
            {
                Globals.Logger.Info($"Removed music redirect for {vanillaName}.");
                Library.VanillaMusicRedirects.Remove(vanillaName);
            }
            else
            {
                Globals.Logger.Info($"Set music redirect {vanillaName} -> {modID} ({cueName})");
                Library.VanillaMusicRedirects[vanillaName] = modID;
            }
        }

        internal EntryType CreateObject<IDType, EntryType>(Mod mod, string modID)
            where IDType : struct, Enum
            where EntryType : Entry<IDType>
        {
            ErrorHelper.ThrowIfNotLoading(mod);
            ErrorHelper.ThrowIfObjectCreationDisabled(mod);
            ErrorHelper.ThrowIfDuplicateEntry<IDType, EntryType>(mod, modID);

            EntryType entry = GameObjectStuff.CreateEntry<IDType, EntryType>();

            Dictionary<IDType, EntryType> storage = Library.GetStorage<IDType, EntryType>();

            IDType gameID = ID.AllocateID<IDType>();

            entry.GameID = gameID;
            entry.ModID = modID;
            entry.Mod = mod;

            storage[gameID] = entry;

            return entry;
        }

        internal bool TryGetGameEntry<IDType, EntryType>(Mod mod, string modID, out EntryType entry)
            where IDType : struct, Enum
            where EntryType : Entry<IDType>
        {
            return Library.TryGetModEntry<IDType, EntryType>(mod, modID, out entry);
        }

        internal bool TryGetGameID<IDType, EntryType>(Mod mod, string modID, out IDType value)
            where IDType : struct, Enum
            where EntryType : Entry<IDType>
        {
            bool found = Library.TryGetModEntry<IDType, EntryType>(mod, modID, out EntryType entry);

            value = found ? entry.GameID : default;
            return found;
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

            _harmony = new Harmony(typeof(ModManager).FullName);

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

            try
            {
                using (FileStream stream = new FileStream(configPath, FileMode.Open, FileAccess.Read))
                {
                    KVPConfig config = new KVPConfig();
                    config.FromStream(stream);

                    if (config.TryGet("log_level", out string level) && Enum.TryParse(level, out LogLevels value))
                    {
                        Logger.LogLevel = value;
                        Logger.Debug("Set logging level to " + value);
                    }
                    if (config.TryGet("harmony_debug", out bool harmonyDebug))
                    {
                        Harmony.DEBUG = harmonyDebug;
                        Logger.Debug("Harmony DEBUG mode is " + (harmonyDebug ? "enabled" : "disabled"));
                    }
                    if (config.TryGet("log_console_output", out bool consoleOutput))
                    {
                        if (consoleOutput && Globals.Logger.NextLogger == null)
                        {
                            var time = Launcher.LaunchTime;

                            Globals.Logger.NextLogger = new FileLogger(LogLevels.Debug, "GrindScript")
                            {
                                FilePath = Path.Combine("Logs", $"ConsoleLog_{time.Year}.{time.Month}.{time.Day}_{time.Hour}.{time.Minute}.{time.Second}.txt")
                            };

                            Logger.Debug("Enabled console file logging!");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error($"Encountered an exception while reading config file {ConfigName}! Exception: {e.Message}");
            }
        }
    }
}
