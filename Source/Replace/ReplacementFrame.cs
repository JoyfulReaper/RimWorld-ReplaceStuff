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
using Replace_Stuff.DestroyedRestore;
using Replace_Stuff.Utilities;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
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
class ReplacementFrame : Frame
{
    /// <summary>The building targeted for replacement.</summary>
    public Thing targetThing;

    /// <summary>The material definition of the original structure, used for resource recovery calculations.</summary>
    public ThingDef targetStuff;

    /// <summary>Encapsulated state data transferred from the target structure to the new one.</summary>
    public ReplaceData replaceData;

    public delegate Func<int, int> GetBuildingResourcesLeaveCalculatorDel(Thing oldThing, DestroyMode mode);

    private const float MaxDeconstructWork = 3000f;
    private static readonly Dictionary<ReplaceFrameKey, List<ThingDefCountClass>> _cachedReplaceCosts = new();
    private const float LargeConstructionThreshold = 1400f;
    private static Difficulty _cachedDifficulty;

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
        WorkToDeconstructDef(def, targetStuff);

    /// <summary>
    /// Returns the sum of labor for deconstruction and construction.
    /// </summary>
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
        if (targetThing is not null && targetThing.Spawned)
        {
            RSLog.Debug($"CompleteConstruction() START: Old Rot={targetThing.Rotation}");

            var newThing = ThingMaker.MakeThing((ThingDef)def.entityDefToBuild, Stuff);
            RSLog.Debug($"CompleteConstruction() AFTER MAKETHING: New Rot={newThing.Rotation}");

            List<Thing> storedThings = null;

            if (targetThing is Building_Storage oldStorage)
            {
                storedThings = GenReplace.ExtractStoredThings(oldStorage);
            }

            BuildingStateTransfer.Apply(replaceData, newThing);
            GenReplace.ApplyReplacementState(targetThing, newThing, replaceData, worker);
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

            foreach (var thing in GenConstruct.GetAttachedBuildings(targetThing))
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
        RSLog.Debug($"Failed replace frame! work was {workDone}, Decon is {WorkToDeconstructDef(def, targetStuff)}, total is {WorkToBuild}");

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
            var count = GetRequiredMaterialCount(oldDef, stuffDef);
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
    /// when the player selects the <see cref="ReplacementFrame"/>.
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
                GetRequiredMaterialCount()
            ]));
        stringBuilder.Append("WorkLeft".Translate() + ": " + this.WorkLeft.ToStringWorkAmount());

        return stringBuilder.ToString();
    }

    /// <summary>Saves and loads the state of the replacement process during game save/load cycles.</summary>
    public override void ExposeData()
    {
        base.ExposeData();

        Scribe_References.Look(ref targetThing, "oldThing");
        Scribe_Defs.Look(ref targetStuff, "oldStuff");
        Scribe_Deep.Look(ref replaceData, "replaceData");
    }
}