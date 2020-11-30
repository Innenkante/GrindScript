using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HarmonyLib;
using Microsoft.Xna.Framework;

//https://stackoverflow.com/questions/7299097/dynamically-replace-the-contents-of-a-c-sharp-method
namespace SoG.GrindScript
{

    public class Callbacks
    {
        private static List<OnDrawPrototype> _onDrawCallbacks = new List<OnDrawPrototype>();
        private static List<OnPlayerTakeDamagePrototype> _onPlayerTakeDamageCalllbacks = new List<OnPlayerTakeDamagePrototype>();
        private static List<OnPlayerKilledPrototype> _onPlayerKilledCallbacks = new List<OnPlayerKilledPrototype>();
        private static List<PostPlayerLevelUpPrototype> _postPlayerLevelUpCallbacks = new List<PostPlayerLevelUpPrototype>();
        private static List<OnEnemyDamagedPrototype> _onEnemyDamagedCallbacks = new List<OnEnemyDamagedPrototype>();
        private static List<OnNPCDamagedPrototype> _onNpcDamagedCallbacks = new List<OnNPCDamagedPrototype>();
        private static List<OnNPCInteractionPrototype> _onNpcInteractionCallbacks = new List<OnNPCInteractionPrototype>();
        private static List<OnCustomContentLoadPrototype> _onCustomContentLoadCallbacks = new List<OnCustomContentLoadPrototype>();


        private static List<OnArcadiaLoadPrototype> _onArcadiaLoadCallbacks = new List<OnArcadiaLoadPrototype>();


        private static Harmony harmony = new Harmony("GrindScriptCallbackManager");

        #region Callback adders

        public static void AddOnDrawCallback(OnDrawPrototype onDraw)
        {
            _onDrawCallbacks.Add(onDraw);
        }

        public static void AddOnPlayerTakeDamageCallback(OnPlayerTakeDamagePrototype onPlayerTakeDamage)
        {
            _onPlayerTakeDamageCalllbacks.Add(onPlayerTakeDamage);
        }


        public static void AddOnPlayerKilledCallback(OnPlayerKilledPrototype onPlayerKilled)
        {
            _onPlayerKilledCallbacks.Add(onPlayerKilled);
        }

        public static void AddPostPlayerLevelUpCallback(PostPlayerLevelUpPrototype postPlayerLevelUp)
        {
            _postPlayerLevelUpCallbacks.Add(postPlayerLevelUp);
        }

        public static void AddOnEnemyDamagedCallback(OnEnemyDamagedPrototype onEnemyDamaged)
        {
            _onEnemyDamagedCallbacks.Add(onEnemyDamaged);
        }

        public static void AddOnNPCDamagedCallback(OnNPCDamagedPrototype onNpcDamaged)
        {
            _onNpcDamagedCallbacks.Add(onNpcDamaged);
        }

        public static void AddOnNPCInteractionCallback(OnNPCInteractionPrototype onNpcInteraction)
        {
            _onNpcInteractionCallbacks.Add(onNpcInteraction);
        }

        public static void AddOnArcadiaLoadCallback(OnArcadiaLoadPrototype onArcadiaLoad)
        {
            _onArcadiaLoadCallbacks.Add(onArcadiaLoad);
        }

        public static void AddOnCustomContentLoad(OnCustomContentLoadPrototype onCustomContentLoad)
        {
            _onCustomContentLoadCallbacks.Add(onCustomContentLoad);
        }

        #endregion


        #region Callbacks

        private static void OnFinalDrawPrefix()
        {
            _onDrawCallbacks.ForEach(a => a());
        }

        private static void OnPlayerTakeDamagePrefix(ref int iInDamage, ref byte byType)
        {
            foreach (var onPlayerTakeDamageCalllback in _onPlayerTakeDamageCalllbacks)
            {
                onPlayerTakeDamageCalllback(ref iInDamage, ref byType);
            }
        }

        private static void OnPlayerKilledPrefix()
        {
            foreach (var onPlayerKilledCallback in _onPlayerKilledCallbacks)
            {
                onPlayerKilledCallback();
            }
        }

        private static void PostPlayerLevelUp(dynamic xView)
        {
            foreach (var postPlayerLevelUp in _postPlayerLevelUpCallbacks)
            {
                postPlayerLevelUp(new Player(xView));
            }
        }

        private static void OnEnemyTakeDamagePrefix(dynamic xEnemy, ref int iDamage, ref byte byType)
        {
            foreach (var onEnemyDamagedCallback in _onEnemyDamagedCallbacks)
            {
                onEnemyDamagedCallback(new Enemy(xEnemy), ref iDamage, ref byType);
            }
        }

        private static void OnNPCTakeDamagePrefix(dynamic xEnemy, ref int iDamage, ref byte byType)
        {
            foreach (var onNpcDamagedCallback in _onNpcDamagedCallbacks)
            {
                onNpcDamagedCallback(new NPC(xEnemy), ref iDamage, ref byType);
            }
        }

        private static void OnNPCInteractionPrefix(dynamic xView, dynamic xNPC)
        {
            foreach (var onNpcInteractionCallback in _onNpcInteractionCallbacks)
            {
                onNpcInteractionCallback(new NPC(xNPC));
            }
        }

        private static void OnArcadiaLoadPrefix()
        {
            foreach (var onArcadiaLoadCallback in _onArcadiaLoadCallbacks)
            {
                onArcadiaLoadCallback();
            }
        }


        #endregion

        #region Initializing Callbacks

        public static void InitializeOnDrawCallbacks()
        {
            var prefix = typeof(Callbacks).GetTypeInfo().GetPrivateStaticMethod("OnFinalDrawPrefix");
            var original = Utils.GetGameType("SoG.Game1").GetPublicInstanceMethod("FinalDraw");

            harmony.Patch(original, new HarmonyMethod(prefix));

        }


        public static void InitializeOnPlayerTakeDamageCallbacks()
        {
            try
            {
                var prefix = typeof(Callbacks).GetTypeInfo().GetPrivateStaticMethod("OnPlayerTakeDamagePrefix");
                var original = Utils.GetGameType("SoG.Game1").GetPublicInstanceMethod("_Player_TakeDamage");

                harmony.Patch(original, new HarmonyMethod(prefix));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public static void InitializeOnPlayerKilledCallbacks()
        {
            try
            {
                var prefix = typeof(Callbacks).GetTypeInfo().GetPrivateStaticMethod("OnPlayerKilledPrefix");
                var original = Utils.GetGameType("SoG.Game1").GetMethods(BindingFlags.Instance | BindingFlags.Public)
                    .First(m => m.Name == "_Player_KillPlayer" && m.GetParameters().Count() > 1);

                harmony.Patch(original, new HarmonyMethod(prefix));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public static void InitializePostPlayerLevelUpCallbacks()
        {
            var postfix = typeof(Callbacks).GetTypeInfo().GetPrivateStaticMethod("PostPlayerLevelUp");
            var original = Utils.GetGameType("SoG.Game1").GetPublicInstanceMethod("_Player_ApplyLvUpBonus");

            harmony.Patch(original, null, new HarmonyMethod(postfix));
        }

        public static void InitializeOnEnemyTakeDamageCallbacks()
        {
            try
            {


                var prefix = typeof(Callbacks).GetTypeInfo().GetPrivateStaticMethod("OnEnemyTakeDamagePrefix");
                var original = Utils.GetGameType("SoG.Game1").GetPublicInstanceMethod("_Enemy_TakeDamage");

                harmony.Patch(original, new HarmonyMethod(prefix));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public static void InitializeOnNPCTakeDamageCallbacks()
        {
            try
            {
                var prefix = typeof(Callbacks).GetTypeInfo().GetPrivateStaticMethod("OnNPCTakeDamagePrefix");
                var original = Utils.GetGameType("SoG.Game1").GetPublicInstanceMethod("_NPC_TakeDamage");

                harmony.Patch(original, new HarmonyMethod(prefix));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public static void InitializeOnNPCInteractionCallbacks()
        {
            try
            {
                var prefix = typeof(Callbacks).GetTypeInfo().GetPrivateStaticMethod("OnNPCInteractionPrefix");
                var original = Utils.GetGameType("SoG.Game1").GetPublicInstanceMethod("_NPC_Interact");

                harmony.Patch(original, new HarmonyMethod(prefix));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public static void InitializeOnArcadiaLoadCallbacks()
        {
            try
            {
                var prefix = typeof(Callbacks).GetTypeInfo().GetPrivateStaticMethod("OnArcadiaLoadPrefix");
                var original = Utils.GetGameType("SoG.Game1").GetPublicInstanceMethod("_LevelLoading_DoStuff_Arcadia");

                harmony.Patch(original, new HarmonyMethod(prefix));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public static void InitializeOnCustomContentLoad()
        {
            _onCustomContentLoadCallbacks.ForEach(onLoad => onLoad());
        }

        public static void InitializeUniquePatches()
        {
            try
            {
                // GetItemInstance prefix patch
                var prefix = typeof(ModItem).GetTypeInfo().GetPrivateStaticMethod("OnGetItemInstancePrefix");
                var original = Utils.GetGameType("SoG.ItemCodex").GetMethod("GetItemInstance");

                harmony.Patch(original, new HarmonyMethod(prefix));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            try
            {
                // GetEquipmentInfo prefix patches
                // (EquipmentCodex declares 4 functions, so I'm patching all 4 with the same function. Yeehaw.)
                // (it should work the same, since the functions effectively act as separate storage mediums)
                // (the patches just change the storage in question to a shared dictionary)
                var prefix = typeof(ModEquipment).GetTypeInfo().GetPrivateStaticMethod("OnGetEquipmentInfoPrefix");

                var original = Utils.GetGameType("SoG.EquipmentCodex").GetMethod("GetArmorInfo");
                harmony.Patch(original, new HarmonyMethod(prefix));

                original = Utils.GetGameType("SoG.EquipmentCodex").GetMethod("GetAccessoryInfo");
                harmony.Patch(original, new HarmonyMethod(prefix));

                original = Utils.GetGameType("SoG.EquipmentCodex").GetMethod("GetShieldInfo");
                harmony.Patch(original, new HarmonyMethod(prefix));

                original = Utils.GetGameType("SoG.EquipmentCodex").GetMethod("GetShoesInfo");
                harmony.Patch(original, new HarmonyMethod(prefix));


                // Facegear Codex's GetHatInfo patch
                prefix = typeof(ModFacegear).GetTypeInfo().GetPrivateStaticMethod("OnGetFacegearInfoPrefix");

                original = Utils.GetGameType("SoG.FacegearCodex").GetMethod("GetHatInfo");
                harmony.Patch(original, new HarmonyMethod(prefix));

                // Hat Codex's GetHatInfo patch
                prefix = typeof(ModHat).GetTypeInfo().GetPrivateStaticMethod("OnGetHatInfoPrefix");

                original = Utils.GetGameType("SoG.HatCodex").GetMethod("GetHatInfo");
                harmony.Patch(original, new HarmonyMethod(prefix));

                // Weapon Codex's GetWeaponInfo patch
                prefix = typeof(ModWeapon).GetTypeInfo().GetPrivateStaticMethod("OnGetWeaponInfoPrefix");

                original = Utils.GetGameType("SoG.WeaponCodex").GetMethod("GetWeaponInfo");
                harmony.Patch(original, new HarmonyMethod(prefix));

                // Item Content Manager related patches
                prefix = typeof(ModLibrary).GetTypeInfo().GetPrivateStaticMethod("OnOneHandedDictionaryFillPrefix");

                original = Utils.GetGameType("WeaponAssets.WeaponAssetLoader").GetPrivateStaticMethod("OneHandedDictionaryFill");
                harmony.Patch(original, new HarmonyMethod(prefix));

                prefix = typeof(ModLibrary).GetTypeInfo().GetPrivateStaticMethod("OnTwoHandedDictionaryFillPrefix");

                original = Utils.GetGameType("WeaponAssets.WeaponAssetLoader").GetPrivateStaticMethod("TwoHandedDictionaryFill");
                harmony.Patch(original, new HarmonyMethod(prefix));

                prefix = typeof(ModLibrary).GetTypeInfo().GetPrivateStaticMethod("OnLoadBatchPrefix");

                original = Utils.GetGameType("WeaponAssets.WeaponContentManager").GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).Where(item => item.Name == "LoadBatch").ElementAt(1);
                harmony.Patch(original, new HarmonyMethod(prefix));

                prefix = typeof(ModLibrary).GetTypeInfo().GetPrivateStaticMethod("On_Animations_GetAnimationSetPrefix");

                original = Utils.GetGameType("SoG.Game1").GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(item => item.Name == "_Animations_GetAnimationSet").ElementAt(1);
                harmony.Patch(original, new HarmonyMethod(prefix));

                
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        #endregion
    }
}

