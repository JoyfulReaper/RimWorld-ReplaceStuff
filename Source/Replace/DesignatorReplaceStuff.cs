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

using Replace_Stuff.NewThing;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Replace_Stuff.Replace;

public class Designator_ReplaceStuff : Designator
{
    public override DrawStyleCategoryDef DrawStyleCategory =>
        DrawStyleCategoryDefOf.Orders;

    private ThingDef stuffDef;

    private static readonly Vector2 DragPriceDrawOffset = new Vector2(19f, 17f);
    private static readonly Dictionary<BuildableDef, HashSet<ThingDef>> _allowedStuffCache = new();

    public Designator_ReplaceStuff()
    {
        soundDragSustain = SoundDefOf.Designate_DragBuilding;
        soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
        soundSucceeded = SoundDefOf.Designate_PlaceBuilding;

        defaultLabel = "TD.Replace".Translate();
        defaultDesc = "TD.ReplaceDesc".Translate();
        icon = TexDefOf.replaceIcon;
        iconProportions = new Vector2(1f, 1f);
        iconDrawScale = 1f;
        this.ResetStuffToDefault();

        hotKey = KeyBindingDefOf.Command_ColonistDraft;
    }

    public void ResetStuffToDefault()
    {
        stuffDef = ThingDefOf.WoodLog;
    }

    public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
    {
        var result = base.GizmoOnGUI(topLeft, maxWidth, parms);

        var w = GetWidth(maxWidth);
        var rect = new Rect(topLeft.x + w / 2, topLeft.y, w / 2, Height / 2);
        Widgets.ThingIcon(rect, stuffDef);

        return result;
    }

    public override void DrawMouseAttachments()
    {
        base.DrawMouseAttachments();
        if (!ArchitectCategoryTab.InfoRect.Contains(UI.MousePositionOnUIInverted))
        {
            var cost = 0;
            var dragCells = Find.DesignatorManager.Dragger.DragCells;

            // Loop over the cells being dragged
            for (int c = 0; c < dragCells.Count; c++)
            {
                var cell = dragCells[c];
                var thingsInCell = cell.GetThingList(Map);

                for (int t = 0; t < thingsInCell.Count; t++)
                {
                    var thing = thingsInCell[t];

                    if (thing is not ReplacementFrame && CanReplaceStuffFor(stuffDef, thing))
                    {
                        if (GenConstruct.BuiltDefOf(thing.def) is ThingDef builtDef)
                        {
                            cost += Mathf.RoundToInt((float)builtDef.costStuffCount / stuffDef.VolumePerUnit);
                        }
                    }
                }
            }

            var drawPoint = Event.current.mousePosition + DragPriceDrawOffset;
            var iconRect = new Rect(drawPoint.x, drawPoint.y, 27f, 27f);
            GUI.color = stuffDef.uiIconColor;
            GUI.DrawTexture(iconRect, stuffDef.uiIcon);

            var textRect = new Rect(drawPoint.x + 29f, drawPoint.y, 999f, 29f);
            var text = cost.ToString();
            if (Map.resourceCounter.GetCount(stuffDef) < cost)
            {
                GUI.color = Color.red;
                text = text + " (" + "NotEnoughStoredLower".Translate() + ")";
            }
            else
                GUI.color = Color.white;

            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(textRect, text);
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;
        }
    }

    public override void ProcessInput(Event ev)
    {
        if (!CheckCanInteract())
            return;

        var amounts = Map.resourceCounter.AllCountedAmounts;
        var list = new List<FloatMenuOption>(amounts.Count);
        var godMode = DebugSettings.godMode;

        foreach (var kvp in amounts)
        {
            var def = kvp.Key;
            if (!def.IsStuff || (!godMode && kvp.Value <= 0))
                continue;

            list.Add(new FloatMenuOption(def.LabelCap, () =>
            {
                base.ProcessInput(ev);
                Find.DesignatorManager.Select(this);
                stuffDef = def;
            }, def));
        }

        if (list.Count == 0)
        {
            Messages.Message("NoStuffsToBuildWith".Translate(), MessageTypeDefOf.RejectInput);
        }
        else
        {
            Find.WindowStack.Add(new FloatMenu(list) { vanishIfMouseDistant = true });
            Find.DesignatorManager.Select(this);
        }
    }

    public override void SelectedUpdate()
    {
        GenDraw.DrawNoBuildEdgeLines();
        if (!ArchitectCategoryTab.InfoRect.Contains(UI.MousePositionOnUIInverted))
        {
            var mousePos = UI.MouseCell();
            if (mousePos.InBounds(Map))
                DrawGhost(CanDesignateCell(mousePos).Accepted ? new Color(0.5f, 1f, 0.6f, 0.4f) : new Color(1f, 0f, 0f, 0.4f));
        }
    }

    protected virtual void DrawGhost(Color ghostCol)
    {
        GhostDrawer.DrawGhostThing(UI.MouseCell(), Rot4.North, stuffDef, null, ghostCol, AltitudeLayer.Blueprint);
    }

    public override AcceptanceReport CanDesignateCell(IntVec3 cell)
    {
        DesignatorContext.designating = true;
        try
        {
            if (!ReplacementValidator.CanReplaceStuffAt(stuffDef, cell, Map))
                return false;

            var things = cell.GetThingList(Map);
            for (int i = 0; i < things.Count; i++)
            {
                if (things[i] is ReplacementFrame rf && rf.EntityToBuildStuff() == stuffDef)
                    return false;
            }

            return true;
        }
        finally
        {
            DesignatorContext.designating = false;
        }
    }

    public static bool CanReplaceStuffFor(ThingDef stuff, Thing thing, ThingDef matchDef = null)
    {
        // Can't replace enemy items
        if (thing.Faction != Faction.OfPlayer && thing.Faction != null)
            return false;

        if (thing is Blueprint bp)
        {
            if (bp.EntityToBuildStuff() == stuff)
                return false;
        }
        else if (thing is Frame frame)
        {
            if (frame.EntityToBuildStuff() == stuff)
                return false;
        }
        else if (thing.def.HasReplacementFrrame())
        {
            if (thing.Stuff == stuff)
                return false;
        }
        else
        {
            return false; // Not a replaceable structure type
        }

        BuildableDef builtDef = GenConstruct.BuiltDefOf(thing.def);
        if (matchDef != null && builtDef != matchDef)
            return false;

        if (!GenConstruct.CanBuildOnTerrain(builtDef, thing.Position, thing.Map, thing.Rotation, thing, stuff))
            return false;

        if (thing.BeingReplacedByNewThing() != null)
            return false;

        if (!_allowedStuffCache.TryGetValue(builtDef, out var allowedSet))
        {
            allowedSet = new HashSet<ThingDef>(GenStuff.AllowedStuffsFor(builtDef));
            _allowedStuffCache[builtDef] = allowedSet;
        }

        return allowedSet.Contains(stuff);
    }

    public override void DesignateSingleCell(IntVec3 cell)
    {
        FindReplace(Map, cell, stuffDef);
    }

    // Credit: https://github.com/MemeGoddess/RimWorld-ReplaceStuff/pull/10
    public static void FindReplace(Map map, IntVec3 cell, ThingDef stuffDef)
    {
        Thing firstReplaceable = null;
        Thing firstBlueprintOrFrame = null;

        var replaceables = cell.GetThingList(map);

        for (int i = 0; i < replaceables.Count; i++)
        {
            var replaceable = replaceables[i];
            if (!CanReplaceStuffFor(stuffDef, replaceable))
                continue;

            firstReplaceable ??= replaceable;

            if (replaceable is Blueprint_Build || replaceable is Frame)
            {
                firstBlueprintOrFrame = replaceable;
                break;
            }
        }

        var thingToReplace = firstBlueprintOrFrame ?? firstReplaceable;

        if (thingToReplace is not null)
        {
            ReplacementHandler.ExecuteReplacement(thingToReplace, stuffDef);
        }
    }

    public override void DrawPanelReadout(ref float curY, float width)
    {
        Widgets.InfoCardButton(width - 24f - 6f, 6f, stuffDef);
        Text.Font = GameFont.Tiny;
    }

    public override void RenderHighlight(List<IntVec3> dragCells)
    {
        DesignatorUtility.RenderHighlightOverSelectableCells(this, dragCells);
    }
}
