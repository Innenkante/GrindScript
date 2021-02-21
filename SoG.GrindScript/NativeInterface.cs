using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SoG.GrindScript
{
    public class NativeInterface
    {
        private static List<BaseScript> _loadedPlugins = new List<BaseScript>();

        private static bool LoadMod(string mod)
        {
            Console.WriteLine("Trying to load mod:" + mod);
            try
            {
                var assembly = Assembly.LoadFile(mod);

                var toCreate = assembly.GetTypes().First(t => t.BaseType == typeof(BaseScript));

                var instance = (BaseScript) toCreate?.GetConstructor(new Type[]{ })?.Invoke(new object[] { });


                _loadedPlugins.Add(instance);

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            Console.WriteLine("Loaded " + mod);

            return true;

        }

        private static bool LoadMods()
        {
            if (!Directory.Exists("Mods"))
            {
                Console.WriteLine("Mod directory does not exist, creating...");
                Directory.CreateDirectory("Mods");
            }

            if (!Directory.Exists("ModContent"))
            {
                Console.WriteLine("ModContent directory does not exist, creating...");
                Directory.CreateDirectory("ModContent");
            }

            var dir = Path.GetFullPath(Directory.GetCurrentDirectory() + "\\Mods");

            foreach (var file in Directory.GetFiles(dir))
            {
                LoadMod(file);
            }

            return true;
        }

        private static void AddMods()
        {
            _loadedPlugins.ForEach(script => Callbacks.AddOnCustomContentLoad(script.OnCustomContentLoad));
            _loadedPlugins.ForEach(script => Callbacks.AddOnDrawCallback(script.OnDraw));
            _loadedPlugins.ForEach(script => Callbacks.AddOnPlayerTakeDamageCallback(script.OnPlayerDamaged));
            _loadedPlugins.ForEach(script => Callbacks.AddOnPlayerKilledCallback(script.OnPlayerKilled));
            _loadedPlugins.ForEach(script => Callbacks.AddPostPlayerLevelUpCallback(script.PostPlayerLevelUp));
            _loadedPlugins.ForEach(script => Callbacks.AddOnEnemyDamagedCallback(script.OnEnemyDamaged));
            _loadedPlugins.ForEach(script => Callbacks.AddOnNPCDamagedCallback(script.OnNPCDamaged));
            _loadedPlugins.ForEach(script => Callbacks.AddOnNPCInteractionCallback(script.OnNPCInteraction));
            _loadedPlugins.ForEach(script => Callbacks.AddOnArcadiaLoadCallback(script.OnArcadiaLoad));
            _loadedPlugins.ForEach(script => Callbacks.AddOnChatParseCallback(script.OnChatParseCommand));
            _loadedPlugins.ForEach(script => Callbacks.AddOnItemUseCallback(script.OnItemUse));
        }

        private static void ApplyPatches()
        {
            Callbacks.InitializeOnDrawCallbacks();
            Callbacks.InitializeOnPlayerTakeDamageCallbacks();
            Callbacks.InitializeOnPlayerKilledCallbacks();
            Callbacks.InitializePostPlayerLevelUpCallbacks();
            Callbacks.InitializeOnEnemyTakeDamageCallbacks();
            Callbacks.InitializeOnNPCTakeDamageCallbacks();
            Callbacks.InitializeOnNPCInteractionCallbacks();
            Callbacks.InitializeOnArcadiaLoadCallbacks();
            Callbacks.InitializeOnCustomContentLoad();
            Callbacks.InitializeOnChatParseCommandCallbacks();
            Callbacks.InitializeOnItemUseCallbacks();

            Callbacks.InitializeUniquePatches();
        }

        public static int Initialize(string argument)
        {
            Console.WriteLine("Intializing");
            
            Utils.Initialize(AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "Secrets Of Grindea"));

            Callbacks.InitializeLoadPatch();

            Console.WriteLine("Initialize Complete");
            return 1;
        }

        public static void LoadGrindscript()
        {
            LoadMods();

            AddMods();

            ApplyPatches();

            string sPreviousVersion = (string)Utils.GetGameType("SoG.Game1").GetDeclaredField("sVersion").GetValue(Utils.GetTheGame());

            Utils.GetTheGame().sVersionNumberOnly += "-grindscript";
            Utils.GetGameType("SoG.Game1").GetDeclaredField("sVersion").SetValue(Utils.GetTheGame(), sPreviousVersion + " w\\GrindScript");

            Console.WriteLine("Version: " + Utils.GetGameType("SoG.Game1").GetDeclaredField("sVersion").GetValue(Utils.GetTheGame()));
            Console.WriteLine("Version Number Only: " + Utils.GetTheGame().sVersionNumberOnly);
        }


    }
}
