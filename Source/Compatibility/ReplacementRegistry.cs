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

using Replace_Stuff.Interfaces;
using Replace_Stuff.Utilities;
using System.Collections.Generic;
using Verse;

namespace Replace_Stuff.Compatibility
{
    public static class ReplacementRegistry
    {
        private static readonly Dictionary<string, IReplacementHandler> _handlerRegistry = new();

        private static readonly Dictionary<ThingDef, HashSet<ThingDef>> _interchangeablePools = new();

        internal static bool IsRegistered(string name) =>
            _handlerRegistry.ContainsKey(name);

        internal static bool TryGetHandler(string name, out IReplacementHandler handler) =>
            _handlerRegistry.TryGetValue(name, out handler);

        internal static void RegisterHandler(string name, IReplacementHandler handler)
        {
            if (!_handlerRegistry.ContainsKey(name))
                _handlerRegistry.Add(name, handler);
        }

        internal static string GetKeyForComp(ThingComp comp)
        {
            if (comp is null)
                return null;

            var type = comp.GetType();
            return _handlerRegistry.ContainsKey(type.FullName) ? type.FullName : null;
        }

        internal static void AddInterchangeableItems(ReplaceList items)
        {
            // The pipeline now handles state transfer execution. 
            // Here, we strictly register the validation rule for the UI/Blueprints.
            AddInterchangeableList(items.items);
        }

        internal static void AddInterchangeableList(List<ThingDef> items)
        {
            if (items == null || items.Count < 2)
                return;

            var fullSet = new HashSet<ThingDef>(items);
            foreach (var item in items)
            {
                if (item == null) continue;

                if (!_interchangeablePools.TryGetValue(item, out var existingPool))
                {
                    _interchangeablePools[item] = fullSet;
                }
                else
                {
                    // Safe merge pattern if multiple mod lists register overlapping configurations
                    existingPool.UnionWith(fullSet);
                    foreach (var member in fullSet)
                    {
                        if (member != null)
                            _interchangeablePools[member] = existingPool;
                    }
                }
            }
        }

        /// <summary>
        /// O(1) Fast-path lookup to determine if two structural definitions are explicitly marked as interchangeable.
        /// </summary>
        internal static bool AreInterchangeable(ThingDef a, ThingDef b)
        {
            return a != null && b != null &&
                   _interchangeablePools.TryGetValue(a, out var pool) &&
                   pool.Contains(b);
        }

        public static void DebugListHandlers()
        {
            foreach (var kvp in _handlerRegistry)
            {
                RSLog.Debug($"Registry Entry: {kvp.Key} -> Handler: {kvp.Value.GetType().Name}");
            }
        }
    }
}