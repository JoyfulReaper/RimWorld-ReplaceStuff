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

using Replace_Stuff.Replace;
using RimWorld;
using Verse;

namespace Replace_Stuff.Utilities;

public static class InstantReplaceUtility
{
    public static Thing InstantReplace(Thing oldThing, ThingDef stuffDef, Pawn worker = null, Faction faction = null)
    {
        Thing newThing = ThingMaker.MakeThing(oldThing.def, stuffDef);
        GenSpawn.Spawn(newThing, oldThing.Position, oldThing.Map, oldThing.Rotation, WipeMode.Vanish);
        ReplaceFrame.FinalizeReplace(oldThing, newThing, worker, faction);

        return newThing;
    }
}
