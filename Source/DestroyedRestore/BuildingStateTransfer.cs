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
        // TODO: Should the quality depend on the worker rebuilding the thing?
        if (thing.TryGetComp<CompQuality>() is CompQuality qc)
            data.quality = qc.Quality;

        // Bill stacks
        if (thing is Building_WorkTable table)
            data.bills = table.BillStack.Bills.ToList();

        // Storage tracking 
        if (thing is Building_Storage)
        {
            CaptureStorageItems(data, thing);
        }
        else if (thing is IStoreSettingsParent)
        {
            CaptureStorageFiltersAndPriority(data, thing);
        }

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


        CaptureAttachements(data, thing, visited);

        return data;
    }

    private static void CaptureAttachements(ReplaceData data, Thing thing, HashSet<int> visited)
    {
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
    }

    private static void CaptureStorageFiltersAndPriority(ReplaceData data, Thing thing)
    {
        // Storage filters/pirority
        if (thing is IStoreSettingsParent storageParent)
        {
            RSLog.Debug("CAPTURE STORAGE");
            var settings = storageParent.GetStoreSettings();
            RSLog.Debug($"DEBUG: Before copy - Priority: {settings?.Priority.ToString() ?? "NULL"}, Allowed: {settings?.filter?.AllowedDefCount.ToString() ?? "NULL"}");

            data.storageFilter = new ThingFilter();
            data.storageFilter.CopyAllowancesFrom(settings.filter);
            data.storagePriority = settings.Priority;

            RSLog.Debug(
                $"CAPTURE IstoreSettingsParent:" +
                // $" StorageGroup={data.St" +
                $" oldRot={data.rotation} " +
                $" newRot={thing.Rotation} " +
                $" settings={settings?.GetHashCode()} " +
                $" priorityBefore={settings.Priority}");
        }
    }


    private static void CaptureStorageItems(ReplaceData data, Thing thing)
    {
        // Stored items
        if (thing is Building_Storage storage)
        {
            data.settings = new StorageSettings();
            data.settings.CopyFrom(storage.GetStoreSettings());

            StorageGroup currentGroup = GetStorageGroup(storage);
            if (currentGroup != null)
            {
                data.storageLabel = currentGroup.RenamableLabel;
                data.belongedToGroup = true;
            }
            else
            {
                data.belongedToGroup = false;
            }
        }
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


        // Quality
        if (data.quality.HasValue && thing.TryGetComp<CompQuality>() is CompQuality cq)
        {
            cq.SetQuality(data.quality.Value, ArtGenerationContext.Colony);
        }


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

        // Storage restoration 
        if (thing is Building_Storage)
        {
            ApplyStorageItems(data, thing);
        }
        else if (thing is IStoreSettingsParent)
        {
            ApplyStorageFiltersAndPriority(data, thing);
        }

        ApplyAttachements(data, thing);
    }

    public static void ApplyAttachements(ReplaceData data, Thing thing)
    {
        // Attachments (ex Wall Lamps)
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

    public static void ApplyStorageFiltersAndPriority(ReplaceData data, Thing thing)
    {
        // Storage filters/priority
        if (thing is IStoreSettingsParent storageParent)
        {
            var settings = storageParent.GetStoreSettings();

            if (data.storageFilter != null)
                settings.filter.CopyAllowancesFrom(data.storageFilter);

            if (data.storagePriority.HasValue)
                settings.Priority = data.storagePriority.Value;

            if (thing is Building_Storage concreteStorage)
            {
                concreteStorage.Notify_SettingsChanged();
            }
        }
    }

    public static void ApplyStorageItems(ReplaceData data, Thing thing)
    {
        // Stored items & Custom Storage Naming
        if (thing is Building_Storage storage)
        {
            if (data.belongedToGroup && !string.IsNullOrEmpty(data.storageLabel))
            {
                // Check if the group already exists on the map (multi-shelf setup)
                StorageGroup existingGroup = storage.Map?.storageGroups?.StorageGroupsForReading
                    .FirstOrDefault(g => g.RenamableLabel == data.storageLabel);

                if (existingGroup != null)
                {
                    // Join the existing group
                    if (!existingGroup.members.Contains(storage))
                    {
                        existingGroup.members.Add(storage);
                    }
                    SetStorageGroup(storage, existingGroup);
                    existingGroup.Notify_SettingsChanged();
                }
                else
                {
                    // Group disbanded because it dropped below 2 members.
                    // Locate the orphaned companion to safely forge a valid 2-member group.
                    Building_Storage companion = FindOrphanedCompanion(storage.Map, storage.GroupingLabel, storage.Position, storage.def);

                    if (companion != null)
                    {
                        StorageGroup newGroup = storage.Map.storageGroups.NewGroup(data.storageLabel);

                        // Force the label onto the IRenamable property
                        newGroup.RenamableLabel = data.storageLabel;

                        // Bind both to satisfy the engine's >1 member invariant
                        if (!newGroup.members.Contains(companion)) newGroup.members.Add(companion);
                        if (!newGroup.members.Contains(storage)) newGroup.members.Add(storage);

                        SetStorageGroup(companion, newGroup);
                        SetStorageGroup(storage, newGroup);

                        if (data.settings != null)
                        {
                            newGroup.GetStoreSettings().CopyFrom(data.settings);
                        }
                        else if (data.storageFilter != null)
                        {
                            var settings = newGroup.GetStoreSettings();
                            settings.filter.CopyAllowancesFrom(data.storageFilter);
                            if (data.storagePriority.HasValue)
                                settings.Priority = data.storagePriority.Value;
                        }

                        newGroup.Notify_SettingsChanged();
                    }
                    else
                    {
                        // EDGE CASE: No companion survived. 
                        // DO NOT create a StorageGroup. Gracefully downgrade to a standalone shelf.
                        var settings = storage.GetStoreSettings();
                        if (data.settings != null)
                        {
                            settings.CopyFrom(data.settings);
                        }
                        else if (data.storageFilter != null)
                        {
                            settings.filter.CopyAllowancesFrom(data.storageFilter);
                            if (data.storagePriority.HasValue)
                                settings.Priority = data.storagePriority.Value;
                        }
                        storage.Notify_SettingsChanged();
                    }
                }
            }
        }
    }

    private static Building_Storage FindOrphanedCompanion(Map map, string groupLabel, IntVec3 currentLoc, ThingDef storageDef)
    {
        if (map == null) return null;

        foreach (var building in map.listerBuildings.AllBuildingsColonistOfClass<Building_Storage>())
        {
            if (building.def == storageDef &&
                GetStorageGroup(building) == null &&
                building.Position.DistanceToSquared(currentLoc) <= 25)
            {
                return building;
            }
        }
        return null;
    }

    #region Reflection Helpers
    private static StorageGroup GetStorageGroup(Building_Storage storage)
    {
        // Try property first
        var prop = storage.GetType().GetProperty("StorageGroup", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        if (prop != null) return
                prop.GetValue(storage) as StorageGroup;

        // Fallback to internal/private backing field
        var field = storage.GetType().GetField("storageGroup", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return
            field?.GetValue(storage) as StorageGroup;
    }

    private static void SetStorageGroup(IStorageGroupMember member, StorageGroup group)
    {
        var prop = member.GetType().GetProperty("StorageGroup", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        if (prop != null && prop.CanWrite)
        {
            prop.SetValue(member, group);
            return;
        }

        var field = member.GetType().GetField("storageGroup", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field?.SetValue(member, group);
    }
    #endregion
}