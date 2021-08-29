using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using SoG.Modding.API;
using SoG.Modding.Core;
using SoG.Modding.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SoG.Modding.Patches
{
    public static class HelperCallbacks
    {
        /// <summary>
        /// Initializes all mod content.
        /// </summary>
        internal static void InContentLoad()
        {
            foreach (BaseScript mod in Globals.API.Registry.LoadedMods)
            {
                Globals.API.Registry.CallWithContext(mod, x =>
                {
                    try
                    {
                        x.LoadContent();
                    }
                    catch (Exception e)
                    {
                        Globals.Logger.Error($"Mod {mod.GetType().Name} threw an exception during LoadContent: {e.Message}");
                    }
                });
            }
        }

        /// <summary>
        /// Executes additional code after a level's blueprint has been processed.
        /// </summary>
        internal static void InLevelLoadDoStuff(Level.ZoneEnum type, bool staticOnly)
        {
            // Modifying vanilla levels not supported yet

            if (!type.IsFromMod())
                return;

            ModLevelEntry entry = Globals.API.Registry.Library.Levels[type];

            try
            {
                entry.Config.Loader?.Invoke(staticOnly);
            }
            catch (Exception e)
            {
                Globals.Logger.Error($"Loader threw an exception for level {type}! Exception: {e}");
            }
        }

        /// <summary>
        /// Parses chat messages for custom commands.
        /// </summary>
        internal static bool InChatParseCommand(string command, string message, int connection)
        {
            string[] words = command.Split(':');
            if (words.Length < 2)
                return false; // Is probably a vanilla command

            string target = words[0];
            string trueCommand = command.Substring(command.IndexOf(':') + 1);

            if (!Globals.API.Registry.Library.Commands.TryGetValue(target, out var parsers))
            {
                CAS.AddChatMessage($"[{GSCommands.APIName}] Unknown mod!");
                return true;
            }
            if (!parsers.TryGetValue(trueCommand, out var parser))
            {
                if (trueCommand == "Help")
                {
                    InChatParseCommand($"{GSCommands.APIName}:Help", target, connection);
                    return true;
                }

                CAS.AddChatMessage($"[{target}] Unknown command!");
                return true;
            }

            Globals.Logger.Debug($"Parsed command {target} : {trueCommand}, argument list: {message}");
            parser(message, connection);

            return true;
        }

        /// <summary>
        /// For modded enemies, creates an enemy and runs its "constructor".
        /// </summary>
        internal static Enemy InGetEnemyInstance(EnemyCodex.EnemyTypes gameID, Level.WorldRegion assetRegion)
        {
            if (!gameID.IsFromMod())
                return null; // Switch case will take care of vanilla enemies

            Enemy enemy = new Enemy() { enType = gameID };

            enemy.xRenderComponent.xOwnerObject = enemy;

            Globals.API.Registry.Library.Enemies[gameID].Config.Constructor?.Invoke(enemy);

            return enemy;
        }

        /// <summary>
        /// For modded enemies, checks if the enemy has an elite scaler defined.
        /// If yes, then the enemy is made elite.
        /// Returns true if the enemy was made elite, false otherwise.
        /// The return value is used for subsequent vanilla code.
        /// </summary>
        internal static bool InEnemyMakeElite(Enemy enemy)
        {
            if (!enemy.enType.IsFromMod())
                return false;

            var eliteScaler = Globals.API.Registry.Library.Enemies[enemy.enType].Config.EliteScaler;

            if (eliteScaler != null)
            {
                eliteScaler.Invoke(enemy);
                return true;
            }
            else
            {
                return false;
            }
        }

        public static string GetCueName(string ID) => Globals.API.Registry.GetCueName(ID);

        public static SoundBank GetEffectSoundBank(string ID) => Globals.API.Registry.GetEffectSoundBank(ID);

        public static SoundBank GetMusicSoundBank(string ID) => Globals.API.Registry.GetMusicSoundBank(ID);

        public static SpriteBatch SpriteBatch => typeof(Game1).GetField("spriteBatch", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Globals.Game) as SpriteBatch;

        public static TCMenuWorker TCMenuWorker { get; } = new TCMenuWorker();
    }
}
