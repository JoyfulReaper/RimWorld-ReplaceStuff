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

using RimWorld;
using Verse;
using HarmonyLib;

namespace Replace_Stuff.PlaceBridges.Patches;

[HarmonyPatch(typeof(GenSpawn), "SpawningWipes")]
public static class Patch_GenSpawn
{
    //public static bool SpawningWipes(BuildableDef newEntDef, BuildableDef oldEntDef)
    public static bool Prefix(BuildableDef oldEntDef, bool __result)
    {
        // Don't wipes bridge blueprints
        if (oldEntDef is ThingDef tdef && (GenConstruct.BuiltDefOf(tdef) ?? oldEntDef).IsBridgelike())
        {
            __result = false;
            return false;
        }
        return true;
    }
}