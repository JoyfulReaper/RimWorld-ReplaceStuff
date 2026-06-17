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

namespace Replace_Stuff.NewThing
{
    internal static class ThingExtenstions
    {
        public static Thing BeingReplacedByNewThing(this Thing oldThing)
        {
            foreach (IntVec3 checkPos in GenAdj.OccupiedRect(oldThing.Position, oldThing.Rotation, oldThing.def.size))
                foreach (Thing newThing in checkPos.GetThingList(oldThing.Map))
                    if (newThing.CanReplace(oldThing))
                        return newThing;

            return null;
        }
    }
}
