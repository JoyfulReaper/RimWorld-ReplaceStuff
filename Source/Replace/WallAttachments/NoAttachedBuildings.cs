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

namespace Replace_Stuff.Replace.WallAttachments
{
    /// <summary>
    /// Prevents replacement frames from reporting wall attachments.
    /// </summary>
    /// <remarks>
    /// A <see cref="ReplaceFrame"/> temporarily occupies the same cell as the
    /// building being replaced. Any attached structures (wall lights, vents,
    /// coolers, etc.) should remain associated with the original building
    /// until replacement is complete.
    ///
    /// Returning an empty list prevents RimWorld from treating the temporary
    /// frame as a valid attachment host during placement and validation checks.
    /// </remarks>
    [HarmonyPatch(typeof(GenConstruct), nameof(GenConstruct.GetAttachedBuildings))]
    public static class NoAttachedBuildings
    {
        private static List<Thing> emptyList = [];

        /// <summary>
        /// Returns an empty attachment list for replacement frames,
        /// bypassing the original method.
        /// </summary>
        public static bool Prefix(Thing thing, ref List<Thing> __result)
        {
            if (thing is ReplaceFrame)
            {
                __result = emptyList;
                return false;
            }
            return true;
        }
    }
}