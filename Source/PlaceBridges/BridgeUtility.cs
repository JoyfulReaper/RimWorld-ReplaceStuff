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

using Replace_Stuff.Utilities;
using RimWorld;
using System.Linq;
using Verse;

namespace Replace_Stuff.PlaceBridges;

/// <summary>
/// Handles logic for determining and placing required bridge blueprints.
/// </summary>
public static class BridgeUtility
{
    public static TerrainDef GetNeededBridge(BuildableDef def, IntVec3 pos, Map map, ThingDef stuff)
    {
        if (!pos.InBounds(map))
            return null;

        var needed = def.GetTerrainAffordanceNeed(stuff);
        return BridgelikeTerrain.FindBridgeFor(map.terrainGrid.TerrainAt(pos), needed, map);
    }

    public static void PlaceBridgeIfNeeded(BuildableDef sourceDef, IntVec3 pos, Map map, Rot4 rotation, Faction faction, ThingDef stuff)
    {
        var bridgeDef = BridgeUtility.GetNeededBridge(sourceDef, pos, map, stuff);

        if (bridgeDef is null || !bridgeDef.IsResearchFinished)
            return;

        if (pos.GetThingList(map).Any(t => t.def.entityDefToBuild == bridgeDef))
            return; //Already building!

        RSLog.Debug($"placing {bridgeDef} for {sourceDef}({sourceDef.GetTerrainAffordanceNeed(stuff)}) on {map.terrainGrid.TerrainAt(pos)}");
        GenConstruct.PlaceBlueprintForBuild(bridgeDef, pos, map, rotation, faction, null); //Are there bridge precepts/styles?...
    }
}