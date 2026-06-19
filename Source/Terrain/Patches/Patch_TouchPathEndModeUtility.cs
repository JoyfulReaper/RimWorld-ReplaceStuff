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
using System.Linq;
using Verse;
using Verse.AI;

namespace Replace_Stuff.OverMineable.Patches;

//Include blueprints and frames in IsCornerTouchAllowed
//(Frames were included, but ReplaceStuff removes their 'edifice' status so they need to be re-included)
[HarmonyPatch(typeof(TouchPathEndModeUtility), nameof(TouchPathEndModeUtility.IsCornerTouchAllowed_NewTemp))]
public static class IsCornerTouchAllowed
{
    //public static bool IsCornerTouchAllowed(int cornerX, int cornerZ, int adjCardinal1X, int adjCardinal1Z, int adjCardinal2X, int adjCardinal2Z, PathingContext pc)
    public static void Postfix(ref bool __result, IntVec3 adjCardinalX,
        IntVec3 adjCardinalZ,
        PathingContext pc,
        Thing target)
    {
        if (__result)
            return;

        if (target == null)
            return;

        if (pc.map.thingGrid.ThingsListAtFast(target.Position)
            .Any(thing => (thing is Blueprint || thing is Frame) && 
                TouchPathEndModeUtility.MakesOccupiedCellsAlwaysReachableDiagonally(thing.def)))
        {
            __result = true;
            return;
        }
    }
}