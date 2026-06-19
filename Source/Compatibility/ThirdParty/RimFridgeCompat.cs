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
using Replace_Stuff.NewThing;
using System;
using System.Reflection;
using Verse;

namespace Replace_Stuff.Compatibility.ThirdParty;

/// <summary>
/// A compatibility handler that uses reflection to detect "RimFridge" buildings.
/// It dynamically retrieves the 'DesiredTemp' field, allowing the mod to 
/// synchronize temperature settings between replaced refrigerators without 
/// requiring a hard dependency on the external mod.
/// </summary>
[StaticConstructorOnStartup]
public static class RimFridgeCompat
{
    public static Type fridgeType;
    public static FieldInfo DesiredTempInfo;

    static RimFridgeCompat()
    {
        try
        {
            fridgeType = AccessTools.TypeByName("Building_Refrigerator");
            if (fridgeType != null)
                DesiredTempInfo = AccessTools.Field(fridgeType, "DesiredTemp");
        }
        catch (System.Reflection.ReflectionTypeLoadException) //Aeh, this happens to people, should not happen, meh.
        {
            Verse.Log.Warning("Failed to check for RimFridges");
        }
    }

    [ReplacementRule(priority: 10)]
    public static void RegisterFridgeRules()
    {
        if (fridgeType != null)
        {
            ReplacementMatcher.AddRule(d => d.thingClass == fridgeType);
        }
    }
}