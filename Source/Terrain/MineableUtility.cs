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


using RimWorld;
using Verse;

namespace Replace_Stuff.Terrain;

static class MineableUtility
{
    public static bool IsMineableRock(this Thing t) =>
        IsMineableRock(t.def);

    public static bool IsMineableRock(this ThingDef td)
    {
        return td.mineable && !td.IsSmoothed;
    }

    public static bool IsBlockingRock(this Thing t, Thing placedThing)
        => IsBlockingRock(t.def, placedThing.def);

    public static bool IsBlockingRock(this ThingDef td, BuildableDef placingDef)
    {
        //This checks ForceAllow, but not AllowsPlacing, since AllowsPlacing defaults to true, and PlaceWorks like ShowFacilites would be true.
        return td.IsMineableRock() && !placingDef.ForceAllowPlaceOver(td);
    }
}



[DefOf]
public static class ConceptDefOf
{
    public static ConceptDef BuildersTryMine;
}