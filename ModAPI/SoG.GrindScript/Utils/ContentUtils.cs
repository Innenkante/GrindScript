using HarmonyLib;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SoG.Modding.Utils
{
    // If an FNA update for SoG comes, methods from this class will need an update.
    // It's all reflection hackery, after all.
    public static class ContentUtils
    {
        private static FieldInfo s_disposableAssetsField = AccessTools.Field(typeof(ContentManager), "disposableAssets");

        private static FieldInfo s_loadedAssetsField = AccessTools.Field(typeof(ContentManager), "loadedAssets");

        private static MethodInfo s_getCleanPathMethod = AccessTools.Method(AccessTools.TypeByName("Microsoft.Xna.Framework.TitleContainer"), "GetCleanPath");

        /// <summary>
        /// Unloads the asset from the provided manager.
        /// Unlike Unload(), only one asset is unloaded.
        /// If unload succeeded, the relevant asset is disposed.
        /// </summary>
        /// <returns>True if unloading succeeded, false otherwise.</returns>
        public static bool ForceUnloadAsset(ContentManager manager, string path)
        {
            var disposableAssets = (List<IDisposable>) s_disposableAssetsField.GetValue(manager);

            var loadedAssets = (Dictionary<string, object>) s_loadedAssetsField.GetValue(manager);

            var cleanPath = (string) s_getCleanPathMethod.Invoke(null, new object[] { path });

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
    }
}
