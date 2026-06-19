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

using Replace_Stuff.Data;
using Replace_Stuff.Utilities;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace Replace_Stuff.Replace;

/// <summary>
/// Handles the extraction and restoration of items inside storage buildings 
/// during the replacement process.
/// </summary>
internal static class StorageReplacementEngine
{
    /// <summary>
    /// Extracts items from a storage building before it is destroyed.
    /// </summary>
    /// <param name="targetThing">The old building being replaced.</param>
    /// <returns>A list of items stored in the building, or null if not a storage building.</returns>
    internal static List<Thing> ExtractStoredItems(Thing targetThing)
    {
        if (targetThing is Building_Storage storage)
        {
            return ReplacementUtility.ExtractStoredThings(storage);
        }

        return null;
    }

    /// <summary>
    /// Restores items to the newly created storage building.
    /// </summary>
    /// <param name="newThing">The new building that was just spawned.</param>
    /// <param name="storedThings">The list of items extracted from the old building.</param>
    internal static void RestoreStoredItems(Thing newThing, List<Thing> storedThings)
    {
        // Only attempt restore if we actually extracted items and the new building supports storage
        if (storedThings != null && newThing is Building_Storage storage)
        {
            ReplacementUtility.RestoreStoredThings(storage, storedThings);
        }
    }

    internal static void CaptureStorageFiltersAndPriority(ReplacementData data, Thing thing)
    {
        // Storage filters/pirority
        if (thing is IStoreSettingsParent storageParent)
        {
            RSLog.Debug("CAPTURE STORAGE");
            var settings = storageParent.GetStoreSettings();
            RSLog.Debug($"DEBUG: Before copy - Priority: {settings?.Priority.ToString() ?? "NULL"}, Allowed: {settings?.filter?.AllowedDefCount.ToString() ?? "NULL"}");

            data.storageSettings = new StorageSettings();
            data.storageSettings.CopyFrom(settings);

            RSLog.Debug(
                $"CAPTURE IstoreSettingsParent:" +
                $" oldRot={data.rotation} " +
                $" newRot={thing.Rotation} " +
                $" settings={settings?.GetHashCode()} " +
                $" priorityBefore={settings?.Priority.ToString() ?? "NULL"}");
        }
    }

    internal static void CaptureStorageItems(ReplacementData data, Thing thing)
    {
        // Stored items
        if (thing is Building_Storage storage)
        {
            data.storageSettings = new StorageSettings();
            data.storageSettings.CopyFrom(storage.GetStoreSettings());
            data.storagePriority = data.storageSettings.Priority;

            var currentGroup = GetStorageGroup(storage);
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

    internal static void ApplyStorageItems(ReplacementData data, Thing thing)
    {
        // Stored items & Custom Storage Naming
        if (thing is Building_Storage storage)
        {
            if (!data.belongedToGroup || string.IsNullOrEmpty(data.storageLabel))
            {
                ApplyCapturedSettings(data, storage);
                return;
            }

            if (data.belongedToGroup && !string.IsNullOrEmpty(data.storageLabel))
            {
                // Check if the group already exists on the map (multi-shelf setup)
                var existingGroup = storage.Map?.storageGroups?.StorageGroupsForReading
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
                    // Group disbanded. Locate the orphaned companion to safely forge a valid 2-member group.
                    var companion = FindOrphanedCompanion(storage.Map, data.storageLabel, storage.GroupingLabel, storage.Position, storage.Rotation, storage.def);
                    if (companion is not null)
                    {
                        var newGroup = storage.Map.storageGroups.NewGroup(data.storageLabel);
                        newGroup.RenamableLabel = data.storageLabel;

                        if (!newGroup.members.Contains(companion))
                            newGroup.members.Add(companion);
                        if (!newGroup.members.Contains(storage))
                            newGroup.members.Add(storage);

                        SetStorageGroup(companion, newGroup);
                        SetStorageGroup(storage, newGroup);

                        // Use the unified settings object
                        ApplyCapturedSettings(data, newGroup.GetStoreSettings());

                        newGroup.Notify_SettingsChanged();
                    }
                    else
                    {
                        // No companion survived. Downgrade to a standalone shelf.
                        ApplyCapturedSettings(data, storage);
                    }
                }
            }
        }
    }

    private static void ApplyCapturedSettings(ReplacementData data, Building_Storage storage)
    {
        ApplyCapturedSettings(data, storage.GetStoreSettings());
        storage.Notify_SettingsChanged();
    }

    private static void ApplyCapturedSettings(ReplacementData data, StorageSettings settings)
    {
        if (data.storageSettings is not null)
        {
            settings.CopyFrom(data.storageSettings);
        }

        if (data.storagePriority.HasValue)
        {
            settings.Priority = data.storagePriority.Value;
        }
    }

    private static Building_Storage FindOrphanedCompanion(Map map, string expectedLabel, string groupLabel, IntVec3 currentLoc, Rot4 rotation, ThingDef storageDef)
    {
        if (map == null) return null;

        foreach (var building in map.listerBuildings.AllBuildingsColonistOfClass<Building_Storage>())
        {
            if (building.def == storageDef &&
                GetStorageGroup(building) == null &&
                building.Position.DistanceToSquared(currentLoc) <= 25 &&
                LabelsMatch(expectedLabel, groupLabel, GetGroupingLabel(building)))
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

    private static string GetGroupingLabel(Building_Storage storage)
    {
        var prop = storage.GetType().GetProperty("GroupingLabel", BindingFlags.Public | BindingFlags.Instance);
        if (prop != null)
            return prop.GetValue(storage) as string;

        var field = storage.GetType().GetField("groupingLabel", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        return field?.GetValue(storage) as string;
    }

    private static bool LabelsMatch(string expectedLabel, string originalGroupLabel, string candidateLabel)
    {
        if (string.IsNullOrEmpty(expectedLabel) && string.IsNullOrEmpty(originalGroupLabel))
            return !string.IsNullOrEmpty(candidateLabel);

        return (!string.IsNullOrEmpty(expectedLabel) && candidateLabel == expectedLabel)
            || (!string.IsNullOrEmpty(originalGroupLabel) && candidateLabel == originalGroupLabel);
    }
    #endregion
}