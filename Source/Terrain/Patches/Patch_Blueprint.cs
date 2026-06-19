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

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using RimWorld;
using Verse;
using HarmonyLib;

namespace Replace_Stuff.OverMineable.Patches;

//Blueprint can become a frame even if final thing would be blocked
[HarmonyPatch(typeof(Blueprint), "TryReplaceWithSolidThing")]
public static class BlueprintToFrameUnderRock
{
    //public virtual bool TryReplaceWithSolidThing(Pawn workerPawn, out Thing createdThing, out bool jobEnded)
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        //Replace
        MethodInfo FirstBlockingThingInfo = AccessTools.Method(typeof(GenConstruct), "FirstBlockingThing");

        List<CodeInstruction> list = instructions.ToList();
        for(int i=0;i< list.Count; i++)
        {
            CodeInstruction inst = list[i];
            yield return inst;
            if (inst.Calls(FirstBlockingThingInfo))
            {
                //Frame can be made
                yield return new CodeInstruction(OpCodes.Pop);
                i++;
                yield return new CodeInstruction(OpCodes.Br, list[i].operand) { labels = list[i].labels };
            }
        }
    }
}