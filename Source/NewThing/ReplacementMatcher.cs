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
using Replace_Stuff.CoolersOverWalls;
using Replace_Stuff.Utilities;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Verse;

namespace Replace_Stuff.NewThing;

/// <summary>
/// The central registry and logic engine for structure replacements. 
/// It maintains a list of 'Replacement' rules and caches results to optimize 
/// performance.
/// </summary>
[StaticConstructorOnStartup]
public static class ReplacementMatcher
{
    private static readonly List<ReplacementRule> _replacements = new();

    [Unsaved]
    private static readonly Dictionary<(ThingDef, ThingDef), bool> _replacementCache = new();

    [Unsaved]
    private static readonly Dictionary<int, System.WeakReference<Thing>> _thingReplacementCache = new();

    [Unsaved]
    private static readonly Queue<int> _cacheOrder = new();

    private const int MAX_CACHE_SIZE = 500;

    /// <summary>
    /// Defines specific replacement matching behaviors for various building categories.
    /// This registry acts purely as a validation layer to determine if one ThingDef 
    /// can be built over another (e.g., walls over walls, beds over beds).
    /// State transfer logic (bills, temperatures, ownership) has been migrated to 
    /// the ReplacementPipeline and BuildingStateTransfer systems.
    /// </summary>
    static ReplacementMatcher()
    {
        // Scan for auto-registered rules
        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        var foundRules = new List<(MethodInfo method, int priority)>();

        foreach (var type in assembly.GetTypes())
        {
            foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                var attr = (ReplacementRuleAttribute)Attribute.GetCustomAttribute(method, typeof(ReplacementRuleAttribute));
                if (attr != null)
                {
                    foundRules.Add((method, attr.Priority));
                }
            }
        }

        // Register them (sorted by priority)
        foreach (var rule in foundRules.OrderByDescending(r => r.priority))
        {
            // Invoke the method to get the predicate and add the rule
            // Assuming the method signature is: public static void AddMyRule() { AddRule(...); }
            try
            {
                rule.method.Invoke(null, null);
                RSLog.Debug($"Successfully executed registration rule: {rule.method.Name}");
            }
            catch (Exception e)
            {
                RSLog.Error($"Failed to execute registration rule {rule.method.Name}: {e.Message}");
            }
        }

        // Walls/Fences
        AddRule(d => d.IsWall() || (d.building?.isFence ?? false),
                o => o.IsWall() || (o.building?.isFence ?? false));

        // Doors
        AddRule(d => d.IsWall() || typeof(Building_Door).IsAssignableFrom(d.thingClass));

        // Coolers
        AddRule(d => typeof(Building_Cooler).IsAssignableFrom(d.thingClass));

        // Beds
        AddRule(d => typeof(Building_Bed).IsAssignableFrom(d.thingClass) && d.GetStatValueAbstract(StatDefOf.WorkToBuild) > 0f);

        // Fences
        DesignationCategoryDef fencesDef = DefDatabase<DesignationCategoryDef>.GetNamed("Fences", false);
        if (fencesDef != null)
            AddRule(d => d.designationCategory == fencesDef);

        // Tables
        AddRule(d => d.IsTable);

        // Fridges
        //AddRule(d => RimFridgeCompat.fridgeType != null && d.thingClass == RimFridgeCompat.fridgeType);

        // Growers
        AddRule(d => typeof(IPlantToGrowSettable).IsAssignableFrom(d.thingClass));

        // Power
        AddRule(d => typeof(Building_Battery).IsAssignableFrom(d.thingClass));
        AddRule(d => d.placeWorkers?.Any(w => w == typeof(PlaceWorker_WatermillGenerator)) ?? false);
        AddRule(d => d.placeWorkers?.Any(w => w == typeof(PlaceWorker_WindTurbine)) ?? false);
        AddRule(d => d.placeWorkers?.Any(w => w == typeof(PlaceWorker_OnSteamGeyser)) ?? false);
    }

    public static void AddRule(Predicate<ThingDef> newCheck, Predicate<ThingDef> oldCheck = null)
    {
        _replacements.Add(new ReplacementRule(newCheck, oldCheck ?? newCheck));
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

        foreach (var r in _replacements)
        {
            if (!r.Matches(newDef, oldDef))
                continue;

            _replacementCache.Add((newDef, oldDef), true);
            return true;
        }

        _replacementCache.Add((newDef, oldDef), false);
        return false;
    }

    public static bool TryFindTarget(this Thing newThing, out Thing oldThing)
    {
        oldThing = null;

        if (!newThing.Spawned)
            return false;

        var thingID = newThing.thingIDNumber;

        if (_thingReplacementCache.TryGetValue(thingID, out var weakRef))
        {
            // Check if the cached thing is valid
            if (weakRef.TryGetTarget(out oldThing) && !oldThing.Destroyed)
                return true;

            // If it was destroyed/null, clean it up
            _thingReplacementCache.Remove(thingID);
        }

        // Perform the expensive search
        var result = newThing.def.TryFindTarget(newThing.Position, newThing.Rotation, newThing.Map, out oldThing);

        // Only cache positive results
        if (result && oldThing != null)
        {
            // Don't cache identical replacements
            if (newThing.def == oldThing.def && newThing.Stuff == oldThing.Stuff)
            {
                oldThing = null;
                return false;
            }

            // Cache Management: Evict oldest if full
            if (_thingReplacementCache.Count >= MAX_CACHE_SIZE)
            {
                int oldestID = _cacheOrder.Dequeue();
                _thingReplacementCache.Remove(oldestID);
            }

            _thingReplacementCache[thingID] = new System.WeakReference<Thing>(oldThing);
            _cacheOrder.Enqueue(thingID);
        }

        return result;
    }

    public static bool TryFindTarget(this ThingDef newDef, IntVec3 pos, Rot4 rotation, Map map, out Thing oldThing)
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
