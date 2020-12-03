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

        public static int Initialize(string argument)
        {
            LoadMods();

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

            Callbacks.InitializeUniquePatches();

            return 1;
        }




    }
}
