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

using Verse;

namespace Replace_Stuff.Replace;

/// <summary>
/// Handles the registration of replacement frame definitions for custom furniture in RimWorld.
/// </summary>
internal static class ReplacementFrameDefRegistrar
{
    /// <summary>
    /// Tracks whether the replacement frame definitions have been registered.
    /// </summary>
    private static bool _registered;

    /// <summary>
    /// Registers all replacement frame definitions.
    /// </summary>
    public static void Register()
    {
        if (_registered)
            return;

        _registered = true;

        foreach (ThingDef def in ReplacementFrameDefGenerator.GenerateReplacementFrameDefs())
        {
            RegisterDef(def);
        }
    }

    /// <summary>
    /// Registers a single replacement frame definition.
    /// </summary>
    /// <param name="def">The ThingDef to register.</param>
    private static void RegisterDef(ThingDef def)
    {
        def.PostLoad();
        DefDatabase<ThingDef>.Add(def);
    }
}
