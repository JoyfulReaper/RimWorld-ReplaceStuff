/*/*
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

using Replace_Stuff.DestroyedRestore;
using Replace_Stuff.Replace;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Replace_Stuff.Utilities;

/// <summary>
/// Performs an immediate replacement of an existing <see cref="Thing"/> with a new version 
/// constructed from the specified <see cref="ThingDef"/>.
/// </summary>
/// <param name="oldThing">The target structure to be replaced.</param>
/// <param name="newStuff">The material definition for the new structure.</param>
/// <param name="worker">Optional: The pawn performing the replacement, if applicable.</param>
/// <param name="faction">Optional: The faction context for the replacement.</param>
/// <returns>
/// The newly spawned <see cref="Thing"/> if successful; otherwise, <c>null</c> if the input 
/// is invalid or the target was already destroyed.
/// </returns>
/// <remarks>
/// This method captures the state of the <paramref name="oldThing"/>, spawns the new replacement 
/// using <see cref="WipeMode.Vanish"/>, initializes the replacement frame, and restores the 
/// captured state data to the new instance.
/// </remarks>
public static class ReplaceUtility
{
    public static Thing InstantReplace(Thing oldThing, ThingDef newStuff, Pawn worker = null, Faction faction = null)
    {
        if (oldThing is null || oldThing.Map is null)
            return null;

        if (!oldThing.Destroyed)
        {
            var data = BuildingStateTransfer.Capture(oldThing, new HashSet<int>());
            var newThing = ThingMaker.MakeThing(oldThing.def, newStuff);

            GenSpawn.Spawn(newThing, oldThing.Position, oldThing.Map, oldThing.Rotation, WipeMode.Vanish);
            ReplacementFrame.InitializeReplacement(oldThing, newThing, worker);
            BuildingStateTransfer.Apply(data, newThing);

            return newThing;
        }
        else
        {
            RSLog.Error("InstantReplace()Tried to replace a destroyed thing.");
            return null;
        }
    }
}