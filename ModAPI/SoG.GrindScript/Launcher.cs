using SoG.Modding.Core;
using SoG.Modding.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoG.Modding
{
    public static class Launcher
    {
        private static bool launched = false;

        public static void Launch()
        {
            if (!launched)
            {
                launched = true;

                Globals.Logger = new ConsoleLogger(LogLevels.Debug, "GrindScript");

                Globals.API = new GrindScript();
                Globals.API.Setup();
            }
        }
    }
}
