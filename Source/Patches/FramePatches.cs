/*
 * REPLACE STUFF: Perfomance Edition
 * * * Part of this code is based on Replace Stuff
 * Copyright (c) 2025 Alex Tearse-Doyle
 * Licensed under the MIT License.
 *
 * Modified by Kyle Givler
 * Copyright (c) 2026 Kyle Givler
 * Licensed under the MIT License.
 */

using HarmonyLib;
using Replace_Stuff.Replace;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Replace_Stuff.Patches;

/// <summary>
/// Intercepts requests for material costs to ensure the game uses 
/// <see cref="ReplaceFrame.TotalMaterialCost"/> for replacement tasks.
/// </summary>
[HarmonyPatch(typeof(Frame), nameof(Frame.TotalMaterialCost))]
internal static class Frame_TotalMaterialCost_Patch
{
    public static bool Prefix(Frame __instance, ref List<ThingDefCountClass> __result)
    {
        if (__instance is ReplaceFrame rf)
        {
            __result = rf.TotalMaterialCost();
            return false;
        }
        return true;
    }
}

/// <summary>
/// Redirects construction completion to <see cref="ReplaceFrame.CompleteConstruction"/>,
/// handling the destruction of the old object and spawning of the new one.
/// </summary>
[HarmonyPatch(typeof(Frame), nameof(Frame.CompleteConstruction))]
internal static class Frame_CompleteConstruction_Patch
{
    public static bool Prefix(Frame __instance, Pawn worker)
    {
#if DEBUG
        System.Diagnostics.Debugger.Break();
#endif

        if (__instance is ReplaceFrame rf)
        {
            rf.CompleteConstruction(worker);
            return false;
        }
        return true;
    }
}

/// <summary>
/// Redirects construction failure to <see cref="ReplaceFrame.FailConstruction"/>,
/// ensuring partial material refunds for failed replacements.
/// </summary>
[HarmonyPatch(typeof(Frame), nameof(Frame.FailConstruction))]
internal static class Frame_FailConstruction_Patch
{
    public static bool Prefix(Frame __instance, Pawn worker)
    {
        if (__instance is ReplaceFrame rf)
        {
            rf.FailConstruction(worker);
            return false;
        }
        return true;
    }
}

/// <summary>
/// Updates the work progress bar by intercepting the getter for <see cref="Frame.WorkToBuild"/>,
/// returning the combined deconstruction and construction labor required for replacements.
/// </summary>
[HarmonyPatch(typeof(Frame), nameof(Frame.WorkToBuild), MethodType.Getter)]
internal static class Frame_WorkToBuild_Patch
{
    public static bool Prefix(Frame __instance, ref float __result)
    {
        if (__instance is ReplaceFrame rf)
        {
            __result = rf.WorkToBuild;
            return false;
        }
        return true;
    }
}