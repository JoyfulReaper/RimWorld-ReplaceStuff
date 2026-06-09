/*
 * REPLACE STUFF: Performance  Edition
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

namespace Replace_Stuff.Replace;

/// <summary>
/// Suppresses the dedicated blueprint deconstruction work giver introduced in RimWorld 1.6.
/// </summary>
/// <remarks>
/// RimWorld 1.6 added a dedicated job via <see cref="WorkGiver_DeconstructForBlueprint"/> to handle 
/// deconstruction before blueprint placement. However, because <see cref="WorkGiver_ConstructDeliverResourcesToBlueprints"/> 
/// already naturally processes these requirements, this dedicated check introduces redundant scanning overhead.
/// Short-circuiting this global look-up saves precious tick time in heavily populated or complex colony grids.
/// </remarks>
[HarmonyPatch(typeof(WorkGiver_DeconstructForBlueprint), nameof(WorkGiver_DeconstructForBlueprint.PotentialWorkThingsGlobal))]
public static class SuppressBlueprintDeconstruction
{
    /// <summary>
    /// Forces the global potential work pool to return empty, effectively disabling this specific work scanner.
    /// </summary>
    /// <param name="__result">The intercepted collection of potential target structures.</param>
    /// <returns>Always returns <c>false</c> to entirely skip the vanilla global scanning logic.</returns>
    // public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
    public static bool Prefix(ref IEnumerable<Thing> __result)
    {
        __result = Enumerable.Empty<Thing>();
        return false;
    }
}