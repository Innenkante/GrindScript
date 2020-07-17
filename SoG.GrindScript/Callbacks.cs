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
        private static List<OnEnemyDamagedPrototype> _onEnemyDamagedCallbacks = new List<OnEnemyDamagedPrototype>();
        private static List<OnNPCDamagedPrototype> _onNpcDamagedCallbacks = new List<OnNPCDamagedPrototype>();
        private static List<OnNPCInteractionPrototype> _onNpcInteractionCallbacks = new List<OnNPCInteractionPrototype>();


        private static Harmony harmony = new Harmony("GrindScriptCallbackManager");

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
            foreach (var onNpcInteractionPrototype in _onNpcInteractionCallbacks)
            {
                onNpcInteractionPrototype(new NPC(xNPC));
            }
        }


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


    }
}

