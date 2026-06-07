using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Replace_Stuff.DestroyedRestore
{
    [StaticConstructorOnStartup]
    public static class BuildingStateTransfer
    {
        public static Dictionary<Type, Action<Thing, Thing>> handlers;
        static BuildingStateTransfer()
        {
            handlers = new Dictionary<Type, Action<Thing, Thing>>();

            //Here are the types 
            handlers[typeof(Building_WorkTable)] = delegate (Thing fromThing, Thing toThing)
            {
                if (fromThing is Building_WorkTable from && toThing is Building_WorkTable to)
                    foreach (Bill bill in from.BillStack)
                        to.BillStack.AddBill(bill);
            };
            handlers[typeof(Building_Cooler)] = delegate (Thing fromThing, Thing toThing)
            {
                if (fromThing is Building_Cooler from && toThing is Building_Cooler to)
                    to.compTempControl.targetTemperature = from.GetComp<CompTempControl>().targetTemperature;
            };
            handlers[typeof(Building_Heater)] = delegate (Thing fromThing, Thing toThing)
            {
                if (fromThing is Building_Heater from && toThing is Building_Heater to)
                    to.compTempControl.targetTemperature = from.GetComp<CompTempControl>().targetTemperature;
            };
            handlers[typeof(Building_PlantGrower)] = delegate (Thing fromThing, Thing toThing)
            {
                if (fromThing is Building_PlantGrower from && toThing is Building_PlantGrower to)
                    to.SetPlantDefToGrow(from.GetPlantDefToGrow());
            };
            //handlers[typeof(Building_Storage)] = delegate (Thing fromThing, Thing toThing)
            //{
            //	if (fromThing is Building_Storage from && toThing is Building_Storage to)
            //		to.settings.CopyFrom(from.GetStoreSettings());
            //};
            //If building_bed didn't forget their owner it'd be easier;
            //otherwise the entire system could save an object instead of the Building before it despawns. 

            //CompProperties_Refuelable if targetFuelLevelConfigurable
        }

        public static ReplaceData Capture(Thing thing)
        {
            ReplaceData data = new();

            data.faction = thing.Faction;

            if (thing.TryGetComp<CompQuality>() is CompQuality q)
                data.quality = q.Quality;

            if (thing is IStoreSettingsParent storage)
                data.storageSettings =
                    storage.GetStoreSettings();

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

            return data;
        }

        public static void Apply(ReplaceData data, Thing thing)
        {
            if (data is null)
                return;

            thing.SetFactionDirect(data.faction);

            if (data.quality.HasValue && thing.TryGetComp<CompQuality>() is CompQuality q)
            {
                q.SetQuality(
                    data.quality.Value,
                    ArtGenerationContext.Colony);
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

            if (data.bills != null && thing is Building_WorkTable table)
            {
                foreach (Bill bill in data.bills)
                    table.BillStack.AddBill(bill);
            }

            if (data.storageSettings != null && thing is IStoreSettingsParent storage)
            {
                StorageSettings tmp = new StorageSettings(storage);
                tmp.CopyFrom(storage.GetStoreSettings());

                data.storageSettings = tmp;

                storage.GetStoreSettings()
                    .CopyFrom(data.storageSettings);
            }
        }

        public static bool CanDo(Thing thing)
        {
            return FindHandler(thing) != null;
        }

        public static void Transfer(Thing from, Thing to)
        {
            FindHandler(from)?.Invoke(from, to);
        }

        private static Action<Thing, Thing> FindHandler(Thing thing)
        {
            Type actual = thing.GetType();

            foreach (var pair in handlers)
            {
                if (pair.Key.IsAssignableFrom(actual))
                    return pair.Value;
            }

            return null;
        }
    }
}