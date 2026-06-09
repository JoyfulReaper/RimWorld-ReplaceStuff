/*
 * REPLACE STUFF: Perfomance Edition
 * Copyright (c) 2026 Kyle Givler
 * Licensed under the MIT License.
 */

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

    public Rot4 rotation;

    public List<AttachedBuildingData> attachedBuildings = new();

    public string storageLabel;


    // Storage
    public ThingFilter storageFilter;

    public StoragePriority? storagePriority;

    public void ExposeData()
    {
        Scribe_Values.Look(ref storageLabel, "label");
        Scribe_References.Look(ref faction, "faction");
        Scribe_Values.Look(ref quality, "quality");
        Scribe_Values.Look(ref rotation, "rotation");
        Scribe_Values.Look(ref targetTemperature, "targetTemperature");
        Scribe_Defs.Look(ref plantDef, "plantDef");
        Scribe_Collections.Look(ref bills, "bills", LookMode.Deep);
        Scribe_Deep.Look(ref storageFilter, "storageFilter");
        Scribe_Values.Look(ref storagePriority, "storagePriority");
        Scribe_Collections.Look(ref attachedBuildings, "attachedBuildings", LookMode.Deep);
    }
}