using Microsoft.Xna.Framework.Content;
using SoG.Modding.Utils;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Quests;
using Lidgren.Network;
using HarmonyLib;
using SoG.Modding.Content;
using Microsoft.Xna.Framework;

namespace SoG.Modding
{
    /// <summary>
    /// The base class for all mods.
    /// </summary>
    /// <remarks>
    /// Mod DLLs need to have at one class that is derived from <see cref="Mod"/>. That class will be constructed by GrindScript when loading.
    /// </remarks>
    public abstract class Mod : IModMetadata
    {
        /// <summary>
        /// Gets the name of the mod. <para/>
        /// The name of a mod is used as an identifier, and should be unique between different mods!
        /// </summary>
        public virtual string NameID => GetType().Name;

        /// <summary>
        /// Gets the version of the mod.
        /// </summary>
        public virtual Version ModVersion => new Version(0, 0, 0, 0);

        /// <summary>
        /// Gets whenever the mod should have object creation disabled. <para/>
        /// Mods that have object creation disabled can't use methods such as <see cref="CreateItem(string)"/>. <para/>
        /// Additionally, mod information won't be sent in multiplayer or written in save files.
        /// </summary>
        public virtual bool DisableObjectCreation => false;

        /// <summary>
        /// Gets whenever <see cref="ModManager.GetMod(string)"/> can return a reference to this mod. <para/>
        /// Setting this to false will prevent other mods from accessing your mod, other than through reflection, or static methods of your own.
        /// </summary>
        public virtual bool AllowDiscoveryByMods => true;

        /// <summary>
        /// Gets or sets the disable status of the mod. Disabled mods act as if they weren't loaded at all.
        /// </summary>
        internal bool Disabled { get; set; } = false;

        /// <summary>
        /// Gets the default logger for this mod.
        /// </summary>
        public ILogger Logger { get; protected set; }

        /// <summary>
        /// Gets the default content manager for this mod. The root path is set to "Content/".
        /// </summary>
        public ContentManager Content { get; internal set; }

        /// <summary>
        /// Gets the path to the mod's assets, relative to the "ModContent" folder.
        /// The default value is "ModContent/{NameID}".
        /// </summary>
        public string AssetPath => Path.Combine("ModContent", NameID) + "/";

        /// <summary>
        /// Gets a reference to the mod manager.
        /// </summary>
        public ModManager Manager { get; internal set; }

        /// <summary>
        /// Gets whenever the mod is currently being loaded.
        /// </summary>
        public bool InLoad => Manager.Loader.ModInLoad == this;

        public Mod()
        {
            var time = Launcher.LaunchTime;

            Logger = new ConsoleLogger(Globals.Logger?.LogLevel ?? LogLevels.Debug, NameID)
            {
                SourceColor = ConsoleColor.Yellow,
                NextLogger = new FileLogger(Globals.Logger?.LogLevel ?? LogLevels.Debug, NameID)
                {
                    FilePath = Path.Combine("Logs", $"ConsoleLog_{time.Year}.{time.Month}.{time.Day}_{time.Hour}.{time.Minute}.{time.Second}.txt")
                }
            };
        }

        #region Virtual Methods

        /// <summary>
        /// Called when the mod is loaded. This is where all game stuff you want to use should be created.
        /// </summary>
        public abstract void Load();

        /// <summary>
        /// Called after all mods have been loaded.
        /// You can use this method to do some thing that you can't do in <see cref="Load"/>,
        /// such as getting audio IDs.
        /// </summary>
        public virtual void PostLoad() { }

        /// <summary>
        /// Called when the mod is unloaded. Use this method to clean up after your mod. <para/>
        /// For instance, you can undo Harmony patches, or revert changes to game data. <para/>
        /// Keep in mind that modded game objects such as Items are removed automatically.
        /// </summary>
        /// <remarks>
        /// Mods are unloaded in the inverse order that they were loaded in.
        /// </remarks>
        public abstract void Unload();

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

        /// <summary>
        /// Called before an enemy is created. You can edit the parameters before returning.
        /// </summary>
        public virtual void OnEnemySpawn(ref EnemyCodex.EnemyTypes enemy, ref Vector2 position, ref bool isElite, ref bool dropsLoot, ref int bitLayer, ref float virtualHeight, float[] behaviourVariables) { }

        /// <summary>
        /// Called after an enemy is created. If the enemy type was changed via prefix callback, the original type can be retrieved from original.
        /// </summary>
        public virtual void PostEnemySpawn(Enemy entity, EnemyCodex.EnemyTypes enemy, EnemyCodex.EnemyTypes original, Vector2 position, bool isElite, bool dropsLoot, int bitLayer, float virtualHeight, float[] behaviourVariables) { }

        #endregion

        #region Update Callbacks

        /// <summary>
        /// Called before stats are updated for an entity. 
        /// You can query the entity using <see cref="BaseStats.xOwner"/>.
        /// </summary>
        /// <param name="stats"> The entity's stats. </param>
        public virtual void OnBaseStatsUpdate(BaseStats stats) { }

        /// <summary>
        /// Called after stats are updated for an entity. 
        /// You can query the entity using <see cref="BaseStats.xOwner"/>.
        /// </summary>
        /// <param name="stats"> The entity's stats. </param>
        public virtual void PostBaseStatsUpdate(BaseStats stats) { }

        #endregion

        #region Game Object Creation Methods

        public AudioEntry CreateAudio()
        {
            return Manager.CreateObject<GrindScriptID.AudioID, AudioEntry>(this, "");
        }

        public CommandEntry CreateCommands()
        {
            return Manager.CreateObject<GrindScriptID.CommandID, CommandEntry>(this, "");
        }

        public CurseEntry CreateCurse(string modID)
        {
            return Manager.CreateObject<RogueLikeMode.TreatsCurses, CurseEntry>(this, modID);
        }

        public EnemyEntry CreateEnemy(string modID)
        {
            return Manager.CreateObject<EnemyCodex.EnemyTypes, EnemyEntry>(this, modID);
        }

        public EquipmentEffectEntry CreateEquipmentEffect(string modID)
        {
            return Manager.CreateObject<EquipmentInfo.SpecialEffect, EquipmentEffectEntry>(this, modID);
        }

        public ItemEntry CreateItem(string modID)
        {
            return Manager.CreateObject<ItemCodex.ItemTypes, ItemEntry>(this, modID);
        }

        public LevelEntry CreateLevel(string modID)
        {
            return Manager.CreateObject<Level.ZoneEnum, LevelEntry>(this, modID);
        }

        public NetworkEntry CreateNetwork()
        {
            return Manager.CreateObject<GrindScriptID.NetworkID, NetworkEntry>(this, "");
        }

        public PerkEntry CreatePerk(string modID)
        {
            return Manager.CreateObject<RogueLikeMode.Perks, PerkEntry>(this, modID);
        }

        public PinEntry CreatePin(string modID)
        {
            return Manager.CreateObject<PinCodex.PinType, PinEntry>(this, modID);
        }

        public QuestEntry CreateQuest(string modID)
        {
            return Manager.CreateObject<QuestCodex.QuestID, QuestEntry>(this, modID);
        }

        public SpellEntry CreateSpell(string modID)
        {
            return Manager.CreateObject<SpellCodex.SpellTypes, SpellEntry>(this, modID);
        }

        public StatusEffectEntry CreateStatusEffect(string modID)
        {
            return Manager.CreateObject<BaseStats.StatusEffectSource, StatusEffectEntry>(this, modID);
        }

        public WorldRegionEntry CreateWorldRegion(string modID)
        {
            return Manager.CreateObject<Level.WorldRegion, WorldRegionEntry>(this, modID);
        }

        #endregion

        #region Game Object Getters

        public AudioEntry GetAudio()
        {
            Manager.TryGetGameEntry<GrindScriptID.AudioID, AudioEntry>(this, "", out var entry);
            return entry;
        }

        public CommandEntry GetCommands()
        {
            Manager.TryGetGameEntry<GrindScriptID.CommandID, CommandEntry>(this, "", out var entry);
            return entry;
        }

        public CurseEntry GetCurse(string modID)
        {
            Manager.TryGetGameEntry<RogueLikeMode.TreatsCurses, CurseEntry>(this, modID, out var value);
            return value;
        }

        public EnemyEntry GetEnemy(string modID)
        {
            Manager.TryGetGameEntry<EnemyCodex.EnemyTypes, EnemyEntry>(this, modID, out var value);
            return value;
        }

        public EquipmentEffectEntry GetEquipmentEffect(string modID)
        {
            Manager.TryGetGameEntry<EquipmentInfo.SpecialEffect, EquipmentEffectEntry>(this, modID, out var value);
            return value;
        }

        public ItemEntry GetItem(string modID)
        {
            Manager.TryGetGameEntry<ItemCodex.ItemTypes, ItemEntry>(this, modID, out var value);
            return value;
        }

        public LevelEntry GetLevel(string modID)
        {
            Manager.TryGetGameEntry<Level.ZoneEnum, LevelEntry>(this, modID, out var value);
            return value;
        }

        public NetworkEntry GetNetwork()
        {
            Manager.TryGetGameEntry<GrindScriptID.NetworkID, NetworkEntry>(this, "", out var entry);
            return entry;
        }

        public PerkEntry GetPerk(string modID)
        {
            Manager.TryGetGameEntry<RogueLikeMode.Perks, PerkEntry>(this, modID, out var value);
            return value;
        }

        public PinEntry GetPin(string modID)
        {
            Manager.TryGetGameEntry<PinCodex.PinType, PinEntry>(this, modID, out var value);
            return value;
        }

        public QuestEntry GetQuest(string modID)
        {
            Manager.TryGetGameEntry<QuestCodex.QuestID, QuestEntry>(this, modID, out var value);
            return value;
        }

        public SpellEntry GetSpell(string modID)
        {
            Manager.TryGetGameEntry<SpellCodex.SpellTypes, SpellEntry>(this, modID, out var value);
            return value;
        }

        public StatusEffectEntry GetStatusEffect(string modID)
        {
            Manager.TryGetGameEntry<BaseStats.StatusEffectSource, StatusEffectEntry>(this, modID, out var value);
            return value;
        }

        public WorldRegionEntry GetWorldRegion(string modID)
        {
            Manager.TryGetGameEntry<Level.WorldRegion, WorldRegionEntry>(this, modID, out var value);
            return value;
        }

        #endregion

        #region Other Methods

        public void AddCraftingRecipe(ItemCodex.ItemTypes result, Dictionary<ItemCodex.ItemTypes, ushort> ingredients)
        {
            if (ingredients == null)
                throw new ArgumentNullException(nameof(ingredients));

            if (!Crafting.CraftSystem.RecipeCollection.ContainsKey(result))
            {
                var kvps = new KeyValuePair<ItemDescription, ushort>[ingredients.Count];

                int index = 0;
                foreach (var kvp in ingredients)
                    kvps[index++] = new KeyValuePair<ItemDescription, ushort>(ItemCodex.GetItemDescription(kvp.Key), kvp.Value);

                ItemDescription description = ItemCodex.GetItemDescription(result);
                Crafting.CraftSystem.RecipeCollection.Add(result, new Crafting.CraftSystem.CraftingRecipe(description, kvps));
            }

            Globals.Logger.Info($"Added recipe for item {result}!");
        }

        /// <summary>
        /// Sends a packet to the chosen Client if currently playing as a Server; otherwise, it does nothing.
        /// </summary>
        public void SendToClient(ushort packetID, Action<BinaryWriter> data, PlayerView receiver, SequenceChannel channel = SequenceChannel.PrioritySpells_20, NetDeliveryMethod reliability = NetDeliveryMethod.ReliableOrdered)
        {
            if (!NetUtils.IsServer)
                return;

            Globals.Game._Network_SendMessage(NetUtils.WriteModData(this, packetID, data), receiver, channel, reliability);
        }

        /// <summary>
        /// Sends a packet to all Clients, if currently playing as a Server; otherwise, it does nothing.
        /// </summary>
        public void SendToAllClients(ushort packetID, Action<BinaryWriter> data, SequenceChannel channel = SequenceChannel.PrioritySpells_20, NetDeliveryMethod reliability = NetDeliveryMethod.ReliableOrdered)
        {
            if (!NetUtils.IsServer)
                return;

            Globals.Game._Network_SendMessage(NetUtils.WriteModData(this, packetID, data), channel, reliability);
        }

        /// <summary>
        /// Sends a packet to all Clients, except one, if currently playing as a Server; otherwise, it does nothing.
        /// </summary>
        public void SendToAllClientsExcept(ushort packetID, Action<BinaryWriter> data, PlayerView excluded, SequenceChannel channel = SequenceChannel.PrioritySpells_20, NetDeliveryMethod reliability = NetDeliveryMethod.ReliableOrdered)
        {
            if (!NetUtils.IsServer)
                return;

            foreach (PlayerView view in Globals.Game.dixPlayers.Values)
            {
                if (view == excluded)
                    continue;

                Globals.Game._Network_SendMessage(NetUtils.WriteModData(this, packetID, data), channel, reliability);
            }
        }

        /// <summary>
        /// Sends a packet to the Server if currently playing as a Client; otherwise, it does nothing.
        /// </summary>
        public void SendToServer(ushort packetID, Action<BinaryWriter> data, SequenceChannel channel = SequenceChannel.PrioritySpells_20, NetDeliveryMethod reliability = NetDeliveryMethod.ReliableOrdered)
        {
            if (!NetUtils.IsClient)
                return;

            Globals.Game._Network_SendMessage(NetUtils.WriteModData(this, packetID, data), channel, reliability);
        }

        #endregion
    }
}