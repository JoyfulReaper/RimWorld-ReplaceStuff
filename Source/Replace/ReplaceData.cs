using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Replace_Stuff.DestroyedRestore;

public class ReplaceData : IExposable
{
    public Faction faction;

    public QualityCategory? quality;

    public float? targetTemperature;

    public ThingDef plantDef;

    public List<Bill> bills;

    // Storage
    public ThingFilter storageFilter;

    public StoragePriority? storagePriority;


    public void ExposeData()
    {
        Scribe_References.Look(ref faction, "faction");
        Scribe_Values.Look(ref quality, "quality");
        Scribe_Values.Look(ref targetTemperature, "targetTemperature");
        Scribe_Defs.Look(ref plantDef, "plantDef");
        Scribe_Collections.Look(ref bills, "bills", LookMode.Deep);
    }
}