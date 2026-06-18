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

using RimWorld;
using Verse;

namespace Replace_Stuff.PlaceBridges;

public static class BridgeConflictUtility
{
    /// <summary>
    /// Cleans up existing blueprints or frames that conflict with a new bridge placement.
    /// </summary>
    public static void HandleBlueprintConflict(BuildableDef defToBuild, DestroyMode mode, Map map, IntVec3 pos)
    {
        if (!defToBuild.IsBridgelike() || mode == DestroyMode.Vanish || mode == DestroyMode.FailConstruction)
            return;

        var things = map.thingGrid.ThingsListAtFast(pos);
        for (int i = things.Count - 1; i >= 0; i--)
        {
            Thing t = things[i];

            // Only target blueprints/frames that aren't the bridge itself
            if ((t is Blueprint || t is Frame) && !t.def.entityDefToBuild.IsBridgelike())
            {
                if (!t.Destroyed && !Find.Selector.IsSelected(t))
                {
                    t.Destroy(DestroyMode.Refund);
                }
            }
        }
    }
}