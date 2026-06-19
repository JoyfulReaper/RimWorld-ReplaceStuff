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

//namespace Replace_Stuff.OverWallCoolers;

// I don't remember why I commented this out...
// So... I'm not going to delete it yet... Instead of trying to figure it out right now.
// Pretty sure I commented it b/c its not needed anymore.
//[HarmonyPatch(typeof(GenConstruct), "BlocksConstruction")]
// class CoolerWallShare_Blocks
// {
//     //public static bool BlocksConstruction(Thing constructible, Thing t)
//     public static void Postfix(Thing constructible, Thing t, ref bool __result)
//     {
//         if (!__result) return;

//         BuildableDef cDef = constructible.def.entityDefToBuild ?? constructible.def;
//         BuildableDef tDef = t.def.entityDefToBuild ?? t.def;

//         //Power conduit sharing is hardcoded, so cooler sharing is hardcoded too
//         if ((cDef.IsWall() && tDef.IsOverWall()) ||
//                 (tDef.IsWall() && cDef.IsOverWall()))
//             __result = false;
//     }
// }

// This should not be needed anymore. Set canPlaceOverWall in xml instead now.
// [HarmonyPatch(typeof(GenConstruct), "CanPlaceBlueprintOver")]
// class CoolerWallShare_Blueprint
// {
//     //public static bool CanPlaceBlueprintOver(BuildableDef newDef, ThingDef oldDef)
//     public static void Postfix(BuildableDef newDef, ThingDef oldDef, ref bool __result)
//     {
//         if (__result) return;

//         BuildableDef oldBuildDef = GenConstruct.BuiltDefOf(oldDef);
//         if (oldDef.category == ThingCategory.Building || oldDef.IsBlueprint || oldDef.IsFrame)
//         {
//             //Power conduit sharing is hardcoded, so cooler sharing is hardcoded too
//             if ((newDef.IsOverWall() && oldBuildDef.IsWall()) || (newDef.IsWall() && oldBuildDef.IsOverWall()))
//             {
//                 __result = true;
//             }
//         }
//     }
// }
