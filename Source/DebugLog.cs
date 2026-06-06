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
    /// <summary>
    /// Log a message to Verse.Log.Messages if built in debug mode
    /// </summary>
    /// <param name="x"></param>
    [System.Diagnostics.Conditional("DEBUG")]
    public static void Message(string x)
    {
        Verse.Log.Message(x);
    }
}