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
using Verse;
using System.Collections.Generic;
using System.Reflection;

namespace Replace_Stuff.CoolersOverWalls;


[HarmonyPatch]
static class Patch_PlaceWorker_AllowsPlacing
{
    static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(PlaceWorker_Cooler), "AllowsPlacing");
        yield return AccessTools.Method(typeof(PlaceWorker_Vent), "AllowsPlacing");
    }

    static bool Prefix(BuildableDef def, ref AcceptanceReport __result)
    {
        if (OverWallDef.IsOverWall(def))
        {
            __result = true;
            return false; // Skip vanilla placement check
        }

        return true; // Let vanilla run for normal stuff
    }
}

// The orginal patch targets everything, not just the added coolers and vents
// I don't think this was intenonal
// [HarmonyPatch(typeof(PlaceWorker_Cooler), "AllowsPlacing")]
// class AllowBuildPlugged
// {
// 	//public override AcceptanceReport AllowsPlacing(BuildableDef def, IntVec3 center, Rot4 rot, Map map, Thing thingToIgnore = null)
// 	public static bool Prefix(ref AcceptanceReport __result)
// 	{
// 		__result = true;
// 		return false;
// 	}
// }
// [HarmonyPatch(typeof(PlaceWorker_Vent), "AllowsPlacing")]
// class AllowBuildPlugged_Vent
// {
// 	//public override AcceptanceReport AllowsPlacing(BuildableDef def, IntVec3 center, Rot4 rot, Map map, Thing thingToIgnore = null)
// 	public static bool Prefix(ref AcceptanceReport __result)
// 	{
// 		__result = true;
// 		return false;
// 	}
// }
