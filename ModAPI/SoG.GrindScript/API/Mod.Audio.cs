using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;
using System.IO;
using SoG.Modding.Core;
using SoG.Modding.ModUtils;
using System;
using System.Diagnostics;
using SoG.Modding.API.Configs;
using HarmonyLib;

namespace SoG.Modding.API
{
    public abstract partial class Mod
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

            if (!InLoad)
            {
                Globals.Logger.Error("Can not create objects outside of a load context.", source: nameof(CreateAudio));
                return;
            }

            if (Audio.IsReady)
            {
                throw new InvalidOperationException("Audio for a mod cannot be defined twice.");
            }

            Audio.IsReady = true;

            AudioEngine audioEngine = AccessTools.Field(typeof(SoundSystem), "audioEngine").GetValue(Globals.Game.xSoundSystem) as AudioEngine;
            
            Audio.IndexedEffectCues.AddRange(config.EffectCueNames);

            foreach (var kvp in config.MusicCueNames)
            {
                string bankName = kvp.Key;

                foreach (var music in kvp.Value)
                {
                    Audio.IndexedMusicBanks.Add(bankName);
                    Audio.IndexedMusicCues.Add(music);
                }
            }

            string root = Path.Combine(Content.RootDirectory, AssetPath);

            Utils.TryLoadWaveBank(Path.Combine(root, "Sound", Name + "Effects.xwb"), audioEngine, out Audio.EffectsWB);
            Utils.TryLoadSoundBank(Path.Combine(root, "Sound", Name + "Effects.xsb"), audioEngine, out Audio.EffectsSB);
            Utils.TryLoadSoundBank(Path.Combine(root, "Sound", Name + "Music.xsb"), audioEngine, out Audio.MusicSB);
            Utils.TryLoadWaveBank(Path.Combine(root, "Sound", Name + ".xwb"), audioEngine, out Audio.UniversalWB);
        }

        /// <summary>
        /// Instructs the SoundSystem to play the target modded music instead of the vanilla music. <para/>
        /// If redirect is the empty string, any existing redirects are cleared.
        /// </summary>
        public void RedirectVanillaMusic(string vanilla, string redirect)
        {
            var songRegionMap = AccessTools.Field(typeof(SoundSystem), "dssSongRegionMap").GetValue(Globals.Game.xSoundSystem) as Dictionary<string, string>;

            if (!songRegionMap.ContainsKey(vanilla))
            {
                Globals.Logger.Warn($"Redirecting {vanilla} to {redirect} is not possible since {vanilla} is not a vanilla music!");
                return;
            }

            bool isModded = Utils.SplitAudioID(redirect, out int entryID, out bool isMusic, out int cueID);
            var entry = Registry.Mods[entryID].Audio;

            string cueName = entry != null && cueID >= 0 && cueID < entry.IndexedMusicCues.Count ? entry.IndexedMusicCues[cueID] : null;

            if ((!isModded || !isMusic || cueName == null) && !(redirect == ""))
            {
                Globals.Logger.Warn($"Redirecting {vanilla} to {redirect} is not possible since {redirect} is not a modded music!");
                return;
            }

            var redirectedSongs = Registry.Library.VanillaMusicRedirects;
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
            return Registry.GetMusicID(this, audioID);
        }

        public string GetEffectID(string cueName)
        {
            return Registry.GetEffectID(this, cueName);
        }
    }
}
