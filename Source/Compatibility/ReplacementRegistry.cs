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
using Replace_Stuff.NewThing;
using System;
using System.Collections.Generic;
using Verse;

namespace Replace_Stuff.Compatibility
{
    public static class ReplacementRegistry
    {
        private static readonly Dictionary<string, IReplacementHandler> _handlerRegistry = new();

        internal static bool TryGetHandler(string name, out IReplacementHandler handler) =>
            _handlerRegistry.TryGetValue(name, out handler);

        internal static void RegisterHandler(string name, IReplacementHandler handler)
        {
            if (!_handlerRegistry.ContainsKey(name))
                _handlerRegistry.Add(name, handler);
        }

        internal static void AddInterchangeableItems(ReplaceList items)
        {
            // The pipeline now handles state transfer execution. 
            // Here, we strictly register the validation rule for the UI/Blueprints.
            AddInterchangeableList(items.items);
        }

        internal static void AddInterchangeableList(List<ThingDef> items)
        {
            if (items.Count < 2) return;

            ReplacementValidator.replacements.Add(
                new ReplacementValidator.ReplacementRule(
                    ListContainsThingDef(new HashSet<ThingDef>(items))
                )
            );
        }

        static Predicate<ThingDef> ListContainsThingDef(HashSet<ThingDef> list) =>
            list.Contains;
    }
}
