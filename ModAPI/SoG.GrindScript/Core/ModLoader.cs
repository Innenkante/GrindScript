using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using SoG.Modding.GrindScriptMod;
using SoG.Modding.Utils;

namespace SoG.Modding
{
    internal class ModLoader
    {
        internal Mod LoadContext { get; private set; }

        internal List<Mod> Mods { get; } = new List<Mod>();

        internal ModLibrary Library { get; } = new ModLibrary();

        internal ID ID { get; } = new ID();

        /// <summary>
        /// Sets the given mod as the current load context, and calls the given method.
        /// GrindScript uses this method when calling each mod's LoadContent() method.
        /// </summary>
        internal void CallWithContext(Mod mod, Action<Mod> call)
        {
            Mod previous = LoadContext;
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
        internal void LoadMods(List<string> ignoredMods, GrindScript coreMod)
        {
            Mods.Clear();

            Mods.Add(coreMod);
            SetupMod(coreMod);

            var currentDir = Directory.GetCurrentDirectory();

            var dir = Path.Combine(currentDir, "Mods");

            List<string> fullPathIgnored = ignoredMods.Select(x => Path.Combine(dir, x)).ToList();

            var candidates = Directory.GetFiles(dir)
                .Where(x => x.EndsWith(".dll"))
                .ToList();

            var selected = candidates
                .Where(x => !fullPathIgnored.Contains(x))
                .ToList();

            int ignoreCount = candidates.Count - selected.Count;

            Globals.Logger.Info($"Loading {selected.Count} mods (ignored {ignoreCount} mods from ignore list)...");

            foreach (var file in selected)
            {
                LoadMod(file);
            }

            Mods.Sort((x, y) => string.Compare(x.NameID, y.NameID));

            for (int i = 0; i < Mods.Count; i++)
            {
                Mods[i].ModIndex = i;
            }
        }

        /// <summary>
        /// Loads a mod and instantiates its BaseScript derived class (if any).
        /// </summary>
        private void LoadMod(string path)
        {
            string shortPath = ModUtils.ShortenModPaths(path);

            Globals.Logger.Info("Loading mod " + shortPath);

            try
            {
                Assembly assembly = Assembly.LoadFile(path);
                Type type = assembly.DefinedTypes.First(t => t.BaseType == typeof(Mod));
                Mod mod = Activator.CreateInstance(type) as Mod;

                bool conflictingID = Mods.Any(x => x.NameID == mod.NameID);

                if (conflictingID)
                {
                    Globals.Logger.Error($"Mod {shortPath} with NameID {mod.NameID} conflicts with a previously loaded mod.");
                    return;
                }

                SetupMod(mod);

                Mods.Add(mod);
            }
            catch (BadImageFormatException) { /* Ignore non-managed DLLs */ }
            catch (Exception e)
            {
                Globals.Logger.Error($"Failed to load mod {Utils.ModUtils.ShortenModPaths(path)}. Exception message: {Utils.ModUtils.ShortenModPaths(e.Message)}");
            }
        }

        private void SetupMod(Mod mod)
        {
            mod.Registry = this;
            mod.Logger.LogLevel = Globals.Logger.LogLevel;

            mod.Content = new ContentManager(Globals.Game.Content.ServiceProvider, Globals.Game.Content.RootDirectory);
        }

        /// <summary>
        /// Gets the ID of the effect that has the given cue name. <para/>
        /// This ID can be used to play effects with methods such as SoundSystem.PlayCue.
        /// </summary>

        public string GetEffectID(Mod owner, string cueName)
        {
            var effects = owner.Audio.IndexedEffectCues;

            for (int i = 0; i < effects.Count; i++)
            {
                if (effects[i] == cueName)
                {
                    return $"GS_{owner.ModIndex}_S{i}";
                }
            }

            return "";
        }

        /// <summary>
        /// Gets the ID of the effect that has the given cue name. <para/>
        /// This ID can be used to play effects with methods such as SoundSystem.PlayCue.
        /// </summary>

        public string GetEffectID(int modIndex, string cueName)
        {
            if (modIndex < 0 || modIndex >= Mods.Count)
            {
                Globals.Logger.Warn("Sound ID is invalid!");
                return "";
            }
            return GetEffectID(Mods[modIndex], cueName);
        }

        /// <summary>
        /// Gets the ID of the music that has the given cue name. <para/>
        /// This ID can be used to play effects with <see cref="SoundSystem.PlaySong"/>.
        /// </summary>

        public string GetMusicID(Mod owner, string cueName)
        {
            var music = owner.Audio.IndexedMusicCues;

            for (int i = 0; i < music.Count; i++)
            {
                if (music[i] == cueName)
                    return $"GS_{owner.ModIndex}_M{i}";
            }

            return "";
        }

        /// <summary>
        /// Gets the ID of the music that has the given cue name. <para/>
        /// This ID can be used to play effects with <see cref="SoundSystem.PlaySong"/>.
        /// </summary>

        public string GetMusicID(int modIndex, string cueName)
        {
            if (modIndex < 0 || modIndex >= Mods.Count)
            {
                Globals.Logger.Warn("Music ID is invalid!");
                return "";
            }
            return GetMusicID(Mods[modIndex], cueName);
        }

        /// <summary>
        /// Gets the cue name based on the modded ID. <para/>
        /// </summary>

        public string GetCueName(string GSID)
        {
            if (!Utils.ModUtils.SplitAudioID(GSID, out int entryID, out bool isMusic, out int cueID))
                return "";
            var entry = Mods[entryID].Audio;
            return isMusic ? entry.IndexedMusicCues[cueID] : entry.IndexedEffectCues[cueID];
        }

        /// <summary>
        /// Retrieves a new Cue for the given modded audio ID.
        /// </summary>

        public Cue GetEffectCue(string audioID)
        {
            bool success = Utils.ModUtils.SplitAudioID(audioID, out int entryID, out bool isMusic, out int cueID);
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
            bool success = Utils.ModUtils.SplitAudioID(audioID, out int entryID, out bool isMusic, out _);
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
            bool success = Utils.ModUtils.SplitAudioID(audioID, out int entryID, out bool isMusic, out _);
            if (!(success && !isMusic))
                return null;

            return Mods[entryID].Audio?.EffectsWB;
        }

        /// <summary>
        /// Retrieves the SoundBank associated with the given modded audio ID.
        /// </summary>

        public SoundBank GetMusicSoundBank(string audioID)
        {
            bool success = Utils.ModUtils.SplitAudioID(audioID, out int entryID, out bool isMusic, out _);
            if (!(success && isMusic))
                return null;

            return Mods[entryID].Audio?.MusicSB;
        }

        /// <summary>
        /// Retrieves the WaveBank associated with the given modded audio ID.
        /// </summary>

        public string GetMusicWaveBank(string audioID)
        {
            bool success = Utils.ModUtils.SplitAudioID(audioID, out int entryID, out bool isMusic, out int cueID);
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
    }
}
