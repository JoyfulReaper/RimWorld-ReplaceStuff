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

using System.Collections.Generic;
using Verse;

namespace Replace_Stuff.Replace
{
    internal class ReplacementValidator
    {
        public static bool CanReplaceStuffAt(ThingDef stuff, IntVec3 cell, Map map)
        {
            List<Thing> things = cell.GetThingList(map);
            for (int i = 0; i < things.Count; i++)
            {
                if (Designator_ReplaceStuff.CanReplaceStuffFor(stuff, things[i]))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
