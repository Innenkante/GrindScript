using Microsoft.Xna.Framework.Content;
using SoG.Modding.Core;
using SoG.Modding.Utils;
using System;
using System.IO;

namespace SoG.Modding.API
{
    /// <summary>
    /// Represents the base class for all mods.
    /// Mod DLLs should have one class that subclasses from BaseScript to be usable. <para/>
    /// All mods need to implement the LoadContent method. GrindScript will call this method when modded content should be loaded. <para/>
    /// BaseScript has a few callbacks that can be overriden to add extra behavior to the game for certain events.
    /// </summary>
    public abstract partial class BaseScript
    {
        /// <summary>
        /// The name of the mod, used as an identifier.
        /// By default, it is equal to GetType().FullName.
        /// </summary>
        public virtual string Name => GetType().FullName;

        /// <summary>
        /// The path to the mod's assets relative to the "Content" folder.
        /// By default, it is equal to "ModContent/{Name}".
        /// </summary>
        public virtual string AssetPath => Name;

        /// <summary>
        /// The index of the mod in load order.
        /// </summary>
        internal int LoadOrder { get; set; }

        /// <summary>
        /// The library of modded content owned by this mod.
        /// This library requires save / load bookkeeping.
        /// </summary>
        internal readonly ModLibrary Library = new ModLibrary();

        /// <summary>
        /// The default Logger for this mod.
        /// </summary>
        public ConsoleLogger Logger { get; private set; }

        /// <summary>
        /// The default ContentManager for this mod.
        /// </summary>
        public ContentManager Content { get; internal set; }


        /// <summary>
        /// A reference to the modding API, for ease of use.
        /// </summary>
        public GrindScript ModAPI { get; internal set; }

        public BaseScript()
        {
            Logger = new ConsoleLogger(LogLevels.Debug, GetType().Name) { SourceColor = ConsoleColor.Yellow };
        }

        /// <summary>
        /// Creates modded game content. GrindScript calls this method during the game's initialization.
        /// </summary>
        public abstract void LoadContent();

        #region Saving and Loading Callbacks

        /// <summary>
        /// Called when character files (".cha") need saving.
        /// You can write your Story Mode character-related information into the given stream.
        /// </summary>
        public virtual void SaveCharacterData(BinaryWriter stream) { }

        /// <summary>
        /// Called when character files (".cha") need loading.
        /// You can load your previously written data from this stream.
        /// The method must also handle cases where the stream is empty, or incomplete.
        /// </summary>
        public virtual void LoadCharacterData(BinaryReader stream) { }

        /// <summary>
        /// Called when world files (".wld") need saving.
        /// You can write your Story Mode world-related information into the given stream.
        /// </summary>
        public virtual void SaveWorldData(BinaryWriter stream) { }

        /// <summary>
        /// Called when world files (".wld") need loading.
        /// You can load your previously written data from this stream.
        /// The method must also handle cases where the stream is empty, or incomplete.
        /// </summary>
        public virtual void LoadWorldData(BinaryReader stream) { }

        /// <summary>
        /// Called when arcade files (".sav") need saving.
        /// You can write your Arcade-related information into the given stream.
        /// </summary>
        public virtual void SaveArcadeData(BinaryWriter stream) { }

        /// <summary>
        /// Called when arcade files (".sav") need loading.
        /// You can load your previously written data from this stream.
        /// The method must also handle cases where the stream is empty, or incomplete.
        /// </summary>
        public virtual void LoadArcadeData(BinaryReader stream) { }

        #endregion

        #region Game Hooks

        /// <summary>
        /// Called when the game begins a frame render.
        /// </summary>
        public virtual void OnDrawBegin() { }

        /// <summary>
        /// Called when a player is damaged by something.
        /// </summary>
        public virtual void OnPlayerDamaged(PlayerView player, ref int damage, ref byte type) { }

        /// <summary>
        /// Called when a player dies.
        /// </summary>
        public virtual void OnPlayerKilled(PlayerView player) { }

        /// <summary>
        /// Called after a player levels up.
        /// During save file loading, this method is called multiple times to initialize the player's stats to their level.
        /// </summary>
        public virtual void PostPlayerLevelUp(PlayerView player) { }

        /// <summary>
        /// Called when an enemy is damaged by something.
        /// </summary>
        public virtual void OnEnemyDamaged(Enemy enemy, ref int damage, ref byte type) { }

        /// <summary>
        /// Called when an NPC is damaged by something.
        /// </summary>
        public virtual void OnNPCDamaged(NPC enemy, ref int damage, ref byte type) { }

        /// <summary>
        /// Called when a player interacts with an NPC
        /// </summary>
        public virtual void OnNPCInteraction(NPC npc) { }

        /// <summary>
        /// Called when the game loads Arcadia's level.
        /// </summary>
        public virtual void OnArcadiaLoad() { }

        /// <summary>
        /// Called when a player uses an item. This method can be used to implement behavior for usable items.
        /// Items can be used if they have the "Usable" item category.
        /// </summary>
        public virtual void OnItemUse(ItemCodex.ItemTypes enItem, PlayerView xView, ref bool bSend) { }

        /// <summary>
        /// Called when a new Arcade room is entered, after it has been prepared by the game
        /// (i.e. enemies have been spawned, traps laid out, etc.)
        /// </summary>
        public virtual void PostArcadeRoomStart(GameSessionData.RogueLikeSession session) { }

        // TODO Implement the following callbacks

        /// <summary>
        /// Called after a spell charge started.
        /// </summary>
        public virtual void PostSpellActivation(PlayerView xView, ISpellActivation xact, SpellCodex.SpellTypes enType, int iBoostState) { }

        #endregion
    }
}