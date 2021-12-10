using HarmonyLib;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SoG.Modding.Utils
{
    /// <summary>
    /// Provides helper methods for loading and unloading game assets.
    /// </summary>
    public static class AssetUtils
    {
        private static readonly FieldInfo s_disposableAssetsField = AccessTools.Field(typeof(ContentManager), "disposableAssets");

        private static readonly FieldInfo s_loadedAssetsField = AccessTools.Field(typeof(ContentManager), "loadedAssets");

        private static readonly MethodInfo s_getCleanPathMethod = AccessTools.Method(AccessTools.TypeByName("Microsoft.Xna.Framework.TitleContainer"), "GetCleanPath");

        /// <summary>
        /// Unloads a single asset from the given ContentManager.
        /// If the asset is found, it is disposed of, and removed from the ContentManager.
        /// </summary>
        /// <returns>True if asset was found and unloaded, false otherwise.</returns>
        public static bool UnloadAsset(ContentManager manager, string path)
        {
            GetContentManagerFields(manager, out List<IDisposable> disposableAssets, out Dictionary<string, object> loadedAssets);

            var cleanPath = GetContentManagerCleanPath(path);

            if (loadedAssets.ContainsKey(cleanPath))
            {
                object asset = loadedAssets[cleanPath];

                loadedAssets.Remove(cleanPath);

                if (asset is IDisposable disposable)
                {
                    disposableAssets.Remove(disposable);
                    disposable.Dispose();
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Tries to load a texture from the given path and ContentManager.
        /// Returns true if the operation succeeded, false otherwise.
        /// If the operation failed, result is set to RenderMaster.txNullTexture.
        /// </summary>
        public static bool TryLoadTexture(string path, ContentManager manager, out Texture2D result)
        {
            try
            {
                result = manager.Load<Texture2D>(path);
                return true;
            }
            catch
            {
                result = RenderMaster.txNullTex;
                return false;
            }
        }

        /// <summary>
        /// Tries to load a WaveBank using the provided path and AudioEngine.
        /// Returns true if the operation succeeded, false otherwise.
        /// If the operation failed, result is set to null.
        /// </summary>
        public static bool TryLoadWaveBank(string assetPath, AudioEngine engine, out WaveBank result)
        {
            try
            {
                result = new WaveBank(engine, assetPath);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        /// <summary>
        /// Tries to load a SoundBank using the provided path and AudioEngine, and returns it if successful.
        /// If an exception is thrown during load, null is returned, and a warning message is logged.
        /// </summary>
        public static bool TryLoadSoundBank(string assetPath, AudioEngine engine, out SoundBank result)
        {
            try
            {
                result = new SoundBank(engine, assetPath);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }


        /// <summary>
        /// Experimental internal method that unloads all modded assets from a manager.
        /// Modded assets are assets for which <see cref="ModUtils.IsModContentPath(string)"/> returns true.
        /// </summary>
        internal static void UnloadModContentPathAssets(ContentManager manager)
        {
            GetContentManagerFields(manager, out List<IDisposable> disposableAssets, out Dictionary<string, object> loadedAssets);

            foreach (var kvp in loadedAssets.Where(x => ModUtils.IsModContentPath(x.Key)).ToList())
            {
                loadedAssets.Remove(kvp.Key);

                if (kvp.Value is IDisposable disposable)
                {
                    disposableAssets.Remove(disposable);
                    disposable.Dispose();
                }
            }
        }

        private static void GetContentManagerFields(ContentManager manager, out List<IDisposable> disposableAssets, out Dictionary<string, object> loadedAssets)
        {
            disposableAssets = (List<IDisposable>)s_disposableAssetsField.GetValue(manager);

            loadedAssets = (Dictionary<string, object>)s_loadedAssetsField.GetValue(manager);
        }

        private static string GetContentManagerCleanPath(string path)
        {
            return (string)s_getCleanPathMethod.Invoke(null, new object[] { path });
        }
    }
}
