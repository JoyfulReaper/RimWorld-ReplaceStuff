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

using Replace_Stuff.Replace;
using Replace_Stuff.Utilities;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Replace_Stuff.DestroyedRestore;

[StaticConstructorOnStartup]
public static class BuildingStateTransfer
{
    static BuildingStateTransfer() { }

    public static ReplaceData Capture(Thing thing, HashSet<int> visited)
    {
        RSLog.Debug($"CAPTURE {thing.def.defName} implements IStoreSettingsParent = {thing is IStoreSettingsParent}");

        if (!visited.Add(thing.thingIDNumber))
            return null;

        // Faction and rotation
        ReplaceData data = new()
        {
            faction = thing.Faction,
            rotation = thing.Rotation
        };

        // Quality
        if (thing.TryGetComp<CompQuality>() is CompQuality qc)
            data.quality = qc.Quality;

        // Bill stacks
        if (thing is Building_WorkTable table)
            data.bills = table.BillStack.Bills.ToList();

        // Storage filters/pirority
        if (thing is IStoreSettingsParent storageParent)
        {
            RSLog.Debug("CAPTURE STORAGE");
            var settings = storageParent.GetStoreSettings();
            RSLog.Debug($"DEBUG: Before copy - Priority: {settings?.Priority.ToString() ?? "NULL"}, Allowed: {settings?.filter?.AllowedDefCount.ToString() ?? "NULL"}");

            data.storageFilter = new ThingFilter();
            data.storageFilter.CopyAllowancesFrom(settings.filter);
            data.storagePriority = settings.Priority;


            //var t = data.Sto

            RSLog.Debug(
                $"CAPTURE IstoreSettingsParent:" +
                // $" StorageGroup={data.St" +
                $" oldRot={data.rotation} " +
                $" newRot={thing.Rotation} " +
                $" settings={settings?.GetHashCode()} " +
                $" priorityBefore={settings.Priority}");
        }

        // Stored items
        if (thing is Building_Storage storage)
            data.storageLabel = storage.label;

        // Coolers TODO these can be combined with heater
        if (thing is Building_Cooler cooler)
            data.targetTemperature =
                cooler.compTempControl.targetTemperature;

        // Heaters
        if (thing is Building_Heater heater)
            data.targetTemperature =
                heater.compTempControl.targetTemperature;

        // Growers
        if (thing is Building_PlantGrower grower)
            data.plantDef =
                grower.GetPlantDefToGrow();


        // Attachments (ex: Wall Lamps)
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
        //LOG storage parent, priorityallowed defs and rotation
        RSLog.Debug(
            $"APPLY CALLED " +
            $"Thing={thing} " +
            $"Rot={thing.Rotation} " +
            $"Priority={data.storagePriority} " +
            $"Defs={data.storageFilter?.AllowedDefCount}");


        if (data is null)
            return;

        thing.SetFactionDirect(data.faction);

        // Quality
        //if (data.quality.HasValue && thing.TryGetComp<CompQuality>() is CompQuality cq)
        //{
        //    cq.SetQuality(data.quality.Value, ArtGenerationContext.Colony);
        //}


        //// Target temperature
        //if (data.targetTemperature.HasValue)
        //{
        //    if (thing is Building_Cooler cooler)
        //        cooler.compTempControl.targetTemperature =
        //            data.targetTemperature.Value;

        //    if (thing is Building_Heater heater)
        //        heater.compTempControl.targetTemperature =
        //            data.targetTemperature.Value;
        //}

        //// Growers
        //if (data.plantDef != null && thing is Building_PlantGrower grower)
        //{
        //    grower.SetPlantDefToGrow(data.plantDef);
        //}

        ////if (data.bills != null && thing is Building_WorkTable table)
        ////{
        ////    foreach (Bill bill in data.bills)
        ////        table.BillStack.AddBill(bill);
        ////}

        //// Bill stacks
        //if (data.bills != null && thing is Building_WorkTable table && table.BillStack.Count == 0)
        //{
        //    foreach (Bill bill in data.bills)
        //        table.BillStack.AddBill(bill);
        //}

        // Storage filters/pirority
        if (thing is IStoreSettingsParent storageParent)
        {
            var settings = storageParent.GetStoreSettings();
            RSLog.Debug($"Apply(): ");

            RSLog.Debug(
                $"APPLY IS IstorageParent:" +
                $" oldRot={data.rotation} " +
                $" newRot={thing.Rotation} " +
                $" settings={settings?.GetHashCode()} " +
                $" priorityBefore={settings.Priority}");


            if (data.storageFilter != null)
                settings.filter.CopyAllowancesFrom(data.storageFilter);

            if (data.storagePriority.HasValue)
                settings.Priority = data.storagePriority.Value;
        }

        //// Stored items
        //if (thing is Building_Storage storage)
        //    storage.label = data.storageLabel;

        //// Attachments (ex Wall Lamps)
        //foreach (var attachment in data.attachedBuildings)
        //{
        //    RSLog.Debug($"RESTORING ATTACHMENT {attachment.def.defName} at {attachment.position}");
        //    Thing newAttachment = ThingMaker.MakeThing(attachment.def, attachment.stuff);

        //    GenSpawn.Spawn(
        //        newAttachment,
        //        attachment.position,
        //        thing.Map,
        //        attachment.rotation,
        //        WipeMode.Vanish);

        //    newAttachment.SetFactionDirect(attachment.faction);
        //    newAttachment.RemoveFromStatWorkerCaches();
        //    newAttachment.Notify_ColorChanged();
        //    newAttachment.HitPoints = Mathf.Min(attachment.hitPoints, newAttachment.MaxHitPoints);

        //    if (attachment.quality.HasValue && newAttachment.TryGetComp<CompQuality>() is CompQuality aCq)
        //    {
        //        aCq.SetQuality(
        //            attachment.quality.Value,
        //            ArtGenerationContext.Colony);
        //    }

        //    if (attachment.state != null)
        //        Apply(attachment.state, newAttachment);
        //}
    }
}