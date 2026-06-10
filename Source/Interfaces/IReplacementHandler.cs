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

namespace Replace_Stuff.Interfaces;

/// <summary>
/// Allows other mods to implement IReplacementHandler and run custom
/// actions during the replacement process, such as transferring
/// storage contents or preserving custom data.
/// </summary>

// Better name than IReplacementComp
public interface IReplacementHandler
{
    // Runs before old thing is destroyed
    void PreAction(Thing newThing, Thing oldThing);

    // Runs after the new thing is spawned
    void PostAction(Thing newThing, Thing oldThing);
}