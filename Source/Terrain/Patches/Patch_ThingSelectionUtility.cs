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

using HarmonyLib;
using RimWorld;
using Verse;

namespace Replace_Stuff.OverMineable.Patches;

[HarmonyPatch(typeof(ThingSelectionUtility), "SelectableByMapClick")]
class FoggedSelectable
{
    // Allows selecting blueprints in fogged areas
    //public static bool SelectableByMapClick(Thing t)
    public static bool Prefix(ref bool __result, Thing t)
    {
        if (t.def.IsBlueprint) // && t.def.selectable && t.Spawned //redundant checks
        {
            __result = true;
            return false;
        }
        return true;
    }
}