using SoG.Modding.Utils;
using System;
using System.IO;

namespace SoG.Modding
{
    /// <summary>
    /// Static class that provides an entry method for GrindScript.
    /// </summary>
    public static class Launcher
    {
        private static bool launched = false;

        public static DateTime LaunchTime { get; private set; }

        /// <summary>
        /// Launches GrindScript.
        /// </summary>
        public static void Launch()
        {
            if (!launched)
            {
                launched = true;

                var time = LaunchTime = DateTime.Now;

                Globals.Logger = new ConsoleLogger(LogLevels.Debug, "GrindScript")
                {
                    SourceColor = ConsoleColor.Yellow
                };

                Globals.Manager = new ModManager();
                Globals.Manager.Setup();
            }
        }
    }
}
