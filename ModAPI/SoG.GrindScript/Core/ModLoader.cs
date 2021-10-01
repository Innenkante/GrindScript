using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using SoG.Modding.GrindScriptMod;
using SoG.Modding.Utils;
using SoG.Modding.Patches;

namespace SoG.Modding
{
    internal class ModLoader
    {
        private ModManager _manager;

        private List<Mod> _loadOrder = new List<Mod>();

        public Mod ModInLoad { get; private set; }

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
            LoadAssemblies(_manager.GetLoadableMods(), _manager.GrindScript);
            LoadMods();
        }


        /// <summary>
        /// Loads all mods found in the "/Mods" directory
        /// </summary>
        private void LoadAssemblies(List<string> modList, GrindScript coreMod)
        {
            _manager.Mods.Clear();
            _loadOrder.Clear();

            _manager.Mods.Add(coreMod);
            _loadOrder.Add(coreMod);
            SetupMod(coreMod);

            foreach (var file in modList)
            {
                LoadAssembly(file);
            }

            _manager.Mods.Sort((x, y) => string.Compare(x.NameID, y.NameID));

            for (int i = 0; i < _manager.Mods.Count; i++)
            {
                _manager.Mods[i].ModIndex = i;
            }
        }

        /// <summary>
        /// Loads a mod and instantiates its BaseScript derived class (if any).
        /// </summary>
        private void LoadAssembly(string path)
        {
            string shortPath = ModUtils.ShortenModPaths(path);

            Globals.Logger.Info("Loading mod " + shortPath);

            try
            {
                Assembly assembly = Assembly.LoadFrom(path);
                Type type = assembly.DefinedTypes.First(t => t.BaseType == typeof(Mod));
                Mod mod = Activator.CreateInstance(type) as Mod;

                bool conflictingID = _manager.Mods.Any(x => x.NameID == mod.NameID);

                if (conflictingID)
                {
                    Globals.Logger.Error($"Mod {shortPath} with NameID {mod.NameID} conflicts with a previously loaded mod.");
                    return;
                }

                SetupMod(mod);

                _loadOrder.Add(mod);
                _manager.Mods.Add(mod);
            }
            catch (BadImageFormatException) { /* Ignore non-managed DLLs */ }
            catch (Exception e)
            {
                Globals.Logger.Error($"Failed to load mod {ModUtils.ShortenModPaths(path)}. Exception message: {ModUtils.ShortenModPaths(e.Message)}");
            }
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

            foreach (Mod mod in _manager.Mods)
            {
                ModInLoad = mod;
                try
                {
                    mod.Load();
                }
                catch (Exception e)
                {
                    Globals.Logger.Error($"Mod {mod.GetType().Name} threw an exception during mod content loading: {e.Message}");
                }
                ModInLoad = null;
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

            List<Mod> unloadOrder = _loadOrder.AsEnumerable().Reverse().ToList();

            Globals.Game.xSoundSystem.StopSong(false);

            foreach (Mod mod in unloadOrder)
            {
                mod.Unload();
                mod.Audio.Cleanup();
            }

            Globals.Game.xSoundSystem.PlaySong(currentMusic, true);

            _manager.Library.Unload();

            _manager.ID.Reset();

            _loadOrder.Clear();

            _manager.Mods.Clear();

            // Unloads some mod textures for enemies. Textures are always requeried, so it's allowed
            InGameMenu.contTempAssetManager?.Unload();

            // Experimental / Risky. Unloads all mod assets
            AssetUtils.UnloadModContentPathAssets(RenderMaster.contPlayerStuff);

            Crafting.CraftSystem.InitCraftSystem();
        }
    }
}
