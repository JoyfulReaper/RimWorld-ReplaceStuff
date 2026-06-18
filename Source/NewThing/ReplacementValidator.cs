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

using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Verse;

namespace Replace_Stuff.NewThing;

// TODO: This class doesn't belong here, move it
/// <summary>
/// A compatibility handler that uses reflection to detect "RimFridge" buildings.
/// It dynamically retrieves the 'DesiredTemp' field, allowing the mod to 
/// synchronize temperature settings between replaced refrigerators without 
/// requiring a hard dependency on the external mod.
/// </summary>
[StaticConstructorOnStartup]
public static class FridgeCompat
{
    public static Type fridgeType;
    public static FieldInfo DesiredTempInfo;
    static FridgeCompat()
    {
        try
        {
            fridgeType = AccessTools.TypeByName("Building_Refrigerator");
            if (fridgeType != null)
                DesiredTempInfo = AccessTools.Field(fridgeType, "DesiredTemp");
        }
        catch (System.Reflection.ReflectionTypeLoadException) //Aeh, this happens to people, should not happen, meh.
        {
            Verse.Log.Warning("Replace Stuff failed to check for RimFridges");
        }
    }
}

/// <summary>
/// The central registry and logic engine for structure replacements. 
/// It maintains a list of 'Replacement' rules and caches results to optimize 
/// performance.
/// </summary>
[StaticConstructorOnStartup]
public static class ReplacementValidator
{
    public static List<ReplacementRule> replacements;

    [Unsaved]
    private static readonly Dictionary<(ThingDef, ThingDef), bool> _replacementCache = new();

    [Unsaved]
    private static readonly Dictionary<int, Thing> thingReplacementCache = new Dictionary<int, Thing>();

    /// <summary>
    /// Defines specific replacement matching behaviors for various building categories.
    /// This registry acts purely as a validation layer to determine if one ThingDef 
    /// can be built over another (e.g., walls over walls, beds over beds).
    /// State transfer logic (bills, temperatures, ownership) has been migrated to 
    /// the ReplacementPipeline and BuildingStateTransfer systems.
    /// </summary>
    static ReplacementValidator()
    {
        replacements = new List<ReplacementRule>();

        // Only allow material replacement for buildings that actually need it
        // Walls, Fences, and similar structural elements
        replacements.Add(new ReplacementRule(
            d => d.IsWall() || (d.building?.isFence ?? false),
            o => o.IsWall() || (o.building?.isFence ?? false)
        ));

        //----------------------VALID REPLACEMENTS-----------------------

        // walls/fences/door
        replacements.Add(new ReplacementRule(d => d.IsWall() ||
            (d.building?.isPlaceOverableWall ?? false) ||
            (d.building?.isFence ?? false) ||
            typeof(Building_Door).IsAssignableFrom(d.thingClass)));

        // coolers
        replacements.Add(new ReplacementRule(d => typeof(Building_Cooler).IsAssignableFrom(d.thingClass)));
        //replacements.Add(new Replacement(d => typeof(Building_Cooler).IsAssignableFrom(d.thingClass),
        //    postAction: (n, o) =>
        //    {
        //        Building_Cooler newCooler = n as Building_Cooler;
        //        Building_Cooler oldCooler = o as Building_Cooler;
        //        //newCooler.compPowerTrader.PowerOn = oldCooler.compPowerTrader.PowerOn;	//should be flickable
        //        newCooler.compTempControl.targetTemperature = oldCooler.compTempControl.targetTemperature;
        //    }
        //    ));

        // beds
        static bool isBed(ThingDef d)
        {
            return typeof(Building_Bed).IsAssignableFrom(d.thingClass);

        }
        replacements.Add(new ReplacementRule(
            d => isBed(d) && d.GetStatValueAbstract(StatDefOf.WorkToBuild) > 0f,
            isBed
        ));
        //replacements.Add(new Replacement(
        //    d => isBed(d) && d.GetStatValueAbstract(StatDefOf.WorkToBuild) > 0f,
        //    isBed,
        //    preAction: (n, o) =>
        //    {
        //        Building_Bed newBed = n as Building_Bed;
        //        Building_Bed oldBed = o as Building_Bed;
        //        newBed.ForPrisoners = oldBed.ForPrisoners;
        //        newBed.Medical = oldBed.Medical;
        //        oldBed.OwnersForReading.ListFullCopy().ForEach(p => p.ownership.ClaimBedIfNonMedical(newBed));
        //    }
        //    ));

        // fences as a category (a mod)
        DesignationCategoryDef fencesDef = DefDatabase<DesignationCategoryDef>.GetNamed("Fences", false);
        if (fencesDef != null)
            replacements.Add(new ReplacementRule(d => d.designationCategory == fencesDef));

        // Just tables.
        replacements.Add(new ReplacementRule(d => d.IsTable));

        // Fridges from a mod
        replacements.Add(new ReplacementRule(d => d.thingClass == FridgeCompat.fridgeType));
        //replacements.Add(new Replacement(d => d.thingClass == FridgeCompat.fridgeType,
        //    postAction: (n, o) =>
        //    {
        //        FridgeCompat.DesiredTempInfo.SetValue(n, FridgeCompat.DesiredTempInfo.GetValue(o));
        //    }));

        // Allow all "plant growable items" to replace each other, and when they do attempt to set the growing plant type
        replacements.Add(new ReplacementRule(
            building => typeof(IPlantToGrowSettable).IsAssignableFrom(building.thingClass)));
        //replacements.Add(new Replacement(
        //    building => typeof(IPlantToGrowSettable).IsAssignableFrom(building.thingClass),
        //    postAction: (newItem, oldItem) =>
        //    {
        //        ((IPlantToGrowSettable)newItem).SetPlantDefToGrow(((IPlantToGrowSettable)oldItem).GetPlantDefToGrow());
        //    }));

        replacements.Add(new ReplacementRule(
            building => typeof(Building_Battery).IsAssignableFrom(building.thingClass)));

        // We can use placeWorkers and comps to check what kind of power is being generated so that we don't have to worry
        // about each item individually
        replacements.Add(new ReplacementRule(
            building => building.placeWorkers?.Any(placeWorker =>
                placeWorker == typeof(PlaceWorker_WatermillGenerator)) ?? false));
        replacements.Add(new ReplacementRule(
            building => building.placeWorkers?.Any(placeWorker =>
                placeWorker == typeof(PlaceWorker_WindTurbine)) ?? false));
        replacements.Add(new ReplacementRule(
            building => building.placeWorkers?.Any(placeWorker =>
                placeWorker == typeof(PlaceWorker_OnSteamGeyser)) ?? false));

        /* 1.6 added these as replaceTags (handled in CanReplace):
			replacements.Add(new Replacement(d => d.building?.isSittable ?? false));

			// Also requires PlaceWorker changes to match this
			replacements.Add(new Replacement(d =>
				(d.building?.isPowerConduit ?? false)
				|| typeof(Building_PowerSwitch).IsAssignableFrom(d.thingClass),
				o => o.building?.isPowerConduit ?? false));
			*/

        //---------------------------------------------
    }

    // Holds predicates and execution hooks
    public class ReplacementRule
    {
        private Predicate<ThingDef> _newCheck, _oldCheck;

        public ReplacementRule(Predicate<ThingDef> n, Predicate<ThingDef> o = null)
        {
            _newCheck = n;
            _oldCheck = o ?? n;
        }

        public bool Matches(ThingDef n, ThingDef o)
        {
            return n != null && o != null && _newCheck(n) && _oldCheck(o);
        }
    }

    public static bool CanReplace(this ThingDef newDef, ThingDef oldDef)
    {
        if (!oldDef.building?.IsDeconstructible ?? false)
            return false;

        newDef = GenConstruct.BuiltDefOf(newDef) as ThingDef;

        if (newDef == oldDef && !newDef.MadeFromStuff)
        {
            return false;
        }

        if (_replacementCache.TryGetValue((newDef, oldDef), out var result))
        {
            return result;
        }

        // 1.6 added some tags for replacement. Add them here so Replace Stuff does them in-place
        try
        {
            if (newDef != null)
            {
                if (GenConstruct.HasMatchingReplacementTag(newDef, oldDef))
                {
                    _replacementCache.Add((newDef, oldDef), true);
                    return true;
                }
            }
        }
#pragma warning disable CS0168 // Variable is declared but never used
        catch (Exception ex)
#pragma warning restore CS0168 // Variable is declared but never used
        {
            Debugger.Break();
        }

        foreach (var r in replacements)
        {
            if (!r.Matches(newDef, oldDef))
                continue;

            _replacementCache.Add((newDef, oldDef), true);
            return true;
        }

        _replacementCache.Add((newDef, oldDef), false);
        return false;
    }

    //public static void FinalizeNewThingReplace(this Thing newThing, Thing oldThing)
    //{
    //    // TODO: We transfer bills in the replacement pipeleine now. Verify and remove code
    //    //if (_replacementCache.TryGetValue((newThing.def, oldThing.def), out var result) && result)
    //    //{
    //    //    if (newThing is Building_WorkTable && oldThing is Building_WorkTable)
    //    //    {
    //    //        TransferBills(newThing, oldThing);
    //    //    }
    //    //    if (newThing is Building_Storage && oldThing is Building_Storage)
    //    //    {
    //    //        TransferStorageSettings(newThing, oldThing);
    //    //    }
    //    //}

    //    // FIXME
    //    for (int i = 0; i < replacements.Count; i++)
    //    {
    //        Replacement r = replacements[i];
    //        if (r.Matches(newThing.def, oldThing.def))
    //            r.Replace(newThing, oldThing);
    //    }
    //}

    //public static void PreFinalizeNewThingReplace(this Thing newThing, Thing oldThing)
    //{
    //    // FIXME
    //    for (int i = 0; i < replacements.Count; i++)
    //    {
    //        Replacement r = replacements[i];
    //        if (r.Matches(newThing.def, oldThing.def))
    //        {
    //            r.PreReplace(newThing, oldThing);
    //        }
    //    }
    //}

    // TODO: Verify this is handled correctly in the
    // replacement pipeline or storage engine now
    //private static void TransferBills(Thing n, Thing o)
    //{
    //    if (n is Building_WorkTable newTable && o is Building_WorkTable oldTable)
    //    {
    //        foreach (Bill bill in oldTable.BillStack)
    //        {
    //            newTable.BillStack.AddBill(bill);
    //        }
    //    }
    //}

    // TODO: Verify this is handled correctly in the
    // replacement pipeline or storage engine now
    ///// <summary>
    ///// Transfer Storage Settings between Things
    ///// </summary>
    ///// <param name="n">new store</param>
    ///// <param name="o">old store</param>
    //private static void TransferStorageSettings(Thing n, Thing o)
    //{
    //    if (n is not Building_Storage newStore || o is not Building_Storage oldStore)
    //        return;

    //    // Leverages vanilla's built-in event runner to execute after spawning loops finish
    //    LongEventHandler.ExecuteWhenFinished(() => newStore.settings.CopyFrom(oldStore.settings));
    //}

    public static bool TryFindTarget(this Thing newThing, out Thing oldThing)
    {
        oldThing = null;

        if (!newThing.Spawned)
            return false;

        int thingID = newThing.thingIDNumber;

        if (thingReplacementCache.TryGetValue(thingID, out oldThing))
            return oldThing != null && !oldThing.Destroyed;

        if (thingReplacementCache.Count > 500)
            thingReplacementCache.Clear();

        bool result = newThing.def.IsNewThingReplacement(newThing.Position, newThing.Rotation, newThing.Map, out oldThing);
        if (result && oldThing != null)
        {
            if (newThing.def == oldThing.def && newThing.Stuff == oldThing.Stuff)
            {
                oldThing = null;
                result = false;
            }
        }

        thingReplacementCache[thingID] = result ? oldThing : null;

        return result;
    }

    public static bool IsNewThingReplacement(this ThingDef newDef, IntVec3 pos, Rot4 rotation, Map map, out Thing oldThing)
    {
        if (map == null)
        {
            oldThing = null;
            return false;
        }

        foreach (IntVec3 checkPos in GenAdj.OccupiedRect(pos, rotation, newDef.Size))
        {
            foreach (Thing oThing in checkPos.GetThingList(map))
            {
                if (!newDef.CanReplace(oThing.def))
                    continue;

                // Don't replace identical stuff buildings.
                if (newDef.MadeFromStuff &&
                    oThing.Stuff != null &&
                    newDef == oThing.def &&
                    newDef.entityDefToBuild == null)
                {
                    continue;
                }

                oldThing = oThing;
                return true;
            }
        }

        oldThing = null;
        return false;
    }

    public static bool CanReplace(this Thing newThing, Thing oldThing)
    {
        return newThing.def.CanReplace(oldThing.def);
    }
}
