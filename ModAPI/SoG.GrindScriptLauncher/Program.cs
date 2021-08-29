using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace SoG.GrindScriptLauncher
{
    class Program
    {
        const string whodis = "GrindScriptLauncher - ";

        static Assembly SoG;
        static MethodInfo SoGMain;

        static Assembly GrindScript;
        static MethodInfo GSInit;

        static void LogErrorAndQuit(string error)
        {
            Console.WriteLine(whodis + error);
            Console.WriteLine(whodis + "Hit Enter to exit.");
            Console.ReadLine();
            Environment.Exit(1);
        }

        static void LaunchGrindScript()
        {
            try
            {
                GSInit.Invoke(null, new object[0]);
            }
            catch (Exception e)
            {
                LogErrorAndQuit("Exception during GrindScript Init call: " + e.Message);
            }
        }

        static void LaunchSoG()
        {
            try
            {
                SoGMain.Invoke(null, new object[] { new string[0] });
            }
            catch (Exception e)
            {
                LogErrorAndQuit("Exception during SoG Main call: " + e.Message);
            }
        }

        static void Main(string[] args)
        {
            Console.Title = "GrindScript";
            try
            {
                Console.WriteLine(whodis + "Loading Assemblies");

                SoG = Assembly.LoadFile(Directory.GetCurrentDirectory() + "\\Secrets Of Grindea.exe");
                GrindScript = Assembly.LoadFile(Directory.GetCurrentDirectory() + "\\GrindScript.dll");

                SoGMain = SoG.DefinedTypes.First(t => t.FullName == "SoG.Program").GetMethod("Main", BindingFlags.Static | BindingFlags.NonPublic);
                GSInit = GrindScript.DefinedTypes.First(t => t.FullName == "SoG.Modding.Launcher").GetMethod("Launch", BindingFlags.Public | BindingFlags.Static);

                Console.WriteLine(whodis + "Launching GrindScript");
                LaunchGrindScript();

                Console.WriteLine(whodis + "Launching SoG");
                new Thread(LaunchSoG).Start();
            }
            catch (Exception e)
            {
                LogErrorAndQuit("Exception during Launcher execution: " + e.Message);
            }
        }
    }
}