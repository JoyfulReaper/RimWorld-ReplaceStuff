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
using Replace_Stuff.Replace;

namespace Replace_Stuff.NewThing.Patches;

[HarmonyPatch(typeof(Frame), nameof(Frame.CompleteConstruction))]
//public void CompleteConstruction(Pawn worker)
public static class Patch_Frame_CompleteConstruction
{
    public static void Prefix(Frame __instance)
    {
        Patch_GenSpawn.IsReplacementInProgress = __instance.TryFindTarget(out Thing replacement);
    }
}

/// <summary>
/// Adjusts the construction work amount required for replacement frames.
/// This patch intercepts the WorkToBuild getter for Frames to ensure that 
/// the total effort includes the labor required to deconstruct the existing 
/// structure being replaced
/// </summary>

[HarmonyPatch(typeof(Frame), "WorkToBuild", MethodType.Getter)]
public static class NewThingDeconstructWork
{
    //public float WorkToBuild
    public static void Postfix(Frame __instance, ref float __result)
    {
        if (__instance is ReplacementFrame)
            return; //ReplaceFrame already has its WorkToBuild set to the correct methods.

        if (__instance.TryFindTarget(out Thing oldThing))
            __result += ReplacementFrame.WorkToDeconstructDef(oldThing.def, oldThing.Stuff);
    }
}