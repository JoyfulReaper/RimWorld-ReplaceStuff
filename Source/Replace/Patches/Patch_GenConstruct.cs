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
using Replace_Stuff.OverWallCoolers;
using Replace_Stuff.Terrain;
using RimWorld;
using Verse;

namespace Replace_Stuff.Replace.Patches;

[HarmonyPatch(typeof(GenConstruct), "BlocksConstruction")]
public static class GenConstruct_BlocksConstruction
{
    public static bool Prefix(Thing constructible, Thing t, ref bool __result)
    {
        // We only care about Blueprints
        if (constructible is Blueprint_Build bp)
        {
            // Wall over a Wall
            if (bp.def.entityDefToBuild == ThingDefOf.Wall && t.def == ThingDefOf.Wall)
            {
                // materials are same
                if (bp.stuffToUse == t.Stuff)
                {
                    __result = true; // block
                    return false;
                }
            }
        }
        return true;
    }

    [HarmonyPriority(Priority.Last)]
    public static void Postfix(Thing constructible, Thing t, ref bool __result)
    {
        // Frame override
        if (__result && t is Frame)
        {
            __result = false;
            return;
        }

        // Mineables
        if (!__result && t.IsBlockingRock(constructible))
        {
            __result = true;
            return;
        }

        // Cooler/Wall
        if (__result)
        {
            var cDef = constructible.def.entityDefToBuild ?? constructible.def;
            var tDef = t.def.entityDefToBuild ?? t.def;
            if ((cDef.IsWall() && tDef.IsOverWall()) || (tDef.IsWall() && cDef.IsOverWall()))
            {
                __result = false;
                return;
            }
        }

        // Replacement
        if (constructible is Blueprint_Build bp)
        {
            var entDef = bp.def.entityDefToBuild;
            if (entDef == null) return;

            if (RimWorld.GenConstruct.CanReplace(entDef, t.def, bp.stuffToUse, t.Stuff))
            {
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