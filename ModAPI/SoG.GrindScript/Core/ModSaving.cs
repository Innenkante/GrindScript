using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using SoG.Modding.API;

namespace SoG.Modding.Core
{
    // Currently this class only houses internals
    // Later on, we can allow each mod to store some flags or something

    using Quests;

    public class ModSaving
    {
        public enum ModDataBlock : int
        {
            ModData_CharacterFile = 0,
            ModData_WorldFile = 1,
            ModData_ArcadeFile = 2,

            ItemID = 1000,
            EnemyID = 1001,
            QuestID_Character = 1002,
            QuestID_World = 1003,
            QuestID_Arcade = 1004,
            PerkID = 1005,
            CurseID = 1006,
        }

        public const int Version = 1;

        public const string SaveFileExtension = ".gs";

        private ModLoader _mods;

        internal ModSaving(ModLoader mods)
        {
            _mods = mods;
        }

        #region GrindScript internal methods

        internal void SaveModCharacter(BinaryWriter file)
        {
            SaveGrindScriptFile(file,
                ModDataBlock.ItemID,
                ModDataBlock.ModData_CharacterFile,
                ModDataBlock.EnemyID
                );
        }

        internal void LoadModCharacter(BinaryReader file)
        {
            LoadGrindScriptFile(file,
                ModDataBlock.ItemID,
                ModDataBlock.ModData_CharacterFile,
                ModDataBlock.EnemyID
                );
        }

        internal void SaveModWorld(BinaryWriter file)
        {
            SaveGrindScriptFile(file,
                ModDataBlock.ModData_WorldFile,
                ModDataBlock.QuestID_World
                );
        }

        internal void LoadModWorld(BinaryReader file)
        {
            LoadGrindScriptFile(file,
                ModDataBlock.ModData_WorldFile,
                ModDataBlock.QuestID_World
                );
        }

        internal void SaveModArcade(BinaryWriter file)
        {
            SaveGrindScriptFile(file,
                ModDataBlock.ItemID,
                ModDataBlock.ModData_ArcadeFile,
                ModDataBlock.PerkID,
                ModDataBlock.CurseID,
                ModDataBlock.EnemyID
                );
        }

        internal void LoadModArcade(BinaryReader file)
        {
            LoadGrindScriptFile(file,
                ModDataBlock.ItemID,
                ModDataBlock.ModData_ArcadeFile,
                ModDataBlock.PerkID,
                ModDataBlock.CurseID,
                ModDataBlock.EnemyID
                );
        }

        #endregion

        #region Helper write and read logic

        private void LoadGrindScriptFile(BinaryReader file, params ModDataBlock[] blocks)
        {
            HashSet<ModDataBlock> blockSet = new HashSet<ModDataBlock>(blocks);

            bool fileIsEmpty = file.PeekChar() == -1;

            if (fileIsEmpty)
                return;

            ItemCodex.ItemTypes itemShuffleIndex = ID.ItemIDEnd;
            RogueLikeMode.Perks perkShuffleIndex = ID.PerkIDEnd;
            RogueLikeMode.TreatsCurses curseShuffleIndex = ID.CurseIDEnd;
            EnemyCodex.EnemyTypes enemyShuffleIndex = ID.EnemyIDEnd;
            QuestCodex.QuestID questShuffleIndex = ID.QuestIDEnd;

            int previousVersion = file.ReadInt32();

            if (previousVersion != Version)
            {
                Globals.Logger.Info($"Loading save file with mismatched version. GrindScript save version is {Version}, while file version is {previousVersion}.");
            }

            int scriptCount = file.ReadInt32();

            while (scriptCount-- > 0)
            {
                bool skipLoading = false;

                string modName = file.ReadString();

                int blockCount = file.ReadInt32();

                Mod mod = _mods.Mods.FirstOrDefault(x => x.Name == modName);

                if (mod == null)
                {
                    Globals.Logger.Warn($"Encountered an unknown mod {modName} during save load! Will skip loading its data...");
                    skipLoading = true;
                }

                ModLibrary library = mod != null ? _mods.Library.GetLibraryOfMod(mod) : null;

                while (blockCount-- > 0)
                {
                    ModDataBlock blockType = ReadEnum<ModDataBlock>(file);

                    int blockSize = file.ReadInt32();

                    if (skipLoading)
                    {
                        file.BaseStream.Position += blockSize;
                        continue;
                    }

                    switch (blockType)
                    {
                        case ModDataBlock.ModData_CharacterFile:
                            ReadModBlock(file, mod.LoadCharacterData);
                            break;
                        case ModDataBlock.ItemID:
                            ReadIDBlock(file, library.Items.Values, itemShuffleIndex, UpdateItemIDs);
                            break;
                        case ModDataBlock.EnemyID:
                            ReadIDBlock(file, library.Enemies.Values, enemyShuffleIndex, UpdateEnemyIDs);
                            break;
                        case ModDataBlock.QuestID_Character:
                            ReadIDBlock(file, library.Quests.Values, questShuffleIndex, UpdateQuestIDs_Character);
                            break;
                        case ModDataBlock.ModData_WorldFile:
                            ReadModBlock(file, mod.LoadWorldData);
                            break;
                        case ModDataBlock.QuestID_World:
                            ReadIDBlock(file, library.Quests.Values, questShuffleIndex, UpdateQuestIDs_World);
                            break;
                        case ModDataBlock.ModData_ArcadeFile:
                            ReadModBlock(file, mod.LoadArcadeData);
                            break;
                        case ModDataBlock.PerkID:
                            ReadIDBlock(file, library.Perks.Values, perkShuffleIndex, UpdatePerkIDs);
                            break;
                        case ModDataBlock.CurseID:
                            ReadIDBlock(file, library.Curses.Values, curseShuffleIndex, UpdateCurseIDs);
                            break;
                        case ModDataBlock.QuestID_Arcade:
                            ReadIDBlock(file, library.Quests.Values, questShuffleIndex, UpdateQuestIDs_Arcade);
                            break;
                        default:
                            Globals.Logger.Warn($"Loading is not supported for block type {blockType}.");
                            file.BaseStream.Position += blockSize;
                            break;
                    }
                }
            }
        }

        private void SaveGrindScriptFile(BinaryWriter file, params ModDataBlock[] blocks)
        {
            HashSet<ModDataBlock> blockSet = new HashSet<ModDataBlock>(blocks);

            file.Write(Version);

            file.Write(_mods.Mods.Count);
            foreach (Mod mod in _mods.Mods)
            {
                file.Write(mod.Name);

                file.Write(blockSet.Count);

                ModLibrary library = _mods.Library.GetLibraryOfMod(mod);

                foreach (ModDataBlock blockType in blockSet)
                {
                    WriteEnum(file, blockType);

                    MemoryStream blockData;
                    using (BinaryWriter blockWriter = new BinaryWriter(blockData = new MemoryStream(1024)))
                    {
                        switch (blockType)
                        {
                            case ModDataBlock.ModData_CharacterFile:
                                WriteModBlock(blockWriter, mod.SaveCharacterData);
                                break;
                            case ModDataBlock.ItemID:
                                WriteIDBlock(blockWriter, library.Items.Values);
                                break;
                            case ModDataBlock.EnemyID:
                                WriteIDBlock(blockWriter, library.Enemies.Values);
                                break;
                            case ModDataBlock.QuestID_Character:
                                WriteIDBlock(blockWriter, library.Quests.Values);
                                break;
                            case ModDataBlock.ModData_WorldFile:
                                WriteModBlock(blockWriter, mod.SaveWorldData);
                                break;
                            case ModDataBlock.QuestID_World:
                                WriteIDBlock(blockWriter, library.Quests.Values);
                                break;
                            case ModDataBlock.ModData_ArcadeFile:
                                WriteModBlock(blockWriter, mod.SaveArcadeData);
                                break;
                            case ModDataBlock.PerkID:
                                WriteIDBlock(blockWriter, library.Perks.Values);
                                break;
                            case ModDataBlock.CurseID:
                                WriteIDBlock(blockWriter, library.Curses.Values);
                                break;
                            case ModDataBlock.QuestID_Arcade:
                                WriteIDBlock(blockWriter, library.Quests.Values);
                                break;
                            default:
                                Globals.Logger.Warn($"Saving is not supported for block type {blockType}.");
                                break;
                        }

                        file.Write((int)blockData.Length);
                        blockData.WriteTo(file.BaseStream);
                    }
                }
            }
        }

        private void WriteEnum<T>(BinaryWriter writer, T value) where T : struct, Enum
        {
            Type integer = typeof(T).GetEnumUnderlyingType();

            if (integer == typeof(sbyte)) writer.Write(Convert.ToSByte(value));
            else if (integer == typeof(short)) writer.Write(Convert.ToInt16(value));
            else if (integer == typeof(int)) writer.Write(Convert.ToInt32(value));
            else if (integer == typeof(long)) writer.Write(Convert.ToInt64(value));
            else if (integer == typeof(byte)) writer.Write(Convert.ToByte(value));
            else if (integer == typeof(ushort)) writer.Write(Convert.ToUInt16(value));
            else if (integer == typeof(uint)) writer.Write(Convert.ToUInt32(value));
            else if (integer == typeof(ulong)) writer.Write(Convert.ToUInt64(value));
            else throw new NotSupportedException("Can't write platform type integers");
        }

        private T ReadEnum<T>(BinaryReader reader) where T : struct, Enum
        {
            Type integer = typeof(T).GetEnumUnderlyingType();

            if (integer == typeof(sbyte)) return (T)Enum.ToObject(typeof(T), reader.ReadSByte());
            else if (integer == typeof(short)) return (T)Enum.ToObject(typeof(T), reader.ReadInt16());
            else if (integer == typeof(int)) return (T)Enum.ToObject(typeof(T), reader.ReadInt32());
            else if (integer == typeof(long)) return (T)Enum.ToObject(typeof(T), reader.ReadInt64());
            else if (integer == typeof(byte)) return (T)Enum.ToObject(typeof(T), reader.ReadByte());
            else if (integer == typeof(ushort)) return (T)Enum.ToObject(typeof(T), reader.ReadUInt16());
            else if (integer == typeof(uint)) return (T)Enum.ToObject(typeof(T), reader.ReadUInt32());
            else if (integer == typeof(ulong)) return (T)Enum.ToObject(typeof(T), reader.ReadUInt64());
            else throw new NotSupportedException("Can't read platform type integers");
        }

        private void UpdateIDs<IDType>(IEnumerable<IEntry<IDType>> entries, Dictionary<IDType, string> oldIDs, ref IDType temporaryID, Action<IDType, IDType> updater) where IDType : struct, Enum
        {
            var previousConflictingID = new Dictionary<string, IDType>();
            var remainingIDs = new Dictionary<IDType, string>(oldIDs);

            while (remainingIDs.Count > 0)
            {
                var pair = remainingIDs.First();

                IDType oldGameID = pair.Key;
                string modID = pair.Value;

                IEntry<IDType> entry = entries.FirstOrDefault(x => x.ModID == modID);

                if (entry == null)
                {
                    Globals.Logger.Warn($"Failed to find {typeof(IDType).Name}:{modID}. Game object was removed or has changed ModID.", source: nameof(UpdateIDs));
                    continue;
                }

                IDType newGameID = entry.GameID;

                if (oldGameID.Equals(newGameID))
                {
                    remainingIDs.Remove(oldGameID);
                    continue;
                }

                bool directSwapIsDangerous = remainingIDs.ContainsKey(newGameID);

                if (directSwapIsDangerous)
                {
                    string conflictModID = remainingIDs[newGameID];

                    updater.Invoke(newGameID, temporaryID);

                    previousConflictingID.Add(conflictModID, newGameID);
                    remainingIDs.Remove(newGameID);
                    remainingIDs.Add(temporaryID, conflictModID);

                    temporaryID = (IDType)Enum.ToObject(typeof(IDType), Convert.ToInt32(temporaryID) + 1);
                }

                updater.Invoke(entry.GameID, newGameID);
                remainingIDs.Remove(oldGameID);

                IDType oldGameID_actual = oldGameID;

                if (previousConflictingID.ContainsKey(modID))
                {
                    oldGameID_actual = previousConflictingID[modID];
                }

                Globals.Logger.Debug($"Updated ID {typeof(IDType).Name}:{modID}. GameID change: {oldGameID_actual} -> {newGameID}.", source: nameof(UpdateIDs));
            }
        }

        private void ReadIDBlock<IDType>(BinaryReader file, IEnumerable<IEntry<IDType>> entries, IDType shuffleIndex, Action<IDType, IDType> shuffler) where IDType : struct, Enum
        {
            Dictionary<IDType, string> oldIDs = new Dictionary<IDType, string>();

            int remainingIDs = file.ReadInt32();

            while (remainingIDs-- > 0)
            {
                IDType gameID = ReadEnum<IDType>(file);
                string modID = file.ReadString();

                oldIDs.Add(gameID, modID);
            }

            UpdateIDs(entries, oldIDs, ref shuffleIndex, shuffler);
        }

        private void WriteIDBlock<IDType>(BinaryWriter file, IEnumerable<IEntry<IDType>> entries) where IDType : struct, Enum
        {
            file.Write(entries.Count());
            foreach (var item in entries)
            {
                WriteEnum(file, item.GameID);
                file.Write(item.ModID);
            }
        }

        private void ReadModBlock(BinaryReader file, Action<BinaryReader> modReader)
        {
            int byteCount = file.ReadInt32();

            byte[] data = file.ReadBytes(byteCount);

            using (BinaryReader reader = new BinaryReader(new MemoryStream(data)))
            {
                modReader.Invoke(reader);
            }
        }

        private void WriteModBlock(BinaryWriter file, Action<BinaryWriter> modWriter)
        {
            MemoryStream stream;

            using (BinaryWriter writer = new BinaryWriter(stream = new MemoryStream()))
            {
                modWriter.Invoke(writer);

                file.Write((int)stream.Length);
                stream.WriteTo(file.BaseStream);
            }
        }

        #endregion

        #region ID Updaters

        private void UpdateItemIDs(ItemCodex.ItemTypes from, ItemCodex.ItemTypes to)
        {
            Inventory inventory = Globals.Game.xLocalPlayer.xInventory;
            Journal journal = Globals.Game.xLocalPlayer.xJournalInfo;

            // Shuffle inventory, discovered items, crafted items, and fishes

            if (inventory.denxInventory.ContainsKey(from))
            {
                inventory.denxInventory[to] = new Inventory.DisplayItem(inventory.denxInventory[from].iAmount, inventory.denxInventory[from].iPickupNumber, ItemCodex.GetItemDescription(to));
                inventory.denxInventory.Remove(from);
            }

            if (journal.henUniqueDiscoveredItems.Contains(from))
            {
                journal.henUniqueDiscoveredItems.Remove(from);
                journal.henUniqueDiscoveredItems.Add(to);
            }

            if (journal.henUniqueCraftedItems.Contains(from))
            {
                journal.henUniqueCraftedItems.Remove(from);
                journal.henUniqueCraftedItems.Add(to);
            }

            if (journal.henUniqueFishies.Contains(from))
            {
                journal.henUniqueFishies.Remove(from);
                journal.henUniqueFishies.Add(to);
            }
        }

        private void UpdatePerkIDs(RogueLikeMode.Perks from, RogueLikeMode.Perks to)
        {
            var session = Globals.Game.xGlobalData.xLocalRoguelikeData;

            if (session.enPerkSlot01 == from)
                session.enPerkSlot01 = to;

            if (session.enPerkSlot02 == from)
                session.enPerkSlot02 = to;

            if (session.enPerkSlot03 == from)
                session.enPerkSlot03 = to;

            for (int i = 0; i < session.lenPerksOwned.Count; i++)
            {
                if (session.lenPerksOwned[i] == from)
                    session.lenPerksOwned[i] = to;
            }
        }

        private void UpdateCurseIDs(RogueLikeMode.TreatsCurses from, RogueLikeMode.TreatsCurses to)
        {
            var session = Globals.Game.xGlobalData.xLocalRoguelikeData;

            if (session.enCurseTreatSlot01 == from)
                session.enCurseTreatSlot01 = to;

            if (session.enCurseTreatSlot02 == from)
                session.enCurseTreatSlot02 = to;

            if (session.enCurseTreatSlot03 == from)
                session.enCurseTreatSlot03 = to;
        }

        private void UpdateEnemyIDs(EnemyCodex.EnemyTypes from, EnemyCodex.EnemyTypes to)
        {
            var cards = Globals.Game.xLocalPlayer.xJournalInfo.henCardAlbum;
            if (cards.Remove(from))
            {
                cards.Add(to);
            }

            var knownEnemies = Globals.Game.xLocalPlayer.xJournalInfo.henKnownEnemies;
            if (knownEnemies.Remove(from))
            {
                knownEnemies.Add(to);
            }

            var killedEnemies = Globals.Game.xLocalPlayer.xJournalInfo.deniMonstersKilled;
            if (killedEnemies.ContainsKey(from))
            {
                killedEnemies.Add(to, killedEnemies[from]);
                killedEnemies.Remove(from);
            }
        }

        private void UpdateQuestIDs_World(QuestCodex.QuestID from, QuestCodex.QuestID to)
        {
            QuestLog log = Globals.Game.xLocalPlayer.xJournalInfo.xQuestLog;

            List<Quest> activeQuests = log.lxActiveQuests;
            List<Quest> completedQuests = log.lxCompletedQuests;

            for (int i = 0; i < activeQuests.Count; i++)
            {
                if (activeQuests[i].enQuestID == from)
                    activeQuests[i].enQuestID = to;
            }

            for (int i = 0; i < completedQuests.Count; i++)
            {
                if (completedQuests[i].enQuestID == from)
                    completedQuests[i].enQuestID = to;
            }

            Quest trackedQuest = Globals.Game.xHUD.xTrackedQuest;

            if (trackedQuest.enQuestID == from)
                trackedQuest.enQuestID = to;
        }

        private void UpdateQuestIDs_Character(QuestCodex.QuestID from, QuestCodex.QuestID to)
        {
            QuestLog log = Globals.Game.xLocalPlayer.xJournalInfo.xQuestLog;

            HashSet<QuestCodex.QuestID> completedQuests = log.henQuestsCompletedOnThisCharacter;

            if (completedQuests.Remove(from))
                completedQuests.Add(to);
        }

        private void UpdateQuestIDs_Arcade(QuestCodex.QuestID from, QuestCodex.QuestID to)
        {
            var data = Globals.Game.xGlobalData.xLocalRoguelikeData;

            if (data.xSavedQuestInstance.enQuestID == from)
                data.xSavedQuestInstance.enQuestID = to;

            if (data._enActiveQuest == from)
                data._enActiveQuest = to;

            List<QuestCodex.QuestID> completedQuests = data.lenCompletedQuests;
            List<QuestCodex.QuestID> remoteQuests = data.lenRemoteCompletedQuests;

            for (int i = 0; i < completedQuests.Count; i++)
            {
                if (completedQuests[i] == from)
                    completedQuests[i] = to;
            }

            for (int i = 0; i < remoteQuests.Count; i++)
            {
                if (remoteQuests[i] == from)
                    remoteQuests[i] = to;
            }
        }

        #endregion
    }
}
