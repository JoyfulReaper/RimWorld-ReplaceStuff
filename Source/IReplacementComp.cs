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

using Verse;

namespace Replace_Stuff
{
    /// <summary>
    /// Allows other mods to implement IReplacementComp and run custom
    /// actions during the replacement process, such as transferring
    /// storage contents or preserving custom data.
    /// </summary>
    public interface IReplacementComp
    {
        // Runs before old thing is destroyed
        abstract void PreAction(Thing newThing, Thing oldThing);

        // Runs after the new thing is spawned
        abstract void PostAction(Thing newThing, Thing oldThing);
    }
}