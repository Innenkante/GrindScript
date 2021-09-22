using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoG.Modding.API
{
    public abstract partial class Mod
    {
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

        #region Game Logic Callbacks

        public virtual void OnDraw() { }

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
        /// Called when an Enemy dies.
        /// </summary>
        public virtual void PostEnemyKilled(Enemy enemy, AttackPhase killer) { }

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
        /// Called when a new Arcade room is entered, after it has been prepared by the game
        /// (i.e. enemies have been spawned, traps laid out, etc.)
        /// </summary>
        public virtual void PostArcadeRoomStart() { }

        /// <summary>
        /// Called when an Arcade room has completed (if applicable).
        /// </summary>
        public virtual void PostArcadeRoomComplete() { }

        /// <summary>
        /// Called when an Enemy has been spawned as part of an Arcade Gauntlet.
        /// </summary>
        public virtual void PostArcadeGauntletEnemySpawned(Enemy enemy) { }

        /// <summary>
        /// Called when a player uses an item. This method can be used to implement behavior for usable items.
        /// Items can be used if they have the "Usable" item category.
        /// </summary>
        public virtual void OnItemUse(ItemCodex.ItemTypes enItem, PlayerView xView, ref bool bSend) { }


        /// <summary>
        /// Called after a spell charge started.
        /// </summary>
        public virtual void PostSpellActivation(PlayerView xView, ISpellActivation xact, SpellCodex.SpellTypes enType, int iBoostState) { }

        /// <summary>
        /// Called after a level was loaded.
        /// </summary>
        public virtual void PostLevelLoad(Level.ZoneEnum level, Level.WorldRegion region, bool staticOnly) { }

        #endregion
    }
}
