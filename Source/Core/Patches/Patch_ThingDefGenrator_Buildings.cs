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

namespace Replace_Stuff.Core.Patches;

/// <summary>
/// Adjusts the rendering altitude of construction frames.
/// By shifting frames to the 'BuildingOnTop' altitude layer, this patch ensures 
/// that construction frames for replacement structures (like doors or walls) 
/// render correctly over the existing building, preventing visual Z-fighting 
/// or flickering while the replacement is being constructed.
/// </summary>
[HarmonyPatch(typeof(ThingDefGenerator_Buildings), "NewFrameDef_Thing")]
public static class FramesDrawOverBuildingsEvenTheDoors
{
    // private static ThingDef NewFrameDef_Thing(ThingDef def)
    public static void Postfix(ThingDef __result)
    {
        __result.altitudeLayer = AltitudeLayer.BuildingOnTop;
    }
}