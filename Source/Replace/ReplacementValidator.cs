// TODO: NAMING: We have two classes named ReplacementValidator in two different namespaces. Verify and rename one.
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

using Replace_Stuff.NewThing;
using RimWorld;
using System.Collections.Generic;
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
    /// Caches the allowed construction materials for each
    /// buildable definition to avoid repeated enumeration.
    /// </summary>
    private static readonly Dictionary<BuildableDef, HashSet<ThingDef>> _allowedStuffCache = new();

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
            if (IsValidReplacement(stuff, things[i]))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Determines whether the specified <see cref="Thing"/> can be
    /// replaced using the supplied construction material.
    /// </summary>
    /// <param name="replacementStuff">
    /// The material that will be used for the replacement.
    /// </param>
    /// <param name="thing">
    /// The existing blueprint, frame, or completed structure
    /// being evaluated.
    /// </param>
    /// <param name="matchDef">
    /// Optional buildable definition that the replacement target
    /// must match. If specified, only Things that resolve to this
    /// buildable definition are considered valid.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the Thing is a valid replacement
    /// candidate for the specified material; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// Validation includes:
    /// <list type="bullet">
    /// <item><description>The Thing belongs to the player.</description></item>
    /// <item><description>The Thing represents a replaceable blueprint, frame, or structure.</description></item>
    /// <item><description>The replacement would actually change the construction material.</description></item>
    /// <item><description>The underlying buildable definition matches <paramref name="matchDef"/>, if supplied.</description></item>
    /// <item><description>The replacement can legally exist on the current terrain.</description></item>
    /// <item><description>The Thing is not already being replaced.</description></item>
    /// <item><description>The selected material is allowed for the target buildable definition.</description></item>
    /// </list>
    /// Allowed stuff definitions are cached to avoid repeated
    /// enumeration of <see cref="GenStuff.AllowedStuffsFor(BuildableDef)"/>.
    /// </remarks>
    public static bool IsValidReplacement(ThingDef replacementStuff, Thing thing, ThingDef matchDef = null)
    {
        if (replacementStuff is null || thing is null)
            return false;

        // Can't replace enemy items
        if (thing.Faction != Faction.OfPlayer && thing.Faction != null)
            return false;

        if (thing is Blueprint bp)
        {
            if (bp.EntityToBuildStuff() == replacementStuff)
                return false;
        }
        else if (thing is Frame frame)
        {
            if (frame.EntityToBuildStuff() == replacementStuff)
                return false;
        }
        else if (thing.def.HasReplacementFrame())
        {
            if (thing.Stuff == replacementStuff)
                return false;
        }
        else
        {
            return false; // Not a replaceable structure type
        }

        var buildableDef = GenConstruct.BuiltDefOf(thing.def);
        if (matchDef != null && buildableDef != matchDef)
            return false;

        if (!_allowedStuffCache.TryGetValue(buildableDef, out var allowedStuffSet))
        {
            allowedStuffSet = new HashSet<ThingDef>(GenStuff.AllowedStuffsFor(buildableDef));
            _allowedStuffCache[buildableDef] = allowedStuffSet;
        }

        if (!allowedStuffSet.Contains(replacementStuff))
            return false;

        if (!GenConstruct.CanBuildOnTerrain(buildableDef, thing.Position, thing.Map, thing.Rotation, thing, replacementStuff))
            return false;

        if (thing.BeingReplacedByNewThing() != null)
            return false;

        return true;
    }
}
