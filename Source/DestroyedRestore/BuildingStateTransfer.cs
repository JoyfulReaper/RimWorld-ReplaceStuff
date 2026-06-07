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
            Log.Message($"CAPTURE {thing.def.defName} implements IStoreSettingsParent = {thing is IStoreSettingsParent}");

            ReplaceData data = new();

            Log.Message($"Filter null={data.storageFilter == null}");


            data.faction = thing.Faction;
            data.rotation = thing.Rotation;

            if (thing.TryGetComp<CompQuality>() is CompQuality q)
                data.quality = q.Quality;

            if (thing is IStoreSettingsParent storage)
            {
                Log.Message("CAPTURE STORAGE");

                StorageSettings settings = storage.GetStoreSettings();

                data.storagePriority = settings.Priority;
                data.storageFilter = new ThingFilter();
                data.storageFilter.CopyAllowancesFrom(settings.filter);

                Log.Message($"CAPTURE: allowed defs = {data.storageFilter.AllowedDefCount}");

                Log.Message($"CAPTURE: priority = {data.storagePriority}");

                Log.Message(
                    $"Captured defs={data.storageFilter.AllowedDefCount}"
                );
            }

            //if (thing is IStoreSettingsParent storage)
            //{
            //    StorageSettings settings = storage.GetStoreSettings();

            //    Log.Message($"{ReplaceStuffPrefomanceMod.settings.debugPrefix} Capturing storage for {thing.def.defName}");
            //    Log.Message($"{ReplaceStuffPrefomanceMod.settings.debugPrefix} Priority: {settings.Priority}");

            //    data.storagePriority = settings.Priority;

            //    data.storageFilter = new ThingFilter();
            //    data.storageFilter.CopyAllowancesFrom(settings.filter);
            //}

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
            Log.Message($"APPLY {thing}");

            if (data is null)
                return;

            Log.Message($"Applying filter null={data.storageFilter == null}");

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
                Log.Message($"[ReplaceStuffPerfomance] Applying storage to {thing.def.defName}");
                StorageSettings settings = storage.GetStoreSettings();

                Log.Message($"[ReplaceStuffPerfomance] Before: {settings.Priority}");

                Log.Message($"[ReplaceStuffPerfomance] Capturing storage for {thing.def.defName}");
                Log.Message($"[ReplaceStuffPerfomance] Priority: {settings.Priority}");

                if (data.storagePriority.HasValue)
                    settings.Priority = data.storagePriority.Value;

                Log.Message($"APPLY: allowed defs = {data.storageFilter.AllowedDefCount}");

                if (data.storageFilter != null)
                    settings.filter.CopyAllowancesFrom(data.storageFilter);

                storage.Notify_SettingsChanged();

                Log.Message($"AFTER APPLY: building defs = {settings.filter.AllowedDefCount}");

                Log.Message($"[ReplaceStuffPerfomance] After: {settings.Priority}");
            }
        }
    }
}