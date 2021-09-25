using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using SoG.Modding.Utils;
using CodeList = System.Collections.Generic.IEnumerable<HarmonyLib.CodeInstruction>;

namespace SoG.Modding.Patches
{
    /// <summary>
    /// Contains patches for SoundSystem class.
    /// </summary>
    [HarmonyPatch(typeof(SoundSystem))]
    internal static class Patch_SoundSystem
    {
        [HarmonyTranspiler]
        [HarmonyPatch(nameof(SoundSystem.PlayInterfaceCue))]
        internal static CodeList PlayInterfaceCue_Transpiler(CodeList code, ILGenerator gen)
        {
            // Original: soundBank.PlayCue(sCueName)
            // Modified: (local1 = GetEffectSoundBank(sCueName)) != null ? local1.PlayCue(sCueName) : soundBank.PlayCue(sCueName)

            Label skipVanillaBank = gen.DefineLabel();
            Label doVanillaBank = gen.DefineLabel();
            LocalBuilder modBank = gen.DeclareLocal(typeof(SoundBank));

            MethodInfo target = typeof(SoundBank).GetMethod("PlayCue", new Type[] { typeof(string) });

            var insertBefore = new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Call, typeof(HelperCallbacks).GetMethod("GetEffectSoundBank")),
                new CodeInstruction(OpCodes.Stloc_S, modBank.LocalIndex),
                new CodeInstruction(OpCodes.Ldloc_S, modBank.LocalIndex),
                new CodeInstruction(OpCodes.Brfalse, doVanillaBank),
                new CodeInstruction(OpCodes.Ldloc_S, modBank.LocalIndex),
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Call, typeof(HelperCallbacks).GetMethod("GetCueName")),
                new CodeInstruction(OpCodes.Call, target),
                new CodeInstruction(OpCodes.Br, skipVanillaBank),
                new CodeInstruction(OpCodes.Nop).WithLabels(doVanillaBank)
            };

            var insertAfter = new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Nop).WithLabels(skipVanillaBank)
            };

            return PatchUtils.InsertAroundMethod(code, target, insertBefore, insertAfter, usesReturnValue: true);
        }

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(SoundSystem.PlayTrackableInterfaceCue))]
        internal static CodeList PlayTrackableInterfaceCue_Transpiler(CodeList code, ILGenerator gen)
        {
            // Original: soundBank.GetCue(sCueName)
            // Modified: (local1 = GetEffectSoundBank(sCueName)) != null ? local1.GetCue(sCueName) : soundBank.GetCue(sCueName)

            Label skipVanillaBank = gen.DefineLabel();
            Label doVanillaBank = gen.DefineLabel();
            LocalBuilder modBank = gen.DeclareLocal(typeof(SoundBank));

            MethodInfo target = typeof(SoundBank).GetMethod("GetCue", new Type[] { typeof(string) });

            var insertBefore = new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Call, typeof(HelperCallbacks).GetMethod("GetEffectSoundBank")),
                new CodeInstruction(OpCodes.Stloc_S, modBank.LocalIndex),
                new CodeInstruction(OpCodes.Ldloc_S, modBank.LocalIndex),
                new CodeInstruction(OpCodes.Brfalse, doVanillaBank),
                new CodeInstruction(OpCodes.Ldloc_S, modBank.LocalIndex),
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Call, typeof(HelperCallbacks).GetMethod("GetCueName")),
                new CodeInstruction(OpCodes.Call, target),
                new CodeInstruction(OpCodes.Br, skipVanillaBank),
                new CodeInstruction(OpCodes.Nop).WithLabels(doVanillaBank)
            };

            var insertAfter = new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Nop).WithLabels(skipVanillaBank)
            };

            return PatchUtils.InsertAroundMethod(code, target, insertBefore, insertAfter, usesReturnValue: true);
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
            // Original: musicBank.GetCue(sCueName)
            // Modified: (local1 = GetMusicSoundBank(sCueName)) != null ? local1.GetCue(sCueName) : soundBank.GetCue(sCueName)

            Label skipVanillaBank = gen.DefineLabel();
            Label doVanillaBank = gen.DefineLabel();
            LocalBuilder modBank = gen.DeclareLocal(typeof(SoundBank));

            MethodInfo target = typeof(SoundBank).GetMethod("GetCue", new Type[] { typeof(string) });

            var insertBefore = new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Call, typeof(HelperCallbacks).GetMethod("GetMusicSoundBank")),
                new CodeInstruction(OpCodes.Stloc_S, modBank.LocalIndex),
                new CodeInstruction(OpCodes.Ldloc_S, modBank.LocalIndex),
                new CodeInstruction(OpCodes.Brfalse, doVanillaBank),
                new CodeInstruction(OpCodes.Ldloc_S, modBank.LocalIndex),
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Call, typeof(HelperCallbacks).GetMethod("GetCueName")),
                new CodeInstruction(OpCodes.Call, target),
                new CodeInstruction(OpCodes.Br, skipVanillaBank),
                new CodeInstruction(OpCodes.Nop).WithLabels(doVanillaBank)
            };

            var insertAfter = new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Nop).WithLabels(skipVanillaBank)
            };

            return PatchUtils.InsertAroundMethod(code, target, insertBefore, insertAfter, usesReturnValue: true);
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
            Label skipVanillaBank = gen.DefineLabel();
            Label doVanillaBank = gen.DefineLabel();
            LocalBuilder modBank = gen.DeclareLocal(typeof(SoundBank));

            MethodInfo target = typeof(SoundBank).GetMethod("GetCue", new Type[] { typeof(string) });

            var insertBefore = new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Call, typeof(HelperCallbacks).GetMethod("GetMusicSoundBank")),
                new CodeInstruction(OpCodes.Stloc_S, modBank.LocalIndex),
                new CodeInstruction(OpCodes.Ldloc_S, modBank.LocalIndex),
                new CodeInstruction(OpCodes.Brfalse, doVanillaBank),
                new CodeInstruction(OpCodes.Ldloc_S, modBank.LocalIndex),
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Call, typeof(HelperCallbacks).GetMethod("GetCueName")),
                new CodeInstruction(OpCodes.Call, target),
                new CodeInstruction(OpCodes.Br, skipVanillaBank),
                new CodeInstruction(OpCodes.Nop).WithLabels(doVanillaBank)
            };

            var insertAfter = new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Nop).WithLabels(skipVanillaBank)
            };

            // Patch both methods
            code = PatchUtils.InsertAroundMethod(code, target, insertBefore, insertAfter, methodIndex: 1, usesReturnValue: true);
            return PatchUtils.InsertAroundMethod(code, target, insertBefore, insertAfter, usesReturnValue: true);
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(SoundSystem.ChangeSongRegionIfNecessary))]
        internal static bool ChangeSongRegionIfNecessary_Prefix(ref SoundSystem __instance, string sSongName)
        {
            // This will probably cause you brain and eye damage if you read it
            
            SoundSystem soundSystem = __instance;
            TypeInfo soundType = typeof(SoundSystem).GetTypeInfo();

            BindingFlags flag = BindingFlags.NonPublic | BindingFlags.Instance;

            var f_musicWaveBank = soundType.GetField("musicWaveBank", flag);
            var f_loadedMusicWaveBank = soundType.GetField("loadedMusicWaveBank", flag);

            var dsxStandbyWaveBanks = soundType.GetField("dsxStandbyWaveBanks", flag).GetValue(soundSystem) as Dictionary<string, WaveBank>;
            var dssSongRegionMap = soundType.GetField("dssSongRegionMap", flag).GetValue(soundSystem) as Dictionary<string, string>;
            var universalMusic = soundType.GetField("universalMusicWaveBank", flag).GetValue(soundSystem) as WaveBank;
            var audioEngine = soundType.GetField("audioEngine", flag).GetValue(soundSystem) as AudioEngine;

            bool currentIsModded = ModUtils.SplitAudioID(sSongName, out int entryID, out bool isMusic, out int cueID);

            if (currentIsModded && !isMusic)
                Globals.Logger.Warn($"Trying to play modded audio as music, but the audio isn't music! ID: {sSongName}");

            Mod mod = currentIsModded ? Globals.ModManager.Mods[entryID] : null;
            Mod.ModAudio entry = currentIsModded ? mod.Audio : null;
            string nextBankName = currentIsModded ? entry.IndexedMusicBanks[cueID] : dssSongRegionMap[sSongName];

            WaveBank currentMusicBank = f_musicWaveBank.GetValue(soundSystem) as WaveBank;

            if (Globals.ModManager.IsUniversalMusicBank(nextBankName))
            {
                if (currentIsModded && entry.UniversalWB == null)
                {
                    Globals.Logger.Error($"{sSongName} requested modded UniversalMusic bank, but the bank does not exist!");
                    return false;
                }

                if (currentMusicBank != null && !Globals.ModManager.IsUniversalMusicBank(currentMusicBank))
                    soundSystem.SetStandbyBank(soundSystem.sCurrentMusicWaveBank, currentMusicBank);

                f_musicWaveBank.SetValue(soundSystem, currentIsModded ? entry.UniversalWB : universalMusic);
            }
            else if (soundSystem.sCurrentMusicWaveBank != nextBankName)
            {
                if (currentMusicBank != null && !Globals.ModManager.IsUniversalMusicBank(currentMusicBank) && !currentMusicBank.IsDisposed)
                    soundSystem.SetStandbyBank(soundSystem.sCurrentMusicWaveBank, currentMusicBank);

                soundSystem.sCurrentMusicWaveBank = nextBankName;

                if (dsxStandbyWaveBanks.ContainsKey(nextBankName))
                {
                    f_musicWaveBank.SetValue(soundSystem, dsxStandbyWaveBanks[nextBankName]);
                    dsxStandbyWaveBanks.Remove(nextBankName);
                }
                else
                {
                    string root = Path.Combine(Globals.Game.Content.RootDirectory, currentIsModded ? mod.AssetPath : "");

                    f_loadedMusicWaveBank.SetValue(soundSystem, new WaveBank(audioEngine, Path.Combine(root, "Sound", $"{nextBankName}.xwb")));
                    f_musicWaveBank.SetValue(soundSystem, null);
                }
                soundSystem.xMusicVolumeMods.iMusicCueRetries = 0;
                soundSystem.xMusicVolumeMods.sSongInWait = sSongName;
                
                AccessTools.Method(soundType, "CheckStandbyBanks").Invoke(soundSystem, new object[] { nextBankName });
            }
            else if (Globals.ModManager.IsUniversalMusicBank(currentMusicBank))
            {
                if (dsxStandbyWaveBanks.ContainsKey(soundSystem.sCurrentMusicWaveBank))
                {
                    f_musicWaveBank.SetValue(soundSystem, dsxStandbyWaveBanks[soundSystem.sCurrentMusicWaveBank]);
                    dsxStandbyWaveBanks.Remove(soundSystem.sCurrentMusicWaveBank);
                    return false;
                }

                string root = Path.Combine(Globals.Game.Content.RootDirectory, currentIsModded ? mod.AssetPath : "");
                string bankToUse = currentIsModded ? nextBankName : soundSystem.sCurrentMusicWaveBank;

                f_loadedMusicWaveBank.SetValue(soundSystem, new WaveBank(audioEngine, Path.Combine(root, "Sound", bankToUse + ".xwb")));
                f_musicWaveBank.SetValue(soundSystem, null);

                soundSystem.xMusicVolumeMods.iMusicCueRetries = 0;
                soundSystem.xMusicVolumeMods.sSongInWait = sSongName;
            }

            return false; // Never returns control to original
        }
    }
}
