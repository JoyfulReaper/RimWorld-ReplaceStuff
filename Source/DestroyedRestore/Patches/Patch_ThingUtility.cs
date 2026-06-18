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

namespace Replace_Stuff.DestroyedRestore.Patches;

[HarmonyPatch(typeof(ThingUtility), nameof(ThingUtility.CheckAutoRebuildOnDestroyed))]
static class Patch_ThingUtility
{
    // If the original method returned a blueprint (i.e., it successfully placed it), 
    // then __result will be that blueprint.
    public static void Postfix(Thing thing, Map map, Blueprint_Build __result)
    {
        // Only run our logic if a blueprint was actually placed
        if (__result != null)
        {
            ReplacementStateStore.SaveBuilding(thing, map);
        }
    }
}

// Original Transpiler
// /// <summary>
// /// Harmony patch that intercepts vanilla building destruction to capture state data before the entity is purged.
// /// </summary>
// [HarmonyPatch(typeof(ThingUtility), nameof(ThingUtility.CheckAutoRebuildOnDestroyed))]
// static class Patch_ThingUtility
// {
//     /// <summary>
//     /// Transpiles <see cref="ThingUtility.CheckAutoRebuildOnDestroyed"/> to inject a state-saving call
//     /// immediately after an auto-rebuild blueprint is successfully placed.
//     /// </summary>
//     /// <param name="instructions">The original IL instructions.</param>
//     /// <returns>The modified IL instruction sequence.</returns>
//     //public static void CheckAutoRebuildOnDestroyed(Thing thing, DestroyMode mode, Map map, BuildableDef buildingDef)
//     public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
//     {
//         var PlaceBlueprintForBuildInfo = AccessTools.Method(typeof(GenConstruct), nameof(GenConstruct.PlaceBlueprintForBuild));

//         foreach (CodeInstruction i in instructions)
//         {
//             yield return i;

//             // Right after vanilla successfully places the auto-rebuild blueprint, 
//             // inject our call to save the data of the thing that was just destroyed.
//             if (i.Calls(PlaceBlueprintForBuildInfo))
//             {
//                 yield return new CodeInstruction(OpCodes.Ldarg_0); // Arg 0: Thing thing
//                 yield return new CodeInstruction(OpCodes.Ldarg_2); // Arg 2: Map map
//                 yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DestroyedBuildingStore), nameof(DestroyedBuildingStore.SaveBuilding)));//SaveBuilding(thing)
//             }
//         }
//     }
// }
