using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using SoG.Modding.Core;
using SoG.Modding.Extensions;
using SoG.Modding.Utils;

namespace SoG.Modding.Patches
{
    using CodeList = System.Collections.Generic.IEnumerable<HarmonyLib.CodeInstruction>;

    /// <summary>
    /// Contains patches for SoundSystem class.
    /// </summary>

    [HarmonyPatch(typeof(SoundSystem))]
    internal static class SoundSystemPatches
    {
        [HarmonyTranspiler]
        [HarmonyPatch("PlayInterfaceCue")]
        internal static CodeList PlayEffectTranspiler(CodeList code, ILGenerator gen)
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

            return PatchTools.InsertAroundMethod(code, target, insertBefore, insertAfter, missingPopIsOk: true);
        }

        [HarmonyTranspiler]
        [HarmonyPatch("PlayTrackableInterfaceCue")]
        internal static CodeList GetEffectTranspiler_0(CodeList code, ILGenerator gen)
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

            return PatchTools.InsertAroundMethod(code, target, insertBefore, insertAfter, missingPopIsOk: true);
        }

        [HarmonyTranspiler]
        [HarmonyPatch("PlayCue", typeof(string), typeof(Vector2))]
        internal static CodeList GetEffectTranspiler_1(CodeList code, ILGenerator gen) => GetEffectTranspiler_0(code, gen);

        [HarmonyTranspiler]
        [HarmonyPatch("PlayCue", typeof(string), typeof(TransformComponent))]
        internal static CodeList GetEffectTranspiler_2(CodeList code, ILGenerator gen) => GetEffectTranspiler_0(code, gen);

        [HarmonyTranspiler]
        [HarmonyPatch("PlayCue", typeof(string), typeof(Vector2), typeof(float))]
        internal static CodeList GetEffectTranspiler_3(CodeList code, ILGenerator gen) => GetEffectTranspiler_0(code, gen);

        [HarmonyTranspiler]
        [HarmonyPatch("PlayCue", typeof(string), typeof(IEntity), typeof(bool), typeof(bool))]
        internal static CodeList GetEffectTranspiler_4(CodeList code, ILGenerator gen) => GetEffectTranspiler_0(code, gen);

        [HarmonyTranspiler]
        [HarmonyPatch("ReadySongInCue")]
        internal static CodeList GetMusicTranspiler_0(CodeList code, ILGenerator gen)
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

            return PatchTools.InsertAroundMethod(code, target, insertBefore, insertAfter, missingPopIsOk: true);
        }

        [HarmonyTranspiler]
        [HarmonyPatch("PlaySong")]
        internal static CodeList GetMusicTranspiler_1(CodeList code, ILGenerator gen) => GetMusicTranspiler_0(code, gen);

        [HarmonyTranspiler]
        [HarmonyPatch("PlayMixCues")]
        internal static CodeList PlayMixTranspiler(CodeList code, ILGenerator gen)
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
            code = PatchTools.InsertAroundMethod(code, target, insertBefore, insertAfter, methodIndex: 1, missingPopIsOk: true);
            return PatchTools.InsertAroundMethod(code, target, insertBefore, insertAfter, missingPopIsOk: true);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SoundSystem), "PlaySong")]
        internal static void OnPlaySong(ref string sSongName, bool bFadeIn)
        {
            var redirects = Globals.API.Registry.Library.VanillaMusicRedirects;
            string audioIDToUse = sSongName;

            if (!audioIDToUse.StartsWith("GS_") && redirects.ContainsKey(audioIDToUse))
                audioIDToUse = redirects[audioIDToUse];

            sSongName = audioIDToUse;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SoundSystem), "ChangeSongRegionIfNecessary")]
        internal static bool OnChangeSongRegionIfNecessary(ref SoundSystem __instance, string sSongName)
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

            bool currentIsModded = Tools.SplitAudioID(sSongName, out int entryID, out bool isMusic, out int cueID);

            if (currentIsModded && !isMusic)
                Globals.Logger.Warn($"Trying to play modded audio as music, but the audio isn't music! ID: {sSongName}");

            ModAudioEntry entry = currentIsModded ? Globals.API.Registry.Library.Audio[entryID] : null;
            string cueName = currentIsModded ? entry.MusicNames[cueID] : sSongName;
            string nextBankName = currentIsModded ? entry.MusicBankNames[cueName] : dssSongRegionMap[sSongName];

            WaveBank currentMusicBank = f_musicWaveBank.GetValue(soundSystem) as WaveBank;

            if (Globals.API.Registry.IsUniversalMusicBank(nextBankName))
            {
                if (currentIsModded && entry.UniversalWB == null)
                {
                    Globals.Logger.Error($"{sSongName} requested modded UniversalMusic bank, but the bank does not exist!");
                    return false;
                }

                if (currentMusicBank != null && !Globals.API.Registry.IsUniversalMusicBank(currentMusicBank))
                    soundSystem.SetStandbyBank(soundSystem.sCurrentMusicWaveBank, currentMusicBank);

                f_musicWaveBank.SetValue(soundSystem, currentIsModded ? entry.UniversalWB : universalMusic);
            }
            else if (soundSystem.sCurrentMusicWaveBank != nextBankName)
            {
                if (currentMusicBank != null && !Globals.API.Registry.IsUniversalMusicBank(currentMusicBank) && !currentMusicBank.IsDisposed)
                    soundSystem.SetStandbyBank(soundSystem.sCurrentMusicWaveBank, currentMusicBank);

                soundSystem.sCurrentMusicWaveBank = nextBankName;

                if (dsxStandbyWaveBanks.ContainsKey(nextBankName))
                {
                    f_musicWaveBank.SetValue(soundSystem, dsxStandbyWaveBanks[nextBankName]);
                    dsxStandbyWaveBanks.Remove(nextBankName);
                }
                else
                {
                    string root = Path.Combine(Globals.Game.Content.RootDirectory, currentIsModded ? entry.Owner.AssetPath : "");

                    f_loadedMusicWaveBank.SetValue(soundSystem, new WaveBank(audioEngine, Path.Combine(root, "Sound", $"{nextBankName}.xwb")));
                    f_musicWaveBank.SetValue(soundSystem, null);
                }
                soundSystem.xMusicVolumeMods.iMusicCueRetries = 0;
                soundSystem.xMusicVolumeMods.sSongInWait = sSongName;
                soundType.GetPrivateInstanceMethod("CheckStandbyBanks").Invoke(soundSystem, new object[] { nextBankName });
            }
            else if (Globals.API.Registry.IsUniversalMusicBank(currentMusicBank))
            {
                if (dsxStandbyWaveBanks.ContainsKey(soundSystem.sCurrentMusicWaveBank))
                {
                    f_musicWaveBank.SetValue(soundSystem, dsxStandbyWaveBanks[soundSystem.sCurrentMusicWaveBank]);
                    dsxStandbyWaveBanks.Remove(soundSystem.sCurrentMusicWaveBank);
                    return false;
                }

                string root = Path.Combine(Globals.Game.Content.RootDirectory, currentIsModded ? entry.Owner.AssetPath : "");
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
