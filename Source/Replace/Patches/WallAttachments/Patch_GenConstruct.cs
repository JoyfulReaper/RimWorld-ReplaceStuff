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
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Replace_Stuff.Replace.Patches.WallAttachments;

[HarmonyPatch(typeof(GenConstruct), nameof(GenConstruct.GetWallAttachedTo), [typeof(IntVec3), typeof(Rot4), typeof(Map)])]
public static class Patch_GenConstruct_GetWallAttachedTo
{
    /// <summary>
    /// Prevents <see cref="ReplacementFrame"/> objects from being treated as
    /// wall attachments during placement validation.
    /// </summary>
    public static bool Prefix(IntVec3 pos, Rot4 rot, Map map, ref Thing __result)
    {
        IntVec3 c = pos + GenAdj.CardinalDirections[rot.AsInt];

        if (!c.InBounds(map))
        {
            __result = null;
            return false;
        }

        foreach (Thing thing in c.GetThingList(map))
        {
            // --- ignore ReplacementFrames ---
            if (thing is ReplacementFrame)
            {
                continue;
            }
            // ----------------------

            // vanilla logic
            if (GenConstruct.BuiltDefOf(thing.def) is ThingDef { building: not null } thingDef && 
                thingDef.building.supportsWallAttachments)
            {
                __result = thing;
                return false;
            }
        }

        __result = null;
        return false;
    }
}

// The old transpiler... Technically it is more perfomant
// [HarmonyPatch(typeof(GenConstruct), nameof(GenConstruct.GetWallAttachedTo), [typeof(IntVec3), typeof(Rot4), typeof(Map)])]
// public static class Patch_GenConstruct
// {
//     /// <summary>
//     /// Prevents <see cref="ReplacementFrame"/> objects from being treated as
//     /// wall attachments during placement validation.
//     /// </summary>
//     /// <remarks>
//     /// RimWorld scans nearby things when determining whether a wall has
//     /// attached structures (vents, coolers, etc.). During replacement,
//     /// the temporary <see cref="ReplacementFrame"/> should be ignored,
//     /// otherwise it can interfere with attachment logic.
//     /// </remarks>
//     public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
//     {
//         // In the loop: foreach (Thing thing in c.GetThingList(map))
//         // insert:
//         // if(thing is ReplaceFrame) continue;
//         // (This is probably redundant because the wall under the replace frame will probably always be checked first and returned.)

//         // The first br should branch to the entry point for the loop, keep that label to continue to
//         var continueLabel = (Label)instructions.First(ci => ci.opcode == OpCodes.Br_S).operand;
//         var defInfo = AccessTools.Field(typeof(Thing), nameof(Thing.def));

//         var insts = instructions.ToList();
//         for (int i = 0; i < insts.Count; i++)
//         {
//             var inst = insts[i];
//             yield return inst;

//             // Before we get the thing.def, use the thing:
//             if (i + 1 < insts.Count && insts[i + 1].LoadsField(defInfo))
//             {
//                 // stack has: Thing thing from the list
//                 yield return new CodeInstruction(OpCodes.Isinst, typeof(ReplacementFrame));// thing == typeof(ReplaceFrame)
//                 yield return new CodeInstruction(OpCodes.Brtrue_S, continueLabel);// if(thing == typeof(ReplaceFrame)) continue;

//                 // Call ldlocal for Thing again to replace what was there (with, maybe, no labels...)
//                 yield return new CodeInstruction(inst.opcode, inst.operand);
//             }
//         }
//     }
// }

/// <summary>
/// Prevents replacement frames from reporting wall attachments.
/// </summary>
/// <remarks>
/// A <see cref="ReplacementFrame"/> temporarily occupies the same cell as the
/// building being replaced. Any attached structures (wall lights, vents,
/// coolers, etc.) should remain associated with the original building
/// until replacement is complete.
///
/// Returning an empty list prevents RimWorld from treating the temporary
/// frame as a valid attachment host during placement and validation checks.
/// </remarks>
[HarmonyPatch(typeof(GenConstruct), nameof(GenConstruct.GetAttachedBuildings))]
public static class Patch_GenConstruct
{
    private static List<Thing> emptyList = [];

    /// <summary>
    /// Returns an empty attachment list for replacement frames,
    /// bypassing the original method.
    /// </summary>
    public static bool Prefix(Thing thing, ref List<Thing> __result)
    {
        if (thing is null || !thing.Spawned || thing.Map != null)
        {
            __result = emptyList;
            return false;
        }

        if (thing is ReplacementFrame)
        {
            __result = emptyList;
            return false; // skip original method
        }

        return true;
    }
}