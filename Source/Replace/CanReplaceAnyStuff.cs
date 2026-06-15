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

namespace Replace_Stuff.Replace;

//public override AcceptanceReport CanDesignateCell(IntVec3 c)
[HarmonyPatch(typeof(Designator_Build), "CanDesignateCell")]
static class DesignatorContext
{
    public static bool designating;

    public static void Prefix(Designator_Build __instance)
    {
        designating = true;
    }
    public static void Postfix()
    {
        designating = false;
    }
}


[HarmonyPatch(typeof(GenConstruct), nameof(GenConstruct.CanReplace))]
public static class CanReplaceAnyStuff
{
    public static void Postfix(
        ref bool __result,
        BuildableDef placing,
        BuildableDef existing,
        ThingDef placingStuff = null,
        ThingDef existingStuff = null)
    {
        // If it's already allowed by vanilla or another patch, leave it alone.
        if (__result)
            return;

        if (placing is not ThingDef placingDef || existing is not ThingDef existingDef)
            return;

        if (!placingDef.MadeFromStuff)
            return;

        var placingBuilt = placingDef.entityDefToBuild ?? placingDef;
        var existingBuilt = existingDef.entityDefToBuild ?? existingDef;

        // Ensure we are replacing the exact same logical building type
        if (placingBuilt != existingBuilt)
            return;

        // Handle missing material context
        // If the call site didn't provide the material context, we cannot safely 
        // determine if this is a valid material upgrade. Do not force true.
        if (placingStuff == null || existingStuff == null)
            return;

        // Enforce strict material differences
        if (placingStuff == existingStuff)
            return;

        // At this stage:
        // - It's the same logical building
        // - It's made of stuff
        // - We know exactly what stuff both use
        // - The materials are definitively different
        __result = true;
    }
}