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
        private static List<OnItemUsePrototype> _onItemUseCallbacks = new List<OnItemUsePrototype>();


        private static List<OnArcadiaLoadPrototype> _onArcadiaLoadCallbacks = new List<OnArcadiaLoadPrototype>();
        private static List<OnChatParseCommandPrototype> _onChatParseCommandCallbacks = new List<OnChatParseCommandPrototype>();

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

        public static void AddOnChatParseCallback(OnChatParseCommandPrototype onChatParseCommand)
        {
            _onChatParseCommandCallbacks.Add(onChatParseCommand);
        }

        public static void AddOnItemUseCallback(OnItemUsePrototype onItemUse)
        {
            _onItemUseCallbacks.Add(onItemUse);
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
                onEnemyDamagedCallback(xEnemy, ref iDamage, ref byType);
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

        private static bool OnChatParseCommandPrefix(string sMessage, int iConnection)
        {
            // Maybe a transpiler could do the job better, but I'm not familliar enough with the technique
            // Consider this a TODO
            try
            {
                // Original code
                int iIndex = sMessage.IndexOf(' ');
                bool bNoSecondPart = iIndex == -1;
                if (iIndex == -1)
                {
                    iIndex = sMessage.Length;
                }
                string sFirst = sMessage.Substring(0, iIndex);
                sMessage = sMessage.Substring(sMessage.IndexOf(' ') + 1);
                if (bNoSecondPart)
                {
                    sMessage = "";
                }
                sFirst.ToLowerInvariant();
                // End

                bool vanillaExecution = true;
                foreach (var onChatParseCommandCallback in _onChatParseCommandCallbacks)
                {
                    // If a mod callback returns false, the original function will not run
                    // This means that vanilla commands won't run in tandem with mod commands for "false" return
                    // Multiple mod commands with the same name will still run though...
                    vanillaExecution &= onChatParseCommandCallback(sFirst, sMessage, iConnection);
                }
                return vanillaExecution;
            }
            catch (Exception e)
            {
                CAS.AddChatMessage("GrindScript: " + e.Message);
            }
            return true;
        }

        private static void OnItemUsePrefix(ItemCodex.ItemTypes enItem, PlayerView xView, ref bool bSend)
        {
            if (xView.xViewStats.bIsDead)
            {
                return;
            }
            foreach (var onItemUse in _onItemUseCallbacks)
            {
                onItemUse(enItem, xView, ref bSend);
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

        public static void InitializeOnChatParseCommandCallbacks()
        {
            try
            {
                var prefix = typeof(Callbacks).GetTypeInfo().GetPrivateStaticMethod("OnChatParseCommandPrefix");
                var original = Utils.GetGameType("SoG.Game1").GetPublicInstanceMethod("_Chat_ParseCommand");

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

        public static void InitializeOnItemUseCallbacks()
        {
            try
            {
                var prefix = typeof(Callbacks).GetTypeInfo().GetPrivateStaticMethod("OnItemUsePrefix");
                var original = Utils.GetGameType("SoG.Game1").GetDeclaredMethods("_Item_Use").ElementAt(1);
                harmony.Patch(original, new HarmonyMethod(prefix));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public static void InitializeUniquePatches()
        {
            try
            {
                // GetItemInstance prefix patch
                var prefix = typeof(ItemHelper).GetTypeInfo().GetPrivateStaticMethod("GetItemInstance_PrefixPatch");
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
                var prefix = typeof(ItemHelper).GetTypeInfo().GetPrivateStaticMethod("GetEquipmentInfo_PrefixPatch");

                var original = typeof(EquipmentCodex).GetMethod("GetArmorInfo");
                harmony.Patch(original, new HarmonyMethod(prefix));

                original = typeof(EquipmentCodex).GetMethod("GetAccessoryInfo");
                harmony.Patch(original, new HarmonyMethod(prefix));

                original = typeof(EquipmentCodex).GetMethod("GetShieldInfo");
                harmony.Patch(original, new HarmonyMethod(prefix));

                original = typeof(EquipmentCodex).GetMethod("GetShoesInfo");
                harmony.Patch(original, new HarmonyMethod(prefix));


                // Facegear Codex's GetHatInfo patch
                prefix = typeof(ItemHelper).GetTypeInfo().GetPrivateStaticMethod("GetFacegearInfo_PrefixPatch");

                original = typeof(FacegearCodex).GetMethod("GetHatInfo");
                harmony.Patch(original, new HarmonyMethod(prefix));

                // Hat Codex's GetHatInfo patch
                prefix = typeof(ItemHelper).GetTypeInfo().GetPrivateStaticMethod("GetHatInfo_PrefixPatch");

                original = typeof(HatCodex).GetMethod("GetHatInfo");
                harmony.Patch(original, new HarmonyMethod(prefix));

                // Weapon Codex's GetWeaponInfo patch
                prefix = typeof(ItemHelper).GetTypeInfo().GetPrivateStaticMethod("GetWeaponInfo_PrefixPatch");

                original = typeof(WeaponCodex).GetMethod("GetWeaponInfo");
                harmony.Patch(original, new HarmonyMethod(prefix));

                prefix = typeof(ItemHelper).GetTypeInfo().GetPrivateStaticMethod("LoadBatch_PrefixOverwrite");

                original = typeof(WeaponAssets.WeaponContentManager).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).Where(item => item.Name == "LoadBatch").ElementAt(1);
                harmony.Patch(original, new HarmonyMethod(prefix));

                prefix = typeof(ItemHelper).GetTypeInfo().GetPrivateStaticMethod("_Animations_GetAnimationSet_PrefixOverwrite");

                original = typeof(Game1).GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(item => item.Name == "_Animations_GetAnimationSet").ElementAt(1);
                harmony.Patch(original, new HarmonyMethod(prefix));
            
                prefix = typeof(EnemyHelper).GetTypeInfo().GetPrivateStaticMethod("GetEnemyInstance_PrefixPatch");

                original = typeof(EnemyCodex).GetMethod("GetEnemyInstance", BindingFlags.Public | BindingFlags.Static);
                harmony.Patch(original, new HarmonyMethod(prefix));

                prefix = typeof(EnemyHelper).GetTypeInfo().GetPrivateStaticMethod("_Enemy_AdjustForDifficulty_PrefixPatch");

                original = typeof(Game1).GetMethod("_Enemy_AdjustForDifficulty", BindingFlags.Public | BindingFlags.Instance);
                harmony.Patch(original, new HarmonyMethod(prefix));

                prefix = typeof(EnemyHelper).GetTypeInfo().GetPrivateStaticMethod("_Enemy_MakeElite_PrefixPatch");

                original = typeof(Game1).GetMethod("_Enemy_MakeElite", BindingFlags.Public | BindingFlags.Instance);
                harmony.Patch(original, new HarmonyMethod(prefix));

                prefix = typeof(DynEnvHelper).GetTypeInfo().GetPrivateStaticMethod("GetObjectInstance_PrefixPatch");

                original = typeof(DynamicEnvironmentCodex).GetMethod("GetObjectInstance", BindingFlags.Public | BindingFlags.Static);
                harmony.Patch(original, new HarmonyMethod(prefix));
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public static void InitializeLoadPatch()
        {
            try
            {
                // GetItemInstance prefix patch
                //var postfix = typeof(NativeInterface).GetTypeInfo().GetMethod("LoadGrindscript");
                var transpiler = typeof(Callbacks).GetTypeInfo().GetMethod("InitializeLoadPatch_Transpiler", BindingFlags.Public | BindingFlags.Static);
                var original = Utils.GetGameType("SoG.Game1").GetMethod("LoadContent", BindingFlags.Instance | BindingFlags.NonPublic);

                harmony.Patch(original, transpiler: new HarmonyMethod(transpiler));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public static IEnumerable<CodeInstruction> InitializeLoadPatch_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            int iCodeIndex = -1;

            MethodInfo xSearchMethod = Utils.GetGameType("SoG.Game1").GetMethod("_MainMenu_PopulateCharacterSelect", BindingFlags.Public | BindingFlags.Instance);

            List<CodeInstruction> lciInstructions = new List<CodeInstruction>(instructions);
            for(int i = 0; i < lciInstructions.Count; i++)
            {
                bool bMatch =
                        lciInstructions[i].opcode == OpCodes.Call &&
                        lciInstructions[i].opcode.OperandType == OperandType.InlineMethod &&
                        ((MethodInfo)lciInstructions[i].operand) == xSearchMethod;

                if (bMatch)
                {
                    Console.WriteLine("Load Patch transpiling before instruction: " + ((MethodInfo)lciInstructions[i].operand).Name);
                    iCodeIndex = i - 1;
                    break;
                }
            }
            if(iCodeIndex == -1)
            {
                throw new Exception("InitializeLoadPatch_Transpiler failed!");
            }

            List<CodeInstruction> lciNewInstructions = new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Call, typeof(NativeInterface).GetTypeInfo().GetMethod("LoadGrindscript"))
            };

            lciInstructions.InsertRange(iCodeIndex, lciNewInstructions);

            return lciInstructions;
        }

        #endregion
    }
}

