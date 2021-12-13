using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SoG.Modding.Content;
using SoG.Modding.GrindScriptMod;
using SoG.Modding.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace SoG.Modding.Patching.Patches
{
    using CodeList = IEnumerable<CodeInstruction>;

    /// <summary>
    /// Contains patches for Game1 class.
    /// </summary>

    [HarmonyPatch(typeof(Game1))]
    internal static class Patch_Game1
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Game1._EntityMaster_AddEnemy), typeof(ushort), typeof(EnemyCodex.EnemyTypes), typeof(Vector2), typeof(int), typeof(float), typeof(Enemy.SpawnEffectType), typeof(bool), typeof(bool), typeof(float[]))]
        internal static void _EntityMaster_AddEnemyPrefix(ref EnemyCodex.EnemyTypes __state, ref ushort iEnemyID, ref EnemyCodex.EnemyTypes enEnemyType, ref Vector2 p_v2Pos, ref int ibitLayer, ref float fVirtualHeight, ref Enemy.SpawnEffectType enSpawnEffect, ref bool bAsElite, ref bool bDropsLoot, float[] afBehaviourVariables)
        {
            __state = enEnemyType;
            foreach (Mod mod in Globals.Manager.ActiveMods)
                mod.OnEnemySpawn(ref enEnemyType, ref p_v2Pos, ref bAsElite, ref bDropsLoot, ref ibitLayer, ref fVirtualHeight, afBehaviourVariables);
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(Game1._EntityMaster_AddEnemy), typeof(ushort), typeof(EnemyCodex.EnemyTypes), typeof(Vector2), typeof(int), typeof(float), typeof(Enemy.SpawnEffectType), typeof(bool), typeof(bool), typeof(float[]))]
        internal static void _EntityMaster_AddEnemyPostfix(ref EnemyCodex.EnemyTypes __state, Enemy __result, ushort iEnemyID, EnemyCodex.EnemyTypes enEnemyType, Vector2 p_v2Pos, int ibitLayer, float fVirtualHeight, Enemy.SpawnEffectType enSpawnEffect, bool bAsElite, bool bDropsLoot, float[] afBehaviourVariables)
        {
            foreach (Mod mod in Globals.Manager.ActiveMods)
                mod.PostEnemySpawn(__result, enEnemyType, __state, p_v2Pos, bAsElite, bDropsLoot, ibitLayer, fVirtualHeight, afBehaviourVariables);
        }

        /// <summary>
        /// Starts the API, which loads mods and their content.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch("Initialize")] // Protected Method
        internal static void Initialize_Prefix()
        {
            Globals.Manager.Start();
        }

        /// <summary>
        /// Implements custom command parsing.
        /// </summary>
        [HarmonyTranspiler]
        [HarmonyPatch(nameof(Game1._Chat_ParseCommand))]
        internal static CodeList _Chat_ParseCommand_Transpiler(CodeList code, ILGenerator gen)
        {
            var codeList = code.ToList();

            Label afterRet = gen.DefineLabel();

            MethodInfo target = typeof(string).GetMethod("ToLowerInvariant");

            var insert = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldloc_S, 2),
                new CodeInstruction(OpCodes.Ldarg_S, 1),
                new CodeInstruction(OpCodes.Ldarg_S, 2),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PatchHelper), nameof(PatchHelper.InChatParseCommand))),
                new CodeInstruction(OpCodes.Brfalse, afterRet),
                new CodeInstruction(OpCodes.Ret),
                new CodeInstruction(OpCodes.Nop).WithLabels(afterRet)
            };

            return codeList.InsertAfterMethod(target, insert);
        }

        /// <summary>
        /// Implements failsafe custom texture paths for shields.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Game1._Animations_GetAnimationSet), typeof(PlayerView), typeof(string), typeof(string), typeof(bool), typeof(bool), typeof(bool), typeof(bool))]
        internal static bool _Animations_GetAnimationSet_Prefix(PlayerView xPlayerView, string sAnimation, string sDirection, bool bWithWeapon, bool bWithShield, bool bWeaponOnTop, ref PlayerAnimationTextureSet __result)
        {
            ContentManager VanillaContent = RenderMaster.contPlayerStuff;

            __result = new PlayerAnimationTextureSet() { bWeaponOnTop = bWeaponOnTop };

            AssetUtils.TryLoadTexture($"Sprites/Heroes/{sAnimation}/{sDirection}", VanillaContent, out __result.txBase);

            if (bWithShield)
            {
                EquipmentInfo shield = xPlayerView.xEquipment.DisplayShield;
                ItemEntry entry = null;

                if (shield != null)
                {
                    Globals.Manager.Library.TryGetEntry(shield.enItemType, out entry);
                }

                if (entry == null)
                {
                    __result.txShield = Globals.Manager.GrindScript.ErrorTexture;
                }
                else
                {
                    if (entry.useVanillaResourceFormat)
                    {
                        AssetUtils.TryLoadTexture($"Sprites/Heroes/{sAnimation}/Shields/{shield.sResourceName}/{sDirection}", VanillaContent, out __result.txShield);
                    }
                    else
                    {
                        // Mods use a different path format
                        AssetUtils.TryLoadTexture($"{shield.sResourceName}/{sAnimation}/{sDirection}", VanillaContent, out __result.txShield);
                    }
                }
            }

            if (bWithWeapon)
                __result.txWeapon = RenderMaster.txNullTex;

            return false; // Never executes the original
        }

        /// <summary>
        /// Implements failsafe custom texture paths for perks.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Game1._RogueLike_GetPerkTexture))]
        internal static bool _RogueLike_GetPerkTexture_Prefix(RogueLikeMode.Perks enPerk, ref Texture2D __result)
        {
            Globals.Manager.Library.TryGetEntry(enPerk, out PerkEntry entry);

            if (entry == null)
            {
                __result = Globals.Manager.GrindScript.ErrorTexture;
                return false;  // Unknown perk entry??
            }

            if (entry.texturePath == null)
            {
                if (entry.IsVanilla)
                {
                    return true;  // Grab from vanilla
                }

                __result = Globals.Manager.GrindScript.ErrorTexture;
                return false;
            }
            else
            {
                AssetUtils.TryLoadTexture(entry.texturePath, Globals.Game.Content, out __result);
            }

            return false;
        }



        [HarmonyTranspiler]
        [HarmonyPatch(typeof(Game1), nameof(Game1._RogueLike_ActivatePerks))]
        public static CodeList _RogueLike_ActivatePerks_Transpiler(CodeList instructions, ILGenerator generator)
        {
            List<CodeInstruction> codeList = instructions.ToList();

            int start = -1;
            int ldloc_pos = 3;
            while (ldloc_pos-- > 0)
            {
                start = codeList.FindIndex(start + 1, x => x.opcode == OpCodes.Ldloc_0);
            }

            int end = -1;
            int ldloca_pos = 2;
            while (ldloca_pos-- > 0)
            {
                end = codeList.FindIndex(end + 1, x => x.opcode == OpCodes.Ldloca_S && (x.operand as LocalBuilder).LocalIndex == 3);
            }

            Label perkProcessedSkip = generator.DefineLabel();

            codeList[end].WithLabels(perkProcessedSkip);

            List<CodeInstruction> inserted = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldarg_S, 1).WithLabels(codeList[start].labels).WithBlocks(codeList[start].blocks),
                new CodeInstruction(OpCodes.Ldloc_0),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PatchHelper), nameof(PatchHelper.InActivatePerk))),
                new CodeInstruction(OpCodes.Brtrue, perkProcessedSkip)
            };

            codeList.InsertRange(start, inserted);

            return codeList;
        }

        /// <summary>
        /// Implements failsafe custom texture paths for treats and perks.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Game1._RogueLike_GetTreatCurseTexture))]
        internal static bool _RogueLike_GetTreatCurseTexture_Prefix(RogueLikeMode.TreatsCurses enTreat, ref Texture2D __result)
        {
            var storage = Globals.Manager.Library.GetStorage<RogueLikeMode.TreatsCurses, CurseEntry>();

            if (storage.TryGetValue(enTreat, out CurseEntry entry))
            {
                if (string.IsNullOrEmpty(entry.texturePath))
                {
                    // Maybe it's from vanilla. Try loading using original method

                    __result = OriginalMethods._RogueLike_GetTreatCurseTexture(Globals.Game, enTreat);

                    if (__result == null)
                    {
                        __result = Globals.Manager.GrindScript.ErrorTexture;
                    }
                }
                else
                {
                    AssetUtils.TryLoadTexture(entry.texturePath, Globals.Game.Content, out __result);
                }
            }
            else
            {
                __result = RenderMaster.txNullTex;  // Covers the case when the treat / curse is None.
            }

            return false;
        }

        /// <summary>
        /// Implements custom treats and curses.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Game1._RogueLike_GetTreatCurseInfo))]
        internal static bool _RogueLike_GetTreatCurseInfo_Prefix(RogueLikeMode.TreatsCurses enTreatCurse, out string sNameHandle, out string sDescriptionHandle, out float fScoreModifier)
        {
            var storage = Globals.Manager.Library.GetStorage<RogueLikeMode.TreatsCurses, CurseEntry>();
            
            if (storage.TryGetValue(enTreatCurse, out CurseEntry entry))
            {
                sNameHandle = entry.nameHandle;
                sDescriptionHandle = entry.descriptionHandle;
                fScoreModifier = entry.scoreModifier;
            }
            else
            {
                // Covers the cases where the treat / curse is none, or unknown
                sNameHandle = "";
                sDescriptionHandle = "";
                fScoreModifier = 0f;
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Game1._Saving_SaveCharacterToFile))]
        internal static void _Saving_SaveCharacterToFile_Prefix()
        {
            Globals.SetVersionTypeAsModded(false);
        }

        /// <summary>
        /// Implements saving of extra information for ".cha" save files.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(nameof(Game1._Saving_SaveCharacterToFile))]
        internal static void _Saving_SaveCharacterToFile_Postfix(int iFileSlot)
        {
            Globals.SetVersionTypeAsModded(true);

            string ext = ModSaving.SaveFileExtension;

            PlayerView player = Globals.Game.xLocalPlayer;
            string appData = Globals.Game.sAppData;

            int carousel = player.iSaveCarousel - 1;
            if (carousel < 0)
                carousel += 5;

            string backupPath = "";

            string chrFile = $"{appData}Characters/" + $"{iFileSlot}.cha{ext}";

            if (File.Exists(chrFile))
            {
                if (player.sSaveableName == "")
                {
                    player.sSaveableName = player.sNetworkNickname;
                    foreach (char c in Path.GetInvalidFileNameChars())
                        player.sSaveableName = player.sSaveableName.Replace(c, ' ');
                }

                backupPath = $"{appData}Backups/" + $"{player.sSaveableName}_{player.xJournalInfo.iCollectorID}{iFileSlot}/";
                ModUtils.TryCreateDirectory(backupPath);

                File.Copy(chrFile, backupPath + $"auto{carousel}.cha{ext}", overwrite: true);

                string wldFile = $"{appData}Worlds/" + $"{iFileSlot}.wld{ext}";
                if (File.Exists(wldFile))
                {
                    File.Copy(wldFile, backupPath + $"auto{carousel}.wld{ext}", overwrite: true);
                }
            }

            using (BinaryWriter bw = new BinaryWriter(new FileStream($"{chrFile}.temp", FileMode.Create, FileAccess.Write)))
            {
                Globals.Logger.Info($"Saving mod character {iFileSlot}...");
                Globals.Manager.Saving.SaveModCharacter(bw);
            }

            try
            {
                File.Copy($"{chrFile}.temp", chrFile, overwrite: true);
                if (backupPath != "")
                {
                    File.Copy($"{chrFile}.temp", backupPath + $"latest.cha{ext}", overwrite: true);
                }
                File.Delete($"{chrFile}.temp");
            }
            catch { }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Game1._Saving_SaveWorldToFile))]
        internal static void _Saving_SaveWorldToFile_Prefix()
        {
            Globals.SetVersionTypeAsModded(false);
        }

        /// <summary>
        /// Implements saving of extra information for ".wld" save files.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(nameof(Game1._Saving_SaveWorldToFile))]
        internal static void _Saving_SaveWorldToFile_Postfix(int iFileSlot)
        {
            Globals.SetVersionTypeAsModded(true);

            string ext = ModSaving.SaveFileExtension;

            PlayerView player = Globals.Game.xLocalPlayer;
            string appData = Globals.Game.sAppData;

            string backupPath = "";
            string chrFile = $"{appData}Characters/" + $"{iFileSlot}.cha{ext}";
            string wldFile = $"{appData}Worlds/" + $"{iFileSlot}.wld{ext}";

            if (File.Exists(chrFile))
            {
                if (player.sSaveableName == "")
                {
                    player.sSaveableName = player.sNetworkNickname;
                    foreach (char c in Path.GetInvalidFileNameChars())
                        player.sSaveableName = player.sSaveableName.Replace(c, ' ');
                }

                backupPath = $"{appData}Backups/" + $"{player.sSaveableName}_{player.xJournalInfo.iCollectorID}{iFileSlot}/";
                ModUtils.TryCreateDirectory(backupPath);
            }

            using (BinaryWriter bw = new BinaryWriter(new FileStream($"{wldFile}.temp", FileMode.Create, FileAccess.Write)))
            {
                Globals.Logger.Info($"Saving mod world {iFileSlot}...");
                Globals.Manager.Saving.SaveModWorld(bw);
            }

            try
            {
                File.Copy($"{wldFile}.temp", wldFile, overwrite: true);
                if (backupPath != "" && iFileSlot != 100)
                {
                    File.Copy($"{wldFile}.temp", backupPath + $"latest.wld{ext}", overwrite: true);
                }
                File.Delete($"{wldFile}.temp");
            }
            catch { }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Game1._Saving_SaveRogueToFile), typeof(string))]
        internal static void _Saving_SaveRogueToFile_Prefix()
        {
            Globals.SetVersionTypeAsModded(false);
        }

        /// <summary>
        /// Implements saving of extra information for ".sav" save files.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(nameof(Game1._Saving_SaveRogueToFile), typeof(string))]
        internal static void _Saving_SaveRogueToFile_Postfix()
        {
            Globals.SetVersionTypeAsModded(true);

            string ext = ModSaving.SaveFileExtension;

            bool other = CAS.IsDebugFlagSet("OtherArcadeMode");
            string savFile = Globals.Game.sAppData + $"arcademode{(other ? "_other" : "")}.sav{ext}";

            using (BinaryWriter bw = new BinaryWriter(new FileStream($"{savFile}.temp", FileMode.Create, FileAccess.Write)))
            {
                Globals.Logger.Info($"Saving mod arcade...");
                Globals.Manager.Saving.SaveModArcade(bw);
            }

            File.Copy($"{savFile}.temp", savFile, overwrite: true);
            File.Delete($"{savFile}.temp");
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Game1._Loading_LoadCharacterFromFile))]
        internal static void _Loading_LoadCharacterFromFile_Prefix()
        {
            Globals.SetVersionTypeAsModded(false);
        }

        /// <summary>
        /// Implements loading of extra information for ".cha" save files.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(nameof(Game1._Loading_LoadCharacterFromFile))]
        internal static void _Loading_LoadCharacterFromFile_Postfix(int iFileSlot, bool bAppearanceOnly)
        {
            Globals.SetVersionTypeAsModded(true);

            string ext = ModSaving.SaveFileExtension;

            string chrFile = Globals.Game.sAppData + "Characters/" + $"{iFileSlot}.cha{ext}";

            if (!File.Exists(chrFile)) return;

            using (BinaryReader br = new BinaryReader(new FileStream(chrFile, FileMode.Open, FileAccess.Read)))
            {
                Globals.Logger.Info($"Loading mod character {iFileSlot}...");
                Globals.Manager.Saving.LoadModCharacter(br);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Game1._Loading_LoadWorldFromFile))]
        internal static void OnWorldLoad()
        {
            Globals.SetVersionTypeAsModded(false);
        }

        /// <summary>
        /// Implements loading of extra information for ".wld" save files.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(nameof(Game1._Loading_LoadWorldFromFile))]
        internal static void PostWorldLoad(int iFileSlot)
        {
            Globals.SetVersionTypeAsModded(true);

            string ext = ModSaving.SaveFileExtension;

            string wldFile = Globals.Game.sAppData + "Worlds/" + $"{iFileSlot}.wld{ext}";

            if (!File.Exists(wldFile)) return;

            using (BinaryReader br = new BinaryReader(new FileStream(wldFile, FileMode.Open, FileAccess.Read)))
            {
                Globals.Logger.Info($"Loading mod world {iFileSlot}...");
                Globals.Manager.Saving.LoadModWorld(br);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Game1._Loading_LoadRogueFile))]
        internal static void _Loading_LoadRogueFile_Prefix()
        {
            Globals.SetVersionTypeAsModded(false);
        }

        /// <summary>
        /// Implements loading of extra information for ".sav" save files.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(nameof(Game1._Loading_LoadRogueFile))]
        internal static void _Loading_LoadRogueFile_Postfix()
        {
            Globals.SetVersionTypeAsModded(true);

            string ext = ModSaving.SaveFileExtension;

            if (RogueLikeMode.LockedOutDueToHigherVersionSaveFile) return;

            bool other = CAS.IsDebugFlagSet("OtherArcadeMode");
            string savFile = Globals.Game.sAppData + $"arcademode{(other ? "_other" : "")}.sav{ext}";

            if (!File.Exists(savFile)) return;

            using (BinaryReader br = new BinaryReader(new FileStream(savFile, FileMode.Open, FileAccess.Read)))
            {
                Globals.Logger.Info($"Loading mod arcade...");
                Globals.Manager.Saving.LoadModArcade(br);
            }
        }

        /// <summary>
        /// This patch cleans up the mod character info along with the base character save.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(nameof(Game1._Saving_DeleteCharacterFile))]
        internal static void _Saving_DeleteCharacterFile_Postfix(int iFileSlot)
        {
            string path = Globals.Game.sAppData + "Characters/" + iFileSlot + ".cha" + ModSaving.SaveFileExtension;

            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        /// <summary>
        /// This patch cleans up the mod world info along with the base world save.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(nameof(Game1._Saving_DeleteWorldFile))]
        internal static void _Saving_DeleteWorldFile_Postfix(int iFileSlot)
        {
            string path = Globals.Game.sAppData + "Worlds/" + iFileSlot + ".wld" + ModSaving.SaveFileExtension;

            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        /// <summary>
        /// Implements mod content loading by inserting a call before important game code runs.
        /// </summary>
        [HarmonyTranspiler]
        [HarmonyPatch(nameof(Game1.__StartupThreadExecute))]
        internal static CodeList __StartupThreadExecute_Transpiler(CodeList code, ILGenerator gen)
        {
            var codeList = code.ToList();

            MethodInfo target = typeof(DialogueCharacterLoading).GetMethod("Init");

            MethodInfo targetTwo = typeof(Game1).GetMethod(nameof(Game1._Loading_LoadGlobalFile));

            var insert = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PatchHelper), nameof(PatchHelper.DoModContentLoad)))
            };

            var moreInsert = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PatchHelper), nameof(PatchHelper.UpdateVersionNumber)))
            };

            return codeList
                .InsertAfterMethod(target, insert)
                .InsertBeforeMethod(targetTwo, moreInsert);
        }

        /// <summary>
        /// Implements custom level load code.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Game1._LevelLoading_DoStuff))]
        internal static bool _LevelLoading_DoStuff_Prefix(Game1 __instance, Level.ZoneEnum enLevel, bool bStaticOnly)
        {
            if (enLevel.IsFromSoG())
            {
                Globals.Manager.Library.TryGetEntry(enLevel, out LevelEntry entry);
                if (entry == null || entry.loader == null)
                {
                    // No replacement exists. Call the vanilla method instead.

                    OriginalMethods._LevelLoading_DoStuff(__instance, enLevel, bStaticOnly);
                    return false;
                }
            }
            else if (!enLevel.IsFromMod())
            {
                return false; // Unknown mod entry?
            }

            EditedMethods._LevelLoading_DoStuff(__instance, enLevel, bStaticOnly);
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(Game1._Level_Load))]
        internal static void _Level_Load_Postfix(LevelBlueprint xBP, bool bStaticOnly)
        {
            foreach (Mod mod in Globals.Manager.ActiveMods)
                mod.PostLevelLoad(xBP.enZone, xBP.enRegion, bStaticOnly);
        }

        /// <summary>
        /// Implements an updated interface for Treat and Curse shops in Arcade.
        /// The new menus support viewing more than 10 entries at a time.
        /// </summary>
        [HarmonyTranspiler]
        [HarmonyPatch(nameof(Game1._ShopMenu_Render_TreatCurseAssign))]
        internal static CodeList _ShopMenu_Render_TreatCurseAssign_Transpiler(CodeList code, ILGenerator gen)
        {
            var codeList = code.ToList();

            const string ErrorMessage = "ShopMenu_Render_TreatCurseAssign transpiler is invalid!";
            Debug.Assert(codeList[457].opcode == OpCodes.Ldarg_0, ErrorMessage);
            Debug.Assert(codeList[451].opcode == OpCodes.Ldarg_0, ErrorMessage);
            Debug.Assert(codeList[105].opcode == OpCodes.Ldc_I4_5, ErrorMessage);
            Debug.Assert(codeList[94].opcode == OpCodes.Ldc_I4_5, ErrorMessage);
            Debug.Assert(codeList[70].opcode == OpCodes.Ldc_I4_0, ErrorMessage);

            LocalBuilder start = gen.DeclareLocal(typeof(int));
            LocalBuilder end = gen.DeclareLocal(typeof(int));
            LocalBuilder worker = gen.DeclareLocal(typeof(TCMenuWorker));

            var firstInsert = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Call, AccessTools.Property(typeof(PatchHelper), nameof(PatchHelper.TCMenuWorker)).GetGetMethod(true)),
                new CodeInstruction(OpCodes.Stloc_S, worker.LocalIndex),
                new CodeInstruction(OpCodes.Ldloc_S, worker.LocalIndex),
                new CodeInstruction(OpCodes.Call, typeof(TCMenuWorker).GetMethod(nameof(TCMenuWorker.Update))),
                new CodeInstruction(OpCodes.Ldloc_S, worker.LocalIndex),
                new CodeInstruction(OpCodes.Call, typeof(TCMenuWorker).GetProperty(nameof(TCMenuWorker.TCListStart)).GetGetMethod()),
                new CodeInstruction(OpCodes.Stloc_S, start.LocalIndex),
                new CodeInstruction(OpCodes.Ldloc_S, worker.LocalIndex),
                new CodeInstruction(OpCodes.Call, typeof(TCMenuWorker).GetProperty(nameof(TCMenuWorker.TCListEnd)).GetGetMethod()),
                new CodeInstruction(OpCodes.Stloc_S, end.LocalIndex),
                new CodeInstruction(OpCodes.Ldloc_S, start.LocalIndex),
            };

            var secondInsert = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldloc_S, end.LocalIndex)
            };

            var thirdInsert = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldloc_S, worker.LocalIndex),
                new CodeInstruction(OpCodes.Call, AccessTools.Property(typeof(PatchHelper), nameof(PatchHelper.SpriteBatch)).GetGetMethod(true)),
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Ldarg_2),
                new CodeInstruction(OpCodes.Call, typeof(TCMenuWorker).GetMethod(nameof(TCMenuWorker.DrawScroller))),
            };

            var offsetInsert = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldloc_S, start.LocalIndex),
                new CodeInstruction(OpCodes.Sub)
            };

            return codeList
                .InsertAt(457, thirdInsert)
                .ReplaceAt(451, 5, secondInsert)
                .InsertAt(105, offsetInsert)
                .InsertAt(94, offsetInsert)
                .ReplaceAt(70, 1, firstInsert);
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Game1._Player_TakeDamage))]
        internal static void _Player_TakeDamage_Prefix(PlayerView xView, ref int iInDamage, ref byte byType)
        {
            foreach (Mod mod in Globals.Manager.ActiveMods)
                mod.OnPlayerDamaged(xView, ref iInDamage, ref byType);
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Game1._Player_KillPlayer), new Type[] { typeof(PlayerView), typeof(bool), typeof(bool) })]
        internal static void _Player_KillPlayer_Prefix(PlayerView xView)
        {
            foreach (Mod mod in Globals.Manager.ActiveMods)
                mod.OnPlayerKilled(xView);
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(Game1._Player_ApplyLvUpBonus))]
        internal static void _Player_ApplyLvUpBonus_Postfix(PlayerView xView)
        {
            foreach (Mod mod in Globals.Manager.ActiveMods)
                mod.PostPlayerLevelUp(xView);
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Game1._Enemy_TakeDamage))]
        internal static void _Enemy_TakeDamage_Prefix(Enemy xEnemy, ref int iDamage, ref byte byType)
        {
            foreach (Mod mod in Globals.Manager.ActiveMods)
                mod.OnEnemyDamaged(xEnemy, ref iDamage, ref byType);
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Game1._NPC_TakeDamage))]
        internal static void _NPC_TakeDamage_Prefix(NPC xEnemy, ref int iDamage, ref byte byType)
        {
            foreach (Mod mod in Globals.Manager.ActiveMods)
                mod.OnNPCDamaged(xEnemy, ref iDamage, ref byType);
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Game1._NPC_Interact))]
        internal static void _NPC_Interact_Prefix(PlayerView xView, NPC xNPC)
        {
            foreach (Mod mod in Globals.Manager.ActiveMods)
                mod.OnNPCInteraction(xNPC);
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Game1._LevelLoading_DoStuff_Arcadia))]
        internal static void _LevelLoading_DoStuff_Arcadia_Prefix()
        {
            foreach (Mod mod in Globals.Manager.ActiveMods)
                mod.OnArcadiaLoad();

            // Just in case it didn't get set before; submitting modded runs is not a good idea
            Globals.Game.xGameSessionData.xRogueLikeSession.bTemporaryHighScoreBlock = true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Game1._Item_Use), new Type[] { typeof(ItemCodex.ItemTypes), typeof(PlayerView), typeof(bool) })]
        internal static void _Item_Use_Prefix(ItemCodex.ItemTypes enItem, PlayerView xView, ref bool bSend)
        {
            if (xView.xViewStats.bIsDead)
                return;
            foreach (Mod mod in Globals.Manager.ActiveMods)
                mod.OnItemUse(enItem, xView, ref bSend);
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(Game1._LevelLoading_DoStuff_ArcadeModeRoom))]
        internal static void _LevelLoading_DoStuff_ArcadeModeRoom_Postfix()
        {
            foreach (Mod mod in Globals.Manager.ActiveMods)
                mod.PostArcadeRoomStart();
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(Game1._Skill_ActivateSkill))]
        internal static void _Skill_ActivateSkill_Postfix(PlayerView xView, ISpellActivation xact, SpellCodex.SpellTypes enType, int iBoostState)
        {
            foreach (Mod mod in Globals.Manager.ActiveMods)
                mod.PostSpellActivation(xView, xact, enType, iBoostState);
        }

        // Currently this patch does not apply beastmode to modded enemies
        // Nor does it force boss enemies to ignore pets
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Game1._Enemy_AdjustForDifficulty))]
        internal static bool _Enemy_AdjustForDifficulty_Prefix(Enemy xEn)
        {
            var storage = Globals.Manager.Library.GetStorage<EnemyCodex.EnemyTypes, EnemyEntry>();

            if (storage.TryGetValue(xEn.enType, out EnemyEntry entry))
            {
                if (entry.difficultyScaler == null && entry.IsVanilla)
                {
                    return true;  // No replacement found, run vanilla code
                }

                entry.difficultyScaler.Invoke(xEn);
                return false;
            }

            return false;  // Unknown mod entry?
        }

        /// <summary>
        /// Overrides OutputError to always output an error log in "Logs" folder,
        /// instead of launching the Grindea Beta Error Report tool.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Game1.OutputError), typeof(string), typeof(string))]
        internal static bool OutputError_Prefix(string p_sLocation, string e)
        {
            if (CAS.IsDebugFlagSet_Release("silentsend"))
            {
                // Ignore silent sends for now
                return false;
            }

            if (e.Contains("OutOfMemoryException") && e.Contains("VertexBuffer"))
            {
                Globals.Game.xOptions.bLoneBats = true;
                Globals.Game.xOptions.SaveText();
            }

            e = e.Replace("C:\\Dropbox\\Eget jox\\!DugTrio\\Legend Of Grindia\\Legend Of Grindia\\Legend Of Grindia", "(path)");
            e = e.Replace("F:\\Stable Branch\\Legend Of Grindia\\Legend Of Grindia", "(path)");

            StringBuilder msg = new StringBuilder(2048);

            msg.Append("An error happened while running a modded game instance!").AppendLine();
            msg.Append("=== Exception message ===").AppendLine();
            msg.Append(e).AppendLine();
            msg.Append("=== Game Settings ===").AppendLine();
            msg.Append("Game Version = " + Globals.Game.sVersionNumberOnly).AppendLine();
            msg.Append("Fullscreen = " + Globals.Game.xOptions.enFullScreen).AppendLine();
            msg.Append("Network role = " + Globals.Game.xNetworkInfo.enCurrentRole).AppendLine();
            msg.Append("Extra Error Info => " + DebugKing.dssExtraErrorInfo.Count + " pairs").AppendLine();

            foreach (KeyValuePair<string, string> kvp in DebugKing.dssExtraErrorInfo)
            {
                msg.Append("  " + kvp.Key + " = " + kvp.Value).AppendLine();
            }

            msg.Append("=== GrindScript Info ===").AppendLine();
            msg.Append("Active Mods => " + Globals.Manager.ActiveMods.Count + " mods").AppendLine();

            foreach (Mod mod in Globals.Manager.ActiveMods)
            {
                msg.Append("  " + mod.ToString()).AppendLine();
            }

            msg.Append("All Mods => " + Globals.Manager.Mods.Count + " mods").AppendLine();

            foreach (Mod mod in Globals.Manager.Mods)
            {
                msg.Append("  " + mod.ToString()).AppendLine();
            }

            var time = DateTime.Now;

            string logName = $"CrashLog_{time.Year}.{time.Month}.{time.Day}_{time.Hour}.{time.Minute}.{time.Second}.txt";

            StreamWriter writer = null;
            try
            {
                writer = new StreamWriter(new FileStream(Path.Combine("Logs", logName), FileMode.Create, FileAccess.Write));
                writer.Write(msg.ToString());
            }
            catch { }
            finally
            {
                writer?.Close();
            }

            OnExiting_Postfix();

            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnExiting")] // Protected Method
        internal static void OnExiting_Postfix()
        {
            (Globals.Logger?.NextLogger as FileLogger)?.FlushToDisk();

            foreach (Mod mod in Globals.Manager.ActiveMods)
            {
                (mod.Logger?.NextLogger as FileLogger)?.FlushToDisk();
            }
        }

        /// <summary>
        /// Transpiles processing of client messages by the server.
        /// </summary>
        [HarmonyTranspiler]
        [HarmonyPatch(nameof(Game1._Network_ParseClientMessage))]
        internal static CodeList _Network_ParseClientMessage_Transpiler(CodeList code, ILGenerator gen)
        {
            // Finds the method end. Used to insert mod packet parsing
            bool isMethodEnd(List<CodeInstruction> list, int index)
            {
                return
                    list[index].opcode == OpCodes.Leave_S &&
                    list[index + 1].opcode == OpCodes.Ldc_I4_1 &&
                    list[index + 2].opcode == OpCodes.Ret;
            }

            // Finds the demo check in message 97 parser. Used to check mod list compatibility
            bool isMessage97VersionCheck(List<CodeInstruction> list, int index)
            {
                return
                    list[index].opcode == OpCodes.Ldarg_0 &&
                    list[index + 1].opcode == OpCodes.Ldfld &&
                    ReferenceEquals(list[index + 1].operand, typeof(Game1).GetField(nameof(Game1.bIsDemo))) &&
                    list[index + 2].opcode == OpCodes.Brfalse_S;
            }

            List<CodeInstruction> codeList = code.ToList();

            // First Insertion

            int methodEndIndex = PatchUtils.FindPosition(codeList, isMethodEnd);
            int firstIndex = methodEndIndex + 1;

            var firstInsert = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Ldloc_3),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PatchHelper), nameof(PatchHelper.InNetworkParseClientMessage))),
            };

            firstInsert[0].WithLabels(codeList[firstIndex].labels.ToArray());
            codeList[firstIndex].labels.Clear();

            codeList.InsertAt(firstIndex, firstInsert);

            // Second Insertion

            int versionCheckIndex = PatchUtils.FindPosition(codeList, isMessage97VersionCheck);
            MethodInfo secondTargetMethod = typeof(string).GetMethod("op_Equality", BindingFlags.Static | BindingFlags.Public);

            var secondInsert = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PatchHelper), nameof(PatchHelper.CheckModListCompatibility)))
            };

            return codeList.InsertAfterMethod(secondTargetMethod, secondInsert, startOffset: versionCheckIndex, editsReturnValue: true);
        }

        /// <summary>
        /// Transpiles processing of server messages by the client.
        /// First insertion allows mod packets from server to be parsed.
        /// </summary>
        [HarmonyTranspiler]
        [HarmonyPatch(nameof(Game1._Network_ParseServerMessage))]
        internal static CodeList _Network_ParseServerMessage_Transpiler(CodeList code, ILGenerator gen)
        {
            bool isMethodEnd(List<CodeInstruction> codeToSearch, int index)
            {
                return
                    codeToSearch[index].opcode == OpCodes.Leave_S &&
                    codeToSearch[index + 1].opcode == OpCodes.Ldc_I4_1 &&
                    codeToSearch[index + 2].opcode == OpCodes.Ret;
            }

            bool isMessage19VersionSend(List<CodeInstruction> list, int index)
            {
                return
                    list[index].opcode == OpCodes.Ldarg_0 &&
                    list[index + 1].opcode == OpCodes.Ldfld &&
                    ReferenceEquals(list[index + 1].operand, typeof(Game1).GetField(nameof(Game1.bIsDemo))) &&
                    list[index + 2].opcode == OpCodes.Brfalse_S;
            }

            List<CodeInstruction> codeList = code.ToList();

            // First Insertion

            int methodEndIndex = PatchUtils.FindPosition(codeList, isMethodEnd);
            int firstInsertIndex = methodEndIndex + 1;

            var firstInsert = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Ldloc_1),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PatchHelper), nameof(PatchHelper.InNetworkParseServerMessage))),
            };

            firstInsert[0].WithLabels(codeList[firstInsertIndex].labels.ToArray());
            codeList[firstInsertIndex].labels.Clear();

            codeList.InsertAt(firstInsertIndex, firstInsert);

            // Second Insertion

            int versionCheckIndex = PatchUtils.FindPosition(codeList, isMessage19VersionSend);

            MethodInfo secondTargetMethod = typeof(Game1).GetMethod(nameof(Game1._Network_SendMessage), new Type[] { typeof(OutMessage), typeof(int), typeof(Lidgren.Network.NetDeliveryMethod) });

            var secondInsert = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldloc_S, 81),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PatchHelper), nameof(PatchHelper.WriteModList)))
            };

            codeList.InsertBeforeMethod(secondTargetMethod, secondInsert, startOffset: versionCheckIndex);

            return codeList;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(Game1._Enemy_HandleDeath))]
        internal static void _Enemy_HandleDeath_Postfix(Enemy xEnemy, AttackPhase xAttackPhaseThatHit)
        {
            foreach (Mod mod in Globals.Manager.ActiveMods)
                mod.PostEnemyKilled(xEnemy, xAttackPhaseThatHit);
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Game1._RogueLike_ActivatePin))]
        internal static bool _RogueLike_ActivatePinPrefix(PlayerView xView, PinCodex.PinType enEffect, bool bSend)
        {
            EditedMethods.SendPinActivation(Globals.Game, xView, enEffect, bSend);

            Globals.Manager.Library.TryGetEntry(enEffect, out PinEntry entry);

            if (entry == null)
            {
                return false;  // Unknown mod object??
            }

            if (entry.equipAction == null && entry.IsVanilla)
            {
                EditedMethods.ApplyPinEffect(Globals.Game, xView, enEffect, bSend);
                return false;
            }

            entry.equipAction?.Invoke(xView);
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Game1._RogueLike_DeactivatePin))]
        internal static bool _RogueLike_DeactivatePin_Prefix(PlayerView xView, PinCodex.PinType enEffect, bool bSend)
        {
            EditedMethods.SendPinDeactivation(Globals.Game, xView, enEffect, bSend);

            Globals.Manager.Library.TryGetEntry(enEffect, out PinEntry entry);

            if (entry == null)
            {
                return false;  // Unknown mod object??
            }

            if (entry.unequipAction == null && entry.IsVanilla)
            {
                EditedMethods.RemovePinEffect(Globals.Game, xView, enEffect, bSend);
                return false;
            }

            entry.unequipAction?.Invoke(xView);
            return false;
        }

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(Game1._RogueLike_GetRandomPin))]
        internal static CodeList _RogueLike_GetRandomPin_Transpiler(CodeList code, ILGenerator gen)
        {
            List<CodeInstruction> codeList = code.ToList();

            int start = codeList.FindIndex(x => x.opcode == OpCodes.Stloc_0) + 1;

            int newobj_pos = 2;
            int end = -1;
            while (newobj_pos-- > 0)
            {
                end = codeList.FindIndex(end + 1, x => x.opcode == OpCodes.Newobj);
            }

            List<CodeInstruction> toInsert = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldloc_0),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patch_Game1), nameof(InGetRandomPin))),
            };

            codeList.RemoveRange(start, end - start);
            codeList.InsertAt(start, toInsert);

            return codeList;
        }

        private static void InGetRandomPin(List<PinCodex.PinType> list)
        {
            foreach (var pair in Globals.Manager.Library.GetStorage<PinCodex.PinType, PinEntry>())
            {
                if (pair.Value.conditionToDrop == null || pair.Value.conditionToDrop.Invoke())
                {
                    list.Add(pair.Key);
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(Game1._Menu_CharacterSelect_Render))]
        internal static void _Menu_CharacterSelect_Render_Postfix()
        {
            PatchHelper.MainMenuWorker.CheckStorySaveCompatibility();
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(Game1._Menu_Render_TopMenu))]
        internal static void _Menu_Render_TopMenu_Postfix()
        {
            PatchHelper.MainMenuWorker.CheckArcadeSaveCompatiblity();

            PatchHelper.MainMenuWorker.RenderModMenuButton();
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(Game1._Menu_TopMenu_Interface))]
        internal static void _Menu_TopMenu_Interface_Postfix()
        {
            PatchHelper.MainMenuWorker.PostTopMenuInterface();
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(Game1._Menu_Update))]
        internal static void _Menu_Update_Postfix()
        {
            PatchHelper.MainMenuWorker.MenuUpdate();
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(Game1._Menu_Render))]
        internal static void _Menu_Render_Postfix()
        {
            SpriteBatch spriteBatch = Globals.SpriteBatch;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.Default, null);

            if (Globals.Game.xGlobalData.xMainMenuData.enMenuLevel == MainMenuWorker.ReservedModMenuID)
            {
                PatchHelper.MainMenuWorker.ModMenuRender();
            }

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, null);
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Game1.EquipmentSpecialEffectAdded))]
        internal static bool EquipmentSpecialEffectAddedPrefix(EquipmentInfo.SpecialEffect enEffect, PlayerView xView)
        {
            Globals.Manager.Library.TryGetEntry<EquipmentInfo.SpecialEffect, EquipmentEffectEntry>(enEffect, out var entry);

            if (entry != null)
            {
                if (entry._onEquip == null && entry.IsVanilla)
                {
                    return true;  // Use vanilla equip add
                }

                entry?._onEquip(xView);
            }

            return false;  // Unknown mod object?
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Game1.EquipmentSpecialEffectRemoved))]
        internal static bool EquipmentSpecialEffectRemovedPrefix(EquipmentInfo.SpecialEffect enEffect, PlayerView xView)
        {
            Globals.Manager.Library.TryGetEntry<EquipmentInfo.SpecialEffect, EquipmentEffectEntry>(enEffect, out var entry);

            if (entry != null)
            {
                if (entry._onRemove == null && entry.IsVanilla)
                {
                    return true;  // Use vanilla equip add
                }

                entry?._onRemove(xView);
            }

            return false;  // Unknown mod object?
        }
    }
}
