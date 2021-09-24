using System.Collections.Generic;
using SoG.Modding.Utils;
using System.Reflection;
using Microsoft.Xna.Framework.Graphics;

namespace SoG.Modding
{
    /// <summary>
    /// Provides a common access point to default objects.
    /// </summary>
    public static class Globals
    {
        /// <summary>
        /// The game instance. Use it to access core game logic.
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
        public static string GameLongVersion => typeof(Game1).GetField("sVersion", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(Game) as string;

        public static SpriteBatch SpriteBatch => typeof(Game1).GetField("spriteBatch", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Game) as SpriteBatch;

        /// <summary>
        /// Changes the game version between the vanilla version and modded version.
        /// This should be used before and after saving and loading, so that the saved files use the vanilla version.
        /// </summary>
        internal static void SetVersionTypeAsModded(bool modded)
        {
            Globals.Game.sVersionNumberOnly = GameVanillaVersion + (modded ? "-modded" : "");
        }

        /// <summary>
        /// The default Logger.
        /// </summary>
        internal static ILogger Logger { get; set; }

        private static ModCore _API = null;

        internal static ModCore API
        {
            get
            {
                return _API;
            }
            set
            {
                _API = value;
                Library = _API.Loader.Library;
                Mods = _API.Loader.Mods;
            }
        }

        internal static ModLibrary Library { get; private set; }

        internal static List<Mod> Mods { get; private set; }
    }
}
