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

using Replace_Stuff.Replace;
using Replace_Stuff.Utilities;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Replace_Stuff.DestroyedRestore;


/// <summary>
/// Map component responsible for storing, serializing, and recovering metadata 
/// from buildings that were destroyed and scheduled for automatic rebuilding.
/// </summary>
public class DestroyedBuildingStore : MapComponent
{
    /// <summary>
    /// Tracks captured building metadata, indexed by the cell coordinates where the destruction occurred.
    /// </summary>
    public Dictionary<IntVec3, ReplaceData> destroyedBuildings;
    //Actually want this to be deep-ref since it's despawned!


    /// <summary>
    /// Initializes a new instance of the <see cref="DestroyedBuildingStore"/> component for a specific map.
    /// </summary>
    /// <param name="map">The map this component tracks.</param>
    public DestroyedBuildingStore(Map map) : base(map)
    {
        destroyedBuildings = new Dictionary<IntVec3, ReplaceData>();
    }

    /// <summary>
    /// Captures and serializes a building's metadata (settings, bills, configurations) 
    /// right before it is completely despawned.
    /// </summary>
    /// <param name="thing">The building instance being destroyed.</param>
    /// <param name="map">The map containing the building.</param>
    public static void SaveBuilding(Thing thing, Map map)
    {
        var data = BuildingStateTransfer.Capture(thing, new HashSet<int>());
        var comp = map.GetComponent<DestroyedBuildingStore>();
        comp.destroyedBuildings[thing.Position] = data;
        thing.ForceSetStateToUnspawned();
    }

    /// <summary>
    /// Restores saved metadata onto a newly constructed building if it was placed via an auto-rebuild blueprint.
    /// </summary>
    /// <param name="newBuilding">The newly completed building instance.</param>
    /// <param name="pos">The target position grid cell.</param>
    /// <param name="map">The map where revival is occurring.</param>
    public static void ReviveBuilding(Thing newBuilding, IntVec3 pos, Map map)
    {
        var comp = map.GetComponent<DestroyedBuildingStore>();

        if (comp.destroyedBuildings.TryGetValue(pos, out ReplaceData data))
        {
            BuildingStateTransfer.Apply(data, newBuilding);
            comp.destroyedBuildings.Remove(pos);
        }
    }

    /// <summary>
    /// Manually purges cached building data from a specific cell coordinate.
    /// </summary>
    /// <param name="pos">The cell position to clear.</param>
    /// <param name="map">The map containing the cell.</param>
    public static void RemoveAt(IntVec3 pos, Map map)
    {
        var comp = map.GetComponent<DestroyedBuildingStore>();

        if (comp.destroyedBuildings.Remove(pos))
        {
            RSLog.Debug($"Forgetting destroyed building state at {pos}");
        }
    }


    /// <summary>
    /// Handles XML saving/loading for the component. Performs a pre-save check 
    /// to clean up orphaned entries if a player cancels an auto-rebuild task.
    /// </summary>
    public override void ExposeData()
    {
        // Pre-save cleanup: Prevents save-file bloat.
        // If the player canceled the blueprint, or it was destroyed before being built,
        // we remove the data so it doesn't linger indefinitely in the save file.
        if (Scribe.mode == LoadSaveMode.Saving)
        {
            foreach (IntVec3 pos in destroyedBuildings.Keys.ToList())
            {
                if (!pos.GetThingList(map).Any(t => t.def.IsFrame || t.def.IsBlueprint))
                {
                    Verse.Log.Warning(
                        $"[ReplaceStuffPerfomance] - Forgetting orphaned building state at {pos}");
                    destroyedBuildings.Remove(pos);
                }
            }
        }

        // Save the collection using Deep mode because ReplaceData contains complex reference types/lists.
        Scribe_Collections.Look(ref destroyedBuildings, "destroyedBuildings", LookMode.Value, LookMode.Deep);
    }
}
