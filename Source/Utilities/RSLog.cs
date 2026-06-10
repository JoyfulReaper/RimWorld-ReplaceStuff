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

using System.Diagnostics;

namespace Replace_Stuff.Utilities;

static class RSLog
{
    // TODO: Option for if the prefix should be colored or not
    // the color tags can make the log harder to read
    public static string PrefixColored =>
        $"<color=#66CCFF>{LoggingPrefix}</color>";

    public static string LoggingPrefix =>
        ReplaceStuffPerformance.settings.MessagePrefix;

    /// <summary>
    /// Log a message to Verse.Log.Messages if built in debug mode
    /// </summary>
    /// <param name="x"></param>
    [Conditional("DEBUG")]
    public static void Debug(string message)
    {
        Verse.Log.Message($"{PrefixColored}: {message}");
    }

    /// <summary>
    /// Log a message to Verse.Log.Messages regardless of debug mode
    /// </summary>
    /// <param name="x"></param>
    public static void Info(string message)
    {
        Verse.Log.Message($"{PrefixColored}: {message}");
    }

    /// <summary>
    /// Log a message to Verse.Log.Warning
    /// </summary>
    /// <param name="x"></param>
    public static void Warning(string message)
    {
        Verse.Log.Warning($"{LoggingPrefix}: {message}");
    }

    /// <summary>
    /// Log a message to Verse.Log.Error
    /// </summary>
    /// <param name="x"></param>
    public static void Error(string message)
    {
        Verse.Log.Error($"{LoggingPrefix}: {message}");
    }
}