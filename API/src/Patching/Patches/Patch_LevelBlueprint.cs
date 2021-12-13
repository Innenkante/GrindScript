using HarmonyLib;
using LevelLoading;
using SoG.Modding.Content;
using SoG.Modding.Extensions;
using System;

namespace SoG.Modding.Patching.Patches
{
    [HarmonyPatch(typeof(LevelBlueprint))]
    internal static class Patch_LevelBlueprint
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(LevelBlueprint.GetBlueprint))]
        internal static bool GetBlueprint_Prefix(ref LevelBlueprint __result, Level.ZoneEnum enZoneToGet)
        {
            Globals.Manager.Library.TryGetEntry(enZoneToGet, out LevelEntry entry);

            if (entry == null)
            {
                return true;
            }

            if (entry.IsVanilla && entry.builder == null)
            {
                return true;  // Go with vanilla method
            }
            else if (entry.IsUnknown)
            {
                __result = new LevelBlueprint();
                __result.CheckForConsistency();

                return false;  // This will either crash the game or softlock it
            }

            LevelBlueprint blueprint = new LevelBlueprint();

            blueprint.CheckForConsistency();

            try
            {
                entry.builder?.Invoke(blueprint);
            }
            catch (Exception e)
            {
                Globals.Logger.Error($"Builder threw an exception for level {enZoneToGet}! Exception: {e}");
                blueprint = new LevelBlueprint();
            }

            blueprint.CheckForConsistency(true);

            // Enforce certain values

            blueprint.enRegion = entry.worldRegion;
            blueprint.enZone = entry.GameID;
            blueprint.sDefaultMusic = ""; // TODO Custom music
            blueprint.sDialogueFiles = ""; // TODO Dialogue Files
            blueprint.sMenuBackground = "bg01_mountainvillage"; // TODO Proper custom backgrounds. Transpiling _Level_Load is a good idea.
            blueprint.sZoneName = ""; // TODO Zone titles

            // Loader setup

            Loader.afCurrentHeightLayers = new float[blueprint.aiLayerDefaultHeight.Length];
            for (int i = 0; i < blueprint.aiLayerDefaultHeight.Length; i++)
                Loader.afCurrentHeightLayers[i] = blueprint.aiLayerDefaultHeight[i];

            Loader.lxCurrentSC = blueprint.lxInvisibleWalls;

            // Return from method

            __result = blueprint;
            return false;
        }

    }
}
