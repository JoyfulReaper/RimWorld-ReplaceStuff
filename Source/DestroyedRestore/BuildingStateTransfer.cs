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

using Replace_Stuff.Replace;
using Replace_Stuff.Utilities;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Replace_Stuff.DestroyedRestore;

[StaticConstructorOnStartup]
public static class BuildingStateTransfer
{
    static BuildingStateTransfer() { }

    public static ReplaceData Capture(Thing thing, HashSet<int> visited)
    {
        RSLog.Debug($"CAPTURE START {thing}");
        RSLog.Debug($"CAPTURE {thing.def.defName} implements IStoreSettingsParent = {thing is IStoreSettingsParent}");

        if (!visited.Add(thing.thingIDNumber))
            return null;

        ReplaceData data = new()
        {
            faction = thing.Faction,
            rotation = thing.Rotation
        };

        if (thing.TryGetComp<CompQuality>() is CompQuality qc)
            data.quality = qc.Quality;

        if (thing is IStoreSettingsParent storage)
        {
            RSLog.Debug("CAPTURE STORAGE");

            var settings = storage.GetStoreSettings();
            RSLog.Debug($"DEBUG: Before copy - Priority: {settings?.Priority.ToString() ?? "NULL"}, Allowed: {settings?.filter?.AllowedDefCount.ToString() ?? "NULL"}");

            data.storagePriority = settings.Priority;
            data.storageFilter = new ThingFilter();
            data.storageFilter.CopyAllowancesFrom(settings.filter);

            RSLog.Debug($"CAPTURE: allowed defs = {data.storageFilter.AllowedDefCount}");
            RSLog.Debug($"CAPTURE: priority = {data.storagePriority}");
            RSLog.Debug($"Captured defs={data.storageFilter.AllowedDefCount}");
            if (thing is Building_Storage bs)
            {
                RSLog.Debug($"storageGroup={(bs.storageGroup != null)}");
                RSLog.Debug($"settings hash={bs.settings.GetHashCode()}");
                RSLog.Debug($"getsettings hash={bs.GetStoreSettings().GetHashCode()}");
                RSLog.Debug($"ThingStore hash = {bs.settings.GetHashCode()}");
                RSLog.Debug($"GetStore hash = {bs.GetStoreSettings().GetHashCode()}");
                RSLog.Debug($"storageGroup null = {bs.storageGroup == null}");
                RSLog.Debug($"ThingStore priority = {bs.settings.Priority}");
                RSLog.Debug($"GetStore priority = {bs.GetStoreSettings().Priority}");
                var slotGroup = bs.GetSlotGroup();

                RSLog.Debug(
                    $"=== CAPTURE SLOTGROUP ===\n" +
                    $"Thing: {thing}\n" +
                    $"Rotation: {thing.Rotation}\n" +
                    $"Cells: {string.Join(", ", slotGroup.CellsList)}");
            }
        }

        if (thing is Building_Cooler cooler)
            data.targetTemperature =
                cooler.compTempControl.targetTemperature;

        if (thing is Building_Heater heater)
            data.targetTemperature =
                heater.compTempControl.targetTemperature;

        if (thing is Building_PlantGrower grower)
            data.plantDef =
                grower.GetPlantDefToGrow();

        if (thing is Building_WorkTable table)
            data.bills =
                table.BillStack.Bills.ToList();

        var attached = GenConstruct.GetAttachedBuildings(thing);
        foreach (var at in attached)
        {
            data.attachedBuildings.Add(
                new AttachedBuildingData
                {
                    def = at.def,
                    stuff = at.Stuff,

                    position = at.Position,
                    rotation = at.Rotation,

                    hitPoints = at.HitPoints,
                    faction = at.Faction,

                    quality = at.TryGetComp<CompQuality>()?.Quality,

                    state = Capture(at, visited)
                });
        }

        return data;
    }

    public static void Apply(ReplaceData data, Thing thing)
    {
        RSLog.Debug(
            $"APPLY CALLED " +
            $"Thing={thing} " +
            $"Rot={thing.Rotation} " +
            $"Priority={data.storagePriority} " +
            $"Defs={data.storageFilter?.AllowedDefCount}");

        RSLog.Debug(
            $"=== APPLY ROTATION ===\n" +
            $"Thing: {thing}\n" +
            $"Captured rotation: {data?.rotation}\n" +
            $"Current rotation: {thing.Rotation}");

        if (data is null)
            return;

        thing.SetFactionDirect(data.faction);

        if (data.quality.HasValue && thing.TryGetComp<CompQuality>() is CompQuality cq)
        {
            cq.SetQuality(data.quality.Value, ArtGenerationContext.Colony);
        }

        if (data.targetTemperature.HasValue)
        {
            if (thing is Building_Cooler cooler)
                cooler.compTempControl.targetTemperature =
                    data.targetTemperature.Value;

            if (thing is Building_Heater heater)
                heater.compTempControl.targetTemperature =
                    data.targetTemperature.Value;
        }

        if (data.plantDef != null && thing is Building_PlantGrower grower)
        {
            grower.SetPlantDefToGrow(data.plantDef);
        }

        //if (data.bills != null && thing is Building_WorkTable table)
        //{
        //    foreach (Bill bill in data.bills)
        //        table.BillStack.AddBill(bill);
        //}

        if (data.bills != null && thing is Building_WorkTable table && table.BillStack.Count == 0)
        {
            foreach (Bill bill in data.bills)
                table.BillStack.AddBill(bill);
        }

        if (thing is IStoreSettingsParent storage)
        {
            RSLog.Debug($"Applying storage to {thing.def.defName}");
            var settings = storage.GetStoreSettings();
            //storage.Notify_SettingsChanged();

            RSLog.Debug($"=== STORAGE AFTER APPLY ===\n" +
                $"Thing: {thing}\n" +
                $"Rotation: {thing.Rotation}\n" +
                $"Priority: {settings.Priority}\n" +
                $"Allowed defs: {settings.filter.AllowedDefCount}");

            if (thing is Building_Storage bs)
            {
                RSLog.Debug(
                    $"storageGroup={(bs.storageGroup != null)}");

                RSLog.Debug(
                    $"settings hash={bs.settings.GetHashCode()}");

                RSLog.Debug(
                    $"getsettings hash={bs.GetStoreSettings().GetHashCode()}");
                var slotGroup = bs.GetSlotGroup();

                RSLog.Debug(
                    $"=== APPLY SLOTGROUP ===\n" +
                    $"Thing: {thing}\n" +
                    $"Rotation: {thing.Rotation}\n" +
                    $"Cells: {string.Join(", ", slotGroup.CellsList)}");

                foreach (var c in bs.AllSlotCellsList())
                {
                    RSLog.Debug($"CELL {c}");
                }
            }

            RSLog.Debug($"Before: {settings.Priority}");
            RSLog.Debug($"Capturing storage for {thing.def.defName}");
            RSLog.Debug($"Priority: {settings.Priority}");
            RSLog.Debug($"APPLY: allowed defs = {data.storageFilter.AllowedDefCount}");

            if (data.storageFilter != null)
                settings.filter.CopyAllowancesFrom(data.storageFilter);

            if (data.storagePriority.HasValue)
                settings.Priority = data.storagePriority.Value;

            if (thing is Building_Storage bsss)
            {
                RSLog.Debug($"ThingStore hash = {bsss.settings.GetHashCode()}");
                RSLog.Debug($"GetStore hash = {bsss.GetStoreSettings().GetHashCode()}");
                RSLog.Debug($"storageGroup null = {bsss.storageGroup == null}");
                RSLog.Debug($"ThingStore priority = {bsss.settings.Priority}");
                RSLog.Debug($"GetStore priority = {bsss.GetStoreSettings().Priority}");
            }

            //storage.Notify_SettingsChanged(); Probably not needed RW does it in StorageSettings.CopyFrom()
            RSLog.Debug($"Final check: {settings.Priority}, {settings.filter.AllowedDefCount}");

            if (data.storagePriority.HasValue && settings.Priority != data.storagePriority.Value)
            {
                settings.Priority = data.storagePriority.Value;
                RSLog.Debug($"Re-asserted Priority to {settings.Priority} after Notify.");
            }

            if (thing is Building_Storage bus)
            {
                var slotGroup = bus.GetSlotGroup();

                RSLog.Debug(
                    $"=== POST NOTIFY SLOTGROUP ===\n" +
                    $"Thing: {thing}\n" +
                    $"Rotation: {thing.Rotation}\n" +
                    $"Cells: {string.Join(", ", slotGroup.CellsList)}\n" +
                    $"Priority: {settings.Priority}\n" +
                    $"AllowedDefs: {settings.filter.AllowedDefCount}");
            }
            RSLog.Debug($"AFTER APPLY: building defs = {settings.filter.AllowedDefCount}");
            RSLog.Debug($"Pritory After: {settings.Priority}");
        }

        foreach (var attachment in data.attachedBuildings)
        {
            RSLog.Debug($"RESTORING ATTACHMENT {attachment.def.defName} at {attachment.position}");
            Thing newAttachment = ThingMaker.MakeThing(attachment.def, attachment.stuff);

            GenSpawn.Spawn(
                newAttachment,
                attachment.position,
                thing.Map,
                attachment.rotation,
                WipeMode.Vanish);

            newAttachment.SetFactionDirect(attachment.faction);
            newAttachment.RemoveFromStatWorkerCaches();
            newAttachment.Notify_ColorChanged();
            newAttachment.HitPoints = Mathf.Min(attachment.hitPoints, newAttachment.MaxHitPoints);

            if (attachment.quality.HasValue && newAttachment.TryGetComp<CompQuality>() is CompQuality aCq)
            {
                aCq.SetQuality(
                    attachment.quality.Value,
                    ArtGenerationContext.Colony);
            }

            if (attachment.state != null)
                Apply(attachment.state, newAttachment);

        }

    }
}