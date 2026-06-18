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

namespace Replace_Stuff.PlaceBridges.Patches;

[HarmonyPatch(typeof(TerrainGrid), nameof(TerrainGrid.RemoveTopLayer))]
public static class DestroyedTerrain
{
    //public void RemoveTopLayer(IntVec3 c, bool doLeavings = true)
    public static void Prefix(TerrainGrid __instance, IntVec3 c, Map ___map)
    {
        CancelAboveBridges.CancelAbove(__instance.TerrainAt(c), DestroyMode.KillFinalize, ___map, c);
    }
}