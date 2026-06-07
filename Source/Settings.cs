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

using Replace_Stuff.PlaceBridges;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Replace_Stuff;

/// <summary>
/// Welcome to the Setttings :)
/// </summary>
public class Settings : ModSettings
{
    internal bool hideOverwallCoolers;
    internal bool hideNormalCoolers;

    private string _version = "0.0.12";
    private List<string> _preferredBridgeOrder = new();

    public readonly string debugPrefix = "[ReplaceStuffPerfomance]";

    public string Version => _version;

    // Settings Dialog
    public void DoWindowContents(Rect inRect)
    {
        var listing = new Listing_Standard();
        listing.Begin(inRect);

        listing.CheckboxLabeled("TD.SettingsNoOverwallCoolers".Translate(), ref hideOverwallCoolers);
        listing.CheckboxLabeled("TD.SettingsNoNormalCoolers".Translate(), ref hideNormalCoolers);

        // Can't re-order a list if there is only one item
        if (BridgelikeTerrain.allBridgeTerrains.Count > 1)
            DoBridgeList(listing);

        listing.End();
    }

    // TODO: Replace with a cleaner reorder UI.
    // Current implementation favors simplicity and stability.
    private void DoBridgeList(Listing_Standard options)
    {
        options.GapLine();

        Text.Font = GameFont.Medium;
        options.Label("TD.SettingsPreferredBridge".Translate());
        Text.Font = GameFont.Small;

        const float rowHeight = 28f;

        for (int i = 0; i < BridgelikeTerrain.allBridgeTerrains.Count; i++)
        {
            var bridge = BridgelikeTerrain.allBridgeTerrains[i];

            var row = options.GetRect(rowHeight);

            var labelRect = new Rect(row.x, row.y, row.width - 60f, row.height);
            var upRect = new Rect(row.xMax - 56f, row.y, 28f, row.height);
            var downRect = new Rect(row.xMax - 28f, row.y, 28f, row.height);

            Widgets.DefLabelWithIcon(labelRect, bridge);

            // Up button always occupies the left slot.
            GUI.enabled = i > 0;
            if (Widgets.ButtonText(upRect, "▲"))
            {
                var temp = BridgelikeTerrain.allBridgeTerrains[i - 1];
                BridgelikeTerrain.allBridgeTerrains[i - 1] = bridge;
                BridgelikeTerrain.allBridgeTerrains[i] = temp;
                break;
            }

            // Down button always occupies the right slot.
            GUI.enabled = i < BridgelikeTerrain.allBridgeTerrains.Count - 1;
            if (Widgets.ButtonText(downRect, "▼"))
            {
                var temp = BridgelikeTerrain.allBridgeTerrains[i + 1];
                BridgelikeTerrain.allBridgeTerrains[i + 1] = bridge;
                BridgelikeTerrain.allBridgeTerrains[i] = temp;
                break;
            }

            GUI.enabled = true;
        }
    }

    public override void ExposeData()
    {
        Scribe_Values.Look(ref _version, "Version", Version);
        Scribe_Values.Look(ref hideOverwallCoolers, "hideOverwallCoolers");
        Scribe_Values.Look(ref hideNormalCoolers, "hideNormalCoolers");

        if (Scribe.mode == LoadSaveMode.Saving)
        {
            _preferredBridgeOrder =
                BridgelikeTerrain.allBridgeTerrains
                .ConvertAll(x => x.defName);
        }

        Scribe_Collections.Look(ref _preferredBridgeOrder, "bridgePrefNames");

        if (Scribe.mode == LoadSaveMode.LoadingVars)
        {
            LongEventHandler.ExecuteWhenFinished(ApplyBridgeOrder);
        }
    }

    private void ApplyBridgeOrder()
    {
        if (_preferredBridgeOrder == null || _preferredBridgeOrder.Count == 0)
            return;

        for (int i = _preferredBridgeOrder.Count - 1; i >= 0; i--)
        {
            var def = DefDatabase<TerrainDef>.GetNamed(_preferredBridgeOrder[i], false);

            if (def == null)
                continue;

            if (!BridgelikeTerrain.allBridgeTerrains.Remove(def))
                continue;

            BridgelikeTerrain.allBridgeTerrains.Insert(0, def);
        }
    }
}