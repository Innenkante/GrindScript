using SoG.Modding.Utils;
using System.Reflection;
using Microsoft.Xna.Framework.Graphics;
using HarmonyLib;
using System;

namespace SoG.Modding
{
    /// <summary>
    /// Provides a common access point to default objects.
    /// </summary>
    public static class Globals
    {
        private static FieldInfo s_sVersion = AccessTools.Field(typeof(Game1), "sVersion");

        private static FieldInfo s_spriteBatch = AccessTools.Field(typeof(Game1), "spriteBatch");

        /// <summary>
        /// The version of the mod tool.
        /// </summary>
        public static Version ModToolVersion => new Version("0.16.1");

        /// <summary>
        /// Secrets of Grindea's game instance.
        /// </summary>
        public static Game1 Game { get; internal set; }

        /// <summary>
        /// The game's initial (vanilla) version.
        /// </summary>
        public static string GameVersion { get; internal set; }

        /// <summary>
        /// The game's modded version, long form.
        /// </summary>
        public static string GameVersionFull => s_sVersion.GetValue(Game) as string;

        /// <summary>
        /// The game's sprite batch. 
        /// </summary>
        public static SpriteBatch SpriteBatch => s_spriteBatch.GetValue(Game) as SpriteBatch;

        /// <summary>
        /// Changes the perceived version of the game.
        /// This is used when saving / loading the vanilla saves, so that they don't write the modded version.
        /// </summary>
        internal static void SetVersionTypeAsModded(bool modded)
        {
            Game.sVersionNumberOnly = GameVersion + (modded ? "-modded" : "");
        }

        /// <summary>
        /// The default Logger.
        /// </summary>
        internal static ILogger Logger { get; set; }

        /// <summary>
        /// The mod manager.
        /// </summary>
        internal static ModManager Manager { get; set; }
    }
}
