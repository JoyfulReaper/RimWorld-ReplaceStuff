/*/*
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

using Replace_Stuff;
using Replace_Stuff.Compatibility;
using Replace_Stuff.DestroyedRestore;
using Replace_Stuff.Interfaces;
using Replace_Stuff.Replace;
using Replace_Stuff.Utilities;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

internal static class ReplacementPipeline
{
    /// <summary>
    /// The Replacement Pipeline: Where it all Happens!
    /// </summary>
    /// <param name="replacementFrame"></param>
    /// <param name="worker"></param>
    internal static void ExecuteReplacementPipeline(ReplacementFrame replacementFrame, Pawn worker)
    {
        var activeComps = replacementFrame.ReplaceData.compHandlers;

        if (replacementFrame.TargetThing is null || !replacementFrame.TargetThing.Spawned)
        {
            replacementFrame.resourceContainer.TryDropAll(replacementFrame.Position, replacementFrame.Map, ThingPlaceMode.Near);
            replacementFrame.Destroy(DestroyMode.Cancel);

            return;
        }

        var oldThing = replacementFrame.TargetThing;
        var newThing = CreateReplacement(replacementFrame);
        RunHandlers(activeComps, oldThing, h => h.PreAction(replacementFrame.ReplaceData, oldThing, newThing));
        var transientState = StorageReplacementEngine.ExtractStoredItems(oldThing);
        DeconstructDropStuff(oldThing);

        oldThing.Destroy(DestroyMode.Vanish);

        SpawnReplacement(newThing, replacementFrame);
        InitializeReplacement(oldThing, newThing, worker);
        ApplyPersistentState(newThing, replacementFrame.ReplaceData);
        StorageReplacementEngine.RestoreStoredItems(newThing, transientState);

        // Post-Action: Run after newThing is spawned
        RunHandlers(activeComps, oldThing, h => h.PostAction(replacementFrame.ReplaceData, oldThing, newThing));

        Cleanup(oldThing, worker, replacementFrame.resourceContainer);
    }

    private static void Cleanup(Thing targetThing, Pawn worker, ThingOwner resourceContainer)
    {
        resourceContainer.ClearAndDestroyContents(DestroyMode.Vanish);

        RSLog.Debug(
            $"Cleanup: old Spawned={targetThing.Spawned} Destroyed={targetThing.Destroyed}");

        foreach (var thing in GenConstruct.GetAttachedBuildings(targetThing))
        {
            if (!thing.Destroyed)
                thing.Destroy(DestroyMode.Vanish);
        }

        worker?.records.Increment(RecordDefOf.ThingsConstructed);
        worker?.records.Increment(RecordDefOf.ThingsDeconstructed);
    }

    internal static void InitializeReplacement(Thing oldThing, Thing newThing, Pawn worker)
    {
        // Current design: New buildings spawn at full health.
        // Future consideration: Add an option to calculate HitPoints based on the 
        // old building's percentage of MaxHitPoints. TODO
        // newThing.HitPoints = Mathf.RoundToInt(oldThing.HitPoints * ((float)newThing.MaxHitPoints / oldThing.MaxHitPoints)); // For keeping hit points if we decide to
        newThing.SetFactionDirect(oldThing.Faction);
        newThing.RemoveFromStatWorkerCaches();

        newThing.HitPoints = newThing.MaxHitPoints;
        newThing.Notify_ColorChanged();

        ApplyConstructionQuality(newThing, worker);
    }

    private static void ApplyPersistentState(Thing newThing, ReplaceData replaceData)
    {
        BuildingStateTransfer.Apply(replaceData, newThing);
    }

    private static void ApplyConstructionQuality(Thing newThing, Pawn worker)
    {
        if (worker != null && newThing.TryGetComp<CompQuality>() is CompQuality compQuality)
        {
            QualityCategory qualityCreatedByPawn = QualityUtility.GenerateQualityCreatedByPawn(worker, SkillDefOf.Construction);
            compQuality.SetQuality(qualityCreatedByPawn, ArtGenerationContext.Colony);
            QualityUtility.SendCraftNotification(newThing, worker);
        }
    }

    /// <summary>
    /// Calculate resources to drop for the old thing before destroying it
    /// </summary>
    /// <param name="oldThing">Thing to drop resource for</param>
    /// 
    private static void DeconstructDropStuff(Thing oldThing)
    {
        if (oldThing is null || !oldThing.Spawned || oldThing.Map is null)
            return;

        var oldDef = oldThing.def;
        var stuffDef = oldThing.Stuff;

        if (stuffDef == null)
            return;

        // We use our own calculator here instead of standard GenLeaving.DoLeavingsFor 
        // because we only want to drop the 'stuff' (material) used in construction,
        // rather than all items (like components/steel) usually dropped by deconstruction.
        if (GenLeaving.CanBuildingLeaveResources(oldThing, DestroyMode.Deconstruct))
        {
            var count = ReplacementFrame.GetRequiredMaterialCount(oldDef, stuffDef);
            var leaveCount = ReplacementFrame.GetBuildingResourcesLeaveCalculator(oldThing, DestroyMode.Deconstruct)(count);
            if (leaveCount > 0)
            {
                var leftThing = ThingMaker.MakeThing(stuffDef);
                leftThing.stackCount = leaveCount;
                GenDrop.TryDropSpawn(leftThing, oldThing.Position, oldThing.Map, ThingPlaceMode.Near, out _);
            }
        }
    }

    private static void SpawnReplacement(Thing newThing, ReplacementFrame replacementFrame)
    {
        // IMPORTANT:
        // GenSpawn.Spawn(..., WipeMode.Vanish) immediately destroys the
        // existing building occupying the cell. Any state needed from
        // targetThing must be captured before spawning.
        GenSpawn.Spawn(newThing, replacementFrame.Position, replacementFrame.Map, newThing.Rotation, WipeMode.Vanish);

        RSLog.Debug(
            $"SpawnReplacement(): " +
            $"Spawned={newThing.Spawned} " +
            $"Pos={newThing.Position} " +
            $"Rot={newThing.Rotation}");
    }

    private static Thing CreateReplacement(ReplacementFrame replacementFrame)
    {
        RSLog.Debug($"CreateReplacement() START: Old Rot={replacementFrame.TargetThing.Rotation}");
        var newThing = ThingMaker.MakeThing((ThingDef)replacementFrame.def.entityDefToBuild, replacementFrame.Stuff);
        RSLog.Debug($"CreateReplacement() AFTER MAKETHING: New Rot={newThing.Rotation}");

        return newThing;
    }

    private static void RunHandlers(List<string> compNames, Thing oldThing, Action<IReplacementHandler> action)
    {
        // Run modern, registered handlers from the string list
        foreach (var compName in compNames)
        {
            if (ReplacementRegistry.TryGetHandler(compName, out var handler))
            {
                try
                {
                    action(handler);
                }
                catch (Exception e)
                {
                    RSLog.Error($"Error executing replacement handler {compName}: {e.Message}");
                }
            }
        }

        // Scan the old building for legacy IReplacementComp components attached to it
        if (oldThing is ThingWithComps thingWithComps)
        {
            foreach (var comp in thingWithComps.AllComps)
            {
                // ignore the obsolete warning
#pragma warning disable CS0618
                if (comp is IReplacementComp legacyComp)
                {
                    try
                    {
                        // Wrap the old instance in the bridge so the modern pipeline can invoke it seamlessly
                        var bridge = new LegacyReplacementBridge(legacyComp);
                        action(bridge);
                    }
                    catch (Exception e)
                    {
                        RSLog.Error($"Error executing legacy replacement comp {comp.GetType().Name}: {e.Message}");
                    }
                }
#pragma warning restore CS0618
            }
        }
    }
}