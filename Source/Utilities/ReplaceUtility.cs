/*
 * REPLACE STUFF: Perfomance Edition
 * Copyright (c) 2026 Kyle Givler
 * Licensed under the MIT License.
 */

using Replace_Stuff.DestroyedRestore;
using Replace_Stuff.Replace;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Replace_Stuff.Utilities;

public static class ReplaceUtility
{
    public static Thing InstantReplace(Thing oldThing, ThingDef newStuff, Pawn worker = null, Faction faction = null)
    {
        if (oldThing is null || oldThing.Destroyed || oldThing.Map is null)
            return null;

        var data = BuildingStateTransfer.Capture(oldThing, new HashSet<int>());
        var newThing = ThingMaker.MakeThing(oldThing.def, newStuff);

        GenSpawn.Spawn(newThing, oldThing.Position, oldThing.Map, oldThing.Rotation, WipeMode.Vanish);
        GenReplace.ApplyReplacementState(oldThing, newThing, data, worker, faction);

        return newThing;
    }
}