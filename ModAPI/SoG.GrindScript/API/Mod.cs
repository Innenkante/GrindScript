using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using SoG.Modding.Core;
using SoG.Modding.ModUtils;
using System;
using System.Collections.Generic;
using System.IO;

namespace SoG.Modding.API
{
    /// <summary>
    /// Represents the base class for all mods.
    /// Mod DLLs should have one class that subclasses from BaseScript to be usable. <para/>
    /// All mods need to implement the LoadContent method. GrindScript will call this method when modded content should be loaded. <para/>
    /// BaseScript has a few callbacks that can be overriden to add extra behavior to the game for certain events.
    /// </summary>
    public abstract partial class Mod
    {
        /// <summary>
        /// The name of the mod, used as an identifier.
        /// The default value is GetType().Name.
        /// This should be unique, as mods with the same identifier will cause conflicts.
        /// </summary>
        public virtual string Name => GetType().Name;

        /// <summary>
        /// The default Logger for this mod.
        /// </summary>
        public ILogger Logger { get; protected set; }

        /// <summary>
        /// The default ContentManager for this mod.
        /// </summary>
        public ContentManager Content { get; internal set; }

        public Mod()
        {
            var time = Launcher.LaunchTime;

            Logger = new ConsoleLogger(Globals.Logger?.LogLevel ?? LogLevels.Debug, Name)
            {
                SourceColor = ConsoleColor.Yellow,
                NextLogger = new FileLogger(Globals.Logger?.LogLevel ?? LogLevels.Debug, Name)
                {
                    FilePath = Path.Combine("Logs", $"ConsoleLog_{time.Year}.{time.Month}.{time.Day}_{time.Hour}.{time.Minute}.{time.Second}.txt")
                }
            };
        }

        /// <summary>
        /// Use this method to create or load your modded game content.
        /// </summary>
        public abstract void Load();

        /// <summary>
        /// The path to the mod's assets, relative to the "ModContent" folder.
        /// By default, it is equal to "ModContent/{ShortName}".
        /// </summary>
        public string AssetPath => Path.Combine("ModContent", Name) + "/";

        /// <summary>
        /// A reference to the registry, for ease of use.
        /// </summary>
        internal ModLoader Registry { get; set; }

        /// <summary>
        /// The index of the mod in load order.
        /// </summary>
        internal int ModIndex { get; set; }

        /// <summary>
        /// The packets parsable by this mod.
        /// </summary>
        internal Dictionary<ushort, ModPacket> ModPackets { get; } = new Dictionary<ushort, ModPacket>();

        /// <summary>
        /// The commands defined by this mod.
        /// </summary>
        internal Dictionary<string, CommandParser> ModCommands { get; } = new Dictionary<string, CommandParser>();

        /// <summary>
        /// Stores modded audio defined by this mod.
        /// </summary>
        internal ModAudio Audio { get; } = new ModAudio();

        /// <summary>
        /// Gets a collection of the modded game objects for this mod.
        /// </summary>
        private ModLibrary GetLibrary()
        {
            return Registry.Library.GetLibraryOfMod(this);
        }

        public class ModPacket
        {
            /// <summary>
            /// A delegate used to parse client messages on the server.
            /// The BinaryReader can be used to read the data written by the mod.
            /// The identifier can be used to retrieve the PlayerView.
            /// </summary>
            public Action<BinaryReader, long> ParseOnServer { get; set; }

            /// <summary>
            /// A delegate used to parse server messages on the client.
            /// The BinaryReader can be used to read the data written by the mod.
            /// </summary>
            public Action<BinaryReader> ParseOnClient { get; set; }
        }

        internal class ModAudio
        {
            public bool IsReady = false;

            public SoundBank EffectsSB; // "<Mod>Effects.xsb"

            public WaveBank EffectsWB; // "<Mod>Music.xwb"

            public SoundBank MusicSB; //"<Mod>Music.xsb"

            public WaveBank UniversalWB; // "<Mod>.xwb", never unloaded

            public List<string> IndexedEffectCues = new List<string>();

            public List<string> IndexedMusicCues = new List<string>();

            public List<string> IndexedMusicBanks = new List<string>();
        }
    }
}