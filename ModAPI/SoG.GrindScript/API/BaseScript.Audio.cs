using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;
using System.IO;
using SoG.Modding.Core;
using SoG.Modding.Utils;
using System;
using System.Diagnostics;
using SoG.Modding.API.Configs;

namespace SoG.Modding.API
{
    public abstract partial class BaseScript
    {
        /// <summary>
        /// Configures custom audio for the current mod, using the config provided. <para/>
        /// Config must not be null.
        /// </summary>
        public void CreateAudio(AudioConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            BaseScript mod = ModAPI.Registry.LoadContext;

            if (mod == null)
            {
                Globals.Logger.Error("Can not create objects outside of a load context.", source: nameof(CreateAudio));
                return;
            }

            string assetPath = mod.AssetPath;
            int ID = mod.LoadOrder;
            ModAudioEntry entry = ModAPI.Registry.Library.Audio[ID];

            AudioEngine audioEngine = typeof(SoundSystem).GetField("audioEngine", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Globals.Game.xSoundSystem) as AudioEngine;

            if (entry.IsReady)
            {
                Globals.Logger.Warn($"Audio Entry {ID} is being redefined for a second time! This may cause issues.");

                if (entry.EffectsSB != null)
                {
                    if (!entry.EffectsSB.IsDisposed)
                        entry.EffectsSB.Dispose();
                    entry.EffectsSB = null;
                }

                if (entry.EffectsWB != null)
                {
                    if (!entry.EffectsWB.IsDisposed)
                        entry.EffectsWB.Dispose();
                    entry.EffectsWB = null;
                }

                if (entry.MusicSB != null)
                {
                    if (!entry.MusicSB.IsDisposed)
                        entry.MusicSB.Dispose();
                    entry.MusicSB = null;
                }

                if (entry.UniversalWB != null)
                {
                    if (!entry.UniversalWB.IsDisposed)
                        entry.UniversalWB.Dispose();
                    entry.UniversalWB = null;
                }
            }
            entry.IsReady = true;

            // Assign indexes to effects
            Dictionary<int, string> effectIDToCue = new Dictionary<int, string>();
            int effectID = 0;
            foreach (var effect in config.EffectCues)
                effectIDToCue[effectID++] = effect;

            string modName = entry.Owner.GetType().Name;

            // Assign indexes to music
            Dictionary<int, string> musicIDToCue = new Dictionary<int, string>();
            Dictionary<string, string> cueToWaveBank = new Dictionary<string, string>();
            int musicID = 0;
            foreach (var kvp in config.RegionCues)
            {
                foreach (var music in kvp.Value)
                {
                    cueToWaveBank[music] = kvp.Key;
                    musicIDToCue[musicID++] = music;
                }
                if (!kvp.Key.StartsWith(modName))
                    Globals.Logger.Warn($"Music WaveBank {kvp.Key} from mod {modName} does not follow the naming convention, and may cause conflicts!");
            }

            string root = Path.Combine(entry.Owner.Content.RootDirectory, assetPath);

            Tools.TryLoadWaveBank(Path.Combine(root, "Sound", modName + "Effects.xwb"), audioEngine, out entry.EffectsWB);
            Tools.TryLoadSoundBank(Path.Combine(root, "Sound", modName + "Effects.xsb"), audioEngine, out entry.EffectsSB);
            Tools.TryLoadSoundBank(Path.Combine(root, "Sound", modName + "Music.xsb"), audioEngine, out entry.MusicSB);
            Tools.TryLoadWaveBank(Path.Combine(root, "Sound", modName + ".xwb"), audioEngine, out entry.UniversalWB);

            entry.EffectNames = effectIDToCue;
            entry.MusicNames = musicIDToCue;
            entry.MusicBankNames = cueToWaveBank;
        }

        /// <summary>
        /// Instructs the SoundSystem to play the target modded music instead of the vanilla music. <para/>
        /// If redirect is the empty string, any existing redirects and cleared.
        /// </summary>

        public void RedirectVanillaMusic(string vanilla, string redirect)
        {
            var songRegionMapField = (Dictionary<string, string>)typeof(SoundSystem).GetTypeInfo().GetField("dssSongRegionMap", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Globals.Game.xSoundSystem);
            if (!songRegionMapField.ContainsKey(vanilla))
            {
                Globals.Logger.Warn($"Redirecting {vanilla} to {redirect} is not possible since {vanilla} is not a vanilla music!");
                return;
            }

            bool isModded = Tools.SplitAudioID(redirect, out int entryID, out bool isMusic, out int cueID);
            var entry = ModAPI.Registry.Library.Audio.ContainsKey(entryID) ? ModAPI.Registry.Library.Audio[entryID] : null;
            string cueName = entry != null && entry.MusicNames.ContainsKey(cueID) ? entry.MusicNames[cueID] : null;

            if ((!isModded || !isMusic || cueName == null) && !(redirect == ""))
            {
                Globals.Logger.Warn($"Redirecting {vanilla} to {redirect} is not possible since {redirect} is not a modded music!");
                return;
            }

            var redirectedSongs = ModAPI.Registry.Library.VanillaMusicRedirects;
            bool replacing = redirectedSongs.ContainsKey(vanilla);

            if (redirect == "")
            {
                Globals.Logger.Info($"Song {vanilla} has been cleared of any redirects.");
                redirectedSongs.Remove(vanilla);
            }
            else
            {
                Globals.Logger.Info($"Song {vanilla} is now redirected to {redirect} ({cueName}). {(replacing ? $"Previous redirect was {redirectedSongs[vanilla]}" : "")}");
                redirectedSongs[vanilla] = redirect;
            }
        }

        public string GetMusicID(string audioID)
        {
            return ModAPI.Registry.GetMusicID(this, audioID);
        }
    }
}
