using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SoG.Modding.GrindScriptMod;
using SoG.Modding.Utils;
using System.IO;

namespace SoG.Modding
{
    internal class ModLoader
    {
        private ModManager _manager;

        public Mod ModInLoad { get; private set; }

        private Dictionary<Mod, List<Mod>> _dependencyGraph = new Dictionary<Mod, List<Mod>>();

        public ModLoader(ModManager manager)
        {
            _manager = manager;
        }

        public void Reload()
        {
            if (Globals.Game.xStateMaster.enGameState != StateMaster.GameStates.MainMenu)
            {
                Globals.Logger.Warn("Reloading mods outside of the main menu!");
            }

            UnloadMods();
            LoadAssemblies(GetLoadableMods());
            LoadMods();
        }


        /// <summary>
        /// Loads all mods found in the "/Mods" directory
        /// </summary>
        private void LoadAssemblies(List<string> modList)
        {
            _manager.Mods.Clear();

            List<Mod> candidates = new List<Mod>();

            SetupMod(_manager.VanillaMod);
            candidates.Add(_manager.VanillaMod);

            SetupMod(_manager.GrindScript);
            candidates.Add(_manager.GrindScript);

            foreach (var file in modList)
            {
                Mod mod = LoadAssembly(file);

                if (mod != null)
                {
                    SetupMod(mod);
                    candidates.Add(mod);
                }
            }

            _manager.Mods.AddRange(BuildLoadOrder(candidates));
        }

        private List<Mod> BuildLoadOrder(List<Mod> mods)
        {
            _dependencyGraph.Clear();

            var dependencies = new Dictionary<Mod, List<ModDependencyAttribute>>();

            foreach (var mod in mods)
            {
                dependencies[mod] = mod.GetType().GetCustomAttributes<ModDependencyAttribute>().ToList();

                _dependencyGraph[mod] = new List<Mod>();
            }

            List<Mod> loadOrder = new List<Mod>();

            List<Mod> readyMods = null;

            while ((readyMods = dependencies.Where(x => x.Value.Count == 0).Select(x => x.Key).ToList()).Count() > 0)
            {
                // Sort nodes that can't be "compared" in the graph.
                // This, along with the dependency sort, ensures a deterministic order

                readyMods.Sort((x, y) => string.Compare(x.NameID, y.NameID));

                foreach (var mod in readyMods)
                {
                    dependencies.Remove(mod);
                    loadOrder.Add(mod);

                    foreach (var pair in dependencies)
                    {
                        var depList = pair.Value;

                        for (int i = 0; i < depList.Count; i++)
                        {
                            if (CheckDependency(mod, depList[i]))
                            {
                                _dependencyGraph[mod].Add(pair.Key);
                                depList.RemoveAt(i);
                                i -= 1;
                            }
                        }
                    }
                }
            }

            // Any leftover mods in dependency graph will fail to load due to missing dependencies

            foreach (var pair in dependencies)
            {
                Globals.Logger.Error($"Mod {pair.Key} cannot be loaded. It is missing the following mod dependencies:");

                foreach (var dep in pair.Value)
                {
                    Globals.Logger.Error($"    {dep.NameID}, Version {dep.ModVersion}" + (dep.AllowHigherVersions ? " or higher" : ""));
                }
            }

            return loadOrder;
        }

        private bool CheckDependency(Mod mod, ModDependencyAttribute dep)
        {
            return dep.NameID == mod.NameID &&
                dep.ModVersion == null ||
                dep.AllowHigherVersions && Version.Parse(dep.ModVersion) <= mod.ModVersion ||
                Version.Parse(dep.ModVersion) == mod.ModVersion;
        }

        /// <summary>
        /// Loads a mod and instantiates its BaseScript derived class (if any).
        /// </summary>
        private Mod LoadAssembly(string path)
        {
            string shortPath = ModUtils.ShortenModPaths(path);

            Globals.Logger.Info("Loading assembly " + shortPath);

            try
            {
                Assembly assembly = Assembly.LoadFrom(path);
                Type type = assembly.DefinedTypes.First(t => t.BaseType == typeof(Mod));
                Mod mod = Activator.CreateInstance(type, true) as Mod;

                bool conflictingID = _manager.Mods.Any(x => x.NameID == mod.NameID);

                if (conflictingID)
                {
                    Globals.Logger.Error($"Mod {shortPath} with NameID {mod.NameID} conflicts with a previously loaded mod.");
                    return null;
                }

                return mod;
            }
            catch (BadImageFormatException) { /* Ignore non-managed DLLs */ }
            catch (Exception e)
            {
                Globals.Logger.Error($"Failed to load mod {ModUtils.ShortenModPaths(path)}. Exception message: {ModUtils.ShortenModPaths(e.Message)}");
            }

            return null;
        }

        private void SetupMod(Mod mod)
        {
            mod.Manager = _manager;
            mod.Logger.LogLevel = Globals.Logger.LogLevel;

            mod.Content = new ContentManager(Globals.Game.Content.ServiceProvider, Globals.Game.Content.RootDirectory);
        }

        private void LoadMods()
        {
            Globals.Logger.Debug("Loading mods...");

            // Load Phase

            foreach (Mod mod in _manager.Mods)
            {
                ModInLoad = mod;
                try
                {
                   
                    if (!mod.Disabled)
                    {
                        Globals.Logger.Info("Loading " + mod.NameID);

                        mod.Load();
                        
                        Globals.Logger.Info("Loaded " + mod.NameID);
                    }
                }
                catch (Exception e)
                {
                    Globals.Logger.Error($"Mod {mod.GetType().Name} threw an exception during mod content loading: {e.Message}");

                    _manager.Library.RemoveMod(mod);

                    // Disable depending mods - we can't load them with a broken dependency

                    if (_dependencyGraph[mod].Count > 0)
                    {
                        Globals.Logger.Warn("Disabling depending mods: ");
                    }

                    foreach (var dep in _dependencyGraph[mod])
                    {
                        dep.Disabled = true;
                        Globals.Logger.Warn($"    {dep.NameID}");
                    }

                    mod.Disabled = true;
                }

                ModInLoad = null;

                _manager.Library.GetModView(mod).Initialize();
            }

            // Reloads original recipes
            Crafting.CraftSystem.InitCraftSystem();

            // Post Load Phase

            foreach (Mod mod in _manager.ActiveMods)
            {
                mod.PostLoad();
            }

            // Reloads menu characters for new textures and item descriptions
            Globals.Game._Menu_CharacterSelect_Init();
        }

        /// <summary>
        /// Unloads mods and their game content.
        /// NOTE: Unloading is currently poorly implemented.
        /// Expect the game to behave well only on first load.
        /// </summary>
        private void UnloadMods()
        {
            string currentMusic = Globals.Game.xSoundSystem.xMusicVolumeMods.sCurrentSong;

            Globals.Logger.Debug("Unloading mods...");

            List<Mod> unloadOrder = _manager.Mods.AsEnumerable().Reverse().ToList();

            Globals.Game.xSoundSystem.StopSong(false);

            foreach (Mod mod in unloadOrder)
            {
                if (!mod.Disabled)
                {
                    mod.Unload();
                }
            }

            Globals.Game.xSoundSystem.PlaySong(currentMusic, true);

            _manager.Library.Cleanup();
            _manager.Library.Remove();

            _manager.ID.Reset();

            _manager.Mods.Clear();

            // Unloads some mod textures for enemies. Textures are always requeried, so it's allowed
            InGameMenu.contTempAssetManager?.Unload();

            // Experimental / Risky. Unloads all mod assets
            AssetUtils.UnloadModContentPathAssets(RenderMaster.contPlayerStuff);

            // Reloads the english localization
            Globals.Game.xDialogueGod_Default = DialogueGod.ReadFile("Content/Data/Dialogue/defaultEnglish.dlf");
            Globals.Game.xMiscTextGod_Default = MiscTextGod.ReadFile("Content/Data/Text/defaultEnglish.vtf");

            // Reloads enemy descriptions
            EnemyCodex.denxDescriptionDict.Clear();
            EnemyCodex.lxSortedCardEntries.Clear();
            EnemyCodex.lxSortedDescriptions.Clear();
            EnemyCodex.Init();

            // Reloads perk info
            RogueLikeMode.PerkInfo.lxAllPerks.Clear();
            RogueLikeMode.PerkInfo.Init();

            // Unloads sorted pins
            PinCodex.SortedPinEntries.Clear();

            // Clears all regions
            Globals.Game.xLevelMaster.denxRegionContent.Clear();
        }

        private List<string> GetLoadableMods()
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
                Globals.Logger.Error($"Encountered an exception while reading config file {IgnoredModsName}! Exception: {e.Message}");
                ignoredMods.Clear();
            }
            finally
            {
                reader?.Close();
            }

            return ignoredMods;
        }
    }
}
