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
using System.Collections.Generic;
using System;
using System.Reflection.Emit;

namespace Replace_Stuff.OverMineable.Patches;
    
// At somepoint CanPlaceBlueprintAt turned into a hollow wrapper that just Calls
// CanPlaceBlueprintAt_NewTemp. Updated this to transpile that method instead.
[HarmonyPatch(typeof(GenConstruct), nameof(GenConstruct.CanPlaceBlueprintAt_NewTemp))]
public static class Patch_GenConstruct
{
    //public static AcceptanceReport CanPlaceBlueprintAt(BuildableDef entDef, IntVec3 center, Rot4 rot, Map map, bool godMode = false, Thing thingToIgnore = null)
    // ohheck this method has got a lot more
    //public static AcceptanceReport CanPlaceBlueprintAt(BuildableDef entDef, IntVec3 center, Rot4 rot, Map map, bool godMode = false, Thing thingToIgnore = null, Thing thing = null, ThingDef stuffDef = null, bool ignoreEdgeArea = false, bool ignoreInteractionSpots = false, bool ignoreClearableFreeBuildings = false)
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var FoggedInfo = AccessTools.Method(typeof(GridsUtility), "Fogged", new Type[] { typeof(IntVec3), typeof(Map) });
        var BlueprintAcceptedInfo = AccessTools.Method(typeof(Patch_GenConstruct), nameof(BlueprintOverFogAcceptance));

        bool foundFogged = false;
        foreach (CodeInstruction i in instructions)
        {
            yield return i;
            if (foundFogged)  //skip the brfalse after Fogged
            {
                //This should probably check for DesignatorContext.designating but then more of this code would need to change
                yield return new CodeInstruction(OpCodes.Ldarg_3);//map
                yield return new CodeInstruction(OpCodes.Ldarg_1);//center
                yield return new CodeInstruction(OpCodes.Ldarg_0);//entDef
                yield return new CodeInstruction(OpCodes.Call, BlueprintAcceptedInfo);
                yield return new CodeInstruction(OpCodes.Ret);
                foundFogged = false;
            }
            if (i.Calls(FoggedInfo))
                foundFogged = true;
        }
    }

    //if found fogged:
    public static AcceptanceReport BlueprintOverFogAcceptance(Map map, IntVec3 center, ThingDef entDef)
    {
        if (!BluePrintUtility.IsEnabledBlueprintOverRock)
            return new AcceptanceReport("CannotPlaceInUndiscovered".Translate());
        if (center.GetThingList(map).Any(t => t is Blueprint && t.def.entityDefToBuild == entDef))
            return new AcceptanceReport("IdenticalBlueprintExists".Translate());
        if (entDef.GetStatValueAbstract(StatDefOf.WorkToBuild) == 0f)
            return new AcceptanceReport("CannotPlaceInUndiscovered".Translate());
        return true;
    }
}