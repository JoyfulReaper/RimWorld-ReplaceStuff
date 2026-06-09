/*
 * REPLACE STUFF: Performance  Edition
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

namespace Replace_Stuff.Replace;

public class AttachedBuildingData : IExposable
{
    public ThingDef def;
    public ThingDef stuff;

    public IntVec3 position;
    public Rot4 rotation;

    public int hitPoints;
    public Faction faction;

    public QualityCategory? quality;

    public ReplaceData state;

    public void ExposeData()
    {
        Scribe_Defs.Look(ref def, "def");
        Scribe_Defs.Look(ref stuff, "stuff");
        Scribe_Values.Look(ref position, "position");
        Scribe_Values.Look(ref rotation, "rotation");
        Scribe_Values.Look(ref hitPoints, "hitPoints");
    }
}
