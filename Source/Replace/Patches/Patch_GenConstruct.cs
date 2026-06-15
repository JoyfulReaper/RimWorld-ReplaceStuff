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
using Replace_Stuff.OverMineable;
using RimWorld;
using Verse;

namespace Replace_Stuff.Replace.Patches;


// TODO We are also blocking the user from even placing a blueprint at the UI
// Level so I think this or at least parts of this might not even be needed.
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
            BuildableDef cDef = constructible.def.entityDefToBuild ?? constructible.def;
            BuildableDef tDef = t.def.entityDefToBuild ?? t.def;
            if ((cDef.IsWall() && tDef.IsOverWall()) || (tDef.IsWall() && cDef.IsOverWall()))
            {
                __result = false;
                return;
            }
        }

        // Replacement
        if (constructible is Blueprint_Build bp)
        {
            BuildableDef entDef = bp.def.entityDefToBuild;
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