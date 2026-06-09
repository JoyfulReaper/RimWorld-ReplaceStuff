/*
 * REPLACE STUFF: Perfomance Edition
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

namespace Replace_Stuff.Replace;

/// <summary>
/// Patches the designator build system to support automatic building replacement.
/// </summary>
[HarmonyPatch(typeof(Designator_Build), nameof(Designator_Build.DesignateSingleCell))]
class InterceptDesignator_Build
{
    /// <summary>
    /// Intercepts the build command to check for existing structures that can be replaced.
    /// </summary>
    /// <param name="__instance">The current build designator instance.</param>
    /// <param name="c">The cell coordinate where the player is attempting to build.</param>
    /// <param name="___entDef">The definition of the building being placed.</param>
    /// <param name="___placingRot">The rotation of the building being placed.</param>
    /// <returns>
    /// <c>false</c> if a replacement was performed (skipping vanilla building), 
    /// <c>true</c> if vanilla building behavior should proceed.
    /// </returns>
    public static bool Prefix(Designator_Build __instance, IntVec3 c, BuildableDef ___entDef, Rot4 ___placingRot)
    {
#if DEBUG
        System.Diagnostics.Debugger.Break();
#endif

        if (__instance is null || ___entDef is null) return true;

        if (___entDef is not ThingDef thingDef)
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

        // Optimized search for replaceable items
        var replaceables = c.GetThingList(__instance.Map);
        if (replaceables.Count == 0)
            return true;

        Thing thingToReplace = null;

        for (int i = 0; i < replaceables.Count; i++)
        {
            var replaceable = replaceables[i];

            if (replaceable.Rotation != ___placingRot) continue;
            if (!Designator_ReplaceStuff.CanReplaceStuffFor(__instance.StuffDef, replaceable, thingDef)) continue;

            // Priority: Blueprints and Frames
            if (replaceable is Blueprint_Build || replaceable is Frame)
            {
                thingToReplace = replaceable;
                break; // Found the best target, stop searching
            }

            // Fallback for regular buildings
            if (thingToReplace is null)
            {
                thingToReplace = replaceable;
            }
        }

        if (thingToReplace == null)
            return true;

        ReplaceHandler.ExecuteReplacement(thingToReplace, __instance.StuffDef);
        return false;
    }
}