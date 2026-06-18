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

namespace Replace_Stuff.CoolersOverWalls;

using RimWorld;
using Verse;

 [DefOf]
public static class OverWallDef
{
    public static ThingDef Cooler_Over;
    
    public static ThingDef Cooler_Over2W;

    public static ThingDef Vent_Over;

    public static ThingDef Vent_Over2W;

    public static ThingDef Vent;//not in vanilla

    public static bool IsOverWall(this BuildableDef bdef)
    {
        return bdef == Cooler_Over ||
            bdef == Cooler_Over2W || 
            bdef == Vent_Over || 
            bdef == Vent_Over2W;
    }

    public static bool IsWall(this BuildableDef bdef)
    {
        //return bdef == ThingDefOf.Wall || bdef.IsSmoothed;//Just IsSmoothed doesn't account for modded walls
        return bdef is ThingDef def 
            && def.coversFloor && 
            def.holdsRoof && 
            def.passability == Traversability.Impassable &&
            (def.building?.canBuildNonEdificesUnder ?? true);
    }
}