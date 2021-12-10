using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quests;

namespace SoG.Modding
{
    public static class Game1Extension
    {
        /// <summary>
        /// Adds a quest to the list of active quests if it's not active already.
        /// </summary>
        public static void EXT_AddQuest(this Game1 game, QuestCodex.QuestID questID)
        {
            if (!questID.IsFromMod() && !questID.IsFromSoG())
            {
                Globals.Logger.Error("Couldn't add quest " + questID + ": It's neither a mod nor a vanila quest.");
            }

            QuestLog log = game.xLocalPlayer.xJournalInfo.xQuestLog;

            if (log.FindActiveQuestByEnum(questID) != null)
            {
                return;
            }

            Quest quest = QuestCodex.GetQuestInstance(questID);

            Globals.Game._Dialogue_EnterWorldDialogue("");
            Globals.Game.xDialogueSystem.SetPopUpDialogue(new QuestAcceptRenderComponent(quest, game.Content));

            log.AddQuest(quest);
        }

        /// <summary>
        /// Removes a quest from the list of completed or active quests.
        /// </summary>
        public static void EXT_ForgetQuest(this Game1 game, QuestCodex.QuestID questID)
        {
            if (!questID.IsFromMod() && !Enum.IsDefined(typeof(QuestCodex.QuestID), questID))
            {
                Globals.Logger.Error("Couldn't forget quest " + questID + ": It's neither a mod nor a vanila quest.");
            }

            QuestLog log = game.xLocalPlayer.xJournalInfo.xQuestLog;

            if (CAS.GameMode == StateMaster.GameModes.Story)
            {
                Quest completedQuest = log.FindCompletedQuestByEnum(questID);
                Quest activeQuest = log.FindActiveQuestByEnum(questID);

                log.lxCompletedQuests.Remove(completedQuest);
                log.lxActiveQuests.Remove(activeQuest);
            }
            else
            {
                CAS.LocalRogueLikeData.lenCompletedQuests.Remove(questID);
                CAS.LocalRogueLikeData.lenRemoteCompletedQuests.Remove(questID);
            }
        }

        /// <summary>
        /// Adds a miscellaneous text entry to the default (English) misc text collection of the game.
        /// </summary>
        public static MiscText EXT_AddMiscText(this Game1 game, string group, string entry, string text)
        {
            MiscText miscText = new MiscText();

            Dictionary<string, MiscTextCollection> colDict = game.xMiscTextGod_Default.dsxTextCollections;

            if (!colDict.TryGetValue(group, out MiscTextCollection col))
            {
                col = colDict[group] = new MiscTextCollection();
            }

            col.dsxTexts[entry] = miscText;

            miscText.sUnparsedFullLine = miscText.sUnparsedBaseLine = text;

            return miscText;
        }

        /// <summary>
        /// Retrieves a miscellaneous text entry from the default (English) misc text collection of the game.
        /// </summary>
        public static MiscText EXT_GetMiscText(this Game1 game, string group, string entry)
        {
            game.xMiscTextGod_Default.dsxTextCollections.TryGetValue(group, out MiscTextCollection col);

            MiscText miscText = null;

            col?.dsxTexts.TryGetValue(entry, out miscText);

            return miscText;
        }

        /// <summary>
        /// Removes a miscellaneous text entry from the game.
        /// Only use this for texts that you have created yourself.
        /// </summary>
        public static bool EXT_RemoveMiscText(this Game1 game, string group, string entry)
        {
            game.xMiscTextGod_Default.dsxTextCollections.TryGetValue(group, out MiscTextCollection col);

            return col?.dsxTexts.Remove(entry) ?? false;
        }

        /// <summary>
        /// Reads a world actor sent via a message.
        /// Similar to <see cref="Game1._EntityMaster_ReadActor"/>, however it returns the ID and Entity Type read as out arguments.
        /// Unlike the vanilla function, you can use the read info to initialize a spell later, when the packet for the target entity arrives.
        /// </summary>
        public static WorldActor EXT_ReadWorldActor(this Game1 game, InMessage msg, out IEntity.EntityType type, out long ID)
        {
            type = IEntity.EntityType.NULL;
            ID = 0;

            WorldActor actor = null;
            switch (msg.ReadByte())
            {
                case 250:
                    return null;
                case 1:
                    long connectionID = msg.ReadInt64();
                    type = IEntity.EntityType.Player;
                    ID = connectionID;
                    actor = game.dixPlayers.ContainsKey(connectionID) ? game.dixPlayers[connectionID].xEntity : null;

                    break;
                case 2:
                    ushort enemyID = msg.ReadUInt16();
                    type = IEntity.EntityType.Enemy;
                    ID = enemyID;
                    actor = game.dixEnemyList.ContainsKey(enemyID) ? game.dixEnemyList[enemyID] : null;

                    break;
                case 3:
                    ushort npcID = msg.ReadUInt16();
                    ID = npcID;
                    type = IEntity.EntityType.NPC;
                    actor = game.dixNPCList.ContainsKey(npcID) ? game.dixNPCList[npcID] : null;

                    break;
            }

            return actor;
        }

        /// <summary>
        /// Returns the first enemy spawner that contains the given spawn point, or null if none do.
        /// </summary>
        public static EnemySpawner EXT_GetContainingSpawner(this Game1 game, Vector2 spawnPoint)
        {
            foreach (var bagman in game.xEntityMaster.dbyxBagmen.Values)
            {
                if (bagman is EnemySpawner spawner)
                {
                    foreach (var rect in spawner.lrecSpawnRectangles)
                    {
                        if (rect.Contains((int)spawnPoint.X, (int)spawnPoint.Y))
                        {
                            return spawner;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Returns a candidate for this enemy's home rectangle
        /// </summary>
        public static Rectangle EXT_GetHomeRectangle(this Game1 game, Enemy enemy)
        {
            foreach (var bagman in game.xEntityMaster.dbyxBagmen.Values)
            {
                if (bagman is EnemySpawner spawner)
                {
                    foreach (var rect in spawner.lrecSpawnRectangles)
                    {
                        if (rect.Contains((int)enemy.xTransform.v2Pos.X, (int)enemy.xTransform.v2Pos.Y))
                        {
                            return rect;
                        }
                    }
                }
            }

            return Globals.Game.xLevelMaster.xCurrentLevel.recCurrentBounds;
        }
    }
}
