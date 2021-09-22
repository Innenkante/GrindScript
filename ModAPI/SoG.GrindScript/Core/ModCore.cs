using HarmonyLib;
using SoG.Modding.CoreScript;
using SoG.Modding.ModUtils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SoG.Modding.Core
{
    /// <summary>
    /// Provides access to the objects used for modding, and some miscellaneous functionality.
    /// </summary>
    public class ModCore
    {
        private Harmony _harmony;

        private int _launchState = 0;

        internal ILogger Logger { get; }

        internal ModSaving Saving { get; private set; }

        internal ModLoader Loader { get; private set; }

        internal GrindScript GrindScript { get; private set; }

        internal ModCore()
        {
            Logger = Globals.Logger;

            Loader = new ModLoader();
            Saving = new ModSaving(Loader);

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

            Utils.TryCreateDirectory("Mods");
            Utils.TryCreateDirectory("Content/ModContent");

            _harmony = new Harmony("GrindScriptPatcher");

            Logger.Info("Applying Patches...");

            try
            {
                _harmony.PatchAll(typeof(ModCore).Assembly);
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

            Loader.LoadMods(ReadIgnoredMods(), GrindScript);
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
            const string DefaultConfigName = "GrindScriptConfig_default.txt";

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
    }
}
