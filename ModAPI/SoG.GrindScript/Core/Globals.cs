using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoG.Modding.ModUtils;
using SoG.Modding.API;
using System.Reflection;

namespace SoG.Modding.Core
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
        /// The game's vanilla version.
        /// </summary>
        public static string GameVanillaVersion { get; internal set; }

        /// <summary>
        /// The game's vanilla version.
        /// </summary>
        public static string GameShortVersion => Game.sVersionNumberOnly;

        /// <summary>
        /// The game's vanilla version.
        /// </summary>
        public static string GameLongVersion => typeof(Game1).GetField("sVersion", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(Game) as string;

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
