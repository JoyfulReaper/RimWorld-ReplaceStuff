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

namespace Replace_Stuff.Compatibility;

using Replace_Stuff.Interfaces;
using Replace_Stuff.Replace;
using Verse;

// Tell Visual Studio to shut up about Obsolete warnings
#pragma warning disable CS0618
public class LegacyReplacementBridge : IReplacementHandler
{
    private readonly IReplacementComp legacyComp;
    private Thing cachedOldThing;

    public LegacyReplacementBridge(IReplacementComp comp)
    {
        legacyComp = comp;
    }

    public void PreAction(ReplaceData data, Thing oldThing)
    {
        // Cache the old thing so we can give it to the legacy PostAction later
        cachedOldThing = oldThing;
        legacyComp.PreAction(null, oldThing);
    }

    public void PostAction(ReplaceData data, Thing newThing)
    {
        legacyComp.PostAction(newThing, cachedOldThing);
    }
}
#pragma warning restore CS0618