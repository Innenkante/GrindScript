using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.Reflection;
using SoG.Modding.Core;

namespace SoG.Modding.Utils
{
    public static class Tools
    {
        /// <summary>
        /// Tries to create a directory. This method ignores exceptions thrown (if any).
        /// </summary>
        public static bool TryCreateDirectory(string name)
        {
            try
            {
                Directory.CreateDirectory(name);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Tries to load a texture from the given path and ContentManager.
        /// Returns true if the operation succeeded, false otherwise.
        /// If the operation failed, result is set to RenderMaster.txNullTexture
        /// </summary>
        public static bool TryLoadTex(string assetPath, ContentManager manager, out Texture2D result)
        {
            try
            {
                result = manager.Load<Texture2D>(assetPath);
                return true;
            }
            catch (Exception e)
            {
                Globals.Logger.Warn(ShortenModPaths(e.Message), source: nameof(TryLoadTex));

                result = RenderMaster.txNullTex;
                return false;
            }
        }

        /// <summary>
        /// Tries to load a WaveBank using the provided path and AudioEngine, and returns it if successful.
        /// If an exception is thrown during load, null is returned.
        /// </summary>
        public static bool TryLoadWaveBank(string assetPath, AudioEngine engine, out WaveBank result)
        {
            try
            {
                result =  new WaveBank(engine, assetPath);
                return true;
            }
            catch (Exception e)
            {
                Globals.Logger.Warn(ShortenModPaths(e.Message), source: nameof(TryLoadWaveBank));

                result = null;
                return false;
            }
        }

        /// <summary>
        /// Tries to load a SoundBank using the provided path and AudioEngine, and returns it if successful.
        /// If an exception is thrown during load, null is returned.
        /// </summary>
        public static bool TryLoadSoundBank(string assetPath, AudioEngine engine, out SoundBank result)
        {
            try
            {
                result = new SoundBank(engine, assetPath);
                return true;
            }
            catch (Exception e)
            {
                Globals.Logger.Warn(ShortenModPaths(e.Message), source: nameof(TryLoadSoundBank));

                result = null;
                return false;
            }
        }

        /// <summary>
        /// Splits an audio ID into separate pieces. Returns true on success.
        /// </summary>
        internal static bool SplitAudioID(string ID, out int entryID, out bool isMusic, out int cueID)
        {
            entryID = -1;
            isMusic = false;
            cueID = -1;

            if (!ID.StartsWith("GS_"))
                return false;

            string[] words = ID.Remove(0, 3).Split('_');

            if (words.Length != 2 || !(words[1][0] == 'M' || words[1][0] == 'S'))
                return false;

            try
            {
                entryID = int.Parse(words[0]);
                isMusic = words[1][0] == 'M';
                cueID = int.Parse(words[1].Substring(1));

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Splits message in words, removing any empty results
        /// </summary>
        public static string[] GetArgs(string message)
        {
            return message == null ? new string[0] : message.Split(new char[] { ' ' }, options: StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// Returns a string where common mod paths are replaced with a shortened form.
        /// </summary>
        public static string ShortenModPaths(string path)
        {
            return path
                .Replace('/', '\\')
                .Replace(Directory.GetCurrentDirectory() + @"\Content\ModContent", "(ModContent)")
                .Replace(Directory.GetCurrentDirectory() + @"\Content\Mods", "(Mods)")
                .Replace(Directory.GetCurrentDirectory() + @"\Content", "(Content)")
                .Replace(Directory.GetCurrentDirectory(), "(SoG)");
        }
    }
}
