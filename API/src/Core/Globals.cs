using SoG.Modding.Utils;
using System.Reflection;
using Microsoft.Xna.Framework.Graphics;
using HarmonyLib;

namespace SoG.Modding
{
    /// <summary>
    /// Provides a common access point to default objects.
    /// </summary>
    public static class Globals
    {
        /// <summary>
        /// Secrets of Grindea's game instance.
        /// </summary>
        public static Game1 Game { get; internal set; }

        /// <summary>
        /// The game's initial (vanilla) version.
        /// </summary>
        public static string GameVanillaVersion { get; internal set; }

        /// <summary>
        /// The game's modded version, short form.
        /// </summary>
        public static string GameShortVersion => Game.sVersionNumberOnly;

        /// <summary>
        /// The game's modded version, long form.
        /// </summary>
        public static string GameLongVersion => AccessTools.Field(typeof(Game1), "sVersion").GetValue(Game) as string;

        /// <summary>
        /// The game's sprite batch. 
        /// </summary>
        public static SpriteBatch SpriteBatch => AccessTools.Field(typeof(Game1), "spriteBatch").GetValue(Game) as SpriteBatch;

        /// <summary>
        /// Changes the perceived version of the game.
        /// This is used when saving / loading the vanilla saves, so that they don't write the modded version.
        /// </summary>
        internal static void SetVersionTypeAsModded(bool modded)
        {
            Game.sVersionNumberOnly = GameVanillaVersion + (modded ? "-modded" : "");
        }

        /// <summary>
        /// The default Logger.
        /// </summary>
        internal static ILogger Logger { get; set; }

        private static ModManager _modManager = null;

        internal static ModManager Manager
        {
            get
            {
                return _modManager;
            }
            set
            {
                _modManager = value;
            }
        }
    }
}
