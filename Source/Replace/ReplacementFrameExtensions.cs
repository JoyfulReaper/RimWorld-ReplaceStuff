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

using Verse;

namespace Replace_Stuff.Replace
{
    internal static class ReplacementFrameExtensions
    {
        /// <summary>Checks if the provided building Def has a registered replacement frame.</summary>
        public static bool HasReplacementFrame(this ThingDef def)
        {
            if (def is null)
                return false;

            return ReplacementFrameDefGenerator.BuildingToFrameMap.ContainsKey(def);
        }

        /// <summary>Checks if a Def is a replacement frame.</summary>
        public static bool IsReplacementFrame(this ThingDef def) =>
            def?.thingClass == typeof(ReplacementFrame);
    }
}