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

namespace Replace_Stuff.CoolersOverWalls;

[HarmonyPatch(typeof(GenSpawn), "SpawningWipes")]
class PreventOverWallWipingPatch
{
    //public static bool SpawningWipes(BuildableDef newEntDef, BuildableDef oldEntDef)
    public static void Postfix(BuildableDef newEntDef, BuildableDef oldEntDef, ref bool __result)
    {
        if (!__result) 
            return;

        ThingDef newDef = newEntDef as ThingDef;
        ThingDef oldDef = oldEntDef as ThingDef;
        BuildableDef newBuiltDef = GenConstruct.BuiltDefOf(newDef);
        BuildableDef oldBuiltDef = GenConstruct.BuiltDefOf(oldDef);

        //Power conduit sharing is hardcoded, so cooler sharing is hardcoded too
        if ((newBuiltDef.IsOverWall() && oldBuiltDef.IsWall())
            || (newBuiltDef.IsWall() && oldBuiltDef.IsOverWall()))
        {
            __result = false;
        }
    }
}

[HarmonyPatch(typeof(GenSpawn), "SpawningWipes")]
class ForceCoolerReplacementPatch
{
    //public static bool SpawningWipes(BuildableDef newEntDef, BuildableDef oldEntDef)
    public static void Postfix(BuildableDef newEntDef, BuildableDef oldEntDef, ref bool __result)
    {
        if (__result) 
            return;

        else if (newEntDef is ThingDef newDef && newDef.thingClass == typeof(Building_Cooler) &&
            oldEntDef is ThingDef oldDef && oldDef.thingClass == typeof(Building_Cooler))
            __result = true;
    }
}