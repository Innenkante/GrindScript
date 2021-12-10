using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using SoG.Modding.Content;
using SoG.Modding.GrindScriptMod;
using SoG.Modding.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SoG.Modding.Patching
{
    internal static class PatchHelper
    {
        /// <summary>
        /// Initializes all mod content. Checks arcade save for compatibility. 
        /// </summary>
        public static void DoModContentLoad()
        {
            Globals.ModManager.Loader.Reload();

            MainMenuWorker.AnalyzeStorySavesForCompatibility();
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

            LevelEntry entry = Globals.ModManager.Library.GetStorage<Level.ZoneEnum, LevelEntry>()[type];

            try
            {
                entry.loader?.Invoke(staticOnly);
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

            Mod mod = Globals.ModManager.ActiveMods.FirstOrDefault(x => x.NameID == target);

            if (mod == null)
            {
                CAS.AddChatMessage($"[{Globals.ModManager.GrindScript.NameID}] Unknown mod!");
                return true;
            }

            Globals.ModManager.Library.TryGetModEntry<GrindScriptID.CommandID, CommandEntry>(mod, "", out CommandEntry entry);

            if (entry == null || !entry.commands.TryGetValue(trueCommand, out var parser))
            {
                if (trueCommand == "Help")
                {
                    InChatParseCommand($"{Globals.ModManager.GrindScript.NameID}:Help", target, connection);
                    return true;
                }

                CAS.AddChatMessage($"[{target}] Unknown command!");
                return true;
            }

            string[] args = ModUtils.GetArgs(message);

            Globals.Logger.Debug($"Parsed command {target} : {trueCommand}, arguments: {args.Length}");
            parser(args, connection);

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

            Globals.ModManager.Library.GetStorage<EnemyCodex.EnemyTypes, EnemyEntry>()[gameID].constructor?.Invoke(enemy);

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

            var eliteScaler = Globals.ModManager.Library.GetStorage<EnemyCodex.EnemyTypes, EnemyEntry>()[enemy.enType].eliteScaler;

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

                ServerSideParser parser = null;
                mod.GetNetwork()?.serverSide.TryGetValue(packetID, out parser);

                if (parser == null)
                {
                    return;
                }

                byte[] content = msg.ReadBytes((int)(msg.BaseStream.Length - msg.BaseStream.Position));

                parser.Invoke(new BinaryReader(new MemoryStream(content)), msg.iConnectionIdentifier);
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

                ClientSideParser parser = null;
                mod.GetNetwork()?.clientSide.TryGetValue(packetID, out parser);

                if (parser == null)
                {
                    return;
                }

                byte[] content = msg.ReadBytes((int)(msg.BaseStream.Length - msg.BaseStream.Position));

                parser.Invoke(new BinaryReader(new MemoryStream(content)));
            }
            catch (Exception e)
            {
                Globals.Logger.Error("ParseServerMessage failed! Exception: " + e.Message);
            }
        }

        public static void GauntletEnemySpawned(Enemy enemy)
        {
            foreach (Mod mod in Globals.ModManager.ActiveMods)
                mod.PostArcadeGauntletEnemySpawned(enemy);
        }

        public static void AddModdedPinsToList(List<PinCodex.PinType> list)
        {
            foreach (PinEntry entry in Globals.ModManager.Library.GetStorage<PinCodex.PinType, PinEntry>().Values)
            {
                if (entry.conditionToDrop == null || entry.conditionToDrop.Invoke())
                {
                    list.Add(entry.GameID);
                }
            }
        }

        public static string GetCueName(string GSID)
        {
            if (!ModUtils.SplitAudioID(GSID, out int entryID, out bool isMusic, out int cueID))
                return "";

            if (!Globals.ModManager.Library.TryGetEntry<GrindScriptID.AudioID, AudioEntry>((GrindScriptID.AudioID)entryID, out var entry))
            {
                return "";
            }

            return isMusic ? entry.indexedMusicCues[cueID] : entry.indexedEffectCues[cueID];
        }

        public static SoundBank GetEffectSoundBank(string audioID)
        {
            bool success = ModUtils.SplitAudioID(audioID, out int entryID, out bool isMusic, out _);
            if (!(success && !isMusic))
                return null;

            Globals.ModManager.Library.TryGetEntry<GrindScriptID.AudioID, AudioEntry>((GrindScriptID.AudioID)entryID, out var entry);

            return entry?.effectsSB;
        }

        public static SoundBank GetMusicSoundBank(string audioID)
        {
            bool success = ModUtils.SplitAudioID(audioID, out int entryID, out bool isMusic, out _);

            if (!(success && isMusic))
                return null;

            Globals.ModManager.Library.TryGetEntry<GrindScriptID.AudioID, AudioEntry>((GrindScriptID.AudioID)entryID, out var entry);

            return entry?.musicSB;
        }

        public static bool IsUniversalMusicBank(string bank)
        {
            if (bank == "UniversalMusic")
                return true;

            foreach (var mod in Globals.ModManager.ActiveMods)
            {
                if (mod.NameID == bank)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Checks whenever the given WaveBank is persistent. <para/>
        /// Persistent WaveBanks are never unloaded.
        /// </summary>
        public static bool IsUniversalMusicBank(WaveBank bank)
        {
            if (bank == null)
                return false;

            foreach (var mod in Globals.ModManager.ActiveMods)
            {
                Globals.ModManager.Library.TryGetModEntry<GrindScriptID.AudioID, AudioEntry>(mod, "", out var entry);
                if (entry != null && entry.universalWB == bank)
                    return true;
            }

            FieldInfo universalWaveBankField = typeof(SoundSystem).GetTypeInfo().GetField("universalMusicWaveBank", BindingFlags.NonPublic | BindingFlags.Instance);
            if (bank == universalWaveBankField.GetValue(Globals.Game.xSoundSystem))
                return true;

            return false;
        }


        public static SpriteBatch SpriteBatch => Globals.SpriteBatch;

        public static TCMenuWorker TCMenuWorker { get; } = new TCMenuWorker();

        public static MainMenuWorker MainMenuWorker { get; } = new MainMenuWorker();

        #region Delicate Versioning and Mod List Comparison callbacks

        public static bool CheckModListCompatibility(bool didVersionCheckPass, InMessage msg)
        {
            if (!didVersionCheckPass)
            {
                // It's actually just a crappy implementation of short circuiting for AND ¯\_(ツ)_/¯

                Globals.Logger.Info("Denying connection due to vanilla version discrepancy.");
                Globals.Logger.Info("Check if client is on the same game version, and is running GrindScript.");
                return false;
            }

            int failReason = 0;

            Globals.Logger.Debug("Reading mod list!");

            long savedStreamPosition = msg.BaseStream.Position;

            _ = msg.ReadByte(); // Game mode byte skipped

            bool readGSVersion = Version.TryParse(msg.ReadString(), out Version result);

            if (readGSVersion)
            {
                Globals.Logger.Info("Received GS version from client: " + result);
            }
            else
            {
                Globals.Logger.Info("Couldn't parse GS version from client!");
            }

            if (!readGSVersion || result != Globals.ModManager.GrindScript.ModVersion)
            {
                Globals.Logger.Info("Denying connection due to GrindScript version discrepancy.");
                Globals.Logger.Info("Check that server and client are running on the same GrindScript version.");
                return false;
            }

            List<ModMetadata> clientMods = new List<ModMetadata>();
            var serverMods = Globals.ModManager.ActiveMods;

            int clientModCount = msg.ReadInt32();
            int serverModCount = serverMods.Count;

            for (int index = 0; index < clientModCount; index++)
            {
                clientMods.Add(new ModMetadata()
                {
                    NameID = msg.ReadString(),
                    ModVersion = Version.Parse(msg.ReadString())
                });
            }

            if (clientModCount == serverModCount)
            {
                for (int index = 0; index < clientModCount; index++)
                {
                    if (serverMods[index].DisableObjectCreation)
                    {
                        continue;
                    }

                    if (clientMods[index].NameID != serverMods[index].NameID || clientMods[index].ModVersion != serverMods[index].ModVersion)
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
            foreach (var meta in clientMods)
            {
                Globals.Logger.Debug("    " + meta.NameID + ", v" + (meta.ModVersion?.ToString() ?? "Unknown"));
            }

            Globals.Logger.Debug($"Mods on server: ");
            foreach (var mod in serverMods)
            {
                Globals.Logger.Debug("    " + mod.NameID + ", v" + (mod.ModVersion?.ToString() ?? "Unknown"));
            }

            if (failReason == 1)
            {
                Globals.Logger.Debug($"Client has {clientModCount} mods, while server has {serverModCount}.");
            }
            else if (failReason == 2)
            {
                Globals.Logger.Debug($"Client's mod list doesn't seem compatible with server's mod list.");
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

            msg.Write(Globals.ModManager.GrindScript.ModVersion.ToString());

            msg.Write(Globals.ModManager.ActiveMods.Count);

            foreach (Mod mod in Globals.ModManager.ActiveMods)
            {
                if (mod.DisableObjectCreation)
                {
                    continue;
                }

                msg.Write(mod.NameID);
                msg.Write(mod.ModVersion.ToString());
            }

            Globals.Logger.Debug("Done with mod list!");
        }

        #endregion
    }
}
