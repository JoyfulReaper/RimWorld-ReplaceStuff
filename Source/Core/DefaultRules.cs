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

using Replace_Stuff.Compatibility;
using Replace_Stuff.OverWallCoolers;
using RimWorld;
using Verse;

// using Replace_Stuff.Compatibility.ThirdParty; DO NOT IMPORT THIS HERE
// NOTE: You SHOULD NOT need to import this namespace here
// if you think you need to you are doing it wrong.
// Instead your mod should add the rules itself in its own assembly like:
//[ReplacementRule(priority: 10)]
//public static void RegisterFridgeRules()
//{
//    if (fridgeType != null)
//    {
//        ReplacementMatcher.AddRule(d => d.thingClass == fridgeType);
//    }
//}
// using Replace_Stuff.Compatibility.ThirdParty; DO NOT IMPORT THIS HERE

namespace Replace_Stuff.Core;

public static class DefaultRules
{
    [ReplacementRule(priority: 100)]
    public static void RegisterWalls()
    {
        // Walls/Fences
        ReplacementMatcher.AddRule(
            d => d.IsWall() || (d.building?.isFence ?? false),
            o => o.IsWall() || (o.building?.isFence ?? false)
        );
    }

    [ReplacementRule(priority: 99)]
    public static void RegisterDoors()
    {
        // Doors
        ReplacementMatcher.AddRule(d => d.IsWall() || typeof(Building_Door).IsAssignableFrom(d.thingClass));
    }

    [ReplacementRule(priority: 98)]
    public static void RegisterFences()
    {
        // Fences
        DesignationCategoryDef fencesDef = DefDatabase<DesignationCategoryDef>.GetNamed("Fences", false);
        if (fencesDef != null)
            ReplacementMatcher.AddRule(d => d.designationCategory == fencesDef);
    }

    [ReplacementRule(priority: 97)]
    public static void RegisterTables()
    {
        // Tables
        ReplacementMatcher.AddRule(d => d.IsTable);
    }

    [ReplacementRule(priority: 95)]
    public static void RegisterBeds()
    {
        // Beds
        ReplacementMatcher.AddRule(d => typeof(Building_Bed).IsAssignableFrom(d.thingClass) && d.GetStatValueAbstract(StatDefOf.WorkToBuild) > 0f);
    }

    [ReplacementRule(priority: 90)]
    public static void RegisterCoolers()
    {
        // Coolers
        // Catches vanilla coolers and any wide/modded over-wall variants inheriting from Building_Cooler
        ReplacementMatcher.AddRule(d => d != null && typeof(Building_Cooler).IsAssignableFrom(d.thingClass));
    }

    [ReplacementRule(priority: 90)]
    public static void RegisterGrowers()
    {
        // Growers
        ReplacementMatcher.AddRule(d => typeof(IPlantToGrowSettable).IsAssignableFrom(d.thingClass));
    }

    [ReplacementRule(priority: 90)]
    public static void RegisterPower()
    {
        ReplacementMatcher.AddRule(d => typeof(Building_Battery).IsAssignableFrom(d.thingClass));
        ReplacementMatcher.AddRule(d => d.placeWorkers?.Any(w => w == typeof(PlaceWorker_WatermillGenerator)) ?? false);
        ReplacementMatcher.AddRule(d => d.placeWorkers?.Any(w => w == typeof(PlaceWorker_WindTurbine)) ?? false);
        ReplacementMatcher.AddRule(d => d.placeWorkers?.Any(w => w == typeof(PlaceWorker_OnSteamGeyser)) ?? false);
    }

    [ReplacementRule(priority: 100)]
    public static void RegisterVents()
    {
        // Catches vanilla vents and any modded over-wall vents that inherit from Building_Vent
        ReplacementMatcher.AddRule(d => d != null && typeof(Building_Vent).IsAssignableFrom(d.thingClass));
    }
}