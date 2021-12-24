using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quests;
using SoG.Modding.Content;
using SoG.Modding.Patching;
using SoG.Modding.Utils;

namespace SoG.Modding.GrindScriptMod
{
    /// <summary>
    /// Dummy mod that holds parsed vanilla entries.
    /// </summary>
    public class VanillaMod : Mod
    {
        internal VanillaMod()
        {

        }

        public override bool DisableObjectCreation => true;

        public override Version ModVersion => new Version("0.0.0.0");

        public override string NameID => "SoG";

        public override void Load()
        {
            FileLogger parseLog = null;

            if (Logger.LogLevel <= LogLevels.Debug)
            {
                Logger.Debug("Opening log VanillaParseLog.txt!");

                File.Delete(Path.Combine("Logs", "VanillaParseLog.txt"));
                parseLog = new FileLogger(LogLevels.Debug, "Source")
                {
                    FilePath = Path.Combine("Logs", "VanillaParseLog.txt")
                };
            }

            OriginalMethods.FillTreatList(Globals.Game.xShopMenu.xTreatCurseMenu);
            ParseEntries<RogueLikeMode.TreatsCurses, CurseEntry>(VanillaParser.ParseCurse, parseLog);

            ParseEntries<EnemyCodex.EnemyTypes, EnemyEntry>(VanillaParser.ParseEnemy, parseLog);
            EnemyCodex.lxSortedCardEntries.Clear();
            EnemyCodex.lxSortedDescriptions.Clear();
            EnemyCodex.denxDescriptionDict.Clear();

            ParseEntries<EquipmentInfo.SpecialEffect, EquipmentEffectEntry>(VanillaParser.ParseEquipmentEffect, parseLog);

            ParseEntries<ItemCodex.ItemTypes, ItemEntry>(VanillaParser.ParseItem, parseLog);

            ParseEntries<Level.ZoneEnum, LevelEntry>(VanillaParser.ParseLevel, parseLog);

            OriginalMethods.PerkInfoInit();
            ParseEntries<RogueLikeMode.Perks, PerkEntry>(VanillaParser.ParsePerk, parseLog);
            RogueLikeMode.PerkInfo.lxAllPerks.Clear();

            ParseEntries<PinCodex.PinType, PinEntry>(VanillaParser.ParsePin, parseLog);

            ParseEntries<QuestCodex.QuestID, QuestEntry>(VanillaParser.ParseQuest, parseLog);

            ParseEntries<SpellCodex.SpellTypes, SpellEntry>(VanillaParser.ParseSpell, parseLog);

            ParseEntries<BaseStats.StatusEffectSource, StatusEffectEntry>(VanillaParser.ParseStatusEffect, parseLog);

            ParseEntries<Level.WorldRegion, WorldRegionEntry>(VanillaParser.ParseWorldRegion, parseLog);

            parseLog?.FlushToDisk();
        }

        public override void Unload()
        {
            Logger.Debug("Unloaded VanillaMod!");
        }

        private void ParseEntries<IDType, EntryType>(Func<IDType, EntryType> parser, FileLogger log = null)
            where IDType : struct, Enum
            where EntryType : Entry<IDType>
        {
            Logger.Debug("Parsing " + typeof(IDType) + " entries...");

            var entries = Globals.Manager.Library.GetAllEntries<IDType, EntryType>();
            foreach (var gameID in IDExtension.GetAllSoGIDs<IDType>())
            {
                try
                {
                    EntryType parsedEntry = parser.Invoke(gameID);
                    entries.Add(gameID, parsedEntry);
                }
                catch (Exception e)
                {
                    log?.Debug("Failed to parse entry " + typeof(IDType).Name + ":" + gameID + ". Exception: " + e.Message);
                    continue;
                }
            }
        }
    }
}
