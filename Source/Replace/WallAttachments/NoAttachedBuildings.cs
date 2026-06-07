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
    /// Prevents replacement frames from reporting attached buildings.
    /// </summary>
    /// <remarks>
    /// Attached structures should remain associated with the original
    /// building being replaced rather than the temporary construction
    /// frame. Returning an empty list avoids placement conflicts.
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