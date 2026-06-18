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
using Replace_Stuff.Replace.Patches;
using RimWorld;
using Verse;

namespace Replace_Stuff.NewThing.Patches;

[HarmonyPatch(typeof(GenConstruct), nameof(GenConstruct.NotBlockingAnyInteractionCells))]
public static class ForceAllowInteractionSpot_Patch
{
    public static bool Prefix(ref AcceptanceReport __result)
    {
        if (DesignatorContext.IsInBuildDesignation)
        {
            __result = true;

            return false;
        }

        return true;
    }
}


[HarmonyPatch(typeof(GenConstruct), nameof(GenConstruct.CanPlaceBlueprintAt_NewTemp))]
public static class OverrideInteractionSpotOverlap_Main
{
    public static void Postfix(BuildableDef entDef, IntVec3 center, Rot4 rot, Map map, ref AcceptanceReport __result)
    {
        if (__result.Accepted) return;

        if (entDef is ThingDef newDef)
        {
            bool isReplacement = newDef.TryFindTarget(center, rot, map, out Thing foundThing);

            if (isReplacement)
            {
                Log.Message(
                    $"Replacement detected: " +
                    $"{foundThing.def.defName} -> {newDef.defName}");
                __result = AcceptanceReport.WasAccepted;
            }
            else
            {
                //RSLog.Debug($"Not a replacement: {newDef.defName} at {center}");
            }
        }
    }
}