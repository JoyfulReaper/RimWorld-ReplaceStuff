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

using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Replace_Stuff.Terrain.Bridging.Patches;

[HarmonyPatch(typeof(Designator_Build), "DrawPlaceMouseAttachments")]
static class Patch_Designator_Build
{
    private static readonly List<TerrainDef> neededBridges = new List<TerrainDef>(64);
    private static readonly Dictionary<ThingDef, int> bridgeTotalCost = new Dictionary<ThingDef, int>(8);

    public static AccessTools.FieldRef<Designator_Build, Rot4> placingRot =
        AccessTools.FieldRefAccess<Designator_Build, Rot4>("placingRot");

    public static void Postfix(Designator_Build __instance, float curX, ref float curY)
    {
        neededBridges.Clear();
        bridgeTotalCost.Clear();

        var dragger = Find.DesignatorManager.Dragger;
        var cells = dragger.Dragging ? dragger.DragCells :
            GenAdj.OccupiedRect(UI.MouseCell(), placingRot(__instance), __instance.PlacingDef.Size).Cells;

        // Collect needed bridges
        foreach (var dragPos in cells)
        {
            if (BridgeUtility.GetNeededBridge(__instance.PlacingDef, dragPos, __instance.Map, __instance.StuffDef) is TerrainDef tdef)
                neededBridges.Add(tdef);
        }

        if (neededBridges.Count == 0) return;

        // Calculate costs
        foreach (var bridgeDef in neededBridges)
        {
            if (bridgeDef.costList == null) continue;
            foreach (var bridgeCost in bridgeDef.costList)
            {
                bridgeTotalCost.TryGetValue(bridgeCost.thingDef, out int currentCount);
                bridgeTotalCost[bridgeCost.thingDef] = currentCount + bridgeCost.count;
            }
        }

        // Draw Costs
        Text.Font = GameFont.Small;
        Text.Anchor = TextAnchor.MiddleLeft;

        foreach (var kvp in bridgeTotalCost)
        {
            var costDef = kvp.Key;
            var count = kvp.Value;

            Widgets.ThingIcon(new Rect(curX, curY, 27f, 27f), costDef);

            var label = $"{count} ({TerrainDefOf.Bridge.LabelCap})";
            if (__instance.Map.resourceCounter.GetCount(costDef) < count)
            {
                GUI.color = Color.red;
                label += $" ({"NotEnoughStoredLower".Translate()})";
            }

            Widgets.Label(new Rect(curX + 29f, curY, 999f, 29f), label);
            curY += 29f;
            GUI.color = Color.white;
        }
        Text.Anchor = TextAnchor.UpperLeft;
    }
}