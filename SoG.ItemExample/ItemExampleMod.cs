using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoG.GrindScript;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SoG;
using Watchers;
//using LoopSettings = SoG.Animation.LoopSettings;
//using CancelOptions = SoG.Animation.CancelOptions;
using static SoG.Animation;
using static SoG.AnimInsCriteria;
using static SoG.AnimInsEvent;

namespace SoG.ItemExample
{
    public class ItemExampleMod: BaseScript
    {
        private ItemDescription Misc;
        private ItemDescription Shield;
        private ItemDescription Facegear;
        private ItemDescription Hat;
        private ItemDescription TwoHanded;
        private ItemDescription OneHanded;
        private ItemDescription Usable;

        private DynamicEnvironmentCodex.ObjectTypes enPlayerBomb;

        private EnemyDescription DankSlime;

        public static Dictionary<int, AnimationPrototype> _Animations = new Dictionary<int, AnimationPrototype>();

        public static Dictionary<int, AnimationPrototype> Animations { get => _Animations; private set => _Animations = value; }

        public ItemExampleMod()
        {
            Console.WriteLine("Hello World from Item Example Mod!");
            Console.WriteLine("This mod showcases the API's item support by creating a few custom items.");
        }

        public override void OnCustomContentLoad()
        {
            try
            {
                Console.WriteLine("ItemExample: Trying to load custom content....");


                // Creates a miscellaneous item
                Misc = ItemHelper.CreateItemDescription("Misc Example", "This is a custom misc item!", "roomba", ModContent);
                Misc.AddCategories(ItemCodex.ItemCategories.Misc);

                // Creates a shield
                Shield = ItemHelper.CreateItemDescription("Shield Example", "This custom shield can block even the most damaging attacks!", "WoodenShield", ModContent);
                Shield.AddCategories(ItemCodex.ItemCategories.Shield);

                Shield.CreateInfo<EquipmentInfo>("Wooden", ModContent);
                Shield.Info<EquipmentInfo>().SetStats(ShldHP: 1337);

                // Creates a facegear
                Facegear = ItemHelper.CreateItemDescription("Facegear Example", "This custom facegear saps your stats in exchange for a heavy damage boost!", "Flybold", ModContent);
                Facegear.AddCategories(ItemCodex.ItemCategories.Facegear);

                Facegear.CreateInfo<FacegearInfo>("Flybold", ModContent);
                Facegear.Info<FacegearInfo>().SetRenderOffsets(new Vector2(2f, -1f), new Vector2(5f, -3f), new Vector2(3f, -5f), new Vector2(2f, -1f));
                Facegear.Info<FacegearInfo>().SetStats(CritDMG: 80, Crit: 80, EPRegen: -55, DEF: -35, ASPD: -10, CSPD: -15, EP: -25);

                // Creates a hat
                Hat = ItemHelper.CreateItemDescription("Hat Example", "With this custom hat, your braincase is safe even from a Collector gone mad!", "Slimeus", ModContent);
                Hat.AddCategories(ItemCodex.ItemCategories.Hat);
                Hat.CreateInfo<HatInfo>("Slimeus", ModContent);
                Hat.Info<HatInfo>().SetStats(DEF: 55, HP: 140);
                Hat.Info<HatInfo>().xDefaultSet.SetObstructions(true, true, false);
                Hat.Info<HatInfo>().xDefaultSet.SetRenderOffsets(new Vector2(4f, 7f), new Vector2(5f, 5f), new Vector2(5f, 5f), new Vector2(4f, 7f));

                // Creates a two-handed weapon
                TwoHanded = ItemHelper.CreateItemDescription("Two Handed Example", "This custom two handed weapon swings slow, but hard!", "Claymore", ModContent);
                TwoHanded.AddCategories(ItemCodex.ItemCategories.Weapon, ItemCodex.ItemCategories.TwoHandedWeapon);
                TwoHanded.CreateInfo<WeaponInfo>("Claymore", ModContent);
                TwoHanded.Info<WeaponInfo>().SetStats(ATK: 100, ASPD: -8, Crit: 10, CritDMG: 15);
                TwoHanded.Info<WeaponInfo>().SetWeaponType(WeaponInfo.WeaponCategory.TwoHanded, false);

                // Creates a one-handed weapon
                OneHanded = ItemHelper.CreateItemDescription("The Free Weapon", "When all you have is a bent metal rod, every problem looks like an alien crab.", "Crowbar", ModContent);
                OneHanded.AddCategories(ItemCodex.ItemCategories.Weapon, ItemCodex.ItemCategories.OneHandedWeapon);
                OneHanded.CreateInfo<WeaponInfo>("IronSword", ModContent);
                OneHanded.Info<WeaponInfo>().SetStats(ATK: 80, ASPD: 25);
                OneHanded.Info<WeaponInfo>().SetWeaponType(WeaponInfo.WeaponCategory.OneHanded, false);
                OneHanded.Info<WeaponInfo>().AddSpecialEffect(EquipmentInfo.SpecialEffect._Unique_Pickaxe_InstantBreakEnvironment);

                // Creates an usable item. The usable's behavior is implemented in OnItemUse. Also create an alias so we can refer to this item easier
                Usable = ItemHelper.CreateItemDescription("DA BOMB", "They will never expect a KABOOM!", "roomba", ModContent);
                Usable.AddCategories(ItemCodex.ItemCategories.Usable, ItemCodex.ItemCategories.DontRemoveOnUse, ItemCodex.ItemCategories.DontShowUsesLeft);

                Console.WriteLine("ItemExample: Custom Content Loaded!");

                Console.WriteLine("ItemExample: Loading Animations");

                
                string MONSTER_PATH = "Sprites/Monster/";

                string[] arrTex = new string[]
                {
                    MONSTER_PATH + "Pillar Mountains/Slime/Idle/Up", // 0
                    MONSTER_PATH + "Pillar Mountains/Slime/Run/Up",
                    MONSTER_PATH + "Pillar Mountains/Slime/Damage/Up",
                    MONSTER_PATH + "Pillar Mountains/Slime/Damage/Reverse",
                    MONSTER_PATH + "Pillar Mountains/Slime/Attack/Up",
                    MONSTER_PATH + "Pillar Mountains/Slime/Attack/Right", // 5
                    MONSTER_PATH + "Pillar Mountains/Slime/Attack/Down",
                    MONSTER_PATH + "Pillar Mountains/Slime/Attack/Left",
                    MONSTER_PATH + "Pillar Mountains/Slime/Fall/Impact" // 8
                };

                Animations[0] = new AnimationPrototype(ModContent, 0, 0, arrTex[0], new Vector2(14f, 14f), 4, 8, 25, 18, 0, 0, 8, LoopSettings.Looping, CancelOptions.IgnoreIfPlaying, true, true) { bIgnoreSentTicks = true };

                Animations[1] = new AnimationPrototype(ModContent, 1, 0, arrTex[1], new Vector2(14f, 19f), 4, 14, 25, 23, 0, 0, 14, LoopSettings.Looping, CancelOptions.IgnoreIfPlaying, true, true);
                Animations[1].SetInstructions(
                    new AnimationInstruction(new AnimInsCriteria(Criteria.TriggerAtFrameX, 2f), new AnimInsEvent(EventType.PlaySound, "Slime_Jump", new float[1]))
                    );

                Animations[2] = new AnimationPrototype(ModContent, 2, 0, arrTex[2], new Vector2(14f, 10f), 4, 2, 27, 14, 0, 0, 2, LoopSettings.Clamp, CancelOptions.IgnoreIfPlaying, false, false);
                Animations[2].SetInstructions(
                    new AnimationInstruction(new AnimInsCriteria(Criteria.TriggerAtFrameX, 2f), new AnimInsEvent(EventType.FreezeFrame, 10f, 0f)), 
                    new AnimationInstruction(new AnimInsCriteria(Criteria.TriggerAtEnd), new AnimInsEvent(EventType.PlayAnimation, 3f))
                    );

                Animations[3] = new AnimationPrototype(ModContent, 3, 0, arrTex[3], new Vector2(14f, 10f), 4, 2, 27, 14, 0, 0, 2, LoopSettings.Clamp, CancelOptions.IgnoreIfPlaying, false, false);
                Animations[3].SetInstructions(
                    new AnimationInstruction(new AnimInsCriteria(Criteria.TriggerAtEnd), new AnimInsEvent(EventType.PlayAnimation, new float[1]))
                    );

                Animations[4] = new AnimationPrototype(ModContent, 4, 0, arrTex[4], new Vector2(12f, 17f), 4, 18, 21, 26, 0, 0, 18, LoopSettings.Clamp, CancelOptions.IgnoreIfPlaying, false, true);
                Animations[4].SetInstructions(
                    new AnimationInstruction(new AnimInsCriteria(Criteria.TriggerAtFrameX, 8f), new AnimInsEvent(EventType.PlaySound, "Slime_Attack", 1f)),
                    new AnimationInstruction(new AnimInsCriteria(Criteria.TriggerAtFrameX, 9f), new AnimInsEvent(EventType.FreezeFrame, 10f, 0f)),
                    new AnimationInstruction(new AnimInsCriteria(Criteria.TriggerAtFrameX, 11f), new AnimInsEvent(EventType.AddSmoothPush, 0f, -3f)),
                    new AnimationInstruction(new AnimInsCriteria(Criteria.TriggerAtFrameX, 11f), new AnimInsEvent(EventType.FreezeFrame, 10f, 0f)),
                    new AnimationInstruction(new AnimInsCriteria(Criteria.TriggerAtEnd), new AnimInsEvent(EventType.PlayAnimation, new float[1]))
                    );

                Animations[5] = new AnimationPrototype(ModContent, 5, 1, arrTex[5], new Vector2(17f, 12f), 4, 18, 29, 16, 0, 0, 18, LoopSettings.Clamp, CancelOptions.IgnoreIfPlaying, false, true);
                Animations[5].SetInstructions(
                    new AnimationInstruction(new AnimInsCriteria(Criteria.TriggerAtFrameX, 8f), new AnimInsEvent(EventType.PlaySound, "Slime_Attack", 1f)),
                    new AnimationInstruction(new AnimInsCriteria(Criteria.TriggerAtFrameX, 9f), new AnimInsEvent(EventType.FreezeFrame, 10f, 0f)),
                    new AnimationInstruction(new AnimInsCriteria(Criteria.TriggerAtFrameX, 11f), new AnimInsEvent(EventType.AddSmoothPush, 3f, 0f)),
                    new AnimationInstruction(new AnimInsCriteria(Criteria.TriggerAtFrameX, 11f), new AnimInsEvent(EventType.FreezeFrame, 10f, 0f)),
                    new AnimationInstruction(new AnimInsCriteria(Criteria.TriggerAtEnd), new AnimInsEvent(EventType.PlayAnimation, new float[1]))
                    );

                Animations[6] = new AnimationPrototype(ModContent, 6, 2, arrTex[6], new Vector2(12f, 18f), 4, 18, 21, 26, 0, 0, 18, LoopSettings.Clamp, CancelOptions.IgnoreIfPlaying, false, true);
                Animations[6].SetInstructions(
                    new AnimationInstruction(new AnimInsCriteria(Criteria.TriggerAtFrameX, 8f), new AnimInsEvent(EventType.PlaySound, "Slime_Attack", 1f)),
                    new AnimationInstruction(new AnimInsCriteria(Criteria.TriggerAtFrameX, 9f), new AnimInsEvent(EventType.FreezeFrame, 10f, 0f)),
                    new AnimationInstruction(new AnimInsCriteria(Criteria.TriggerAtFrameX, 11f), new AnimInsEvent(EventType.AddSmoothPush, 0f, 3f)),
                    new AnimationInstruction(new AnimInsCriteria(Criteria.TriggerAtFrameX, 11f), new AnimInsEvent(EventType.FreezeFrame, 10f, 0f)),
                    new AnimationInstruction(new AnimInsCriteria(Criteria.TriggerAtEnd), new AnimInsEvent(EventType.PlayAnimation, new float[1]))
                    );

                Animations[7] = new AnimationPrototype(ModContent, 7, 3, arrTex[7], new Vector2(17f, 12f), 4, 18, 29, 16, 0, 0, 18, LoopSettings.Clamp, CancelOptions.IgnoreIfPlaying, false, true);
                Animations[7].SetInstructions(
                new AnimationInstruction(new AnimInsCriteria(Criteria.TriggerAtFrameX, 8f), new AnimInsEvent(EventType.PlaySound, "Slime_Attack", 1f)),
                    new AnimationInstruction(new AnimInsCriteria(Criteria.TriggerAtFrameX, 9f), new AnimInsEvent(EventType.FreezeFrame, 10f, 0f)),
                    new AnimationInstruction(new AnimInsCriteria(Criteria.TriggerAtFrameX, 11f), new AnimInsEvent(EventType.AddSmoothPush, -3f, 0f)),
                    new AnimationInstruction(new AnimInsCriteria(Criteria.TriggerAtFrameX, 11f), new AnimInsEvent(EventType.FreezeFrame, 10f, 0f)),
                    new AnimationInstruction(new AnimInsCriteria(Criteria.TriggerAtEnd), new AnimInsEvent(EventType.PlayAnimation, new float[1]))
                    );

                Animations[8] = new AnimationPrototype(ModContent, 8, 0, arrTex[8], new Vector2(17f, 12f), 4, 5, 32, 20, 0, 0, 5, LoopSettings.Clamp, CancelOptions.IgnoreIfPlaying, false, false);
                Animations[8].SetInstructions(
                    new AnimationInstruction(new AnimInsCriteria(Criteria.TriggerAtFrameX, 5f), new AnimInsEvent(EventType.CallBackAnimation, new float[1])),
                    new AnimationInstruction(new AnimInsCriteria(Criteria.TriggerAtEnd), new AnimInsEvent(EventType.PlayAnimation, new float[1]))
                    );

                Animations[9] = new AnimationPrototype(ModContent, 40000, 0, arrTex[2], new Vector2(14f, 10f), 4, 1, 27, 14, 0, 0, 2, LoopSettings.Clamp, CancelOptions.IgnoreIfPlaying, false, false);
                Animations[10] = new AnimationPrototype(ModContent, 40001, 1, arrTex[2], new Vector2(14f, 10f), 4, 1, 27, 14, 0, 0, 2, LoopSettings.Clamp, CancelOptions.IgnoreIfPlaying, false, false);
                Animations[11] = new AnimationPrototype(ModContent, 40002, 2, arrTex[2], new Vector2(14f, 10f), 4, 1, 27, 14, 0, 0, 2, LoopSettings.Clamp, CancelOptions.IgnoreIfPlaying, false, false);
                Animations[12] = new AnimationPrototype(ModContent, 40003, 3, arrTex[3], new Vector2(14f, 10f), 4, 1, 27, 14, 0, 0, 2, LoopSettings.Clamp, CancelOptions.IgnoreIfPlaying, false, false);

                Animations[13] = new AnimationPrototype(ModContent, 40004, 0, arrTex[3], new Vector2(14f, 10f), 4, 2, 27, 14, 0, 0, 2, LoopSettings.Clamp, CancelOptions.IgnoreIfPlaying, false, false);
                Animations[13].SetInstructions(
                    new AnimationInstruction(new AnimInsCriteria(AnimInsCriteria.Criteria.TriggerAtEnd), new AnimInsEvent(AnimInsEvent.EventType.PlayAnimation, new float[1]))
                    );

                Animations[14] = new AnimationPrototype(ModContent, 40005, 0, arrTex[3], new Vector2(14f, 10f), 4, 2, 27, 14, 0, 0, 2, LoopSettings.Clamp, CancelOptions.IgnoreIfPlaying, false, false);
                Animations[14].SetInstructions(
                    new AnimationInstruction(new AnimInsCriteria(AnimInsCriteria.Criteria.TriggerAtEnd), new AnimInsEvent(AnimInsEvent.EventType.PlayAnimation, new float[1]))
                    );

                Animations[15] = new AnimationPrototype(ModContent, 40006, 0, arrTex[3], new Vector2(14f, 10f), 4, 2, 27, 14, 0, 0, 2, LoopSettings.Clamp, CancelOptions.IgnoreIfPlaying, false, false);
                Animations[15].SetInstructions(
                    new AnimationInstruction(new AnimInsCriteria(AnimInsCriteria.Criteria.TriggerAtEnd), new AnimInsEvent(AnimInsEvent.EventType.PlayAnimation, new float[1]))
                    );

                Animations[16] = new AnimationPrototype(ModContent, 40007, 0, arrTex[3], new Vector2(14f, 10f), 4, 2, 27, 14, 0, 0, 2, LoopSettings.Clamp, CancelOptions.IgnoreIfPlaying, false, false);
                Animations[16].SetInstructions(
                    new AnimationInstruction(new AnimInsCriteria(AnimInsCriteria.Criteria.TriggerAtEnd), new AnimInsEvent(AnimInsEvent.EventType.PlayAnimation, new float[1]))
                    );

                Console.WriteLine("ItemExample: Loaded Animations!");

                Console.WriteLine("ItemExample: Defining Enemies");

                DankSlime = EnemyHelper.CreateEnemyDescription("Dank Slime", 1, 420);
                DankSlime.SetInstanceBuilder(ModSlimeAI.InstanceBuilder);
                DankSlime.SetDifficultyBuilder(ModSlimeAI.DifficultyBuilder);
                DankSlime.SetCommonSounds("Slime_DamageSolo", "Slime_Death");
                DankSlime.SetSizeAndOffset(new Vector2(15f, 13f), new Vector2(0f, -3f));
                DankSlime.AddLoot(Usable.enType, 60.0f, 2);

                Console.WriteLine("ItemExample: Done with Enemies!");

                Console.WriteLine("ItemExample: Doing wack DynamicEnvironment stuff!");

                enPlayerBomb = DynEnvHelper.CreateDynamicEnvironmentEntry(ModContent, PlayerBomb.InstanceBuilder);

                Console.WriteLine("ItemExample: Done with the wack DynamicEnvironment stuff!");
            }
            catch(Exception e)
            {
                Console.WriteLine(e.StackTrace);
                Console.WriteLine(e.Message);
            }
        }

        public override bool OnChatParseCommand(string command, string argList, int connection)
        {
            var xPlayer = Utils.GetTheGame().xLocalPlayer;
            switch (command)
            {
                case "gibitemsplz":
                    Misc.SpawnItem(xPlayer);
                    Shield.SpawnItem(xPlayer);
                    Facegear.SpawnItem(xPlayer);
                    TwoHanded.SpawnItem(xPlayer);
                    OneHanded.SpawnItem(xPlayer);
                    Hat.SpawnItem(xPlayer);
                    return false; // Do not check vanilla commands
                case "bombtime":
                    Usable.SpawnItem(xPlayer);
                    return false;
                case "mlgslime":
                    DankSlime.SpawnEnemy(xPlayer);
                    return false;
            }
            return true; // Check vanilla commands
        }

        public override void OnItemUse(ItemCodex.ItemTypes enItem, PlayerView xView, ref bool bSend)
        {
            if(enItem == Usable.enType)
            {
                try
                {
                    PlayerEntity xEntity = xView.xEntity;
                    PlayerBomb x = (PlayerBomb)Utils.GetTheGame()._EntityMaster_AddDynamicEnvironment(enPlayerBomb, xEntity.xTransform.v2Pos, xEntity.xRenderComponent.fVirtualHeight, xEntity.xCollisionComponent.ibitCurrentColliderLayer);

                    Vector2 v2Dir = Utility.AnimationDirectionToVector2(xEntity.byAnimationDirection);

                    x.SetInfo_Bounce(v2Dir, 90);
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.StackTrace);
                    Console.WriteLine("A fuck up of type " + e.Message);
                }
            }
        }
    }
}
