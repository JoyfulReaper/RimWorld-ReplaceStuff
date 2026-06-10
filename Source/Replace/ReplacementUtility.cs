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
using Replace_Stuff.Utilities;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Replace_Stuff.Replace;

/// <summary>
/// Provides shared helper methods used throughout the replacement
/// pipeline.
/// </summary>
/// <remarks>
/// Contains functionality shared by both instant replacements
/// and frame-based replacement construction, including replacement
/// frame creation, temporary storage extraction/restoration,
/// and immediate replacement operations.
/// </remarks>
public static class ReplacementUtility
{
    /// <summary>
    /// Creates and spawns a <see cref="ReplacementFrame"/>
    /// for an existing structure.
    /// </summary>
    /// <param name="targetThing">The existing item or building targeted for replacement.</param>
    /// <param name="stuff">The material def choice designated for the replacement structure.</param>
    /// <returns>A fully initialized and spawned <see cref="ReplacementFrame"/> instance if successful; otherwise, <c>null</c>.</returns>
    public static ReplacementFrame SpawnReplacementFrame(Thing targetThing, ThingDef stuff)
    {
        var replacementFrameDef =
            ThingDefGenerator_ReplacementFrame.ReplacementFrameDefFor(targetThing.def);

        if (replacementFrameDef is null)
        {
            RSLog.Debug($"No replace frame def found for {targetThing.def.defName}");
            return null;
        }

        var replaceFrame = (ReplacementFrame)ThingMaker.MakeThing(replacementFrameDef, stuff);

        replaceFrame.replaceData = BuildingStateTransfer.Capture(targetThing, new HashSet<int>());
        replaceFrame.SetFactionDirect(Faction.OfPlayer);
        replaceFrame.targetThing = targetThing;
        replaceFrame.targetStuff = targetThing.Stuff;


        RSLog.Debug(
            $"PlaceReplaceFrame(): BEFORE SPAWN: OldRot={(targetThing is null ? "null" : targetThing.Rotation.ToString())} "
            + $"NewRot=New thing not spawned yet");

        GenSpawn.Spawn(replaceFrame, targetThing.Position, targetThing.Map, targetThing.Rotation);

        RSLog.Debug(
            $"PlaceReplaceFrame(): AFTER SPAWN: OldRot={(targetThing is null ? "null" : targetThing.Rotation.ToString())} "
            + $"NewRot={(replaceFrame is null ? "null" : replaceFrame.Rotation.ToString())} {replaceFrame.Rotation}");

        return replaceFrame;
    }

    /// <summary>
    /// Temporarily removes all items stored within a storage
    /// building so they can be restored after replacement.
    /// </summary>
    /// <param name="storage">
    /// The storage building being replaced.
    /// </param>
    /// <returns>
    /// A list containing all extracted items.
    /// </returns>
    public static List<Thing> ExtractStoredThings(Building_Storage storage)
    {
        DebugStorage(storage, "Before Extract");
        List<Thing> extractedThings = new();

        foreach (Thing thing in storage.GetSlotGroup().HeldThings.ToList())
        {
            extractedThings.Add(thing);
            thing.DeSpawn();
        }

        return extractedThings;
    }

    /// <summary>
    /// Restores previously extracted items to a replacement
    /// storage building.
    /// </summary>
    /// <param name="storage">
    /// The replacement storage building.
    /// </param>
    /// <param name="things">
    /// The extracted items to restore.
    /// </param>
    /// <remarks>
    /// Attempts direct placement first. If an item cannot
    /// be inserted due to capacity or slot restrictions,
    /// it is placed nearby instead.
    /// </remarks>
    public static void RestoreStoredThings(Building_Storage storage, List<Thing> things)
    {
        foreach (Thing thing in things)
        {
            var success = GenPlace.TryPlaceThing(
                thing,
                storage.Position,
                storage.Map,
                ThingPlaceMode.Direct); // Changed from Near for testing TODO

            if (!success)
            {
                success = GenPlace.TryPlaceThing(thing, storage.Position, storage.Map, ThingPlaceMode.Near);
                RSLog.Debug(
                    $"Overflow drop {thing.def.defName} Success={success}");
            }

            RSLog.Debug(
                $"Restore {thing.def.defName} "
                + $"ID={thing.thingIDNumber} "
                + $"Success={success}");

            RSLog.Debug(
                $"{thing.thingIDNumber} "
                + $"Spawned={thing.Spawned} "
                + $"Pos={(thing.Spawned ? thing.Position.ToString() : "UNSPAWNED")}");

            DebugStorage(storage, $"After placing {thing.def.defName}");
        }

        DebugStorage(storage, "After Restore");
    }

    /// <summary>
    /// Emits diagnostic information about a storage building
    /// and its contents during replacement operations.
    /// </summary>
    /// <param name="storage">
    /// The storage building being inspected.
    /// </param>
    /// <param name="stage">
    /// A label identifying the current replacement stage.
    /// </param>
    [System.Diagnostics.Conditional("DEBUG")]
    private static void DebugStorage(Building_Storage storage, string stage)
    {
        if (storage is not null)
        {
            var posStr = storage.Spawned ? storage.Position.ToString() : "UNSPAWNED";
            var hasMap = storage.Map != null;

            RSLog.Debug("DebugStorage(): " +
                $"{stage}: " +
                $"Quantity={storage.GetSlotGroup().HeldThings.Count()} " +
                $"Spawned={storage.Spawned} " +
                $"Map={hasMap} " +
                $"Pos={posStr}");

            if (storage?.GetSlotGroup() == null)
            {
                RSLog.Debug("<no slot group>");
                return;

            }
            else
            {
                RSLog.Debug($"DebugStorage(): {stage}: Held things:");
                RSLog.Debug(String.Join(", ", storage.GetSlotGroup().HeldThings
                    .Select(t => $"{t.def.defName} x{t.stackCount} ({t.thingIDNumber})")));
            }
        }
        else
            RSLog.Debug($"{stage}: Storage is null");
    }

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
    /// This method captures the existing building state,
    /// spawns a replacement using <see cref="WipeMode.Vanish"/>,
    /// initializes the new structure, and reapplies the
    /// captured state.
    /// </remarks>
    public static Thing InstantReplace(Thing oldThing, ThingDef newStuff, Pawn worker = null)
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