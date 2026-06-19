/*
 * REPLACE STUFF: Performance Edition
 * 
 * 
 * Part of this code is based on Replace Stuff
 * Copyright (c) 2025 Alex Tearse-Doyle
 * Licensed under the MIT License.
 *
 * Modified by Kyle Givler
 * Copyright (c) 2026 Kyle Givler
 * Licensed under the MIT License.
 */

using HarmonyLib;
using Replace_Stuff.Utilities;
using RimWorld;
using Verse;

namespace Replace_Stuff.Replace.Patches;

[HarmonyPatch(typeof(GenConstruct), nameof(GenConstruct.BlocksConstruction))]
public static class GenConstruct_BlocksConstruction
{
    public static bool Prefix(Thing constructible, Thing t, ref bool __result)
    {
        if (constructible is Blueprint_Build bp)
        {
            var targetDef = t.def.entityDefToBuild ?? t.def;
            if (bp.def.entityDefToBuild == targetDef && bp.stuffToUse == t.Stuff)
            {
                __result = true;
                return false;
            }
        }
        return true;
    }

    [HarmonyPriority(Priority.Last)]
    public static void Postfix(Thing constructible, Thing t, ref bool __result)
    {
        if (!__result)
            return;

        if (constructible is ReplacementFrame frame)
        {
            if (frame.TargetThing == t)
            {
                RSLog.Debug($"BlocksConstruction: Allowing Frame over target {t.Label}");
                __result = false;
                return;
            }
        }

        if (constructible is Blueprint_Build bp)
        {
            var builtDef = bp.def.entityDefToBuild as ThingDef;
            if (ReplacementCandidateChecker.IsValidReplacement(bp.stuffToUse, t, builtDef))
            {
                RSLog.Debug($"BlocksConstruction: Allowing Blueprint over target {t.Label}");
                __result = false;
                return;
            }
        }
    }
}

[HarmonyPatch(typeof(GenConstruct), nameof(GenConstruct.CanPlaceBlueprintOver))]
public static class GenConstruct_CanPlaceBlueprintOnver
{
    // Can place new over old?
    public static bool Prefix(BuildableDef newDef, ThingDef oldDef, ThingDef newStuff, ThingDef oldStuff, ref bool __result)
    {
        // Walls
        if (newDef != ThingDefOf.Wall || oldDef != ThingDefOf.Wall)
            return true;

        // If the materials are the same, return false/blocked
        if (newStuff == oldStuff)
        {
            __result = false;
            return false;
        }

        // materials are different, let the original logic run
        return true;
    }
}

[HarmonyPatch(typeof(GenConstruct), nameof(GenConstruct.CanReplace))]
public static class GenConstruct_CanReplace
{
    public static void Postfix(
        ref bool __result,
        BuildableDef placing,
        BuildableDef existing,
        ThingDef placingStuff = null,
        ThingDef existingStuff = null)
    {
        // If it's already allowed by vanilla or another patch, leave it alone.
        if (__result)
            return;

        if (placing is not ThingDef placingDef || existing is not ThingDef existingDef)
            return;

        if (!placingDef.MadeFromStuff)
            return;

        var placingBuilt = placingDef.entityDefToBuild ?? placingDef;
        var existingBuilt = existingDef.entityDefToBuild ?? existingDef;

        // Ensure we are replacing the exact same logical building type
        if (placingBuilt != existingBuilt)
            return;

        // Handle missing material context
        // If the call site didn't provide the material context, we cannot safely 
        // determine if this is a valid material upgrade. Do not force true.
        if (placingStuff == null || existingStuff == null)
            return;

        // Enforce strict material differences
        if (placingStuff == existingStuff)
            return;

        // At this stage:
        // - It's the same logical building
        // - It's made of stuff
        // - We know exactly what stuff both use
        // - The materials are definitively different
        __result = true;
    }
}

/// <summary>
/// Converts vanilla replacement blueprints into replacement frames when
/// designation bypasses <see cref="Designator_Build.DesignateSingleCell"/>.
/// </summary>
[HarmonyPatch(typeof(GenConstruct), nameof(GenConstruct.PlaceBlueprintForBuild))]
public static class GenConstruct_PlaceBlueprintForBuild_Replace
{
    public static bool Prefix(
        BuildableDef sourceDef,
        IntVec3 center,
        Map map,
        Rot4 rotation,
        Faction faction,
        ThingDef stuff,
        bool sendBPSpawnedSignal,
        ref Blueprint_Build __result)
    {
        if (faction != Faction.OfPlayer || sourceDef is not ThingDef thingDef)
            return true;

        if (DebugSettings.godMode || sourceDef.GetStatValueAbstract(StatDefOf.WorkToBuild, stuff) == 0f)
            return true;

        if (thingDef.MadeFromStuff && stuff == null)
            return true;

        var placingRot = rotation;
        if (typeof(Building_Door).IsAssignableFrom(thingDef.thingClass))
            placingRot = DoorUtility.DoorRotationAt(center, map, thingDef.building.preferConnectingToFences);

        var target = ReplacementCandidateChecker.FindReplacementTarget(
            map, center, placingRot, thingDef, stuff, preferInProgress: false);

        if (target == null)
            return true;

        var frame = ReplacementUtility.SpawnReplacementFrame(target, stuff);
        if (frame == null)
        {
            RSLog.Warning("ReplacementUtility failed to spawn frame, falling back to vanilla.");
            return true;
        }

        // dummy blueprint for game engine
        var placeholder = (Blueprint_Build)ThingMaker.MakeThing(sourceDef.blueprintDef);
        placeholder.stuffToUse = stuff;
        placeholder.SetFactionDirect(faction);

        if (faction != null && sendBPSpawnedSignal)
        {
            QuestUtility.SendQuestTargetSignals(faction.questTags, "PlacedBlueprint", placeholder.Named("SUBJECT"));
        }

        __result = placeholder;
        return false;
    }
}