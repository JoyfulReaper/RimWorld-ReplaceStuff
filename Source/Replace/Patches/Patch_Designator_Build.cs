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
using System;
using Verse;

namespace Replace_Stuff.Replace.Patches;

/// <summary>
/// Intercepts build designations and converts eligible build orders
/// into replacement operations.
/// </summary>
/// <remarks>
/// When the player places a build designator over an existing
/// structure, this patch checks whether the target can be replaced
/// with the selected material.
///
/// If a valid replacement target is found,
/// <see cref="ReplacementHandler"/> performs the replacement and
/// vanilla blueprint placement is skipped.
/// </remarks>
[HarmonyPatch(typeof(Designator_Build), nameof(RimWorld.Designator_Build.DesignateSingleCell))]
internal class Patch_Designator_Build
{
    /// <summary>
    /// Examines the designated cell for replaceable structures before
    /// vanilla blueprint placement occurs.
    /// </summary>
    /// <param name="__instance">
    /// The active build designator.
    /// </param>
    /// <param name="c">
    /// The cell being designated for construction.
    /// </param>
    /// <param name="___entDef">
    /// The definition being placed.
    /// </param>
    /// <param name="___placingRot">
    /// The placement rotation selected by the player.
    /// </param>
    /// <returns>
    /// <see langword="false"/> if a replacement operation was
    /// performed; otherwise, <see langword="true"/> to allow
    /// vanilla placement logic to continue.
    /// </returns>
    public static bool Prefix(Designator_Build __instance, IntVec3 c, BuildableDef ___entDef, Rot4 ___placingRot)
    {
        if (__instance is null || ___entDef is not ThingDef thingDef)
            return true;

        // Optimized search for replaceable items
        if (c.GetThingList(__instance.Map).Count == 0)
            return true;

        if (thingDef.MadeFromStuff && __instance.StuffDef is null)
        {
            return true;
        }

        // Skip if GodMode or no work needed
        if (DebugSettings.godMode || ___entDef.GetStatValueAbstract(StatDefOf.WorkToBuild, __instance.StuffDef) == 0f)
            return true;

        // Handle door rotation
        if (typeof(Building_Door).IsAssignableFrom(thingDef.thingClass))
            ___placingRot = DoorUtility.DoorRotationAt(c, __instance.Map, thingDef.building.preferConnectingToFences);

        var stuff = __instance.StuffDef;
        var thingToReplace = ReplacementCandidateChecker.FindReplacementTarget(
            __instance.Map, c, ___placingRot, thingDef, stuff);

        if (thingToReplace == null)
            return true;

        ReplacementHandler.ExecuteReplacement(thingToReplace, stuff);
        return false; // Replacement executed, skip vanilla placement
    }
}

/// <summary>
/// Tracks whether execution is currently occurring within
/// <see cref="Designator_Build.CanDesignateCell(IntVec3)"/>.
/// </summary>
/// <remarks>
/// Some replacement-related logic behaves differently depending on
/// whether it is being evaluated during designation validation or
/// during actual placement. A depth counter is used instead of a
/// boolean to safely support nested calls.
/// </remarks>
internal static class DesignatorContext
{
    /// <summary>
    /// Gets a value indicating whether build designation validation
    /// is currently in progress.
    /// </summary>
    private static int _depth;

    /// <summary>
    /// Enters a build designation validation scope.
    /// </summary>    
    public static bool IsInBuildDesignation => 
        _depth > 0;

    /// <summary>
    /// Enters a build designation validation scope.
    /// </summary>
    public static void Enter() => _depth++;

    /// <summary>
    /// Exits a build designation validation scope.
    /// </summary>
    public static void Exit() => _depth = Math.Max(0, _depth - 1);
}

/// <summary>
/// Maintains <see cref="DesignatorContext"/> state while
/// <see cref="Designator_Build.CanDesignateCell(IntVec3)"/>
/// is executing.
/// </summary>
/// <remarks>
/// The prefix enters a designation-validation scope and the postfix
/// exits it, allowing replacement logic to determine whether it is
/// running during build designation checks.
/// </remarks>
//public override AcceptanceReport CanDesignateCell(IntVec3 c)
[HarmonyPatch(typeof(Designator_Build), "CanDesignateCell")]
internal static class Patch_DesignatorContext
{
    public static void Prefix() =>
        DesignatorContext.Enter();


    public static void Finalizer() =>
        DesignatorContext.Exit();

}