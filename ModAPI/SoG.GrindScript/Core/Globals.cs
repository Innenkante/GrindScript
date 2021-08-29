using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoG.Modding.Utils;
using SoG.Modding.API;

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
        /// The default Logger.
        /// </summary>
        internal static ILogger Logger { get; set; }

        private static GrindScript _API = null;

        /// <summary>
        /// The default GrindScript instance.
        /// This should only be used inside static contexts.
        /// </summary>
        internal static GrindScript API
        {
            get
            {
                return _API;
            }
            set
            {
                _API = value;
                Library = _API.Registry.Library;
                LoadedMods = _API.Registry.LoadedMods;
            }
        }

        internal static GlobalLibrary Library { get; private set; }

        internal static List<BaseScript> LoadedMods { get; private set; }
    }
}
