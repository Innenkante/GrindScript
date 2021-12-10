using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using SoG.Modding.Content;
using SoG.Modding.Utils;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using CodeList = System.Collections.Generic.IEnumerable<HarmonyLib.CodeInstruction>;

namespace SoG.Modding.Patching.Patches
{
    /// <summary>
    /// Contains patches for SoundSystem class.
    /// </summary>
    [HarmonyPatch(typeof(SoundSystem))]
    internal static class Patch_SoundSystem
    {
        #region Reflected stuff

        private static FieldInfo s_musicWaveBank = AccessTools.Field(typeof(SoundSystem), "musicWaveBank");

        private static FieldInfo s_loadedMusicWaveBank = AccessTools.Field(typeof(SoundSystem), "loadedMusicWaveBank");

        private static FieldInfo s_standbyWaveBanks = AccessTools.Field(typeof(SoundSystem), "dsxStandbyWaveBanks");

        private static FieldInfo s_songRegionMap = AccessTools.Field(typeof(SoundSystem), "dssSongRegionMap");

        private static FieldInfo s_universalMusic = AccessTools.Field(typeof(SoundSystem), "universalMusicWaveBank");

        private static FieldInfo s_audioEngine = AccessTools.Field(typeof(SoundSystem), "audioEngine");

        private static MethodInfo s_checkStandbyBanks = AccessTools.Method(typeof(SoundSystem), "CheckStandbyBanks");

        #endregion

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(SoundSystem.PlayInterfaceCue))]
        internal static CodeList PlayInterfaceCue_Transpiler(CodeList code, ILGenerator gen)
        {
            var codeList = code.ToList();

            // Original: soundBank.PlayCue(sCueName)
            // Modified: (local1 = GetEffectSoundBank(sCueName)) != null ? local1.PlayCue(sCueName) : soundBank.PlayCue(sCueName)

            Label skipVanillaBank = gen.DefineLabel();
            Label doVanillaBank = gen.DefineLabel();
            LocalBuilder modBank = gen.DeclareLocal(typeof(SoundBank));

            MethodInfo target = typeof(SoundBank).GetMethod("PlayCue", new Type[] { typeof(string) });

            var insertBefore = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Call, typeof(PatchHelper).GetMethod("GetEffectSoundBank")),
                new CodeInstruction(OpCodes.Stloc_S, modBank.LocalIndex),
                new CodeInstruction(OpCodes.Ldloc_S, modBank.LocalIndex),
                new CodeInstruction(OpCodes.Brfalse, doVanillaBank),
                new CodeInstruction(OpCodes.Ldloc_S, modBank.LocalIndex),
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Call, typeof(PatchHelper).GetMethod("GetCueName")),
                new CodeInstruction(OpCodes.Call, target),
                new CodeInstruction(OpCodes.Br, skipVanillaBank),
                new CodeInstruction(OpCodes.Nop).WithLabels(doVanillaBank)
            };

            var insertAfter = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Nop).WithLabels(skipVanillaBank)
            };

            return codeList.InsertAroundMethod(target, insertBefore, insertAfter, editsReturnValue: true);
        }

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(SoundSystem.PlayTrackableInterfaceCue))]
        internal static CodeList PlayTrackableInterfaceCue_Transpiler(CodeList code, ILGenerator gen)
        {
            var codeList = code.ToList();

            // Original: soundBank.GetCue(sCueName)
            // Modified: (local1 = GetEffectSoundBank(sCueName)) != null ? local1.GetCue(sCueName) : soundBank.GetCue(sCueName)

            Label skipVanillaBank = gen.DefineLabel();
            Label doVanillaBank = gen.DefineLabel();
            LocalBuilder modBank = gen.DeclareLocal(typeof(SoundBank));

            MethodInfo target = typeof(SoundBank).GetMethod("GetCue", new Type[] { typeof(string) });

            var insertBefore = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Call, typeof(PatchHelper).GetMethod("GetEffectSoundBank")),
                new CodeInstruction(OpCodes.Stloc_S, modBank.LocalIndex),
                new CodeInstruction(OpCodes.Ldloc_S, modBank.LocalIndex),
                new CodeInstruction(OpCodes.Brfalse, doVanillaBank),
                new CodeInstruction(OpCodes.Ldloc_S, modBank.LocalIndex),
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Call, typeof(PatchHelper).GetMethod("GetCueName")),
                new CodeInstruction(OpCodes.Call, target),
                new CodeInstruction(OpCodes.Br, skipVanillaBank),
                new CodeInstruction(OpCodes.Nop).WithLabels(doVanillaBank)
            };

            var insertAfter = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Nop).WithLabels(skipVanillaBank)
            };

            return codeList.InsertAroundMethod(target, insertBefore, insertAfter, editsReturnValue: true);
        }

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(SoundSystem.PlayCue), typeof(string), typeof(Vector2))]
        internal static CodeList PlayCue_0_Transpiler(CodeList code, ILGenerator gen)
        {
            return PlayTrackableInterfaceCue_Transpiler(code, gen);
        }

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(SoundSystem.PlayCue), typeof(string), typeof(TransformComponent))]
        internal static CodeList PlayCue_1_Transpiler(CodeList code, ILGenerator gen)
        {
            return PlayTrackableInterfaceCue_Transpiler(code, gen);
        }

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(SoundSystem.PlayCue), typeof(string), typeof(Vector2), typeof(float))]
        internal static CodeList PlayCue_2_Transpiler(CodeList code, ILGenerator gen)
        {
            return PlayTrackableInterfaceCue_Transpiler(code, gen);
        }

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(SoundSystem.PlayCue), typeof(string), typeof(IEntity), typeof(bool), typeof(bool))]
        internal static CodeList PlayCue_3_Transpiler(CodeList code, ILGenerator gen)
        {
            return PlayTrackableInterfaceCue_Transpiler(code, gen);
        }

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(SoundSystem.ReadySongInCue))]
        internal static CodeList ReadySongInCue_Transpiler(CodeList code, ILGenerator gen)
        {
            var codeList = code.ToList();

            // Original: musicBank.GetCue(sCueName)
            // Modified: (local1 = GetMusicSoundBank(sCueName)) != null ? local1.GetCue(sCueName) : soundBank.GetCue(sCueName)

            Label skipVanillaBank = gen.DefineLabel();
            Label doVanillaBank = gen.DefineLabel();
            LocalBuilder modBank = gen.DeclareLocal(typeof(SoundBank));

            MethodInfo target = typeof(SoundBank).GetMethod("GetCue", new Type[] { typeof(string) });

            var insertBefore = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Call, typeof(PatchHelper).GetMethod("GetMusicSoundBank")),
                new CodeInstruction(OpCodes.Stloc_S, modBank.LocalIndex),
                new CodeInstruction(OpCodes.Ldloc_S, modBank.LocalIndex),
                new CodeInstruction(OpCodes.Brfalse, doVanillaBank),
                new CodeInstruction(OpCodes.Ldloc_S, modBank.LocalIndex),
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Call, typeof(PatchHelper).GetMethod("GetCueName")),
                new CodeInstruction(OpCodes.Call, target),
                new CodeInstruction(OpCodes.Br, skipVanillaBank),
                new CodeInstruction(OpCodes.Nop).WithLabels(doVanillaBank)
            };

            var insertAfter = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Nop).WithLabels(skipVanillaBank)
            };

            return codeList.InsertAroundMethod(target, insertBefore, insertAfter, editsReturnValue: true);
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(SoundSystem.PlaySong))]
        internal static void PlaySong_Prefix(ref string sSongName, bool bFadeIn)
        {
            var redirects = Globals.ModManager.Library.VanillaMusicRedirects;
            string audioIDToUse = sSongName;

            if (!audioIDToUse.StartsWith("GS_") && redirects.ContainsKey(audioIDToUse))
                audioIDToUse = redirects[audioIDToUse];

            sSongName = audioIDToUse;
        }

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(SoundSystem.PlaySong))]
        internal static CodeList PlaySong_Transpiler(CodeList code, ILGenerator gen)
        {
            return ReadySongInCue_Transpiler(code, gen);
        }

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(SoundSystem.PlayMixCues))]
        internal static CodeList PlayMixCues_Transpiler(CodeList code, ILGenerator gen)
        {
            var codeList = code.ToList();

            Label skipVanillaBank_one = gen.DefineLabel();
            Label doVanillaBank_one = gen.DefineLabel();
            LocalBuilder modBank_one = gen.DeclareLocal(typeof(SoundBank));

            Label skipVanillaBank_two = gen.DefineLabel();
            Label doVanillaBank_two = gen.DefineLabel();
            LocalBuilder modBank_two = gen.DeclareLocal(typeof(SoundBank));

            MethodInfo target = typeof(SoundBank).GetMethod("GetCue", new Type[] { typeof(string) });

            var insertBefore_one = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Call, typeof(PatchHelper).GetMethod("GetMusicSoundBank")),
                new CodeInstruction(OpCodes.Stloc_S, modBank_one.LocalIndex),
                new CodeInstruction(OpCodes.Ldloc_S, modBank_one.LocalIndex),
                new CodeInstruction(OpCodes.Brfalse, doVanillaBank_one),
                new CodeInstruction(OpCodes.Ldloc_S, modBank_one.LocalIndex),
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Call, typeof(PatchHelper).GetMethod("GetCueName")),
                new CodeInstruction(OpCodes.Call, target),
                new CodeInstruction(OpCodes.Br, skipVanillaBank_one),
                new CodeInstruction(OpCodes.Nop).WithLabels(doVanillaBank_one)
            };

            var insertAfter_one = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Nop).WithLabels(skipVanillaBank_one)
            };

            var insertBefore_two = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Call, typeof(PatchHelper).GetMethod("GetMusicSoundBank")),
                new CodeInstruction(OpCodes.Stloc_S, modBank_two.LocalIndex),
                new CodeInstruction(OpCodes.Ldloc_S, modBank_two.LocalIndex),
                new CodeInstruction(OpCodes.Brfalse, doVanillaBank_two),
                new CodeInstruction(OpCodes.Ldloc_S, modBank_two.LocalIndex),
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Call, typeof(PatchHelper).GetMethod("GetCueName")),
                new CodeInstruction(OpCodes.Call, target),
                new CodeInstruction(OpCodes.Br, skipVanillaBank_two),
                new CodeInstruction(OpCodes.Nop).WithLabels(doVanillaBank_two)
            };

            var insertAfter_two = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Nop).WithLabels(skipVanillaBank_two)
            };

            // Patch both methods
            return codeList
                .InsertAroundMethod(target, insertBefore_two, insertAfter_two, methodIndex: 1, editsReturnValue: true)
                .InsertAroundMethod(target, insertBefore_one, insertAfter_one, editsReturnValue: true);
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(SoundSystem.ChangeSongRegionIfNecessary))]
        internal static bool ChangeSongRegionIfNecessary_Prefix(ref SoundSystem __instance, string sSongName)
        {
            SoundSystem system = __instance;

            var dsxStandbyWaveBanks = s_standbyWaveBanks.GetValue(system) as Dictionary<string, WaveBank>;
            var dssSongRegionMap = s_songRegionMap.GetValue(system) as Dictionary<string, string>;

            var universalMusic = s_universalMusic.GetValue(system) as WaveBank;
            var audioEngine = s_audioEngine.GetValue(system) as AudioEngine;

            bool currentIsModded = ModUtils.SplitAudioID(sSongName, out int entryID, out bool isMusic, out int cueID);

            if (currentIsModded && !isMusic)
                Globals.Logger.Warn($"Trying to play modded audio as music, but the audio isn't music! ID: {sSongName}");

            Mod mod = null;
            AudioEntry entry = null;

            if (currentIsModded)
            {
                Globals.ModManager.Library.TryGetEntry((GrindScriptID.AudioID)entryID, out entry);
                mod = entry.Mod;
            }

            string nextBankName = currentIsModded ? entry.indexedMusicBanks[cueID] : dssSongRegionMap[sSongName];

            WaveBank currentMusicBank = s_musicWaveBank.GetValue(system) as WaveBank;

            if (PatchHelper.IsUniversalMusicBank(nextBankName))
            {
                if (currentIsModded && entry.universalWB == null)
                {
                    Globals.Logger.Error($"{sSongName} requested modded UniversalMusic bank, but the bank does not exist!");
                    return false;
                }

                if (currentMusicBank != null && !PatchHelper.IsUniversalMusicBank(currentMusicBank))
                    system.SetStandbyBank(system.sCurrentMusicWaveBank, currentMusicBank);

                s_musicWaveBank.SetValue(system, currentIsModded ? entry.universalWB : universalMusic);
            }
            else if (system.sCurrentMusicWaveBank != nextBankName)
            {
                if (currentMusicBank != null && !PatchHelper.IsUniversalMusicBank(currentMusicBank) && !currentMusicBank.IsDisposed)
                    system.SetStandbyBank(system.sCurrentMusicWaveBank, currentMusicBank);

                system.sCurrentMusicWaveBank = nextBankName;

                if (dsxStandbyWaveBanks.ContainsKey(nextBankName))
                {
                    s_musicWaveBank.SetValue(system, dsxStandbyWaveBanks[nextBankName]);
                    dsxStandbyWaveBanks.Remove(nextBankName);
                }
                else
                {
                    string root = Path.Combine(Globals.Game.Content.RootDirectory, currentIsModded ? mod.AssetPath : "");

                    s_loadedMusicWaveBank.SetValue(system, new WaveBank(audioEngine, Path.Combine(root, "Sound", $"{nextBankName}.xwb")));
                    s_musicWaveBank.SetValue(system, null);
                }
                system.xMusicVolumeMods.iMusicCueRetries = 0;
                system.xMusicVolumeMods.sSongInWait = sSongName;

                s_checkStandbyBanks.Invoke(system, new object[] { nextBankName });
            }
            else if (PatchHelper.IsUniversalMusicBank(currentMusicBank))
            {
                if (dsxStandbyWaveBanks.ContainsKey(system.sCurrentMusicWaveBank))
                {
                    s_musicWaveBank.SetValue(system, dsxStandbyWaveBanks[system.sCurrentMusicWaveBank]);
                    dsxStandbyWaveBanks.Remove(system.sCurrentMusicWaveBank);
                    return false;
                }

                string root = Path.Combine(Globals.Game.Content.RootDirectory, currentIsModded ? mod.AssetPath : "");
                string bankToUse = currentIsModded ? nextBankName : system.sCurrentMusicWaveBank;

                s_loadedMusicWaveBank.SetValue(system, new WaveBank(audioEngine, Path.Combine(root, "Sound", bankToUse + ".xwb")));
                s_musicWaveBank.SetValue(system, null);

                system.xMusicVolumeMods.iMusicCueRetries = 0;
                system.xMusicVolumeMods.sSongInWait = sSongName;
            }

            return false; // Never returns control to original
        }
    }
}
