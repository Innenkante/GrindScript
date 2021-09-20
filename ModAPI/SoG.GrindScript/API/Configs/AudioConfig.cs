using Microsoft.Xna.Framework.Audio;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace SoG.Modding.API.Configs
{
    /// <summary>
    /// Configures modded audio so that cues can be placed from custom built WaveBanks and SoundBanks.
    /// </summary>
    public class AudioConfig
    {
        /// <summary> 
        /// Defines the wave banks used by the mod for music.
        /// The keys represent the name of the wave bank files, without their .xwb extension.
        /// The values contain the cues that are within the respective key's wave bank.
        /// </summary>
        public Dictionary<string, HashSet<string>> MusicCueNames { get; private set; } = new Dictionary<string, HashSet<string>>();

        /// <summary> Defines the cues contained within the sound effects wave bank. </summary>
        public HashSet<string> EffectCueNames { get; private set; } = new HashSet<string>();

        /// <summary>
        /// Adds effect cues to the effect list.
        /// </summary>
        public AudioConfig AddEffects(params string[] effects)
        {
            foreach (var audio in effects)
                EffectCueNames.Add(audio);

            return this;
        }

        /// <summary>
        /// Adds music cues for the specified bank. The bank name is the name of the file, without its .xwb extension. 
        /// </summary>
        public AudioConfig AddMusic(string bankName, params string[] music)
        {
            var setToUpdate = MusicCueNames.TryGetValue(bankName, out var set) ? set : MusicCueNames[bankName] = new HashSet<string>();

            foreach (var audio in music)
                setToUpdate.Add(audio);

            return this;
        }

        public AudioConfig DeepCopy()
        {
            AudioConfig clone = (AudioConfig)MemberwiseClone();

            clone.MusicCueNames = new Dictionary<string, HashSet<string>>();

            foreach (var kvp in MusicCueNames)
            {
                clone.MusicCueNames.Add(kvp.Key, new HashSet<string>(kvp.Value));
            }

            clone.EffectCueNames = new HashSet<string>(EffectCueNames);

            return clone;
        }
    }
}
