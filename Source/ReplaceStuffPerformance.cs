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
using Replace_Stuff.Compatibility;
using Replace_Stuff.Replace;
using Replace_Stuff.Replace.Patches;
using RimWorld;
using UnityEngine;
using Verse;

namespace Replace_Stuff;

/// <summary>
/// I'm a Mod!
/// </summary>
public class ReplaceStuffPerformance : Verse.Mod
{
    public static Settings settings;
    public static Harmony _harmony;

    public ReplaceStuffPerformance(ModContentPack content) : base(content)
    {
        settings = GetSettings<Settings>();

        _harmony = new Harmony("ReplaceStuff.Perfomance");
        _harmony.PatchAll();

#if DEBUG
        Verse.Log.Message($"[ReplaceStuffPerfomance] Version {settings.Version} <color=#66CCFF>DEBUG BUILD</color>");
#else
        Verse.Log.Message($"[ReplaceStuffPerfomance] Version {settings.Version}");
#endif

    }


    [StaticConstructorOnStartup]
    public static class ModStartup
    {
        static ModStartup()
        {
            ThingDefGenerator_ReplaceFrame.AddReplaceFrames();
            CoolersOverWalls.DesignatorBuildDropdownStuffFix.SanityCheck();
            ReplacementLoader.AddRulesFromXML();
            ReserveSharing.Initialize(_harmony);
        }
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        base.DoSettingsWindowContents(inRect);
        settings.DoWindowContents(inRect);
    }

    public override string SettingsCategory()
    {
        // TODO: Do better, make translations
        var result = "TD.ReplaceStuff".Translate();
        return $"{result} Perfomance Version ({settings.Version})";
    }
}