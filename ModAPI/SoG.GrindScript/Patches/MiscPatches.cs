using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Emit;
using HarmonyLib;
using LevelLoading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SoG.Modding.Extensions;
using SoG.Modding.Utils;

namespace SoG.Modding.Patches
{
    using Quests;
    using SoG.Modding.Configs;
    using SoG.Modding.LibraryEntries;
    using CodeList = IEnumerable<CodeInstruction>;

    /// <summary>
    /// Contains miscellaneous patches.
    /// </summary>
    [HarmonyPatch]
    internal static class MiscPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(LevelBlueprint), nameof(LevelBlueprint.GetBlueprint))]
        internal static bool OnGetLevelBlueprint(ref LevelBlueprint __result, Level.ZoneEnum enZoneToGet)
        {
            if (!enZoneToGet.IsFromMod())
                return true;

            LevelBlueprint bprint = new LevelBlueprint();

            bprint.CheckForConsistency();

            LevelEntry entry = Globals.API.Loader.Library.Levels[enZoneToGet];

            try
            {
                entry.Config.Builder?.Invoke(bprint);
            }
            catch (Exception e)
            {
                Globals.Logger.Error($"Builder threw an exception for level {enZoneToGet}! Exception: {e}");
                bprint = new LevelBlueprint();
            }

            bprint.CheckForConsistency(true);

            // Enforce certain values

            bprint.enRegion = entry.Config.WorldRegion;
            bprint.enZone = entry.GameID;
            bprint.sDefaultMusic = ""; // TODO Custom music
            bprint.sDialogueFiles = ""; // TODO Dialogue Files
            bprint.sMenuBackground = "bg01_mountainvillage"; // TODO Proper custom backgrounds. Transpiling _Level_Load is a good idea.
            bprint.sZoneName = ""; // TODO Zone titles


            // Loader setup

            Loader.afCurrentHeightLayers = new float[bprint.aiLayerDefaultHeight.Length];
            for (int i = 0; i < bprint.aiLayerDefaultHeight.Length; i++)
                Loader.afCurrentHeightLayers[i] = bprint.aiLayerDefaultHeight[i];

            Loader.lxCurrentSC = bprint.lxInvisibleWalls;

            // Return from method

            __result = bprint;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(EnemyCodex), nameof(EnemyCodex.GetEnemyDescription))]
        internal static bool OnGetEnemyDescription(ref EnemyDescription __result, EnemyCodex.EnemyTypes enType)
        {
            if (!enType.IsFromMod())
                return true;

            __result = Globals.API.Loader.Library.Enemies[enType].EnemyData;

            return false;
        }

        /// <summary>
        /// Implements custom enemy construction by transpiling the second part of GetEnemyInstance.
        /// (Note that our IDs will always trigger the condition for "CacuteForward" version to be called)
        /// </summary>
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(EnemyCodex), nameof(EnemyCodex.GetEnemyInstance_CacuteForward))]
        internal static CodeList GetEnemyInstanceTranspiler(CodeList code, ILGenerator gen)
        {
            // Assert to check if underlying method hasn't shifted heavily
            OpCode op = OpCodes.Nop;
            Debug.Assert(PatchUtils.TryILAt(code, 20, out op) && op == OpCodes.Ldstr, "GetEnemyInstance transpiler is invalid!");

            var insert = new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HelperCallbacks), nameof(HelperCallbacks.InGetEnemyInstance))),
                new CodeInstruction(OpCodes.Stloc_0) // Store returned enemy
            };

            return PatchUtils.InsertAt(code, insert, 20 + 2);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CardCodex), nameof(CardCodex.GetIllustrationPath))]
        public static bool OnGetIllustrationPatch(ref string __result, EnemyCodex.EnemyTypes enEnemy)
        {
            if (!enEnemy.IsFromMod())
            {
                return true;
            }

            __result = Globals.API.Loader.Library.Enemies[enEnemy].Config.CardIllustrationPath;

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(EnemyCodex), nameof(EnemyCodex.GetEnemyDefaultAnimation))]
        public static bool OnGetEnemyDefaultAnimation(ref Animation __result, EnemyCodex.EnemyTypes enType, ContentManager Content)
        {
            if (!enType.IsFromMod())
            {
                return true;
            }

            __result = Globals.API.Loader.Library.Enemies[enType].Config.DefaultAnimation?.Invoke(Content);

            if (__result == null)
            {
                __result = new Animation(1, 0, RenderMaster.txNullTex, Vector2.Zero);
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(EnemyCodex), nameof(EnemyCodex.GetEnemyDisplayIcon))]
        public static bool OnGetEnemyDisplayIcon(ref Texture2D __result, EnemyCodex.EnemyTypes enType, ContentManager Content)
        {
            if (!enType.IsFromMod())
            {
                return true;
            }

            __result = Globals.API.Loader.Library.Enemies[enType].Config.DisplayIcon?.Invoke(Content);

            if (__result == null)
            {
                __result = RenderMaster.txNullTex;
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(EnemyCodex), nameof(EnemyCodex.GetEnemyLocationPicture))]
        public static bool OnGetEnemyLocationPicture(ref Texture2D __result, EnemyCodex.EnemyTypes enType, ContentManager Content)
        {
            if (!enType.IsFromMod())
            {
                return true;
            }

            __result = Globals.API.Loader.Library.Enemies[enType].Config.DisplayBackground?.Invoke(Content);

            if (__result == null)
            {
                __result = RenderMaster.txNullTex;
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(QuestCodex), nameof(Quests.QuestCodex.GetQuestDescription))]
        public static bool OnGetQuestDescription(ref QuestDescription __result, QuestCodex.QuestID p_enID)
        {
            if (!p_enID.IsFromMod())
            {
                return true;
            }

            __result = Globals.API.Loader.Library.Quests[p_enID].QuestData;

            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(QuestCodex), nameof(Quests.QuestCodex.GetQuestInstance))]
        public static void PostGetQuestInstance(ref Quest __result, QuestCodex.QuestID p_enID)
        {
            if (!p_enID.IsFromMod())
            {
                return;
            }

            Globals.API.Loader.Library.Quests[p_enID].Config.Constructor?.Invoke(__result);

            __result.xReward = Globals.API.Loader.Library.Quests[p_enID].QuestData.xReward;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SpellCodex), nameof(SpellCodex.GetSpellInstance), typeof(SpellCodex.SpellTypes), typeof(int), typeof(Level.WorldRegion))]
        internal static void PostGetSpellInstance(ref ISpellInstance __result, SpellCodex.SpellTypes enType, int iPowerLevel, Level.WorldRegion enOverrideRegion)
        {
            if (!enType.IsFromMod())
                return;

            __result = Globals.API.Loader.Library.Spells[enType].Config.Builder(iPowerLevel, enOverrideRegion);

            if (__result.xRenderComponent == null)
            {
                __result.xRenderComponent = new AnimatedRenderComponent(__result);
                __result.xRenderComponent.xTransform = __result.xTransform;
            }

            __result.xRenderComponent.xOwnerObject = __result;

            if (__result.xRenderComponent is AnimatedRenderComponent arc && arc.dixAnimations.Count == 0)
            {
                arc.dixAnimations.Add(0, new Animation(0, 0, RenderMaster.txNullTex, new Vector2(8f, 6f), 4, 1, 17, 32, 0, 0, 6, Animation.LoopSettings.Looping, Animation.CancelOptions.IgnoreIfPlaying, true, true));
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SpellCodex), nameof(SpellCodex.IsEPBlocking))]
        internal static void PostIsEPBlocking(SpellCodex.SpellTypes enType, ref bool __result)
        {
            if (!enType.IsFromMod())
                return;

            __result = false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SpellCodex), nameof(SpellCodex.IsHidden))]
        internal static void PostIsHidden(SpellCodex.SpellTypes enType, ref bool __result)
        {
            if (!enType.IsFromMod())
                return;

            __result = false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SpellCodex), nameof(SpellCodex.IsMagicSkill))]
        internal static void PostIsMagicSkill(SpellCodex.SpellTypes enType, ref bool __result)
        {
            if (!enType.IsFromMod())
                return;

            __result = Globals.API.Loader.Library.Spells[enType].Config.IsMagicSkill;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SpellCodex), nameof(SpellCodex.IsMeleeSkill))]
        internal static void PostIsMeleeSkill(SpellCodex.SpellTypes enType, ref bool __result)
        {
            if (!enType.IsFromMod())
                return;

            __result = Globals.API.Loader.Library.Spells[enType].Config.IsMeleeSkill;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SpellCodex), nameof(SpellCodex.IsUtilitySkill))]
        internal static void PostIsUtilitySkill(SpellCodex.SpellTypes enType, ref bool __result)
        {
            if (!enType.IsFromMod())
                return;

            __result = Globals.API.Loader.Library.Spells[enType].Config.IsUtilitySkill;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SpellCodex), nameof(SpellCodex.IsTalent))]
        internal static void PostIsTalent(SpellCodex.SpellTypes enType, ref bool __result)
        {
            if (!enType.IsFromMod())
                return;

            __result = false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HudRenderComponent), nameof(HudRenderComponent.GetBuffTexture))]
        internal static void PostGetBuffTexture(ref Texture2D __result, BaseStats.StatusEffectSource en)
        {
            if (!en.IsFromMod())
                return;

            string path = Globals.API.Loader.Library.StatusEffects[en].Config.TexturePath;

            if (string.IsNullOrEmpty(path))
            {
                __result = null;
            }
            else
            {
                Utils.ModUtils.TryLoadTex(Globals.API.Loader.Library.StatusEffects[en].Config.TexturePath, Globals.Game.Content, out __result);
            }

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PinCodex), nameof(PinCodex.GetInfo))]
        internal static void PostGetPinInfo(ref PinInfo __result, PinCodex.PinType enType)
        {
            if (!enType.IsFromMod())
                return;

            string[] palettes = new string[]
            {
                "Test1",
                "Test2",
                "Test3",
                "Test4",
                "Test5"
            };

            PinEntry entry = Globals.API.Loader.Library.Pins[enType];
            PinConfig config = entry.Config;

            __result = new PinInfo(
                enType, 
                "None", 
                config.Description, 
                config.PinShape.ToString(), 
                config.PinSymbol.ToString(),
                config.IsSticky ? "TestLight" : palettes[(int)config.PinColor],
                config.IsSticky,
                config.IsBroken,
                FontManager.GetFontByCategory("SmallTitle", FontManager.FontType.Bold8Spacing1),
                FontManager.GetFontByCategory("InMenuDescription", FontManager.FontType.Reg7)
                );
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(GlobalData.MainMenu), nameof(GlobalData.MainMenu.Transition))]
        internal static void OnMainMenuTransition(GlobalData.MainMenu.MenuLevel enTarget)
        {
            if (enTarget == GlobalData.MainMenu.MenuLevel.CharacterSelect)
            {
                HelperCallbacks.MainMenuWorker.AnalyzeStorySavesForCompatibility();
            }
        }
    }
}
