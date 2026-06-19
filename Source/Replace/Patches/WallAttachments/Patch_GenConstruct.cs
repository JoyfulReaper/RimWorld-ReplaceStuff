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
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Replace_Stuff.Replace.Patches.WallAttachments;

[HarmonyPatch(typeof(GenConstruct), nameof(GenConstruct.GetWallAttachedTo), [typeof(IntVec3), typeof(Rot4), typeof(Map)])]
public static class Patch_GenConstruct_GetWallAttachedTo
{
    /// <summary>
    /// Prevents <see cref="ReplacementFrame"/> objects from being treated as
    /// wall attachments during placement validation.
    /// </summary>
    public static bool Prefix(IntVec3 pos, Rot4 rot, Map map, ref Thing __result)
    {
        IntVec3 c = pos + GenAdj.CardinalDirections[rot.AsInt];

        if (!c.InBounds(map))
        {
            __result = null;
            return false;
        }

        foreach (Thing thing in c.GetThingList(map))
        {
            // --- ignore ReplacementFrames ---
            if (thing is ReplacementFrame)
            {
                continue;
            }
            // ----------------------

            // vanilla logic
            if (GenConstruct.BuiltDefOf(thing.def) is ThingDef { building: not null } thingDef &&
                thingDef.building.supportsWallAttachments)
            {
                __result = thing;
                return false;
            }
        }

        __result = null;
        return false;
    }
}

/// <summary>
/// Prevents replacement frames from reporting wall attachments.
/// </summary>
/// <remarks>
/// A <see cref="ReplacementFrame"/> temporarily occupies the same cell as the
/// building being replaced. Any attached structures (wall lights, vents,
/// coolers, etc.) should remain associated with the original building
/// until replacement is complete.
///
/// Returning an empty list prevents RimWorld from treating the temporary
/// frame as a valid attachment host during placement and validation checks.
/// </remarks>
[HarmonyPatch(typeof(GenConstruct), nameof(GenConstruct.GetAttachedBuildings))]
public static class Patch_GenConstruct
{
    private static List<Thing> emptyList = [];

    /// <summary>
    /// Returns an empty attachment list for replacement frames,
    /// bypassing the original method.
    /// </summary>
    public static bool Prefix(Thing thing, ref List<Thing> __result)
    {
        if (thing is null || !thing.Spawned || thing.Map is null)
        {
            __result = emptyList;
            return false;
        }

        if (thing is ReplacementFrame)
        {
            __result = emptyList;
            return false; // skip original method
        }

        return true;
    }
}