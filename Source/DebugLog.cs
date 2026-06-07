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

namespace Replace_Stuff;

static class Log
{
    public static string Prefix =>
        $"<color=#66CCFF>{ReplaceStuffPrefomanceMod.settings.debugPrefix}</color>";

    /// <summary>
    /// Log a message to Verse.Log.Messages if built in debug mode
    /// </summary>
    /// <param name="x"></param>
    [System.Diagnostics.Conditional("DEBUG")]
    public static void Debug(string x)
    {
        Verse.Log.Message($"{Prefix}: {x}");
    }

    public static void Warning(string message)
    {
        Verse.Log.Warning($"{Prefix}: <color=yellow>{message}</color>");
    }

    public static void Error(string message)
    {
        Verse.Log.Error($"{Prefix}: <color=red>{message}</color>");
    }
}