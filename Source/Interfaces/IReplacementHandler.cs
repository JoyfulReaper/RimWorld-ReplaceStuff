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

using Replace_Stuff.Data;
using Verse;

namespace Replace_Stuff.Interfaces;

/// <summary>
/// Allows other mods to implement IReplacementHandler and run custom
/// actions during the replacement process, such as transferring
/// storage contents or preserving custom data.
/// </summary>
public interface IReplacementHandler
{
    // Runs before old thing is destroyed
    void PreAction(ReplacementData data, Thing oldThing, Thing newThing);

    // Runs after the new thing is spawned
    void PostAction(ReplacementData data, Thing oldThing, Thing newThing);
}