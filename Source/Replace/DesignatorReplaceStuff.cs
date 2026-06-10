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

using Replace_Stuff.NewThing;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using static Replace_Stuff.NewThing.NewThingReplacement;

namespace Replace_Stuff.Replace;

/// <summary>
/// Designator used to replace existing buildings, frames,
/// and blueprints with versions constructed from a different
/// stuff material.
/// </summary>
/// <remarks>
/// Allows players to preserve existing structures while
/// changing their construction material through the
/// Replace Stuff replacement pipeline.
/// </remarks>
public class Designator_ReplaceStuff : Designator
{
    public override DrawStyleCategoryDef DrawStyleCategory =>
        DrawStyleCategoryDefOf.Orders;

    private ThingDef selectedStuffDef;

    private static readonly Vector2 DragPriceDrawOffset = new Vector2(19f, 17f);

    /// <summary>
    /// Caches the allowed construction materials for each
    /// buildable definition to avoid repeated enumeration
    /// during designation and drag operations.
    /// </summary>
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
        this.ResetSelectedStuff();

        hotKey = KeyBindingDefOf.Command_ColonistDraft;
    }

    /// <summary>
    /// Resets the selected replacement material to the default.
    /// </summary>
    public void ResetSelectedStuff()
    {
        selectedStuffDef = ThingDefOf.WoodLog;
    }

    public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
    {
        var result = base.GizmoOnGUI(topLeft, maxWidth, parms);

        var gizmoWidth = GetWidth(maxWidth);
        var rect = new Rect(topLeft.x + gizmoWidth / 2, topLeft.y, gizmoWidth / 2, Height / 2);
        Widgets.ThingIcon(rect, selectedStuffDef);

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

                    if (thing is not ReplacementFrame && CanReplaceThingWithStuff(selectedStuffDef, thing))
                    {
                        if (GenConstruct.BuiltDefOf(thing.def) is ThingDef builtDef)
                        {
                            cost += Mathf.RoundToInt((float)builtDef.costStuffCount / selectedStuffDef.VolumePerUnit);
                        }
                    }
                }
            }

            var drawPoint = Event.current.mousePosition + DragPriceDrawOffset;
            var iconRect = new Rect(drawPoint.x, drawPoint.y, 27f, 27f);
            GUI.color = selectedStuffDef.uiIconColor;
            GUI.DrawTexture(iconRect, selectedStuffDef.uiIcon);

            var textRect = new Rect(drawPoint.x + 29f, drawPoint.y, 999f, 29f);
            var text = cost.ToString();
            if (Map.resourceCounter.GetCount(selectedStuffDef) < cost)
            {
                GUI.color = Color.red;
                text += " (" + "NotEnoughStoredLower".Translate() + ")";
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
                selectedStuffDef = def;
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

    /// <summary>
    /// Draws a ghost preview of the currently selected
    /// replacement material beneath the mouse cursor.
    /// </summary>
    /// <param name="ghostCol">
    /// The color used to render the preview.
    /// </param>
    protected virtual void DrawGhost(Color ghostCol)
    {
        GhostDrawer.DrawGhostThing(UI.MouseCell(), Rot4.North, selectedStuffDef, null, ghostCol, AltitudeLayer.Blueprint);
    }

    /// <summary>
    /// Determines whether the specified cell can be designated
    /// for replacement using the currently selected material.
    /// </summary>
    /// <param name="cell">
    /// The map cell being evaluated.
    /// </param>
    /// <returns>
    /// An <see cref="AcceptanceReport"/> indicating whether the
    /// designation is valid.
    /// </returns>
    /// <remarks>
    /// Temporarily enables designator context while performing
    /// validation. Cells containing an existing replacement frame
    /// targeting the selected material are rejected to prevent
    /// duplicate replacement jobs.
    /// </remarks>
    public override AcceptanceReport CanDesignateCell(IntVec3 cell)
    {
        DesignatorContext.designating = true;
        try
        {
            if (!ReplacementValidator.IsReplacable(selectedStuffDef, cell, Map))
                return false;

            var things = cell.GetThingList(Map);
            for (int i = 0; i < things.Count; i++)
            {
                if (things[i] is ReplacementFrame rf && rf.EntityToBuildStuff() == selectedStuffDef)
                    return false;
            }

            return true;
        }
        finally
        {
            DesignatorContext.designating = false;
        }
    }

    /// <summary>
    /// Determines whether the specified <see cref="Thing"/> can be
    /// replaced using the supplied construction material.
    /// </summary>
    /// <param name="replacementStuff">
    /// The material that will be used for the replacement.
    /// </param>
    /// <param name="thing">
    /// The existing blueprint, frame, or completed structure
    /// being evaluated.
    /// </param>
    /// <param name="matchDef">
    /// Optional buildable definition that the replacement target
    /// must match. If specified, only Things that resolve to this
    /// buildable definition are considered valid.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the Thing is a valid replacement
    /// candidate for the specified material; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// Validation includes:
    /// <list type="bullet">
    /// <item><description>The Thing belongs to the player.</description></item>
    /// <item><description>The Thing represents a replaceable blueprint, frame, or structure.</description></item>
    /// <item><description>The replacement would actually change the construction material.</description></item>
    /// <item><description>The underlying buildable definition matches <paramref name="matchDef"/>, if supplied.</description></item>
    /// <item><description>The replacement can legally exist on the current terrain.</description></item>
    /// <item><description>The Thing is not already being replaced.</description></item>
    /// <item><description>The selected material is allowed for the target buildable definition.</description></item>
    /// </list>
    /// Allowed stuff definitions are cached to avoid repeated
    /// enumeration of <see cref="GenStuff.AllowedStuffsFor(BuildableDef)"/>.
    /// </remarks>
    public static bool CanReplaceThingWithStuff(ThingDef replacementStuff, Thing thing, ThingDef matchDef = null)
    {
        // Can't replace enemy items
        if (thing.Faction != Faction.OfPlayer && thing.Faction != null)
            return false;

        if (thing is Blueprint bp)
        {
            if (bp.EntityToBuildStuff() == replacementStuff)
                return false;
        }
        else if (thing is Frame frame)
        {
            if (frame.EntityToBuildStuff() == replacementStuff)
                return false;
        }
        else if (thing.def.HasReplacementFrame())
        {
            if (thing.Stuff == replacementStuff)
                return false;
        }
        else
        {
            return false; // Not a replaceable structure type
        }

        BuildableDef buildableDef = GenConstruct.BuiltDefOf(thing.def);
        if (matchDef != null && buildableDef != matchDef)
            return false;

        if (!GenConstruct.CanBuildOnTerrain(buildableDef, thing.Position, thing.Map, thing.Rotation, thing, replacementStuff))
            return false;

        if (thing.BeingReplacedByNewThing() != null)
            return false;

        if (!_allowedStuffCache.TryGetValue(buildableDef, out var allowedStuffSet))
        {
            allowedStuffSet = new HashSet<ThingDef>(GenStuff.AllowedStuffsFor(buildableDef));
            _allowedStuffCache[buildableDef] = allowedStuffSet;
        }

        return allowedStuffSet.Contains(replacementStuff);
    }

    public override void DesignateSingleCell(IntVec3 cell)
    {
        ReplaceFirstEligibleThing(Map, cell, selectedStuffDef);
    }

    /// <summary>
    /// Finds the highest-priority replacement candidate in the
    /// specified cell and schedules it for replacement.
    /// </summary>
    /// <param name="map">
    /// The map containing the target cell.
    /// </param>
    /// <param name="cell">
    /// The cell to search for replacement candidates.
    /// </param>
    /// <param name="stuffDef">
    /// The material to use for the replacement.
    /// </param>
    /// <remarks>
    /// Multiple replaceable Things may occupy the same cell.
    /// When this occurs, blueprints and construction frames are
    /// preferred over completed structures to avoid replacing
    /// finished buildings when an unfinished construction target
    /// already exists.
    ///
    /// If no blueprint or frame is found, the first eligible
    /// completed structure is selected.
    ///
    /// Based on work from:
    /// https://github.com/MemeGoddess/RimWorld-ReplaceStuff/pull/10
    /// </remarks>
    public static void ReplaceFirstEligibleThing(Map map, IntVec3 cell, ThingDef stuffDef)
    {
        Thing firstReplaceable = null;
        Thing blueprintOrFrameTarget = null;

        var replaceables = cell.GetThingList(map);
        var count = replaceables.Count;
        for (int i = 0; i < count; i++)
        {
            var replaceable = replaceables[i];
            if (!CanReplaceThingWithStuff(stuffDef, replaceable))
                continue;

            firstReplaceable ??= replaceable;

            if (replaceable is Blueprint_Build || replaceable is Frame)
            {
                blueprintOrFrameTarget = replaceable;
                break;
            }
        }

        var thingToReplace = blueprintOrFrameTarget ?? firstReplaceable;

        if (thingToReplace is not null)
        {
            ReplacementHandler.ExecuteReplacement(thingToReplace, stuffDef);
        }
    }

    public override void DrawPanelReadout(ref float curY, float width)
    {
        Widgets.InfoCardButton(width - 24f - 6f, 6f, selectedStuffDef);
        Text.Font = GameFont.Tiny;
    }

    public override void RenderHighlight(List<IntVec3> dragCells)
    {
        DesignatorUtility.RenderHighlightOverSelectableCells(this, dragCells);
    }
}
