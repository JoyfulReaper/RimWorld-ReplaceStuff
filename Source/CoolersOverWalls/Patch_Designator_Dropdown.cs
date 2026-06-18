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

namespace Replace_Stuff.CoolersOverWalls; 	

[HarmonyPatch(typeof(Designator_Dropdown), MethodType.Constructor)]
static class Patch_Designator_Dropdown
{
    public static void Postfix(Designator_Dropdown __instance)
    {
        if(__instance is not null)
            __instance.Order = 20f;
    }
}