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

namespace Replace_Stuff.Replace;

/// <summary>
/// Provides helper methods for determining whether replacement
/// operations are valid.
/// </summary>
/// <remarks>
/// These checks are used by replacement designators and related
/// systems to determine whether a cell contains a compatible
/// replacement target.
/// </remarks>
internal static class ReplacementValidator
{
    /// <summary>
    /// Determines whether the specified map cell contains at least
    /// one Thing that can be replaced using the given material.
    /// </summary>
    /// <param name="stuff">
    /// The material definition to test as the replacement stuff.
    /// </param>
    /// <param name="cell">
    /// The map cell being examined.
    /// </param>
    /// <param name="map">
    /// The map containing the target cell.
    /// </param>
    /// <returns>
    /// <c>true</c> if any Thing in the cell can be replaced using
    /// the specified material; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsReplacable(ThingDef stuff, IntVec3 cell, Map map)
    {
        var things = cell.GetThingList(map);
        for (int i = 0; i < things.Count; i++)
        {
            if (Designator_ReplaceStuff.CanReplaceThingWithStuff(stuff, things[i]))
            {
                return true;
            }
        }
        return false;
    }
}