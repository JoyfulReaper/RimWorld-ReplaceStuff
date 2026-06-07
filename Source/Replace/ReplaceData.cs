using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Replace_Stuff.DestroyedRestore;

public class ReplaceData : IExposable
{
    public StorageSettings storageSettings; // Currently unused

    public QualityCategory? quality;

    public float? targetTemperature;

    public ThingDef plantDef;

    public List<Bill> bills;

    public Faction faction;

    public void ExposeData()
    {
        Scribe_Values.Look(ref targetTemperature, "targetTemperature");
        Scribe_Values.Look(ref quality, "quality");

        Scribe_Defs.Look(ref plantDef, "plantDef");
        Scribe_References.Look(ref faction, "faction");

        Scribe_Collections.Look(ref bills,
            "bills",
            LookMode.Deep);

        Scribe_Deep.Look(ref storageSettings,
            "storageSettings");
    }
}