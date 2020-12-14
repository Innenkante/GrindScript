using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoG.GrindScript;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SoG.ItemExample
{
    public class Mod: BaseScript
    {
        private bool questTaken = false;
        private bool questFinished = false;
        private ModItem Misc;
        private ModItem Shield;
        private ModItem Facegear;
        private ModItem Hat;
        private ModItem TwoHanded;
        private ModItem OneHanded;
        private ModItem Usable;

        public Mod()
        {

            Console.WriteLine("Hello World from Item Example Mod!");
            Console.WriteLine("This mod showcases the API's item support by creating a few custom items.");
        }

        public override void OnCustomContentLoad()
        {
            Console.WriteLine("ItemExample: Trying to load custom content....");

            Misc = ModItem.AddItem("Misc Example", "This is a custom misc item!", "roomba", 420, this);
            Misc.AddItemCategories(ItemCategories.Misc);

            Shield = ModItem.AddItem("Shield Example", "This custom shield can block even the most damaging attacks!", "WoodenShield", 420, this);
            Shield.AddItemCategories(ItemCategories.Shield);
            Shield.AddEquipmentInfo("Wooden");
            Shield.EquipInfo.SetStats(ShldHP: 1337);

            Facegear = ModItem.AddItem("Facegear Example", "This custom facegear saps your stats in exchange for a heavy damage boost!", "Flybold", 420, this);
            Facegear.AddItemCategories(ItemCategories.Facegear);
            Facegear.AddFacegearInfo("Flybold");
            Facegear.FacegearInfo.SetRenderOffsets(new Vector2(2f, -1f), new Vector2(5f, -3f), new Vector2(3f, -5f), new Vector2(2f, -1f));
            Facegear.FacegearInfo.SetStats(CritDMG: 80, Crit: 80, EPRegen: -55, DEF: -35, ASPD: -10, CSPD: -15, EP: -25);

            Hat = ModItem.AddItem("Hat Example", "With this custom hat, your braincase is safe even from a Collector gone mad!", "Slimeus", 420, this);
            Hat.AddItemCategories(ItemCategories.Hat);
            Hat.AddHatInfo("Slimeus");
            Hat.HatInfo.SetStats(DEF: 55, HP: 140);
            Hat.HatInfo.SetObstructions(true, true, false);
            Hat.HatInfo.SetRenderOffsets(new Vector2(4f, 7f), new Vector2(5f, 5f), new Vector2(5f, 5f), new Vector2(4f, 7f));

            TwoHanded = ModItem.AddItem("Two Handed Example", "This custom two handed weapon swings slow, but hard!", "Claymore", 420, this);
            TwoHanded.AddItemCategories(ItemCategories.Weapon);
            TwoHanded.AddWeaponInfo("Claymore", (int)WeaponCategory.TwoHanded, false);
            TwoHanded.WeaponInfo.SetStats(ATK: 100, ASPD: -8, Crit: 10, CritDMG: 15);

            OneHanded = ModItem.AddItem("The Free Weapon", "When all you have is a bent metal rod, every problem looks like an alien crab.", "Crowbar", 420, this);
            OneHanded.AddItemCategories(ItemCategories.Weapon);
            OneHanded.AddWeaponInfo("IronSword", (int)WeaponCategory.OneHanded, false);
            OneHanded.WeaponInfo.SetStats(ATK: 80, ASPD: 25);
            OneHanded.WeaponInfo.AddSpecialEffect((int)SpecialEffect._Unique_Pickaxe_InstantBreakEnvironment);

            Usable = ModItem.AddItem("DA BOMB", "They will never expect a KABOOM!", "roomba", 69, this);
            Usable.AddItemCategories(ItemCategories.Usable, ItemCategories.DontRemoveOnUse, ItemCategories.DontShowUsesLeft);
            ModLibrary.AddItemAlias("_Example_Usable", Usable.IntType);

            Console.WriteLine("ItemExample: Custom Content Loaded!");
        }

        public override bool OnChatParseCommand(string command, string argList, int connection)
        {
            switch (command)
            {
                case "gibitemsplz":
                    Misc.SpawnOn(LocalGame, LocalPlayer);
                    Shield.SpawnOn(LocalGame, LocalPlayer);
                    Facegear.SpawnOn(LocalGame, LocalPlayer);
                    TwoHanded.SpawnOn(LocalGame, LocalPlayer);
                    OneHanded.SpawnOn(LocalGame, LocalPlayer);
                    Hat.SpawnOn(LocalGame, LocalPlayer);
                    return false; // Do not check vanilla commands
                case "bombtime":
                    Usable.SpawnOn(LocalGame, LocalPlayer);
                    return false;
            }
            return true; // Do check vanilla commands
        }

        public override void OnItemUse(int enItem, dynamic xView, ref bool bSend)
        {
            if(enItem == ModLibrary.ItemAliasValue("_Example_Usable"))
            {
                // Code
                dynamic randMachine = Utils.GetGameType("SoG.CAS").GetProperty("RandomInLogic").GetValue(null);

                /*
                Vector2 v2Dir = new Vector2(0f, -1f);
                float fMos = (float)Math.PI * 2f * (float)randMachine.NextDouble();
                v2Dir = (dynamic)Utils.GetGameType("SoG.Utility").GetMethod("RotateDirectionVector").Invoke(null, new object[] { v2Dir, fMos });
                v2Dir *= 0.5f + 1.5f * (float)randMachine.NextDouble();
                int iExplodeAt = 90;
                dynamic bomb = Utils.GetTheGame()._EntityMaster_AddDynamicEnvironment(Utils.GetEnumObject("SoG.DynamicEnvironmentCodex+ObjectTypes", 58), LocalPlayer.Original.xEntity.xTransform.v2Pos, LocalPlayer.Original.xEntity.xRenderComponent.fVirtualHeight, LocalPlayer.Original.xEntity.xCollisionComponent.ibitCurrentColliderLayer);
                bomb.SetInfo_Bounce(v2Dir, iExplodeAt);
                */

                int iBlowIn = 240;
                dynamic xBadgeBomb = Utils.GetTheGame()._EntityMaster_AddSpellInstance(Utils.GetEnumObject("SoG.SpellCodex+SpellTypes", 3009), xView.xEntity, xView.xEntity.xTransform.v2Pos, true, iBlowIn);
                xBadgeBomb.xAttackPhase.xStats.iBaseDamage = 1400;
                xBadgeBomb.xAttackPhase.xStats.fKnockBack = 60f;
                xBadgeBomb.xAttackPhase.xStats.iBreakingPower = 9;
                xBadgeBomb.iBlowAt = iBlowIn;
                xBadgeBomb.xAttackPhase.lenLayers.Add(Utils.GetEnumObject("SoG.Collider+ColliderLayers", 1)); // Hits players
                Utils.GetTheGame()._EntityMaster_AddWatcher(Utils.ConstructObject("Watchers.WhiteFadeInWatcher", new object[] { xBadgeBomb.xRenderComponent, 8 }));
                Utils.GetTheGame()._EntityMaster_AddWatcher(Utils.ConstructObject("Watchers.ScaleInRenderComponent", new object[] { xBadgeBomb.xRenderComponent, 8 }));

            }
        }
    }
}
