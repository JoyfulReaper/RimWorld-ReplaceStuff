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

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using RimWorld;
using Verse;
using HarmonyLib;

namespace Replace_Stuff.OverMineable.Patches;

//It did create a problem! Putting two edifices in same spot is a problem
//So frames aren't edifices... that shouldn't create a problem, right?
[HarmonyPatch(typeof(ThingDefGenerator_Buildings), "NewFrameDef_Thing")]
public static class FramesArentEdifices
{
    //private static ThingDef NewFrameDef_Thing(ThingDef def)
    public static void Postfix(ThingDef __result)
    {
        __result.building.isEdifice = false;
    }
}