/*
 * REPLACE STUFF: Perfomance Edition
 * 
 * 
 * Part of this code is based on Replace Stuff
 * Copyright (c) 2024 Alex Tearse-Doyle
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
public class ReplaceStuffPrefomanceMod : Verse.Mod
{
    public static Settings settings;
    public static Harmony _harmony;

    public ReplaceStuffPrefomanceMod(ModContentPack content) : base(content)
    {
        settings = GetSettings<Settings>();

        _harmony = new Harmony("ReplaceStuff.Perfomance");
        _harmony.PatchAll();

        Verse.Log.Message($"[ReplaceStuffPerfomance] Version {settings.Version}");
    }


    [StaticConstructorOnStartup]
    public static class ModStartup
    {
        static ModStartup()
        {
            //Hugslibs-added defs will be queued up before this Static Constructor
            //So queue replace frame generation after that
            LongEventHandler.QueueLongEvent(() => ThingDefGenerator_ReplaceFrame.AddReplaceFrames(), null, true, null);
            LongEventHandler.QueueLongEvent(CoolersOverWalls.DesignatorBuildDropdownStuffFix.SanityCheck, null, true, null);
            LongEventHandler.QueueLongEvent(ReplacementLoader.AddRulesFromXML, null, true, null);
            LongEventHandler.QueueLongEvent(() => ReserveSharing.Initialize(ReplaceStuffPrefomanceMod._harmony), "ReplaceStuff.Initializing", false, null);
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