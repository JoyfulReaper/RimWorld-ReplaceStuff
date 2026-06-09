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

using Replace_Stuff.DestroyedRestore;
using Replace_Stuff.Utilities;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Verse;

namespace Replace_Stuff.Replace;

/// <summary>
/// Utility functions handling world interactions, structure conversions, and data transformations for object replacements.
/// </summary>
public static class GenReplace
{
    /// <summary>
    /// Instantiates and spawns a <see cref="ReplacementFrame"/> over an existing structure to begin a material replacement.
    /// </summary>
    /// <param name="targetThing">The existing item or building targeted for replacement.</param>
    /// <param name="stuff">The material def choice designated for the replacement structure.</param>
    /// <returns>A fully initialized and spawned <see cref="ReplacementFrame"/> instance if successful; otherwise, <c>null</c>.</returns>
    public static ReplacementFrame TrySpawnReplacementFrame(Thing targetThing, ThingDef stuff)
    {
        var replacementFrameDefs =
            ThingDefGenerator_ReplacementFrame.ReplaceFrameDefFor(targetThing.def);

        if (replacementFrameDefs is null)
        {
            RSLog.Debug($"No replace frame def found for {targetThing.def.defName}");
            return null;
        }

        var replaceFrame = (ReplacementFrame)ThingMaker.MakeThing(replacementFrameDefs, stuff);
        replaceFrame.replaceData = BuildingStateTransfer.Capture(targetThing, new HashSet<int>());

        replaceFrame.SetFactionDirect(Faction.OfPlayer);
        //oldThing.SetFactionDirect(Faction.OfPlayer); Done in PrepareReplacementBuilding now

        replaceFrame.targetThing = targetThing;
        replaceFrame.targetStuff = targetThing.Stuff;


        RSLog.Debug(
            $"GenReplace.PlaceReplaceFrame(): BEFORE SPAWN: OldRot={(targetThing is null ? "null" : targetThing.Rotation.ToString())} "
            + $"NewRot=New thing not spawned yet");

        GenSpawn.Spawn(replaceFrame, targetThing.Position, targetThing.Map, targetThing.Rotation);

        RSLog.Debug(
            $"GenReplace.PlaceReplaceFrame(): AFTER SPAWN: OldRot={(targetThing is null ? "null" : targetThing.Rotation.ToString())} "
            + $"NewRot={(replaceFrame is null ? "null" : replaceFrame.Rotation.ToString())} {replaceFrame.Rotation}");

        return replaceFrame;
    }

    /// <summary>
    /// Restores previously extracted stock items to the boundaries of a newly updated facility.
    /// </summary>
    /// <param name="storage">The newly built facility unit receiving the items list collection.</param>
    /// <param name="things">The inventory item pieces to return back to world map positions.</param>
    public static List<Thing> ExtractStoredThings(Building_Storage storage)
    {
        DebugStorage(storage, "Before Extract");
        List<Thing> result = new();

        foreach (Thing thing in storage.GetSlotGroup().HeldThings.ToList())
        {
            result.Add(thing);
            thing.DeSpawn();
        }

        return result;
    }

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
    /// Profiles deep inventory state diagnostic data during hot-swap container replacement routines.
    /// </summary>
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
}