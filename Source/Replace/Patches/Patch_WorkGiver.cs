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
using System.Linq;
using Verse;

namespace Replace_Stuff.Replace.Patches;

/// <summary>
/// Disables the dedicated blueprint deconstruction work giver introduced
/// in RimWorld 1.6.
/// </summary>
/// <remarks>
/// RimWorld 1.6 added <see cref="WorkGiver_DeconstructForBlueprint"/> to
/// generate deconstruction jobs for objects blocking blueprint placement.
///
/// This patch prevents the work giver from producing any work targets,
/// allowing blueprint-related deconstruction to be handled through the
/// existing construction workflow instead.
/// </remarks>
[HarmonyPatch(typeof(WorkGiver_DeconstructForBlueprint), nameof(WorkGiver_DeconstructForBlueprint.PotentialWorkThingsGlobal))]
public static class Patch_WorkGiver
{
    /// <summary>
    /// Replaces the global work target list with an empty collection and
    /// skips execution of the original method.
    /// </summary>
    /// <param name="__result">
    /// Receives an empty sequence of potential work targets.
    /// </param>
    /// <returns>
    /// <see langword="false"/> to prevent the original method from
    /// executing.
    /// </returns>
    // public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
    public static bool Prefix(ref IEnumerable<Thing> __result)
    {
        __result = Enumerable.Empty<Thing>();
        return false;
    }
}