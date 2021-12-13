using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quests;
using SoG.Modding.Content;
using SoG.Modding.Patching;

namespace SoG.Modding.GrindScriptMod
{
    /// <summary>
    /// Dummy class that acts as an interface for the game's content.
    /// </summary>
    public class VanillaMod : Mod
    {
        internal VanillaMod()
        {

        }

        public override bool DisableObjectCreation => true;

        public override bool AllowDiscoveryByMods => true;

        public override Version ModVersion => new Version("0.0.0.0");

        public override string NameID => "SoG";

        public override void Load()
        {
            Logger.Debug("Parsing vanilla Objects!");

            Logger.Debug("Curses...");

            var curseEntries = Globals.Manager.Library.GetStorage<RogueLikeMode.TreatsCurses, CurseEntry>();
            foreach (var gameID in IDExtension.GetAllSoGIDs<RogueLikeMode.TreatsCurses>())
            {
                CurseEntry parsedEntry = VanillaParser.ParseCurse(gameID);

                curseEntries.Add(gameID, parsedEntry);
            }

            Logger.Debug("Enemies...");

            var enemyEntries = Globals.Manager.Library.GetStorage<EnemyCodex.EnemyTypes, EnemyEntry>();
            foreach (var gameID in IDExtension.GetAllSoGIDs<EnemyCodex.EnemyTypes>())
            {
                EnemyEntry parsedEntry = VanillaParser.ParseEnemy(gameID);

                enemyEntries.Add(gameID, parsedEntry);
            }

            EnemyCodex.lxSortedCardEntries.Clear();
            EnemyCodex.lxSortedDescriptions.Clear();

            Logger.Debug("Equipment Effects...");

            var specialEffectEntries = Globals.Manager.Library.GetStorage<EquipmentInfo.SpecialEffect, EquipmentEffectEntry>();
            foreach (var gameID in IDExtension.GetAllSoGIDs<EquipmentInfo.SpecialEffect>())
            {
                EquipmentEffectEntry parsedEntry = VanillaParser.ParseEquipmentEffect(gameID);

                specialEffectEntries.Add(gameID, parsedEntry);
            }

            Logger.Debug("Items...");

            var itemEntries = Globals.Manager.Library.GetStorage<ItemCodex.ItemTypes, ItemEntry>();
            foreach (var gameID in IDExtension.GetAllSoGIDs<ItemCodex.ItemTypes>())
            {
                ItemEntry parsedEntry = VanillaParser.ParseItem(gameID);

                itemEntries.Add(gameID, parsedEntry);
            }

            Logger.Debug("Levels...");

            var levelEntries = Globals.Manager.Library.GetStorage<Level.ZoneEnum, LevelEntry>();
            foreach (var gameID in IDExtension.GetAllSoGIDs<Level.ZoneEnum>())
            {
                LevelEntry parsedEntry = VanillaParser.ParseLevel(gameID);

                levelEntries.Add(gameID, parsedEntry);
            }

            Logger.Debug("Perks...");

            OriginalMethods.PerkInfoInit();

            var perkEntries = Globals.Manager.Library.GetStorage<RogueLikeMode.Perks, PerkEntry>();
            foreach (var gameID in IDExtension.GetAllSoGIDs<RogueLikeMode.Perks>())
            {
                PerkEntry parsedEntry = VanillaParser.ParsePerk(gameID);

                perkEntries.Add(gameID, parsedEntry);
            }

            RogueLikeMode.PerkInfo.lxAllPerks.Clear();

            Logger.Debug("Pins...");

            var pinEntries = Globals.Manager.Library.GetStorage<PinCodex.PinType, PinEntry>();
            foreach (var gameID in IDExtension.GetAllSoGIDs<PinCodex.PinType>())
            {
                PinEntry parsedEntry = VanillaParser.ParsePin(gameID);

                pinEntries.Add(gameID, parsedEntry);
            }

            Logger.Debug("Loaded Vanilla Objects!");

            Logger.Debug("Quests...");

            var questEntries = Globals.Manager.Library.GetStorage<QuestCodex.QuestID, QuestEntry>();
            foreach (var gameID in IDExtension.GetAllSoGIDs<QuestCodex.QuestID>())
            {
                QuestEntry parsedEntry = VanillaParser.ParseQuest(gameID);

                questEntries.Add(gameID, parsedEntry);
            }

            Logger.Debug("Spells...");

            var spellEntries = Globals.Manager.Library.GetStorage<SpellCodex.SpellTypes, SpellEntry>();
            foreach (var gameID in IDExtension.GetAllSoGIDs<SpellCodex.SpellTypes>())
            {
                SpellEntry parsedEntry = VanillaParser.ParseSpell(gameID);

                spellEntries.Add(gameID, parsedEntry);
            }

            Logger.Debug("Status Effects...");

            var statusEffectEntries = Globals.Manager.Library.GetStorage<BaseStats.StatusEffectSource, StatusEffectEntry>();
            foreach (var gameID in IDExtension.GetAllSoGIDs<BaseStats.StatusEffectSource>())
            {
                StatusEffectEntry parsedEntry = VanillaParser.ParseStatusEffect(gameID);

                statusEffectEntries.Add(gameID, parsedEntry);
            }

            Logger.Debug("World Regions...");

            var worldRegionEntries = Globals.Manager.Library.GetStorage<Level.WorldRegion, WorldRegionEntry>();
            foreach (var gameID in IDExtension.GetAllSoGIDs<Level.WorldRegion>())
            {
                WorldRegionEntry parsedEntry = VanillaParser.ParseWorldRegion(gameID);

                worldRegionEntries.Add(gameID, parsedEntry);
            }

            Logger.Debug("Parsed all vanilla objects!");
        }

        public override void Unload()
        {
            Logger.Debug("Unloaded VanillaMod!");
        }
    }
}
