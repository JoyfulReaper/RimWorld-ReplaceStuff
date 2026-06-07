/*
 * REPLACE STUFF: Perfomance Edition
 * 
 * 
 * Part of this code is based on Replace Stuff
 * Copyright (c) 2024 Alex Tearse-Doyle
 * Licensed under the MIT License.
 *
 * Modified by Kyle Givler
 * Copyright (c) 2026 Kyle Givler
 * Licensed under the MIT License.
 */

using Replace_Stuff.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Replace_Stuff.Compatibility
{
    internal class ReplacementLoader
    {
        public static void AddRulesFromXML()
        {
            var categories = new Dictionary<string, ReplaceList>();
            var comps = new List<ReplaceList>();

            foreach (var def in DefDatabase<InterchangeableItems>.AllDefs)
            {
                var nonCategory = 0;
                foreach (var list in def.replaceLists)
                {

                    if (list.category.Any())
                    {
                        if (!categories.ContainsKey(list.category)) categories.Add(list.category, new ReplaceList());

                        list.items.ForEach(x =>
                        {
                            x.replaceTags ??= new List<string>();

                            if (!x.replaceTags.Contains(list.category))
                                x.replaceTags.Add(list.category);
                        });

                        categories[list.category].items.AddRange(list.items);
                    }

                    if (list.comps.Any())
                        comps.Add(list);

                    if (!list.category.Any() && !list.comps.Any())
                        nonCategory++;
                }

                if (nonCategory > 0)
                    Verse.Log.Warning($"Loaded Compatibility patch {def.defName} includes {nonCategory} patch{(nonCategory == 1 ? "" : "es")} that have no category or comp.\nThese patches should be updated to use an existing category");
            }

            foreach (var itemList in comps)
            {

                foreach (var compName in itemList.comps)
                {
                    if (ReplacementRegistry.TryGetHandler(compName, out _))
                        continue;

                    var type = Type.GetType(compName);
                    if (type is null)
                        continue;

                    var handler = (IReplacementHandler)Activator.CreateInstance(type);
                    if (handler is null)
                        continue;

                    ReplacementRegistry.RegisterHandler(compName, handler);
                }
            }

            foreach (var itemList in comps)
            {
                ReplacementRegistry.AddInterchangeableItems(itemList);
            }
        }
    }
}
