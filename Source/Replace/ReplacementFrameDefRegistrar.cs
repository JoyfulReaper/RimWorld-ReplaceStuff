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
using System;
using System.Collections.Generic;
using Verse;

namespace Replace_Stuff.Replace;

internal static class ReplacementFrameDefRegistrar
{
    /// <summary>Delegate for accessing the private ShortHashGiver.GiveShortHash method.</summary>
    private delegate void GiveShortHashDelegate(Def d, Type t, HashSet<ushort> h);

    /// <summary>Bridge to the game's internal method for assigning short hashes to dynamic Defs.</summary>
    private static readonly GiveShortHashDelegate GiveShortHash =
        AccessTools.MethodDelegate<GiveShortHashDelegate>(AccessTools.Method(typeof(ShortHashGiver), "GiveShortHash"));

    /// <summary>
    /// Registers newly generated replacement frame Defs into the game's DefDatabase and tracking systems.
    /// </summary>
    /// <param name="addShortHash">When set to <c>true</c>, registers unique identity fingerprint keys inside short reference maps.</param>
    public static void RegisterReplacementFrames(bool addShortHash = true)
    {
        Type type = typeof(ThingDef);

        // Slow reflection since this is only once:
        var takenHashes = ((Dictionary<Type, HashSet<ushort>>)AccessTools.Field(typeof(ShortHashGiver), "takenHashesPerDeftype").GetValue(null))[type];

        foreach (ThingDef current in ReplacementFrameDefGenerator.GenerateReplacementFrameDefs())
        {
            if (addShortHash)  //Wouldn't need this if other mods added defs earlier. Oh well.
                GiveShortHash(current, type, takenHashes);

            current.PostLoad();
            DefDatabase<ThingDef>.Add(current);
        }
    }
}
