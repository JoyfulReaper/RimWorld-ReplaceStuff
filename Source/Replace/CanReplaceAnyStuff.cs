/*
 * REPLACE STUFF: Perfomance Edition
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
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
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
    // Normal stuff replacement only here.
    // public static bool CanReplace(BuildableDef placing, BuildableDef existing, ThingDef placingStuff = null, ThingDef existingStuff = null)
    public static void Postfix(ref bool __result, BuildableDef placing, BuildableDef existing, ThingDef placingStuff = null, ThingDef existingStuff = null)
    {
        if (__result || placingStuff == existingStuff) return; // Fast fail if already true or same material

        if (placing is ThingDef placingDef && existing is ThingDef existingDef && placingDef.MadeFromStuff)
        {
            // Calculate base definitions once
            var placingBuiltDef = placingDef.entityDefToBuild ?? placingDef;
            var existingBuiltDef = existingDef.entityDefToBuild ?? existingDef;

            if (placingBuiltDef == existingBuiltDef && placingBuiltDef.MadeFromStuff)
            {
                __result = true;
            }
        }
    }
}


[HarmonyPatch(typeof(GenConstruct), nameof(GenConstruct.CanPlaceBlueprintAt))]
public static class CanPlaceBlueprintRotDoesntMatter
{
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        MethodInfo RotationEquals = AccessTools.Method(typeof(Rot4), "op_Equality");
        MethodInfo OrRotDoesntMatter = AccessTools.Method(typeof(CanPlaceBlueprintRotDoesntMatter), nameof(OrRotDoesntMatter));

        foreach (CodeInstruction i in instructions)
        {
            yield return i;
            if (i.Calls(RotationEquals))
            {
                yield return new CodeInstruction(OpCodes.Ldarg_0);//BuildableDef entDef
                yield return new CodeInstruction(OpCodes.Call, OrRotDoesntMatter);
            }
        }
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool OrRotDoesntMatter(bool result, BuildableDef entDef)
    {
        return result || PlacingRotationDoesntMatter(entDef);
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool PlacingRotationDoesntMatter(BuildableDef entDef)
    {
        return entDef is ThingDef def &&
            (!def.rotatable ||
            typeof(Building_Door).IsAssignableFrom(def.thingClass));
    }
}