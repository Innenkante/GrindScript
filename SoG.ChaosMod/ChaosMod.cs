using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SoG.GrindScript;

namespace SoG.ChaosMod
{
    public class ChaosMod : BaseScript
    {
        private bool questTaken = false;
        private bool questFinished = false;
        private ModItem alex;
        private ModItem GordonFreeman;
        private ModItem InstaRepair;
        private ModItem Hattus;
        private ModItem Weapon;
        private ModItem WeaponOne;

        bool grant = false;
        

        public ChaosMod()
        {
            
            Console.WriteLine("Hello World from Chaosmod!");
            Console.WriteLine(typeof(ChaosMod).Name);
        }

        public override void OnDraw()
        {

            if (!questTaken)
                return;


           /* var font = GetFont(FontType.Verdana8);


            SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            SpriteBatch.DrawString(font, "Current Floor: " + LocalGame.GetCurrentFloor() + "/" + "5", new Vector2(400, 5), Color.Black);
            SpriteBatch.End();
            */
        }

        public override void OnCustomContentLoad()
        {

            Console.WriteLine("Trying to load custom content....");

            alex = ModItem.AddItem("Alex", "Knows the game", "roomba", 420, this);
            alex.AddItemCategories(ItemCategories.Misc);

            GordonFreeman = ModItem.AddItem("The Freeman's Shield", "*Insert relevant and funny description*", "WoodenShield", 420, this);
            GordonFreeman.AddItemCategories(ItemCategories.Shield);
            GordonFreeman.AddEquipmentInfo("Wooden");
            GordonFreeman.EquipInfo.SetStats(ShldHP: 1337);
            
            InstaRepair = ModItem.AddItem("Critical Blindfold", "True strength reveals itself when all oods are stacked against you.", "Flybold", 420, this);
            InstaRepair.AddItemCategories(ItemCategories.Facegear);
            InstaRepair.AddFacegearInfo("Flybold");
            InstaRepair.FacegearInfo.SetRenderOffsets(new Vector2(2f, -1f), new Vector2(5f, -3f), new Vector2(3f, -5f), new Vector2(2f, -1f));
            InstaRepair.FacegearInfo.SetStats(CritDMG: 80, Crit: 80, EPRegen: -65, DEF: -20, ASPD: -10, CSPD: -20, EP: -20);

            Hattus = ModItem.AddItem("Hattus Maxxus", "Maxximus prottectus!", "Slimeus", 420, this);
            Hattus.AddItemCategories(ItemCategories.Hat);
            Hattus.AddHatInfo("Slimeus");
            Hattus.HatInfo.SetStats(DEF: 420, HP: 69);
            Hattus.HatInfo.SetObstructions(true, true, false);
            Hattus.HatInfo.SetRenderOffsets(new Vector2(4f, 7f), new Vector2(5f, 5f), new Vector2(5f, 5f), new Vector2(4f, 7f));

            Weapon = ModItem.AddItem("The Free Weapon 2.0", "When all you have is a bent metal rod, every problem looks like an alien crab.", "Claymore", 420, this);
            Weapon.AddItemCategories(ItemCategories.Weapon);
            Weapon.AddWeaponInfo("Claymore", (int)WeaponCategory.TwoHanded, false);
            Weapon.WeaponInfo.SetStats(ATK: 75, ASPD: -10, Crit: 10);
            Weapon.WeaponInfo.AddSpecialEffect((int)SpecialEffect._Unique_BladeOfEchoes_Cursed);

            WeaponOne = ModItem.AddItem("The Free Weapon", "When all you have is a bent metal rod, every problem looks like an alien crab.", "IronSword", 420, this);
            WeaponOne.AddItemCategories(ItemCategories.Weapon);
            WeaponOne.AddWeaponInfo("IronSword", (int)WeaponCategory.OneHanded, false);
            WeaponOne.WeaponInfo.SetStats(ATK: 75, ASPD: -10, Crit: 10);
            WeaponOne.WeaponInfo.AddSpecialEffect((int)SpecialEffect._Unique_LightningGlove_StaticTouch);

            Console.WriteLine("Custom Content Loaded!");
        }

        public override void OnPlayerDamaged(ref int damage, ref byte type)
        {
            // disable this code for now
            if (!grant)
            {
                grant = true;
                alex.SpawnOn(LocalGame, LocalPlayer);
                GordonFreeman.SpawnOn(LocalGame, LocalPlayer);
                InstaRepair.SpawnOn(LocalGame, LocalPlayer);
                Weapon.SpawnOn(LocalGame, LocalPlayer);
                WeaponOne.SpawnOn(LocalGame, LocalPlayer);
                Hattus.SpawnOn(LocalGame, LocalPlayer);
            }

            return;

            damage = (3 * LocalGame.GetCurrentFloor()) * damage; //e.g 300%, 600%, 900%... dmg

            Type gameType = Utils.GetGameType("SoG.Game1");
            dynamic game = LocalGame.GetUnderlayingGame();
            dynamic player = game.xLocalPlayer;
            var function = ((TypeInfo)gameType).GetDeclaredMethods("_EntityMaster_AddItem").First();

            //function.Invoke(LocalGame.GetUnderlayingGame(), new[] { GetModItemFromString("BagKnight"), player.xEntity.xTransform.v2Pos, player.xEntity.xRenderComponent.fVirtualHeight, player.xEntity.xCollisionComponent.ibitCurrentColliderLayer, Vector2.Zero });
            //function.Invoke(LocalGame.GetUnderlayingGame(), new[] { GetModItemFromString("BananaMan"), player.xEntity.xTransform.v2Pos, player.xEntity.xRenderComponent.fVirtualHeight, player.xEntity.xCollisionComponent.ibitCurrentColliderLayer, Vector2.Zero });
        }

        public override void OnPlayerKilled()
        {
            if(LocalGame.GetCurrentFloor() < 5)
                Dialogue.AddDialogueLineTo(LocalGame,"I am not going to lie, but it's not looking good...");
            if (LocalGame.GetCurrentFloor() >= 5)
            {
                Dialogue.AddDialogueLineTo(LocalGame,
                    "Looking but I am sorry to tell you, it's floor 10 now....just joking" + Environment.NewLine +
                    "Grab your reward!");

                questFinished = true;
            }
        }

        public override void OnEnemyDamaged(Enemy enemy, ref int damage, ref byte type)
        {
            var currentFloor = (double) LocalGame.GetCurrentFloor();
            var factor = 1 - 0.15 * currentFloor;

            damage = (int)Math.Floor(damage * factor);
        }

        public override void OnNPCDamaged(NPC npc, ref int damage, ref byte type)
        {
            Console.WriteLine("NPC damaged...");
            Console.WriteLine(damage + "::" + type.ToString());
        }

        public override void OnArcadiaLoad()
        {
            Console.WriteLine("Arcadia loaded....!");
            NPC teddy = NPC.AddNPCTo(LocalGame, NPCTypes.Teddy, new Vector2(1000, 250));
            NPC vilya = NPC.AddNPCTo(LocalGame, NPCTypes.Desert_Saloon_PokerCaptain, new Vector2(990,260));

            vilya.IsInteractable = true;
            vilya.LookAtPlayerOnInteraction = true;

            teddy.IsInteractable = true;
            teddy.LookAtPlayerOnInteraction = true;
        }

        public override void OnNPCInteraction(NPC npc)
        {
            if (npc.GetNPCType() == NPCTypes.Teddy)
            {
                if (!questTaken)
                {
                    Dialogue.AddDialogueLineTo(LocalGame, "Nice to meet you " + LocalPlayer.Name +
                                                          ", I hope you are doing well..."
                                                          + Environment.NewLine +
                                                          "Unfortunately you seem a bit too good for this game, therefore... here is your nerf..."
                                                          + Environment.NewLine +
                                                          "Reach Floor 5 and you will get a special reward!");
                    questTaken = true;

                    
                }
                else if(questFinished)
                {
                    Dialogue.AddDialogueLineTo(LocalGame,"You managed to reach floor 5! Here is your reward!" + Environment.NewLine + "*Proceeds to give you one gold coin*");
                    LocalPlayer.Inventory.AddMoney(1);
                }
                else if (questTaken)
                {
                    Dialogue.AddDialogueLineTo(LocalGame, "'If the quest is too hard to take..." + Environment.NewLine + "You are just too weak...'" + Environment.NewLine + "~5Head");
                }
            }
        }
    }


}
