using SoG.Modding.Utils;
using System;
using System.IO;

namespace SoG.Modding
{
    /// <summary>
    /// Provides the Launch method, used to start GrindScript.
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
                    SourceColor = ConsoleColor.Yellow,
                    NextLogger = new FileLogger(LogLevels.Debug, "GrindScript")
                    {
                        FilePath = Path.Combine("Logs", $"ConsoleLog_{time.Year}.{time.Month}.{time.Day}_{time.Hour}.{time.Minute}.{time.Second}.txt")
                    }
                };

                Globals.ModManager = new ModManager();
                Globals.ModManager.Setup();
            }
        }
    }
}
