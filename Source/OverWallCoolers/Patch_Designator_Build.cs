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

namespace Replace_Stuff.OverWallCoolers;

// Controls if the OverTheWall coolers and vents should be visible
[HarmonyPatch(typeof(Designator_Build), "Visible", MethodType.Getter)]
public static class HideCoolerBuild
{
    public static void Postfix(Designator_Build __instance, ref bool __result)
    {
        if (!__result)
            return;

        var def = __instance.PlacingDef;

        if (ReplaceStuffPerformance.settings.hideOverwallCoolers &&
            (def == OverWallDef.Cooler_Over ||
            def == OverWallDef.Cooler_Over2W ||
            def == OverWallDef.Vent_Over ||
            def == OverWallDef.Vent_Over2W))
        {
            __result = false;
            return;
        }

        if (ReplaceStuffPerformance.settings.hideNormalCoolers &&
            (__instance.PlacingDef == ThingDefOf.Cooler ||
            __instance.PlacingDef == OverWallDef.Vent))
        {
            __result = false;
            return;
        }
    }
}