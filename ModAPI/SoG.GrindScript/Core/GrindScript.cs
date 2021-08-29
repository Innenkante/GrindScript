using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework.Graphics;
using SoG.Modding.API;
using SoG.Modding.Extensions;
using SoG.Modding.Utils;
using Microsoft.Xna.Framework.Content;
using SoG.Modding.Patches;
using System.Diagnostics;

namespace SoG.Modding.Core
{
    /// <summary>
    /// Contains the modding tools provided by the API.
    /// </summary>
    public class GrindScript
    {
        private Harmony _harmony;

        private int _launchState = 0;

        internal Texture2D ErrorTexture { get; private set; }

        internal ILogger Logger { get; }

        public Game1 Game { get; private set; }

        internal ModSaving Saving { get; private set; }

        internal ModRegistry Registry { get; private set; }

        internal GrindScript()
        {
            Logger = Globals.Logger;

            Registry = new ModRegistry(this);
            Saving = new ModSaving(Registry);

            Logger.Debug("GrindScript instantiated!");
        }

        /// <remarks> This method contains code that must run before SoG's Main() method, such as Harmony patching. </remarks>
        internal void Setup()
        {
            Debug.Assert(_launchState == 0, $"Expected status 0 (= not set up) in {nameof(Setup)}, but got {_launchState}!");

            _launchState = 1;

            ReadConfig();

            Logger.Info("Setting up Grindscript...");

            if (AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "Secrets Of Grindea") == null)
            {
                throw new InvalidOperationException("Couldn't find Secrets of Grindea.exe in current AppDomain!");
            }

            Tools.TryCreateDirectory("Mods");
            Tools.TryCreateDirectory("Content/ModContent");

            _harmony = new Harmony("GrindScriptPatcher");

            Logger.Info("Applying Patches...");

            try
            {
                _harmony.PatchAll(typeof(GrindScript).Assembly);
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

            Logger.Info("Setting up Secrets of Grindea..."); 

            Assembly gameAssembly = AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "Secrets Of Grindea");

            Game = (Game1)gameAssembly.GetType("SoG.Program").GetField("game").GetValue(null);

            Game.sAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/GrindScript/";

            Game.xGameSessionData.xRogueLikeSession.bTemporaryHighScoreBlock = true;

            Globals.Game = Game;

            if (!Tools.TryLoadTex("Content/ModContent/GrindScript/NullTexGS", Game.Content, out Texture2D errorTexture))
            {
                errorTexture = RenderMaster.txNullTex;
            }

            ErrorTexture = errorTexture;

            List<string> ignoredMods = ReadIgnoredMods();

            Registry.LoadMods(ignoredMods);
        }

        private List<string> ReadIgnoredMods()
        {
            List<string> ignoredMods = new List<string>();

            var dir = Path.GetFullPath(Directory.GetCurrentDirectory() + "\\Mods");

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
                    if (!mod.TrimStart().StartsWith("#"))
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
