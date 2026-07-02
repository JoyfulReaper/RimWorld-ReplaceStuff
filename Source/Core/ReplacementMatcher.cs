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
using Replace_Stuff.Utilities;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Verse;

namespace Replace_Stuff.Core;

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
    private static readonly Dictionary<(ThingDef NewDef, ThingDef OldDef), bool> _replacementCache = new();

    // Cache replacement targets without keeping Thing instances alive.
    // Entries are weak and bounded because frames can be canceled/destroyed
    // without reaching the normal construction completion path.
    [Unsaved]
    private static readonly Dictionary<int, (System.WeakReference<Thing> WeakRef, LinkedListNode<int> Node)> _thingReplacementCache = new();

    [Unsaved]
    private static readonly LinkedList<int> _lruList = new();

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
        var foundRules = new List<(MethodInfo method, int priority)>();

        // Sweep loaded assemblies for classes containing rules
        foreach (var type in GenTypes.AllTypes)
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
    }

    public static void AddRule(Predicate<ThingDef> newCheck, Predicate<ThingDef> oldCheck = null)
    {
        _replacements.Add(new ReplacementRule(newCheck, oldCheck ?? newCheck));
        _replacementCache.Clear();
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
        if (oldDef?.building?.IsDeconstructible != true)
            return false;

        newDef = GenConstruct.BuiltDefOf(newDef) as ThingDef;
        if (newDef == null)
            return false;

        if (newDef == oldDef && !newDef.MadeFromStuff)
            return false;

        var key = (newDef, oldDef);

        if (_replacementCache.TryGetValue(key, out var cached))
            return cached;

        try
        {
            if (GenConstruct.HasMatchingReplacementTag(newDef, oldDef))
                return _replacementCache[key] = true;
        }
        catch
        {
            Debugger.Break();
        }

        if (ReplacementRegistry.AreInterchangeable(newDef, oldDef))
            return _replacementCache[key] = true;

        foreach (var r in _replacements)
        {
            if (r.Matches(newDef, oldDef))
                return _replacementCache[key] = true;
        }

        return _replacementCache[key] = false;
    }


    public static bool TryFindTarget(this Thing newThing, out Thing oldThing)
    {
        oldThing = null;

        if (!newThing.Spawned)
            return false;

        var thingID = newThing.thingIDNumber;

        if (_thingReplacementCache.TryGetValue(thingID, out var cacheEntry))
        {
            if (cacheEntry.WeakRef.TryGetTarget(out oldThing) &&
                oldThing != null &&
                !oldThing.Destroyed &&
                oldThing.Map == newThing.Map &&
                GenAdj.OccupiedRect(newThing.Position, newThing.Rotation, newThing.def.Size)
                    .Contains(oldThing.Position))
            {
                _lruList.Remove(cacheEntry.Node);
                _lruList.AddFirst(cacheEntry.Node);
                return true;
            }

            _lruList.Remove(cacheEntry.Node);
            _thingReplacementCache.Remove(thingID);
            oldThing = null;
        }

        var result = newThing.def.TryFindTarget(
            newThing.Position,
            newThing.Rotation,
            newThing.Map,
            out oldThing);

        if (!result || oldThing == null)
            return false;

        if (newThing.def == oldThing.def && newThing.Stuff == oldThing.Stuff)
        {
            oldThing = null;
            return false;
        }

        if (_thingReplacementCache.Count >= MAX_CACHE_SIZE && _lruList.Last != null)
        {
            int oldestID = _lruList.Last.Value;
            _lruList.RemoveLast();
            _thingReplacementCache.Remove(oldestID);
        }

        var newNode = _lruList.AddFirst(thingID);
        _thingReplacementCache[thingID] = (new System.WeakReference<Thing>(oldThing), newNode);

        return true;
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
            if (!checkPos.InBounds(map))
                continue;

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
        return newThing?.def != null &&
               oldThing?.def != null &&
               newThing.def.CanReplace(oldThing.def);
    }
}