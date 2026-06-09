
/*
 * REPLACE STUFF: Performance  Edition
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

namespace Replace_Stuff.Compatibility;

using HarmonyLib;
using System;
using Verse;

/// <summary>
/// Provides lazy-loaded type metadata and definitions required for cross-mod compatibility with QualityBuilder.
/// </summary>
[StaticConstructorOnStartup]
public static class QualityBuilderCompat
{
    /// <summary>The designation definition used by QualityBuilder to mark skilled-only construction work.</summary>
    public static DesignationDef qualityBuilderDesignation;

    /// <summary>The runtime <see cref="Type"/> token for QualityBuilder's custom thing component.</summary>
    public static Type qualityBuilderCompType;

    /// <summary>The runtime <see cref="Type"/> token for QualityBuilder's component properties.</summary>
    public static Type qualityBuilderPropsType;

    static QualityBuilderCompat()
    {
        try
        {
            qualityBuilderCompType = AccessTools.TypeByName("CompQualityBuilder");
            // NOTE: Verified string literal spelling. Ensure original mod doesn't use standard "CompProperties_QualityBuilder"
            qualityBuilderPropsType = AccessTools.TypeByName("CompProperties_QualityBuilderr");
            qualityBuilderDesignation = DefDatabase<DesignationDef>.GetNamed("SkilledBuilder", false);
        }
        catch (System.Reflection.ReflectionTypeLoadException)
        {
            Verse.Log.Warning("Replace Stuff Performance Edition failed to resolve reflection types for Quality Builder integration.");
        }
    }
}