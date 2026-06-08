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
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;

namespace Replace_Stuff.Replace.WallAttachments;

[HarmonyPatch(typeof(GenConstruct), nameof(GenConstruct.GetWallAttachedTo), [typeof(IntVec3), typeof(Rot4), typeof(Map)])]
public static class NoWallAttachment
{
    /// <summary>
    /// Prevents <see cref="ReplaceFrame"/> objects from being treated as
    /// wall attachments during placement validation.
    /// </summary>
    /// <remarks>
    /// RimWorld scans nearby things when determining whether a wall has
    /// attached structures (vents, coolers, etc.). During replacement,
    /// the temporary <see cref="ReplaceFrame"/> should be ignored,
    /// otherwise it can interfere with attachment logic.
    /// </remarks>
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        // In the loop: foreach (Thing thing in c.GetThingList(map))
        // insert:
        // if(thing is ReplaceFrame) continue;
        // (This is probably redundant because the wall under the replace frame will probably always be checked first and returned.)


        // The first br should branch to the entry point for the loop, keep that label to continue to
        var continueLabel = (Label)instructions.First(ci => ci.opcode == OpCodes.Br_S).operand;
        var defInfo = AccessTools.Field(typeof(Thing), nameof(Thing.def));

        var insts = instructions.ToList();
        for (int i = 0; i < insts.Count; i++)
        {
            var inst = insts[i];
            yield return inst;

            // Before we get the thing.def, use the thing:
            if (i + 1 < insts.Count && insts[i + 1].LoadsField(defInfo))
            {
                // stack has: Thing thing from the list
                yield return new CodeInstruction(OpCodes.Isinst, typeof(ReplaceFrame));// thing == typeof(ReplaceFrame)
                yield return new CodeInstruction(OpCodes.Brtrue_S, continueLabel);// if(thing == typeof(ReplaceFrame)) continue;

                // Call ldlocal for Thing again to replace what was there (with, maybe, no labels...)
                yield return new CodeInstruction(inst.opcode, inst.operand);
            }
        }
    }
}