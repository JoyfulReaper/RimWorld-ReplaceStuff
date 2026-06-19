/*
 * REPLACE STUFF: Performance Edition
 * Copyright (c) 2026 Kyle Givler
 * Licensed under the MIT License.
 */

using Replace_Stuff.Compatibility;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Replace_Stuff.Replace;

public class ReplaceData : IExposable
{
    public Faction faction;
    public QualityCategory? quality;
    public float? targetTemperature;
    public ThingDef plantDef;
    public List<Bill> bills;
    public List<string> compHandlers = new();
    public Rot4 rotation;
    public List<AttachedBuildingData> attachedBuildings = new();

    // Dynamic storage for third-party mod handlers
    public Dictionary<string, string> modData = new();

    // Storage
    public string storageLabel;
    public StoragePriority? storagePriority;
    public StorageSettings storageSettings;
    public bool belongedToGroup;

    public static ReplaceData FromThing(Thing thing)
    {
        var data = new ReplaceData();

        // Core properties
        data.faction = thing.Faction;
        data.rotation = thing.Rotation;

        // Optional/Comp-based properties
        if (thing.TryGetComp<CompTempControl>() is CompTempControl temp)
            data.targetTemperature = temp.targetTemperature;

        if (thing is IPlantToGrowSettable grower)
            data.plantDef = grower.GetPlantDefToGrow();

        // Storage logic
        if (thing is IStoreSettingsParent store)
        {
            var oldSettings = store.GetStoreSettings();

            // Create a new instance so we own the data, not the building
            data.storageSettings = new StorageSettings();
            data.storageSettings.CopyFrom(oldSettings);

            // Now our DTO is safe even if the building is destroyed
            data.storagePriority = oldSettings.Priority;
        }

        // Comp handlers logic
        if (thing is ThingWithComps thingWithComps)
        {
            foreach (var comp in thingWithComps.AllComps)
            {
                var handlerKey = ReplacementRegistry.GetKeyForComp(comp);
                if (!string.IsNullOrEmpty(handlerKey))
                {
                    data.compHandlers.Add(handlerKey);
                }
            }
        }

        return data;
    }

    public void ExposeData()
    {
        Scribe_Values.Look(ref storageLabel, "label");
        Scribe_References.Look(ref faction, "faction");
        Scribe_Values.Look(ref quality, "quality");
        Scribe_Values.Look(ref rotation, "rotation");
        Scribe_Values.Look(ref targetTemperature, "targetTemperature");
        Scribe_Defs.Look(ref plantDef, "plantDef");
        Scribe_Collections.Look(ref bills, "bills", LookMode.Deep);
        Scribe_Collections.Look(ref compHandlers, "compHandlers", LookMode.Value);
        Scribe_Collections.Look(ref modData, "modData", LookMode.Value, LookMode.Value);
        Scribe_Deep.Look(ref storageSettings, "settings");
        Scribe_Values.Look(ref storagePriority, "storagePriority");
        Scribe_Collections.Look(ref attachedBuildings, "attachedBuildings", LookMode.Deep);
        Scribe_Values.Look(ref belongedToGroup, "belongedToGroup");

        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            modData ??= new Dictionary<string, string>();
        }
    }
}
