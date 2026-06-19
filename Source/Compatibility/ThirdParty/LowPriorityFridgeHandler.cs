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

using Replace_Stuff.Interfaces;
using Replace_Stuff.Replace;
using Verse;

namespace Replace_Stuff.Compatibility.ThirdParty;

#if DEBUG
[ReplacementHandler("Building_Refrigerator", priority: -10)]
public class LowPriorityFridgeHandler : IReplacementHandler
{
    public void PreAction(ReplaceData d, Thing o, Thing n) { }
    public void PostAction(ReplaceData d, Thing o, Thing n)
    {
        Verse.Log.Message("Low Priority Handler Triggered!");
    }
}
#else
// Don't include this unless we are in a debug release
#endif