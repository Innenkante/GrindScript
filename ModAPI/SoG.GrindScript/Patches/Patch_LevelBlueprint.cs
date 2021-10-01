using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using LevelLoading;
using SoG.Modding.Extensions;
using SoG.Modding.LibraryEntries;

namespace SoG.Modding.Patches
{
    [HarmonyPatch(typeof(LevelBlueprint))]
    internal static class Patch_LevelBlueprint
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(LevelBlueprint.GetBlueprint))]
        internal static bool GetBlueprint_Prefix(ref LevelBlueprint __result, Level.ZoneEnum enZoneToGet)
        {
            if (!enZoneToGet.IsFromMod())
                return true;

            LevelBlueprint bprint = new LevelBlueprint();

            bprint.CheckForConsistency();

            LevelEntry entry = Globals.ModManager.Library.Levels[enZoneToGet];

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

    }
}
