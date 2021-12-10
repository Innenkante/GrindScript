using HarmonyLib;
using Microsoft.Xna.Framework.Audio;
using SoG.Modding.Utils;
using System.Collections.Generic;
using System.IO;

namespace SoG.Modding.Content
{
    /// <summary>
    /// Contains data for playing custom audio files inside the game.
    /// Each mod can have at most one audio entry. <para/>
    /// Audio files are loaded from a preset "Sound" folder inside the mod's content path. <para/>
    /// You can use XACT3 to generate wave banks and sound banks to use in your mods. <para/>
    /// </summary>
    /// <remarks>
    /// GrindScript uses preset wave bank and sound bank names to load audio data.
    /// Depending on what you do, you will need the following wave banks in the "Sound" folder:
    /// <list type="bullet">
    ///     <item>
    ///         <term>{<see cref="Mod.NameID"/>}Effects.xwb</term>
    ///         <description>The wave bank used for effects</description>
    ///     </item>
    ///     <item>
    ///         <term>{<see cref="Mod.NameID"/>}Effects.xsb</term>
    ///         <description>The sound bank used for effects</description>
    ///     </item>
    ///     <item>
    ///         <term>{<see cref="Mod.NameID"/>}.xwb</term>
    ///         <description>The universal ("never unload") music wave bank</description>
    ///     </item>
    ///     <item>
    ///         <term>{<see cref="Mod.NameID"/>}Music.xsb</term>
    ///         <description>The sound bank used for music</description>
    ///     </item>
    ///     <item>
    ///         <term>Region wave banks</term>
    ///         <description>
    ///             These have the same name as the wave bank names specified in <see cref="AddMusic"/>.
    ///             Each wave bank represents a distinct region that is loaded and unloaded on demand.
    ///         </description>
    ///     </item>
    /// </list> 
    /// </remarks>
    /// <remarks> 
    /// Most of the methods in this class can only be used while a mod is loading, that is, inside <see cref="Mod.Load"/>.
    /// </remarks>
    public class AudioEntry : Entry<GrindScriptID.AudioID>
    {
        #region IEntry Properties

        public override Mod Mod { get; internal set; }

        public override GrindScriptID.AudioID GameID { get; internal set; }

        public override string ModID { get; internal set; }

        #endregion

        #region Internal Data

        internal HashSet<string> effectCueNames = new HashSet<string>();

        internal Dictionary<string, HashSet<string>> musicCueNames = new Dictionary<string, HashSet<string>>();

        internal SoundBank effectsSB; // "<Mod>Effects.xsb"

        internal WaveBank effectsWB; // "<Mod>Music.xwb"

        internal SoundBank musicSB; //"<Mod>Music.xsb"

        internal WaveBank universalWB; // "<Mod>.xwb", never unloaded

        internal List<string> indexedEffectCues = new List<string>();

        internal List<string> indexedMusicCues = new List<string>();

        internal List<string> indexedMusicBanks = new List<string>();

        #endregion

        #region Public Interface

        /// <summary>
        /// Adds effect cues for this mod. <para/>
        /// </summary>
        /// <param name="effects"> A list of effect names to add. </param>
        public void AddEffects(params string[] effects)
        {
            ErrorHelper.ThrowIfNotLoading(Mod);

            foreach (var audio in effects)
                effectCueNames.Add(audio);
        }

        /// <summary>
        /// Adds music cues for this mod. The cues be loaded using the given wave bank.
        /// Keep in mind that the universal music wave bank follows a "never unload" policy.
        /// </summary>
        /// <remarks>
        /// This method can only be used inside <see cref="Mod.Load"/>.
        /// </remarks>
        /// <param name="bankName"> The wave bank name containing the music (without the ".xnb" extension). </param>
        /// <param name="music"> A list of music names to add. </param>
        public void AddMusic(string bankName, params string[] music)
        {
            ErrorHelper.ThrowIfNotLoading(Mod);

            var setToUpdate = musicCueNames.TryGetValue(bankName, out var set) ? set : musicCueNames[bankName] = new HashSet<string>();

            foreach (var audio in music)
                setToUpdate.Add(audio);
        }

        /// <summary>
        /// Removes effect cues from this mod.
        /// </summary>
        /// <remarks>
        /// This method can only be used inside <see cref="Mod.Load"/>.
        /// </remarks>
        /// <param name="effects"> A list of effect names to remove. </param>
        public void RemoveEffects(params string[] effects)
        {
            ErrorHelper.ThrowIfNotLoading(Mod);

            foreach (var audio in effects)
                effectCueNames.Remove(audio);
        }

        /// <summary>
        /// Removes music cues from this mod.
        /// </summary>
        /// <remarks>
        /// This method can only be used inside <see cref="Mod.Load"/>.
        /// </remarks>
        /// <param name="bankName"> The wave bank name containing the music (without the ".xnb" extension). </param>
        /// <param name="music"> A list of music names to remove. </param>
        public void RemoveMusic(string bankName, params string[] music)
        {
            ErrorHelper.ThrowIfNotLoading(Mod);

            if (!musicCueNames.TryGetValue(bankName, out var set))
            {
                return;
            }

            foreach (var audio in music)
                set.Remove(audio);

            musicCueNames.Remove(bankName);
        }


        /// <summary>
        /// Gets the ID of the effect that has the given name. <para/>
        /// This ID can be used to play effects with methods such as <see cref="SoundSystem.PlayCue"/>.
        /// </summary>
        /// <returns> An identifier that can be used to play the effect using vanilla methods. </returns>
        public string GetEffectID(string effectName)
        {
            ErrorHelper.ThrowIfLoading(Mod);

            for (int i = 0; i < indexedEffectCues.Count; i++)
            {
                if (indexedEffectCues[i] == effectName)
                {
                    return $"GS_{(int)GameID}_S{i}";
                }
            }

            return "";
        }

        /// <summary>
        /// Gets the ID of the music that has the given cue name. <para/>
        /// This ID can be used to play music with <see cref="SoundSystem.PlaySong"/>.
        /// </summary>
        /// <returns> An identifier that can be used to play music using vanilla methods. </returns>
        public string GetMusicID(string musicName)
        {
            ErrorHelper.ThrowIfLoading(Mod);

            for (int i = 0; i < indexedMusicCues.Count; i++)
            {
                if (indexedMusicCues[i] == musicName)
                    return $"GS_{(int)GameID}_M{i}";
            }

            return "";
        }

        /// <summary>
        /// Redirects a vanilla music to a modded music name - whenever vanilla music would play
        /// normally, the modded music will play instead.
        /// </summary>
        /// <remarks>
        /// This method cannot be used during <see cref="Mod.Load"/>.
        /// Use it somewhere else, such as <see cref="Mod.PostLoad"/>.
        /// </remarks>
        /// <param name="vanillaName"> The vanilla music to redirect from. </param>
        /// <param name="musicName"> The modded music to redirect to. </param>
        public void RedirectVanillaMusic(string vanillaName, string musicName)
        {
            ErrorHelper.ThrowIfLoading(Mod);

            Mod.Manager.RedirectVanillaMusic(vanillaName, GetMusicID(musicName));
        }

        #endregion

        internal AudioEntry() { }

        internal override void Initialize()
        {
            AudioEngine audioEngine = AccessTools.Field(typeof(SoundSystem), "audioEngine").GetValue(Globals.Game.xSoundSystem) as AudioEngine;

            indexedEffectCues.AddRange(effectCueNames);

            foreach (var kvp in musicCueNames)
            {
                string bankName = kvp.Key;

                foreach (var music in kvp.Value)
                {
                    indexedMusicBanks.Add(bankName);
                    indexedMusicCues.Add(music);
                }
            }

            string root = Path.Combine(Mod.Content.RootDirectory, Mod.AssetPath);

            // Non-unique sound / wave banks will cause audio conflicts
            // This is why the file paths are set in stone
            AssetUtils.TryLoadWaveBank(Path.Combine(root, "Sound", Mod.NameID + "Effects.xwb"), audioEngine, out effectsWB);
            AssetUtils.TryLoadSoundBank(Path.Combine(root, "Sound", Mod.NameID + "Effects.xsb"), audioEngine, out effectsSB);
            AssetUtils.TryLoadSoundBank(Path.Combine(root, "Sound", Mod.NameID + "Music.xsb"), audioEngine, out musicSB);
            AssetUtils.TryLoadWaveBank(Path.Combine(root, "Sound", Mod.NameID + ".xwb"), audioEngine, out universalWB);
        }

        internal override void Cleanup()
        {
            effectsSB?.Dispose();

            effectsWB?.Dispose();

            musicSB?.Dispose();

            universalWB?.Dispose();
        }
    }
}
