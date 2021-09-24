using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SoG.Modding.Utils;
using System.IO;
using SoG.Modding.LibraryEntries;
using SoG.Modding.GrindScriptMod;

namespace SoG.Modding.Patches
{
    internal static class HelperCallbacks
    {
        /// <summary>
        /// Initializes all mod content. Checks arcade save for compatibility.
        /// </summary>
        public static void DoModContentLoad()
        {
            Globals.Logger.Debug("Loading Mod Content...");

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
                        Globals.Logger.Error($"Mod {mod.GetType().Name} threw an exception during mod content loading: {e.Message}");
                    }
                });
            }

            MainMenuWorker.AnalyzeArcadeSavesForCompatibility();
        }

        /// <summary>
        /// Updates version so that we can tell that the game uses GrindScript.
        /// </summary>
        public static void UpdateVersionNumber()
        {
            Globals.Logger.Debug("Updating Version Number...");

            Globals.GameVanillaVersion = Globals.Game.sVersionNumberOnly;

            Globals.SetVersionTypeAsModded(true);

            var versionDisplayField = typeof(Game1).GetField("sVersion", BindingFlags.NonPublic | BindingFlags.Instance);

            versionDisplayField.SetValue(Globals.Game, Globals.GameVanillaVersion as string + " with GrindScript");

            Globals.Logger.Debug("Game Long Version: " + Globals.GameLongVersion);
            Globals.Logger.Debug("Game Short Version: " + Globals.GameShortVersion);
            Globals.Logger.Debug("Game Vanilla Version: " + Globals.GameVanillaVersion);
        }

        /// <summary>
        /// Executes additional code after a level's blueprint has been processed.
        /// </summary>
        public static void InLevelLoadDoStuff(Level.ZoneEnum type, bool staticOnly)
        {
            // Modifying vanilla levels not supported yet

            if (!type.IsFromMod())
                return;

            LevelEntry entry = Globals.API.Loader.Library.Levels[type];

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
        public static bool InChatParseCommand(string command, string message, int connection)
        {
            string[] words = command.Split(':');
            if (words.Length < 2)
                return false; // Is probably a vanilla command

            string target = words[0];
            string trueCommand = command.Substring(command.IndexOf(':') + 1);

            Mod mod = Globals.API.Loader.Mods.FirstOrDefault(x => x.NameID == target);

            if (mod == null)
            {
                CAS.AddChatMessage($"[{Globals.API.GrindScript.NameID}] Unknown mod!");
                return true;
            }

            if (!mod.ModCommands.TryGetValue(trueCommand, out var parser))
            {
                if (trueCommand == "Help")
                {
                    InChatParseCommand($"{Globals.API.GrindScript.NameID}:Help", target, connection);
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
        public static Enemy InGetEnemyInstance(EnemyCodex.EnemyTypes gameID, Level.WorldRegion assetRegion)
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
        public static bool InEnemyMakeElite(Enemy enemy)
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
        public static void InNetworkParseClientMessage(InMessage msg, byte messageType)
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
        public static void InNetworkParseServerMessage(InMessage msg, byte messageType)
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

        public static void GauntletEnemySpawned(Enemy enemy)
        {
            foreach (Mod mod in Globals.API.Loader.Mods)
                mod.PostArcadeGauntletEnemySpawned(enemy);
        }

        public static void AddModdedPinsToList(List<PinCodex.PinType> list)
        {
            foreach (PinEntry entry in Globals.API.Loader.Library.Pins.Values)
            {
                if (entry.Config.ConditionToDrop == null || entry.Config.ConditionToDrop.Invoke())
                {
                    list.Add(entry.GameID);
                }
            }
        }

        public static string GetCueName(string ID) => Globals.API.Loader.GetCueName(ID);

        public static SoundBank GetEffectSoundBank(string ID) => Globals.API.Loader.GetEffectSoundBank(ID);

        public static SoundBank GetMusicSoundBank(string ID) => Globals.API.Loader.GetMusicSoundBank(ID);

        public static SpriteBatch SpriteBatch => Globals.SpriteBatch;

        public static TCMenuWorker TCMenuWorker { get; } = new TCMenuWorker();

        public static MainMenuWorker MainMenuWorker { get; } = new MainMenuWorker();

        #region Delicate Versioning and Mod List Comparison callbacks

        public static bool CheckModListCompatibility(bool didVersionCheckPass, InMessage msg)
        {
            if (!didVersionCheckPass)
            {
                // It's actually just a crappy implementation of short circuiting for AND ¯\_(ツ)_/¯

                Globals.Logger.Debug("Denying connection due to version discrepancy.");
                Globals.Logger.Debug("Check if client is on the same game version, and is running GrindScript.");
                return false;
            }

            int failReason = 0;

            Globals.Logger.Debug("Reading mod list!");

            long savedStreamPosition = msg.BaseStream.Position;

            _ = msg.ReadByte(); // Game mode byte skipped

            List<string> clientModNames = new List<string>();
            var serverMods = Globals.API.Loader.Mods;

            int clientModCount = msg.ReadInt32();
            int serverModCount = serverMods.Count;

            for (int index = 0; index < clientModCount; index++)
            {
                clientModNames.Add(msg.ReadString());
            }

            if (clientModCount == serverModCount)
            {
                for (int index = 0; index < clientModCount; index++)
                {
                    if (clientModNames[index] != serverMods[index].NameID)
                    {
                        failReason = 2;
                        break;
                    }
                }
            }
            else
            {
                failReason = 1;
            }

            Globals.Logger.Debug($"Mods received from client: ");
            foreach (var name in clientModNames)
            {
                Globals.Logger.Debug("    " + name);
            }

            Globals.Logger.Debug($"Mods on server: ");
            foreach (var mod in serverMods)
            {
                Globals.Logger.Debug("    " + mod.NameID);
            }

            if (failReason == 1)
            {
                Globals.Logger.Debug($"Client has {clientModCount} mods, while server has {serverModCount}.");
            }
            else if (failReason == 2)
            {
                Globals.Logger.Debug($"Client's mod list doesn't compare equal to server's mod list.");
            }

            if (failReason != 0)
            {
                Globals.Logger.Debug("Denying connection due to mod discrepancy.");
            }
            else
            {
                Globals.Logger.Debug("Client mod list seems compatible!");
            }

            msg.BaseStream.Position = savedStreamPosition;

            return failReason == 0;
        }

        public static void WriteModList(OutMessage msg)
        {
            Globals.Logger.Debug("Writing mod list!");

            msg.Write(Globals.API.Loader.Mods.Count);

            foreach (Mod mod in Globals.API.Loader.Mods)
            {
                msg.Write(mod.NameID);
            }

            Globals.Logger.Debug("Done with mod list!");
        }

        #endregion
    }
}
