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
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Replace_Stuff.Compatibility;

internal class ReplacementLoader
{
    public static void AddRulesFromXML()
    {
        var comps = new List<ReplaceList>();
        foreach (var def in DefDatabase<InterchangeableItems>.AllDefs)
        {
            foreach (var list in def.replaceLists)
            {
                if (list.comps.Any())
                {
                    comps.Add(list);
                }
            }
        }
        foreach (var itemList in comps)
        {
            foreach (var compName in itemList.comps)
            {
                if (ReplacementRegistry.TryGetHandler(compName, out _))
                    continue;

                var type = GenTypes.GetTypeInAnyAssembly(compName);
                if (type is null)
                {
                    RSLog.Warning($"Could not find replacement handler type: {compName}");
                    continue;
                }

                try {
                    var handler = (IReplacementHandler)Activator.CreateInstance(type);
                    if (handler is null)
                        continue;

                    ReplacementRegistry.RegisterHandler(compName, handler);
                } catch (Exception e) {
                    RSLog.Error($"Failed to create instance of {compName}: {e.Message}");
                    continue;
                }
            }
        }
        foreach (var itemList in comps)
        {
            ReplacementRegistry.AddInterchangeableItems(itemList);
        }
    }
}

