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
using Replace_Stuff.DestroyedRestore;
using Replace_Stuff.Utilities;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
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

    public ReplaceData replaceData;

    public delegate Func<int, int> GetBuildingResourcesLeaveCalculatorDel(Thing oldThing, DestroyMode mode);

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
        def.entityDefToBuild.GetStatValueAbstract(StatDefOf.WorkToBuild, Stuff);

    public float WorkToDeconstruct =>
        WorkToDeconstructDef(def, oldStuff);

    /// <summary>Returns the total labor required to complete the replacement (Deconstruction + Construction).</summary>
    public new float WorkToBuild =>
        WorkToDeconstruct + WorkToReplace;

    // Using AccessTools because GetBuildingResourcesLeaveCalculator is an internal RimWorld method.
    // This allows us to accurately calculate return resources without duplicating game logic.
    public static readonly GetBuildingResourcesLeaveCalculatorDel GetBuildingResourcesLeaveCalculator =
        AccessTools.MethodDelegate<GetBuildingResourcesLeaveCalculatorDel>(AccessTools.Method(typeof(GenLeaving), "GetBuildingResourcesLeaveCalculator"));

    /// <summary>
    /// Calculates the labor required to deconstruct a specific building definition, 
    /// clamped by <see cref="MaxDeconstructWork"/> to prevent excessive replacement times.
    /// </summary>
    public static float WorkToDeconstructDef(ThingDef def, ThingDef oldStuff = null)
    {
        var deWork = (def.entityDefToBuild as ThingDef ?? def)
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
        return TotalStuffNeeded() - CountStuffHas();
    }

    // Note that "new" might not normally be called but base TotalMaterialCost is patched below to act as virtual for this method
    public new List<ThingDefCountClass> TotalMaterialCost()
    {
        // Difficulty changes affect resource return percentages. 
        // We force a cache clear if the storyteller settings have changed to avoid stale data.
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
    /// Completes a replacement frame by creating the replacement building,
    /// transferring captured state, restoring stored contents, and removing
    /// the original structure and construction frame.
    /// </summary>
    /// <param name="worker">
    /// The pawn responsible for finishing the replacement.
    /// </param>
    public new void CompleteConstruction(Pawn worker)
    {
        if (oldThing is not null && oldThing.Spawned)
        {
            RSLog.Debug($"CompleteConstruction() START: Old Rot={oldThing.Rotation}");

            var newThing = ThingMaker.MakeThing((ThingDef)def.entityDefToBuild, Stuff);
            RSLog.Debug($"CompleteConstruction() AFTER MAKETHING: New Rot={newThing.Rotation}");

            List<Thing> storedThings = null;

            if (oldThing is Building_Storage oldStorage)
            {
                storedThings = GenReplace.ExtractStoredThings(oldStorage);
            }

            BuildingStateTransfer.Apply(replaceData, newThing);
            GenReplace.ApplyReplacementState(oldThing, newThing, replaceData, worker);
            //GenSpawn.Spawn(newThing, Position, Map, Rotation, WipeMode.Vanish);

            RSLog.Debug($"CompleteConstruction() AFTER SPAWN: New Rot={newThing.Rotation}");

            if (newThing.Spawned && newThing.Map != null)
            {
                RSLog.Debug(
                    string.Join(", ",
                        newThing.Position
                            .GetThingList(newThing.Map)
                            .Select(t => t.def.defName)));
            }
            RSLog.Debug($"CompleteConstruction() END: New Rot={newThing.Rotation}");

            if (storedThings != null && newThing is Building_Storage newStorage)
            {
                GenReplace.RestoreStoredThings(newStorage, storedThings);
            }

            resourceContainer.ClearAndDestroyContents(DestroyMode.Vanish);

            foreach (var thing in GenConstruct.GetAttachedBuildings(oldThing))
            {
                thing.Destroy(DestroyMode.Vanish);
            }

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
    /// Synchronizes a newly spawned object's state to match its finalized form, 
    /// handling stat cache invalidation, health recovery, and quality assignment.
    /// </summary>
    /// <param name="thing">The newly spawned building.</param>
    /// <param name="worker">The pawn who finished the construction, if applicable.</param>
    public static void PrepareReplacementBuilding(Thing oldThing, Thing newThing, Pawn worker = null, Faction faction = null)
    {
        RSLog.Debug(
            $"PrepareReplacementBuilding(): ttart " +
            $"OldRot={(oldThing is null ? "null" : oldThing.Rotation.ToString())} " +
            $"NewRot={(newThing is null ? "null" : newThing.Rotation.ToString())} {newThing.Rotation}");

        // Set the quality of the new thing base on construction level of builder TODO MAKE THIS OPTION
        //if (worker != null && newThing.TryGetComp<CompQuality>() is CompQuality compQuality)
        //{
        //    QualityCategory qualityCreatedByPawn =
        //        QualityUtility.GenerateQualityCreatedByPawn(worker, SkillDefOf.Construction);

        //    compQuality.SetQuality(qualityCreatedByPawn, ArtGenerationContext.Colony);
        //    QualityUtility.SendCraftNotification(newThing, worker);
        //}

        if (!oldThing.Destroyed)
        {
            RSLog.Debug(
            $"PrepareReplacementBuilding(): START " +
            $"OldRot={(oldThing is null ? "null" : oldThing.Rotation.ToString())} " +
            $"NewRot={(newThing is null ? "null" : newThing.Rotation.ToString())} {newThing.Rotation}");

            //DeconstructDropStuff(oldThing);

            newThing.SetFactionDirect(faction ?? oldThing.Faction);
            newThing.RemoveFromStatWorkerCaches();

            RSLog.Debug(
            $"PrepareReplacementBuilding(): FACTION, STATCACHE SET: OldRot={(oldThing is null ? "null" : oldThing.Rotation.ToString())} "
            + $"NewRot={(newThing is null ? "null" : newThing.Rotation.ToString())} {newThing.Rotation}");

            // Current design: New buildings spawn at full health.
            // Future consideration: Add an option to calculate HitPoints based on the 
            // old building's percentage of MaxHitPoints. TODO
            // newThing.HitPoints = Mathf.RoundToInt(oldThing.HitPoints * ((float)newThing.MaxHitPoints / oldThing.MaxHitPoints)); // For keeping hit points if we decide to
            newThing.HitPoints = newThing.MaxHitPoints;
            newThing.Notify_ColorChanged();

            RSLog.Debug(
                        $"PrepareReplacementBuilding(): HITPOINTS, Notify_ColotChanged: OldRot={(oldThing is null ? "null" : oldThing.Rotation.ToString())} "
                        + $"NewRot={(newThing is null ? "null" : newThing.Rotation.ToString())} {newThing.Rotation}");

            if (worker != null && newThing.TryGetComp<CompQuality>() is CompQuality compQuality)
            {
                QualityCategory qualityCreatedByPawn = QualityUtility.GenerateQualityCreatedByPawn(worker, SkillDefOf.Construction);
                compQuality.SetQuality(qualityCreatedByPawn, ArtGenerationContext.Colony);
                QualityUtility.SendCraftNotification(newThing, worker);

                RSLog.Debug(
                    $"PrepareReplacementBuilding(): QUALITY, CRAFT NOTE: OldRot={(oldThing is null ? "null" : oldThing.Rotation.ToString())} "
                    + $"NewRot={(newThing is null ? "null" : newThing.Rotation.ToString())} {newThing.Rotation}");

            }

            oldThing.Destroy(DestroyMode.Deconstruct);
            if (newThing.Spawned && newThing.Map != null)
            {
                RSLog.Debug(
                    string.Join(", ",
                        newThing.Position
                            .GetThingList(newThing.Map)
                            .Select(t => t.def.defName)));
            }
        }
        else
        {
            RSLog.Error("FinalizeReplace(): oldThing was already destroyed.");
        }
        RSLog.Debug(
            $"PrepareReplacementBuilding(): end " +
            $"OldRot={(oldThing is null ? "null" : oldThing.Rotation.ToString())} " +
            $"NewRot={(newThing is null ? "null" : newThing.Rotation.ToString())} {newThing.Rotation}");
    }

    /// <summary>
    /// Handles the cleanup and feedback when a replacement task fails (e.g., pawn interrupted or material deficit).
    /// </summary>
    /// <param name="worker">The pawn who was attempting the work.</param>
    public new void FailConstruction(Pawn worker)
    {
        RSLog.Debug($"Failed replace frame! work was {workDone}, Decon is {WorkToDeconstructDef(def, oldStuff)}, total is {WorkToBuild}");

        // Cap workDone at the cost of deconstruction. 
        // If they hadn't even finished deconstruction, they shouldn't get progress credit 
        // for the new building construction.
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

    /// <summary>
    /// Calculate resources to drop for the old thing before destroying it
    /// </summary>
    /// <param name="oldThing">Thing to drop resource for</param>
    /// 
    public static void DeconstructDropStuff(Thing oldThing)
    {
        if (oldThing is null || !oldThing.Spawned || oldThing.Map is null)
            return;

#if DEBUG
        // nothing
#else
        if (Current.ProgramState != ProgramState.Playing)
            return;
#endif
        var oldDef = oldThing.def;
        var stuffDef = oldThing.Stuff;

        if (stuffDef == null)
            return;

        // We use our own calculator here instead of standard GenLeaving.DoLeavingsFor 
        // because we only want to drop the 'stuff' (material) used in construction,
        // rather than all items (like components/steel) usually dropped by deconstruction.
        if (GenLeaving.CanBuildingLeaveResources(oldThing, DestroyMode.Deconstruct))
        {
            var count = TotalStuffNeeded(oldDef, stuffDef);
            var leaveCount = GetBuildingResourcesLeaveCalculator(oldThing, DestroyMode.Deconstruct)(count);
            if (leaveCount > 0)
            {
                var leftThing = ThingMaker.MakeThing(stuffDef);
                leftThing.stackCount = leaveCount;
                GenDrop.TryDropSpawn(leftThing, oldThing.Position, oldThing.Map, ThingPlaceMode.Near, out _);
            }
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
        stringBuilder.AppendLine(string.Concat(
            [
                Stuff.LabelCap,
                ": ",
                CountStuffHas(),
                " / ",
                TotalStuffNeeded()
            ]));
        stringBuilder.Append("WorkLeft".Translate() + ": " + this.WorkLeft.ToStringWorkAmount());

        return stringBuilder.ToString();
    }

    /// <summary>Saves and loads the state of the replacement process during game save/load cycles.</summary>
    public override void ExposeData()
    {
        base.ExposeData();

        Scribe_References.Look(ref oldThing, "oldThing");
        Scribe_Defs.Look(ref oldStuff, "oldStuff");
        Scribe_Deep.Look(ref replaceData, "replaceData");
    }
}