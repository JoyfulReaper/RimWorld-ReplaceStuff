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

using HarmonyLib;
using Replace_Stuff.Utilities;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;
using CostListPair = RimWorld.CostListCalculator.CostListPair;
using FastCostListPairComparer = RimWorld.CostListCalculator.FastCostListPairComparer;

namespace Replace_Stuff.Replace;

/// <summary>
/// Represents a specialized construction frame used to replace an existing <see cref="Thing"/> 
/// with a new one in-place.
/// </summary>
/// <remarks>
/// <para>
/// This class extends <see cref="Frame"/> to manage the transition between an existing 
/// structure (<see cref="oldThing"/>) and a new one. It handles the specific economic and 
/// labor requirements of both deconstructing the old object and constructing the replacement.
/// </para>
/// </remarks>
class ReplaceFrame : Frame
{
    /// <summary>The building currently being replaced.</summary>
    public Thing oldThing;
    /// <summary>The material definition of the original structure, used for calculating deconstruction costs.</summary>
    public ThingDef oldStuff;

    private const float MaxDeconstructWork = 3000f;

    private static readonly Dictionary<CostListPair, List<ThingDefCountClass>> _cachedReplaceCosts = new(FastCostListPairComparer.Instance);
    private const float LargeConstructionThreshold = 1400f;

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

    public float WorkToReplace =>
        def.entityDefToBuild.GetStatValueAbstract(
        StatDefOf.WorkToBuild,
        Stuff);

    public float WorkToDeconstruct =>
        WorkToDeconstructDef(def, oldStuff);

    /// <summary>Returns the total labor required to complete the replacement (Deconstruction + Construction).</summary>
    public new float WorkToBuild =>
    WorkToDeconstruct + WorkToReplace;

    /// <summary>Saves and loads the state of the replacement process during game save/load cycles.</summary>
    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref oldThing, "oldThing");
        Scribe_Defs.Look(ref oldStuff, "oldStuff");
    }

    /// <summary>
    /// Calculates the labor required to deconstruct a specific building definition, 
    /// clamped by <see cref="MaxDeconstructWork"/> to prevent excessive replacement times.
    /// </summary>
    public static float WorkToDeconstructDef(ThingDef def, ThingDef oldStuff = null)
    {
        float deWork = (def.entityDefToBuild as ThingDef ?? def)
            .GetStatValueAbstract(StatDefOf.WorkToBuild, oldStuff);
        return Mathf.Min(deWork, MaxDeconstructWork);
    }


    /// <summary>Calculates the total amount of raw material units required for the new structure.</summary>
    public int TotalStuffNeeded()
    {
        return TotalStuffNeeded(def.entityDefToBuild, Stuff);
    }

    /// <summary>Calculates the total amount of raw material units required for the new structure.</summary>
    public static int TotalStuffNeeded(BuildableDef toBuild, ThingDef stuff)
    {
        if (stuff == null || stuff.VolumePerUnit == 0)
            return 0;

        int count = Mathf.RoundToInt((float)toBuild.costStuffCount / stuff.VolumePerUnit);
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
        return TotalStuffNeeded() - CountStuffHas();
    }

    // Note that "new" might not normally be called but base TotalMaterialCost is patched below to act as virtual for this method
    public new List<ThingDefCountClass> TotalMaterialCost()
    {
        // Sync cache clears with the storyteller difficulty changes
        if (CostListCalculator.cachedDifficulty != Find.Storyteller.difficulty)
        {
            CostListCalculator.Reset();
            CostListCalculator.cachedDifficulty = Find.Storyteller.difficulty;
            _cachedReplaceCosts.Clear();
        }

        CostListPair key = new(def.entityDefToBuild, Stuff);

        if (!_cachedReplaceCosts.TryGetValue(key, out var value))
        {
            value = new()
            {
            new(Stuff, TotalStuffNeeded())
            };

            _cachedReplaceCosts[key] = value;
        }

        return value;
    }

    /// <summary>
    /// Finalizes the replacement project by safely swapping the old structure for the new one 
    /// and performing necessary cleanup of map systems.
    /// </summary>
    /// <param name="worker">The pawn performing the construction.</param>
    /// 

    public new void CompleteConstruction(Pawn worker)
    {
        if (oldThing != null && oldThing.Spawned)
        {
            FinalizeReplace(oldThing, Stuff, worker);

            resourceContainer.ClearAndDestroyContents(DestroyMode.Vanish);
            Destroy(DestroyMode.Vanish);

            worker?.records.Increment(RecordDefOf.ThingsConstructed);
            worker?.records.Increment(RecordDefOf.ThingsDeconstructed);
        }
        else
        {
            resourceContainer.TryDropAll(Position, Map, ThingPlaceMode.Near);
            Destroy(DestroyMode.Cancel);
        }
    }

    /// <summary>
    /// Handles the cleanup and feedback when a replacement task fails (e.g., pawn interrupted or material deficit).
    /// </summary>
    /// <param name="worker">The pawn who was attempting the work.</param>
    public new void FailConstruction(Pawn worker)
    {
        Log.Message($"Failed replace frame! work was {workDone}, Decon is {WorkToDeconstructDef(def, oldStuff)}, total is {WorkToBuild}");

        workDone = Mathf.Min(workDone, WorkToDeconstruct);

        if (workDone < WorkToDeconstruct)
            return;

        GenLeaving.DoLeavingsFor(this, Map, DestroyMode.FailConstruction);

        MoteMaker.ThrowText(DrawPos, Map, "TextMote_ConstructionFail".Translate());
        if (Faction == Faction.OfPlayer && WorkToReplace > LargeConstructionThreshold)
        {
            Messages.Message("MessageConstructionFailed".Translate(LabelEntityToBuild, worker.LabelShort, worker.Named("WORKER")),
                new TargetInfo(Position, Map), MessageTypeDefOf.NegativeEvent);
        }
    }

    public delegate Func<int, int> GetBuildingResourcesLeaveCalculatorDel(Thing oldThing, DestroyMode mode);
    public static readonly GetBuildingResourcesLeaveCalculatorDel GetBuildingResourcesLeaveCalculator =
        AccessTools.MethodDelegate<GetBuildingResourcesLeaveCalculatorDel>(AccessTools.Method(typeof(GenLeaving), "GetBuildingResourcesLeaveCalculator"));

    /// <summary>
    /// Calculate resources to drop for the old thing before destroying it
    /// </summary>
    /// <param name="oldThing">Thing to drop resource for</param>
    /// 

    public static void DeconstructDropStuff(Thing oldThing)
    {
        if (Current.ProgramState != ProgramState.Playing)
            return;

        var oldDef = oldThing.def;
        var stuffDef = oldThing.Stuff;

        if (stuffDef == null)
            return;

        //preferably GenLeaving.DoLeavingsFor here, but don't want to drop non-stuff things.
        if (GenLeaving.CanBuildingLeaveResources(oldThing, DestroyMode.Deconstruct))
        {
            var count = TotalStuffNeeded(oldDef, stuffDef);
            var leaveCount = GetBuildingResourcesLeaveCalculator(oldThing, DestroyMode.Deconstruct)(count);
            if (leaveCount > 0)
            {
                Thing leftThing = ThingMaker.MakeThing(stuffDef);
                leftThing.stackCount = leaveCount;
                GenDrop.TryDropSpawn(leftThing, oldThing.Position, oldThing.Map, ThingPlaceMode.Near, out Thing dummyThing);
            }
        }
    }

    // TODO: Maybe remove the paramater 'stuff'
    /// <summary>
    /// Synchronizes a newly spawned object's state to match its finalized form, 
    /// handling stat cache invalidation, health recovery, and quality assignment.
    /// </summary>
    /// <param name="thing">The newly spawned building.</param>
    /// <param name="stuff">The material used for this construction. (Currently not used)</param>
    /// <param name="worker">The pawn who finished the construction, if applicable.</param>
    public static void FinalizeReplace(Thing thing, ThingDef stuff, Pawn worker = null)
    {
        DeconstructDropStuff(thing);

        thing.SetStuffDirect(stuff);
        thing.RemoveFromStatWorkerCaches();
        thing.HitPoints = thing.MaxHitPoints;
        thing.Notify_ColorChanged();

        if (worker != null && thing.TryGetComp<CompQuality>() is CompQuality compQuality)
        {
            QualityCategory qc =
                QualityUtility.GenerateQualityCreatedByPawn(worker, SkillDefOf.Construction);

            compQuality.SetQuality(qc, ArtGenerationContext.Colony);
            QualityUtility.SendCraftNotification(thing, worker);
        }
    }

    /// <summary>
    /// Generates the inspection string displayed in the bottom-left corner of the UI 
    /// when the player selects the <see cref="ReplaceFrame"/>.
    /// </summary>
    /// <returns>A formatted string detailing material progress and remaining labor.</returns>
    public override string GetInspectString()
    {
        if (Stuff == null)
            return base.GetInspectString();

        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("ContainedResources".Translate() + ":");
        stringBuilder.AppendLine(string.Concat(new object[]
        {
            Stuff.LabelCap,
            ": ",
            CountStuffHas(),
            " / ",
            TotalStuffNeeded()
        }));
        stringBuilder.Append("WorkLeft".Translate() + ": " + this.WorkLeft.ToStringWorkAmount());
        return stringBuilder.ToString();
    }
}