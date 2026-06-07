using Replace_Stuff.Utilities;
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
            RSLog.Debug($"CAPTURE {thing.def.defName} implements IStoreSettingsParent = {thing is IStoreSettingsParent}");

            ReplaceData data = new();

            RSLog.Debug($"Filter null={data.storageFilter == null}");


            data.faction = thing.Faction;
            data.rotation = thing.Rotation;

            if (thing.TryGetComp<CompQuality>() is CompQuality q)
                data.quality = q.Quality;

            if (thing is IStoreSettingsParent storage)
            {
                RSLog.Debug("CAPTURE STORAGE");

                StorageSettings settings = storage.GetStoreSettings();

                data.storagePriority = settings.Priority;
                data.storageFilter = new ThingFilter();
                data.storageFilter.CopyAllowancesFrom(settings.filter);

                RSLog.Debug($"CAPTURE: allowed defs = {data.storageFilter.AllowedDefCount}");

                RSLog.Debug($"CAPTURE: priority = {data.storagePriority}");

                RSLog.Debug(
                    $"Captured defs={data.storageFilter.AllowedDefCount}"
                );
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
            RSLog.Debug($"APPLY {thing}");

            if (data is null)
                return;

            RSLog.Debug($"Applying filter null={data.storageFilter == null}");

            thing.SetFactionDirect(data.faction);
            thing.Rotation = data.rotation;

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
                RSLog.Debug($"Applying storage to {thing.def.defName}");
                StorageSettings settings = storage.GetStoreSettings();

                RSLog.Debug($"Before: {settings.Priority}");
                RSLog.Debug($"Capturing storage for {thing.def.defName}");
                RSLog.Debug($"Priority: {settings.Priority}");

                if (data.storagePriority.HasValue)
                    settings.Priority = data.storagePriority.Value;

                RSLog.Debug($"APPLY: allowed defs = {data.storageFilter.AllowedDefCount}");

                if (data.storageFilter != null)
                    settings.filter.CopyAllowancesFrom(data.storageFilter);

                storage.Notify_SettingsChanged();

                RSLog.Debug($"AFTER APPLY: building defs = {settings.filter.AllowedDefCount}");
                RSLog.Debug($"After: {settings.Priority}");
            }
        }
    }
}