/*
 * REPLACE STUFF: Perfomance Edition
 * 
 * 
 * Part of this code is based on Replace Stuff
 * Copyright (c) 2024 Alex Tearse-Doyle
 * Licensed under the MIT License.
 *
 * Modified by Kyle Givler
 * Copyright (c) 2026 Kyle Givler
 * Licensed under the MIT License.
 */

using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Replace_Stuff.Compatibility
{
#if DEBUG
    [HarmonyPatch(typeof(ThingDef), nameof(ThingDef.SpecialDisplayStats))]
    public static class Patch_ThingDef_SpecialDisplayStats
    {
        // Postfix composes the original result with ours
        public static IEnumerable<StatDrawEntry> Postfix(IEnumerable<StatDrawEntry> __result, StatRequest req, ThingDef __instance)
        {
            // Preserve originals
            foreach (var e in __result) yield return e;

            var report =
$"""
ReplaceTags: {(__instance.replaceTags == null ? "None" : string.Join(", ", __instance.replaceTags))}
""";
            // Category, label, value text, tooltip, priority
            yield return new StatDrawEntry(
                StatCategoryDefOf.Basics,
                "defName",
                __instance.defName,
                report,
                displayPriorityWithinCategory: 999);

        }
    }
#endif
}