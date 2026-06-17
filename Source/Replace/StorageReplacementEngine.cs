using Verse;
using RimWorld;
using Replace_Stuff.Utilities;
using System.Collections.Generic;

namespace Replace_Stuff.Replace;

/// <summary>
/// Handles the extraction and restoration of items inside storage buildings 
/// during the replacement process.
/// </summary>
internal static class StorageReplacementEngine
{
    /// <summary>
    /// Extracts items from a storage building before it is destroyed.
    /// </summary>
    /// <param name="targetThing">The old building being replaced.</param>
    /// <returns>A list of items stored in the building, or null if not a storage building.</returns>
    internal static List<Thing> ExtractStoredItems(Thing targetThing)
    {
        if (targetThing is Building_Storage storage)
        {
            return ReplacementUtility.ExtractStoredThings(storage);
        }

        return null;
    }

    /// <summary>
    /// Restores items to the newly created storage building.
    /// </summary>
    /// <param name="newThing">The new building that was just spawned.</param>
    /// <param name="storedThings">The list of items extracted from the old building.</param>
    internal static void RestoreStoredItems(Thing newThing, List<Thing> storedThings)
    {
        // Only attempt restore if we actually extracted items and the new building supports storage
        if (storedThings != null && newThing is Building_Storage storage)
        {
            ReplacementUtility.RestoreStoredThings(storage, storedThings);
        }
    }
}