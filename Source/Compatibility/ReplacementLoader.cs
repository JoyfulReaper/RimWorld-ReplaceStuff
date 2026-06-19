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
        foreach (var def in DefDatabase<InterchangeableItems>.AllDefs)
        {
            foreach (var list in def.replaceLists)
            {
                // Register Interchangeable Items for UI/Blueprints
                if (list.items.Any())
                {
                    ReplacementRegistry.AddInterchangeableItems(list);
                }

                // Register Comps/Handlers for State Transfer
                if (list.comps?.Any() ?? false)
                {
                    foreach (var compName in list.comps)
                    {
                        if (ReplacementRegistry.TryGetHandler(compName, out _))
                            continue;

                        var type = GenTypes.GetTypeInAnyAssembly(compName);
                        if (type is null)
                        {
                            RSLog.Warning($"Could not find replacement handler type: {compName}");
                            continue;
                        }

                        try
                        {
                            var handler = (IReplacementHandler)Activator.CreateInstance(type);
                            if (handler != null)
                            {
                                ReplacementRegistry.RegisterHandler(compName, handler);
                            }
                        }
                        catch (Exception e)
                        {
                            RSLog.Error($"Failed to create instance of {compName}: {e.Message}");
                        }
                    }
                }
            }
        }
    }

    public static void RegisterCodeBasedHandlers()
    {
        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        var allCandidates = new List<(Type type, int priority, string targetName)>();

        // candidates
        foreach (var type in assembly.GetTypes())
        {
            var attribute = (ReplacementHandlerAttribute)Attribute.GetCustomAttribute(type, typeof(ReplacementHandlerAttribute));
            if (attribute != null)
            {
                allCandidates.Add((type, attribute.Priority, attribute.TargetCompName));
            }
        }

        // Sort by Priority
        var sortedCandidates = allCandidates.OrderByDescending(x => x.priority).ToList();

        // Register based on priority
        foreach (var candidate in sortedCandidates)
        {
            var compType = GenTypes.GetTypeInAnyAssembly(candidate.targetName);
            if (compType is null)
            {
                RSLog.Warning($"Could not find comp type {candidate.targetName} for {candidate.type.Name}.");
                continue;
            }

            if (ReplacementRegistry.IsRegistered(compType.FullName))
            {
                RSLog.Warning($"Skipping {candidate.type.Name} for {compType.FullName}: A higher or equal priority handler is already registered.");
                continue; // Another already registered
            }

            try
            {
                var handler = (IReplacementHandler)Activator.CreateInstance(candidate.type);
                ReplacementRegistry.RegisterHandler(compType.FullName, handler);

                RSLog.Debug($"Registered {candidate.type.Name} for {compType.FullName} (Priority: {candidate.priority})");
            }
            catch (Exception e)
            {
                RSLog.Error($"Failed to register {candidate.type.Name}: {e.Message}");
            }
        }
    }
}