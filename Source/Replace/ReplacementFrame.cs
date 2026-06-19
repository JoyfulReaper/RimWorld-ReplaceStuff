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
using Replace_Stuff.Data;
using Replace_Stuff.Utilities;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace Replace_Stuff.Replace;

/// <summary>
/// A specialized <see cref="Frame"/> that handles the atomic transition from an 
/// existing <see cref="Thing"/> to a new one using different materials or definitions.
/// </summary>
/// <remarks>
/// The <see cref="ReplacementFrame"/> manages the lifecycle of a replacement task, 
/// including deconstructing the <see cref="TargetStructure"/>, calculating the 
/// transition cost, and applying state data (bills, settings, etc.) to the new instance.
/// </remarks>
public class ReplacementFrame : Frame
{
    private const float MAX_DECONSTRUCTION_WORK = 3000f;
    private const float LARGE_CONSTRUCTION_THRESHOLD = 1400f;
    private static readonly Dictionary<ReplaceFrameKey, List<ThingDefCountClass>> _cachedReplaceCosts = new();
    private static Difficulty _cachedDifficulty;

    public delegate Func<int, int> GetBuildingResourcesLeaveCalculatorDel(Thing oldThing, DestroyMode mode);

    /// <summary>The building targeted for replacement.</summary>
    public Thing TargetThing;

    /// <summary>The material definition of the original structure, used for resource recovery calculations.</summary>
    public ThingDef TargetStuff;

    /// <summary>Encapsulated state data transferred from the target structure to the new one.</summary>
    public ReplacementData ReplaceData;


    /// <summary>
    /// Start the replacement pipeline for this ReplacementFrame.
    /// </summary>
    /// <param name="worker"></param>
    public void BeginConstruction(Pawn worker)
    {
        ReplacementPipeline.ExecuteReplacementPipeline(this, worker);
    }

    /// <summary>
    /// Handles the cleanup and feedback when a replacement task fails (e.g., pawn interrupted or material deficit).
    /// </summary>
    /// <param name="worker">The pawn who was attempting the work.</param>
    public void FailReplacement(Pawn worker)
    {
        RSLog.Debug($"Failed replace frame! work was {workDone}, Decon is {WorkToDeconstructDef(def, TargetStuff)}, total is {WorkToBuild}");

        // Cap workDone at the cost of deconstruction. 
        // If they hadn't even finished deconstruction, they shouldn't get progress credit 
        // for the new building construction.
        workDone = Mathf.Min(workDone, WorkToDeconstruct);

        if (workDone < WorkToDeconstruct)
            return;

        GenLeaving.DoLeavingsFor(this, Map, DestroyMode.FailConstruction);
        MoteMaker.ThrowText(DrawPos, Map, "TextMote_ConstructionFail".Translate());

        if (Faction == Faction.OfPlayer && WorkToReplace > LARGE_CONSTRUCTION_THRESHOLD)
        {
            Messages.Message("MessageConstructionFailed".Translate(LabelEntityToBuild, worker.LabelShort, worker.Named("WORKER")),
                new TargetInfo(Position, Map), MessageTypeDefOf.NegativeEvent);
        }
    }

    /// <summary>
    /// Generates the inspection string displayed in the bottom-left corner of the UI 
    /// when the player selects the <see cref="ReplacementFrame"/>.
    /// </summary>
    /// <returns>A formatted string detailing material progress and remaining labor.</returns>
    public override string GetInspectString()
    {
        if (Stuff is null)
            return base.GetInspectString();

        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("ContainedResources".Translate() + ":");

        // Optimized to clear out array allocations from string.Concat during UI redraw ticks
        stringBuilder.Append(Stuff.LabelCap).Append(": ")
            .Append(CountStuffHas())
            .Append(" / ")
            .AppendLine(GetRequiredMaterialCount()
            .ToString());

        stringBuilder.Append("WorkLeft".Translate())
            .Append(": ")
            .Append(this.WorkLeft.ToStringWorkAmount());

        return stringBuilder.ToString();
    }

    /// <summary>
    /// Dynamically generates the UI label for the frame, appending a "Replacing" tag 
    /// to clarify the building's current construction state.
    /// </summary>
    /// 
    public override string Label
    {
        get
        {
            string text = def.entityDefToBuild.label + "TD.ReplacingTag".Translate();
            return Stuff != null ? $"{Stuff.label} {text}" : text;
        }
    }

    /// <summary>
    /// Calculates the labor required to complete the construction phase of the replacement.
    /// </summary>
    public float WorkToReplace =>
        def.entityDefToBuild.GetStatValueAbstract(StatDefOf.WorkToBuild, Stuff);

    /// <summary>
    /// Calculates the labor required to deconstruct the <see cref="TargetStructure"/>.
    /// </summary>
    public float WorkToDeconstruct =>
        WorkToDeconstructDef(def, TargetStuff);

    /// <summary>
    /// Returns the sum of labor for deconstruction and construction.
    /// </summary>
    // Frame.WorkToBuild is not virtual.
    // Harmony redirects calls to this replacement implementation.
    public new float WorkToBuild =>
        WorkToDeconstruct + WorkToReplace;

    // Using AccessTools because GetBuildingResourcesLeaveCalculator is an internal RimWorld method.
    // This allows us to accurately calculate return resources without duplicating game logic.
    public static readonly GetBuildingResourcesLeaveCalculatorDel GetBuildingResourcesLeaveCalculator =
        AccessTools.MethodDelegate<GetBuildingResourcesLeaveCalculatorDel>(AccessTools.Method(typeof(GenLeaving), "GetBuildingResourcesLeaveCalculator"));

    /// <summary>
    /// Calculates the labor required to deconstruct a specific building definition, 
    /// clamped by <see cref="MAX_DECONSTRUCTION_WORK"/> to prevent excessive replacement times.
    /// </summary>
    public static float WorkToDeconstructDef(ThingDef def, ThingDef oldStuff = null)
    {
        var deWork = (def.entityDefToBuild as ThingDef ?? def)
            .GetStatValueAbstract(StatDefOf.WorkToBuild, oldStuff);

        return Mathf.Min(deWork, MAX_DECONSTRUCTION_WORK);
    }

    public int GetRequiredMaterialCount()
    {
        return GetRequiredMaterialCount(def.entityDefToBuild, Stuff);
    }

    /// <summary>
    /// Returns the total quantity of material units required to complete the new structure.
    /// </summary>
    /// <param name="toBuild">The definition of the building being constructed.</param>
    /// <param name="stuff">The material being used.</param>
    public static int GetRequiredMaterialCount(BuildableDef toBuild, ThingDef stuff)
    {
        if (stuff == null || stuff.VolumePerUnit == 0)
            return 0;

        var count = Mathf.RoundToInt((float)toBuild.costStuffCount / stuff.VolumePerUnit);
        if (count < 1)
            count = 1;

        return count;
    }

    public int CountStuffHas()
    {
        return resourceContainer.TotalStackCountOfDef(Stuff);
    }

    /// <summary>Calculates the remaining quantity of material required to finish the construction project.</summary>
    public int CountStuffNeeded()
    {
        return GetRequiredMaterialCount() - CountStuffHas();
    }

    // Note that "new" might not normally be called but base TotalMaterialCost is patched below to act as virtual for this method
    public new List<ThingDefCountClass> TotalMaterialCost()
    {
        // Difficulty changes affect resource return percentages. 
        // We force a cache clear if the storyteller settings have changed to avoid stale data.
        if (_cachedDifficulty != Find.Storyteller.difficulty)
        {
            CostListCalculator.Reset();
            _cachedDifficulty = Find.Storyteller.difficulty;
            _cachedReplaceCosts.Clear();
        }

        //CostListPair key = new(def.entityDefToBuild, Stuff);
        ReplaceFrameKey key = new(def.entityDefToBuild, Stuff);

        if (!_cachedReplaceCosts.TryGetValue(key, out var value))
        {
            value = new()
            {
                new(Stuff, GetRequiredMaterialCount())
            };

            _cachedReplaceCosts[key] = value;
        }

        return value;
    }

    /// <summary>Saves and loads the state of the replacement process during game save/load cycles.</summary>
    public override void ExposeData()
    {
        base.ExposeData();

        Scribe_References.Look(ref TargetThing, "oldThing");
        Scribe_Defs.Look(ref TargetStuff, "oldStuff");
        Scribe_Deep.Look(ref ReplaceData, "replaceData");
    }
}