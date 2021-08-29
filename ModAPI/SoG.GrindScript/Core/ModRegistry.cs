using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using SoG.Modding.API;
using SoG.Modding.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SoG.Modding.Core
{
    public class ModRegistry
    {
        private GrindScript _modAPI;

        internal ModRegistry(GrindScript modAPI)
        {
            _modAPI = modAPI;
            Library.Commands[GSCommands.APIName] = GSCommands.GetCommands();
        }

        internal BaseScript LoadContext { get; private set; }

        internal List<BaseScript> LoadedMods { get; } = new List<BaseScript>();

        internal GlobalLibrary Library { get; } = new GlobalLibrary();

        internal IDHolder ID { get; } = new IDHolder();

        /// <summary>
        /// Sets the given mod as the current load context, and calls the given method.
        /// GrindScript uses this method when calling each mod's LoadContent() method.
        /// </summary>
        internal void CallWithContext(BaseScript mod, Action<BaseScript> call)
        {
            BaseScript previous = LoadContext;
            LoadContext = mod;

            try
            {
                call?.Invoke(mod);
            }
            finally
            {
                LoadContext = previous;
            }
        }

        /// <summary>
        /// Loads all mods found in the "/Mods" directory
        /// </summary>
        internal void LoadMods(List<string> ignoredMods)
        {
            var dir = Path.GetFullPath(Directory.GetCurrentDirectory() + "\\Mods");

            var candidates = Directory.GetFiles(dir)
                .Where(x => x.EndsWith(".dll"))
                .ToList();

            var selected = candidates
                .Where(x => !ignoredMods.Contains(x))
                .ToList();

            int ignoreCount = candidates.Count - selected.Count;

            Globals.Logger.Info($"Loading {candidates.Count} mods (ignored {ignoreCount} mods from ignore list)...");

            foreach (var file in candidates)
            {
                LoadMod(file);
            }
        }

        /// <summary>
        /// Loads a mod and instantiates its BaseScript derived class (if any).
        /// </summary>
        private void LoadMod(string path)
        {
            Globals.Logger.Info("Loading mod " + Tools.ShortenModPaths(path));

            try
            {
                Type type = Assembly.LoadFile(path).GetTypes().First(t => t.BaseType == typeof(BaseScript));
                BaseScript mod = type.GetConstructor(Type.EmptyTypes).Invoke(new object[0]) as BaseScript;

                mod.ModAPI = _modAPI;
                mod.Logger.LogLevel = _modAPI.Logger.LogLevel;

                mod.Content = new ContentManager(_modAPI.Game.Content.ServiceProvider, _modAPI.Game.Content.RootDirectory);

                Library.Audio.Add(mod.LoadOrder, new ModAudioEntry(mod, mod.LoadOrder));
                Library.Commands[mod.GetType().Name] = new Dictionary<string, CommandParser>();

                mod.LoadOrder = LoadedMods.Count;
                LoadedMods.Add(mod);

                Globals.Logger.Info($"ModPath set as {mod.AssetPath}");
            }
            catch (BadImageFormatException) { /* Ignore non-managed DLLs */ }
            catch (Exception e)
            {
                Globals.Logger.Error($"Failed to load mod {Tools.ShortenModPaths(path)}. Exception message: {Tools.ShortenModPaths(e.Message)}");
            }
        }


        /// <summary>
        /// Gets the ID of the effect that has the given cue name. <para/>
        /// This ID can be used to play effects with methods such as <see cref="SoundSystem.PlayCue"/>.
        /// </summary>

        public string GetEffectID(BaseScript owner, string cueName)
        {
            if (owner == null)
            {
                Globals.Logger.Warn("Can't get sound ID due to owner being null!");
                return "";
            }
            return GetEffectID(owner.LoadOrder, cueName);
        }

        /// <summary>
        /// Gets the ID of the effect that has the given cue name. <para/>
        /// This ID can be used to play effects with methods such as <see cref="SoundSystem.PlayCue"/>.
        /// </summary>

        public string GetEffectID(int audioEntryID, string cueName)
        {
            var effects = Library.Audio[audioEntryID].EffectNames;
            foreach (var kvp in effects)
            {
                if (kvp.Value == cueName)
                {
                    return $"GS_{audioEntryID}_S{kvp.Key}";
                }
            }
            return "";
        }

        /// <summary>
        /// Gets the ID of the music that has the given cue name. <para/>
        /// This ID can be used to play effects with <see cref="SoundSystem.PlaySong"/>.
        /// </summary>

        public string GetMusicID(BaseScript owner, string cueName)
        {
            if (owner == null)
            {
                Globals.Logger.Warn("Can't get sound ID due to owner being null!");
                return "";
            }
            return GetMusicID(owner.LoadOrder, cueName);
        }

        /// <summary>
        /// Gets the ID of the music that has the given cue name. <para/>
        /// This ID can be used to play effects with <see cref="SoundSystem.PlaySong"/>.
        /// </summary>

        public string GetMusicID(int audioEntryID, string cueName)
        {
            var music = Library.Audio[audioEntryID].MusicNames;
            foreach (var kvp in music)
            {
                if (kvp.Value == cueName)
                    return $"GS_{audioEntryID}_M{kvp.Key}";
            }
            return "";
        }

        /// <summary>
        /// Gets the cue name based on the modded ID. <para/>
        /// </summary>

        public string GetCueName(string GSID)
        {
            if (!Tools.SplitAudioID(GSID, out int entryID, out bool isMusic, out int cueID))
                return "";
            ModAudioEntry entry = Library.Audio[entryID];
            return isMusic ? entry.MusicNames[cueID] : entry.EffectNames[cueID];
        }

        /// <summary>
        /// Retrieves a new Cue for the given modded audio ID.
        /// </summary>

        public Cue GetEffectCue(string audioID)
        {
            bool success = Tools.SplitAudioID(audioID, out int entryID, out bool isMusic, out int cueID);
            if (!(success && !isMusic))
                return null;

            var entry = Library.Audio[entryID];
            return entry.EffectsSB.GetCue(entry.EffectNames[cueID]);
        }

        /// <summary>
        /// Retrieves the SoundBank associated with the given modded audio ID.
        /// </summary>

        public SoundBank GetEffectSoundBank(string audioID)
        {
            bool success = Tools.SplitAudioID(audioID, out int entryID, out bool isMusic, out _);
            if (!(success && !isMusic))
                return null;

            var entry = Library.Audio[entryID];

            if (entry == null)
                return null;

            return entry.EffectsSB;
        }

        /// <summary>
        /// Retrieves the WaveBank associated with the given modded audio ID.
        /// </summary>

        public WaveBank GetEffectWaveBank(string audioID)
        {
            bool success = Tools.SplitAudioID(audioID, out int entryID, out bool isMusic, out _);
            if (!(success && !isMusic))
                return null;

            return Library.Audio[entryID]?.EffectsWB;
        }

        /// <summary>
        /// Retrieves the SoundBank associated with the given modded audio ID.
        /// </summary>

        public SoundBank GetMusicSoundBank(string audioID)
        {
            bool success = Tools.SplitAudioID(audioID, out int entryID, out bool isMusic, out _);
            if (!(success && isMusic))
                return null;

            return Library.Audio[entryID]?.MusicSB;
        }

        /// <summary>
        /// Retrieves the WaveBank associated with the given modded audio ID.
        /// </summary>

        public string GetMusicWaveBank(string audioID)
        {
            bool success = Tools.SplitAudioID(audioID, out int entryID, out bool isMusic, out int cueID);
            if (!(success && isMusic))
                return null;

            var entry = Library.Audio[entryID];

            if (!entry.MusicNames.TryGetValue(cueID, out string cueName))
                return null;

            if (!entry.MusicBankNames.TryGetValue(cueName, out string bank))
                return null;

            return bank;
        }

        /// <summary>
        /// Checks whenever the given name may represent a persistent WaveBank. <para/>
        /// Persistent WaveBanks are never unloaded.
        /// </summary>

        public bool IsUniversalMusicBank(string bank)
        {
            if (bank == "UniversalMusic")
                return true;

            foreach (var kvp in Library.Audio)
            {
                if (kvp.Value.Owner.GetType().Name == bank)
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

            foreach (var kvp in Library.Audio)
            {
                if (kvp.Value.UniversalWB == bank)
                    return true;
            }

            FieldInfo universalWaveBankField = typeof(SoundSystem).GetTypeInfo().GetField("universalMusicWaveBank", BindingFlags.NonPublic | BindingFlags.Instance);
            if (bank == universalWaveBankField.GetValue(Globals.Game.xSoundSystem))
                return true;

            return false;
        }
    }
}
