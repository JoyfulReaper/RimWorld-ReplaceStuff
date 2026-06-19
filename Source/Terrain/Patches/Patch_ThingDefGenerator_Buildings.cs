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

namespace Replace_Stuff.Terrain.Patches;

//It did create a problem! Putting two edifices in same spot is a problem
//So frames aren't edifices... that shouldn't create a problem, right?
[HarmonyPatch(typeof(ThingDefGenerator_Buildings), "NewFrameDef_Thing")]
public static class FramesArentEdifices
{
    //private static ThingDef NewFrameDef_Thing(ThingDef def)
    public static void Postfix(ThingDef __result)
    {
        __result.building.isEdifice = false;
    }
}

//Can't do BaseBlueprintDef since NewBlueprintDef_Thing overwrites drawerType
[HarmonyPatch(typeof(ThingDefGenerator_Buildings), "NewBlueprintDef_Thing")]
public static class RenderBlueprintOverFog
{
    public static void Postfix(ThingDef __result)
    {
        __result.graphicData.renderQueue = ShowGhostOverFog.queueOverFog;
        __result.graphicData.linkFlags &= ~LinkFlags.Rock;//Prevent blueprint walls from showing links with rocks
    }
}