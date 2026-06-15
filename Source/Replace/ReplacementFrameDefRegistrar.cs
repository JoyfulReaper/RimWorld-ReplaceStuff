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

internal static class ReplacementFrameDefRegistrar
{
    private static bool _registered;

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

    private static void RegisterDef(ThingDef def)
    {
        def.PostLoad();
        DefDatabase<ThingDef>.Add(def);
    }
}
