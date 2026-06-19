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
using Verse;

namespace Replace_Stuff.Terrain.Patches;

//Frames can overlap anything. That shouldn't create a problem, right?
[HarmonyPatch(typeof(GenSpawn), "SpawningWipes")]
class Patch_GenSpawn_SpawningWipes
{
    //public static bool SpawningWipes(BuildableDef newEntDef, BuildableDef oldEntDef)
    public static void Postfix(BuildableDef newEntDef, BuildableDef oldEntDef, ref bool __result)
    {
        if (!__result) return;

        if (newEntDef is ThingDef newDef && newDef.IsFrame)
            __result = false;
    }
}