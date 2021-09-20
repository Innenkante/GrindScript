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
using SoG.Modding.ModUtils;
using System.IO;

namespace SoG.Modding.Patches
{
    public static class HelperCallbacks
    {
        /// <summary>
        /// Initializes all mod content.
        /// </summary>
        internal static void InContentLoad()
        {
            foreach (Mod mod in Globals.API.Loader.Mods)
            {
                Globals.API.Loader.CallWithContext(mod, x =>
                {
                    try
                    {
                        x.Load();
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

            ModLevelEntry entry = Globals.API.Loader.Library.Levels[type];

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

            Mod mod = Globals.API.Loader.Mods.FirstOrDefault(x => x.Name == target);

            if (mod == null)
            {
                CAS.AddChatMessage($"[{Globals.API.CoreMod.Name}] Unknown mod!");
                return true;
            }

            if (!mod.ModCommands.TryGetValue(trueCommand, out var parser))
            {
                if (trueCommand == "Help")
                {
                    InChatParseCommand($"{Globals.API.CoreMod.Name}:Help", target, connection);
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
                return new Enemy(); // Switch case will take care of vanilla enemies

            Enemy enemy = new Enemy() { enType = gameID };

            enemy.xRenderComponent.xOwnerObject = enemy;

            Globals.API.Loader.Library.Enemies[gameID].Config.Constructor?.Invoke(enemy);

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

            var eliteScaler = Globals.API.Loader.Library.Enemies[enemy.enType].Config.EliteScaler;

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

        /// <summary>
        /// Called when a server parses a client message. The message type's parser also receives the connection ID of the sender.
        /// </summary>
        internal static void InNetworkParseClientMessage(InMessage msg, byte messageType)
        {
            if (messageType != NetUtils.ModPacketType)
            {
                return;
            }

            try
            {

                NetUtils.ReadModData(msg, out Mod mod, out ushort packetID);

                if (!mod.ModPackets.ContainsKey(packetID))
                {
                    return;
                }

                byte[] content = msg.ReadBytes((int)(msg.BaseStream.Length - msg.BaseStream.Position));

                mod.ModPackets[packetID].ParseOnServer?.Invoke(new BinaryReader(new MemoryStream(content)), msg.iConnectionIdentifier);
            }
            catch (Exception e)
            {
                Globals.Logger.Error("ParseClientMessage failed! Exception: " + e.Message);
            }
        }

        /// <summary>
        /// Called when a client parses a server message.
        /// </summary>
        internal static void InNetworkParseServerMessage(InMessage msg, byte messageType)
        {
            if (messageType != NetUtils.ModPacketType)
            {
                return;
            }

            try
            {
                NetUtils.ReadModData(msg, out Mod mod, out ushort packetID);

                if (!mod.ModPackets.ContainsKey(packetID))
                {
                    return;
                }

                byte[] content = msg.ReadBytes((int)(msg.BaseStream.Length - msg.BaseStream.Position));

                mod.ModPackets[packetID].ParseOnClient?.Invoke(new BinaryReader(new MemoryStream(content)));
            }
            catch (Exception e)
            {
                Globals.Logger.Error("ParseServerMessage failed! Exception: " + e.Message);
            }
        }

        internal static void GauntletEnemySpawned(Enemy enemy)
        {
            foreach (Mod mod in Globals.API.Loader.Mods)
                mod.PostArcadeGauntletEnemySpawned(enemy);
        }

        public static string GetCueName(string ID) => Globals.API.Loader.GetCueName(ID);

        public static SoundBank GetEffectSoundBank(string ID) => Globals.API.Loader.GetEffectSoundBank(ID);

        public static SoundBank GetMusicSoundBank(string ID) => Globals.API.Loader.GetMusicSoundBank(ID);

        public static SpriteBatch SpriteBatch => typeof(Game1).GetField("spriteBatch", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Globals.Game) as SpriteBatch;

        public static TCMenuWorker TCMenuWorker { get; } = new TCMenuWorker();
    }
}
