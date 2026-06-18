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

namespace Replace_Stuff.PlaceBridges.Patches;

[HarmonyPatch(typeof(Blueprint), nameof(Blueprint.DeSpawn))]
public static class CancelBlueprint
{
    //public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
    public static void Prefix(Blueprint __instance, DestroyMode mode)
    {
        CancelAboveBridges.CancelAbove(__instance.def.entityDefToBuild, mode, __instance.Map, __instance.Position);
    }
}