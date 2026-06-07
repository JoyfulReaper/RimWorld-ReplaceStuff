using RimWorld;
using System.Linq;
using Verse;

namespace Replace_Stuff.DestroyedRestore
{
    [StaticConstructorOnStartup]
    public static class BuildingStateTransfer
    {
        static BuildingStateTransfer() { }

        public static ReplaceData Capture(Thing thing)
        {
            ReplaceData data = new();

            data.faction = thing.Faction;

            if (thing.TryGetComp<CompQuality>() is CompQuality q)
                data.quality = q.Quality;

            if (thing is IStoreSettingsParent storage)
            {
                StorageSettings settings = storage.GetStoreSettings();

                data.storagePriority = settings.Priority;

                data.storageFilter = new ThingFilter();
                data.storageFilter.CopyAllowancesFrom(settings.filter);
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
                StorageSettings settings = storage.GetStoreSettings();

                if (data.storagePriority.HasValue)
                    settings.Priority = data.storagePriority.Value;

                if (data.storageFilter != null)
                    settings.filter.CopyAllowancesFrom(data.storageFilter);

                storage.Notify_SettingsChanged();
            }
        }
    }
}