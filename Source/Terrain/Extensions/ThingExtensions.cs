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

using System.Linq;
using Verse;


namespace Replace_Stuff.Terrain.Extensions;

public static class ThingExtensions
{
    public static bool IsUnderFog(this Thing thing)
    {
        return IsUnderFog(thing.Position, thing.Rotation, thing.def);
    }

    public static bool IsUnderFog(this IntVec3 center, Rot4 rot, ThingDef thingDef)
    {
        return GenAdj.OccupiedRect(center, rot, thingDef.Size)
            .Any(pos => Find.CurrentMap.fogGrid.IsFogged(pos));
    }
}