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
using UnityEngine;
using Verse;

namespace Replace_Stuff.Terrain.Patches;

[HarmonyPatch(typeof(PlaySettings), "DoPlaySettingsGlobalControls")]
[StaticConstructorOnStartup]
public static class Patch_PlaySettings_DoPlaySettingsGlobalControls
{
    private static Texture2D _icon = ContentFinder<Texture2D>.Get("BlueprintOverRockToggle", true);

    [HarmonyPostfix]
    public static void AddButton(WidgetRow row, bool worldView)
    {
        if (worldView)
            return;

        row.ToggleableIcon(ref BlueprintUtility.IsEnabledBlueprintOverRock, _icon, "TD.ToggleBlueprintOverRock".Translate());
    }
}


[HarmonyPatch(typeof(PlaySettings), "ExposeData")]
public static class Patch_PlaySettings_ExposeData
{
    public static void Prefix()
    {
        Scribe_Values.Look(ref BlueprintUtility.IsEnabledBlueprintOverRock, "blueprintOverRock", true);
    }
}
