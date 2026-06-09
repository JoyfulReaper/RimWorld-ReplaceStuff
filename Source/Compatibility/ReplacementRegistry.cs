/*
 * REPLACE STUFF: Performance  Edition
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
using System.Linq;
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
            List<string> comps = new();

            if (items.comps.Any())
            {
                comps.AddRange(items.comps
                    .Where(compName => _handlerRegistry.ContainsKey(compName))
                );
            }

            AddInterchangeableList(
                items.items,
                preAction: (newThing, oldThing) => { comps.ForEach(comp => _handlerRegistry[comp].PreAction(newThing, oldThing)); },
                postAction: (newThing, oldThing) => { comps.ForEach(comp => _handlerRegistry[comp].PostAction(newThing, oldThing)); }
            );
        }

        internal static void AddInterchangeableList(List<ThingDef> items, Action<Thing, Thing> preAction = null,
            Action<Thing, Thing> postAction = null)
        {
            if (items.Count < 2) return;

            NewThingReplacement.replacements.Add(
                new NewThingReplacement.Replacement(
                    ListContainsThingDef(new HashSet<ThingDef>(items)),
                    preAction: preAction,
                    postAction: postAction
                )
            );
        }

        static Predicate<ThingDef> ListContainsThingDef(HashSet<ThingDef> list) =>
            list.Contains;
    }
}
