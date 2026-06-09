/*
 * REPLACE STUFF: Perfomance Edition
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

namespace Replace_Stuff.PlaceBridges
{
    /// <summary>
    /// Patches <see cref="Designator_Build.DrawPlaceMouseAttachments"/> to display the cost of 
    /// required bridge terrain alongside the cost of the building being placed.
    /// </summary>
    [HarmonyPatch(typeof(Designator_Build), "DrawPlaceMouseAttachments")]
    static class DesignatorBuildCostCountsBridges
    {
        /// <summary>
        /// Provides access to the private rotation field of the <see cref="Designator_Build"/> instance.
        /// </summary>
        public static AccessTools.FieldRef<Designator_Build, Rot4> placingRot =
            AccessTools.FieldRefAccess<Designator_Build, Rot4>("placingRot");

        /// <summary>
        /// Postfix method that renders bridge material requirements to the screen if terrain needs bridging.
        /// </summary>
        /// <param name="__instance">The current build designator instance.</param>
        /// <param name="curX">The current horizontal draw position.</param>
        /// <param name="curY">The current vertical draw position (modified to allow for new rows).</param>
        //protected override void DrawPlaceMouseAttachments(float curX, ref float curY)
        public static void Postfix(Designator_Build __instance, float curX, ref float curY)
        {
            var neededBridges = new List<TerrainDef>();

            var stuff = __instance.StuffDef;
            var dragger = Find.DesignatorManager.Dragger;
            var cells = dragger.Dragging ? dragger.DragCells :
                GenAdj.OccupiedRect(UI.MouseCell(), placingRot(__instance), __instance.PlacingDef.Size).Cells;

            foreach (var dragPos in cells)
                if (PlaceBridges.GetNeededBridge(__instance.PlacingDef, dragPos, __instance.Map, stuff) is TerrainDef tdef)
                    neededBridges.Add(tdef);

            if (neededBridges.Count == 0)
                return;

            var bridgeTotalCost = new Dictionary<ThingDef, int>();
            float work = 0;
            foreach (var bridgeDef in neededBridges)
            {
                work += bridgeDef.GetStatValueAbstract(StatDefOf.WorkToBuild);
                if (bridgeDef.costList != null)
                    foreach (var bridgeCost in bridgeDef.costList)
                    {
                        bridgeTotalCost.TryGetValue(bridgeCost.thingDef, out int costCount);
                        bridgeTotalCost[bridgeCost.thingDef] = costCount + bridgeCost.count;
                    }
            }

            if (bridgeTotalCost.Count == 0)
            {
                var label = $"{StatDefOf.WorkToBuild.LabelCap}: {work.ToStringWorkAmount()} ({TerrainDefOf.Bridge.LabelCap})"; //Not bridgeCostDef.LabelCap

                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(new Rect(curX + 29f, curY, 999f, 29f), label); //private const float DragPriceDrawNumberX
                curY += 29f;
                Text.Anchor = TextAnchor.UpperLeft;
            }


            // This avoids creating an iterator object every frame the mouse is held down
            foreach (var kvp in bridgeTotalCost)
            {
                var bridgeCostDef = kvp.Key;
                var bridgeCostCount = kvp.Value;

                Widgets.ThingIcon(new Rect(curX, curY, 27f, 27f), bridgeCostDef);

                var label = $"{bridgeCostCount} ({TerrainDefOf.Bridge.LabelCap})";

                if (__instance.Map.resourceCounter.GetCount(bridgeCostDef) < bridgeCostCount)
                {
                    GUI.color = Color.red;
                    label = label + " (" + "NotEnoughStoredLower".Translate() + ")";
                }

                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(new Rect(curX + 29f, curY, 999f, 29f), label);
                curY += 29f;
                Text.Anchor = TextAnchor.UpperLeft;
            }

            GUI.color = Color.white;

            //foreach (var (bridgeCostDef, bridgeCostCount) in bridgeTotalCost.Select(x => (x.Key, x.Value)))
            //{
            //    Widgets.ThingIcon(new Rect(curX, curY, 27f, 27f), bridgeCostDef);

            //    var label = $"{bridgeCostCount} ({TerrainDefOf.Bridge.LabelCap})"; //Not bridgeCostDef.LabelCap
            //                                                                       //This doesn't account for normal building cost + under bridge cost, but what can you do
            //    if (__instance.Map.resourceCounter.GetCount(bridgeCostDef) < bridgeCostCount)
            //    {
            //        GUI.color = Color.red;
            //        label = label + " (" + "NotEnoughStoredLower".Translate() + ")";
            //    }
            //    Text.Font = GameFont.Small;
            //    Text.Anchor = TextAnchor.MiddleLeft;
            //    Widgets.Label(new Rect(curX + 29f, curY, 999f, 29f), label); //private const float DragPriceDrawNumberX
            //    curY += 29f;
            //    Text.Anchor = TextAnchor.UpperLeft;
            //}
            //GUI.color = Color.white;
        }
    }
}
