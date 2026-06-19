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
using System.Linq;
using System.Reflection.Emit;
using Verse;

namespace Replace_Stuff.Terrain.Patches;

// 1) Change Conduit PlaceWorker to allow conduits over conduits
// 2) TL;DR: actually ignore the thingToIgnore argument
//This patch fixes a vanilla bug/oversight, that forgot to check thingToIgnore in conduit's placeworker.
//This wasn't a problem in vanilla, but with replace stuff, conduit blueprints would disappear when doors are opened
//Since revealing new areas now triggers re-checking blueprints, power conduit blueprints would check their tile.
//They'd find themselves, and determine they can't exist because there's already a power conduit there.
//The thingToIgnore arguments already exists, to, you know, ignore that. But it wasn't checked.
[HarmonyPatch(typeof(PlaceWorker_Conduit), "AllowsPlacing")]
public static class Patch_PlaceWorker_Conduit
{
    //AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var entityDefInfo = AccessTools.Field(typeof(ThingDef), "entityDefToBuild");

        //need to find loop continue label
        object continueLabel = null;

        //just easier to find the 3-line code to get thingList[i]
        bool foundLdLoc = false;
        List<CodeInstruction> currentThing = new List<CodeInstruction>();

        List<CodeInstruction> iList = instructions.ToList();
        for (int k = 0; k < iList.Count(); k++)
        {
            CodeInstruction i = iList[k];
            if (!foundLdLoc && i.opcode == OpCodes.Ldloc_0)
            {
                foundLdLoc = true;
                currentThing.AddRange(iList.GetRange(k, 3));
            }
            if (i.LoadsField(entityDefInfo))
            {
                continueLabel = iList[k + 1].operand;
                break;
            }
        }

        foundLdLoc = false;
        foreach (CodeInstruction i in instructions)
        {
            if (!foundLdLoc && i.opcode == OpCodes.Ldloc_0)
            {
                foundLdLoc = true;

                // At start of loop, insert:

                // Noop with start of loop label
                yield return new CodeInstruction(OpCodes.Nop) { labels = i.labels };//start of loop label
                i.labels = new List<Label>();


                // if(thingList[i] == thingToIgnore)
                //	continue;
                foreach (CodeInstruction ci in currentThing)
                    yield return new CodeInstruction(ci.opcode, ci.operand);//thingList[i] (Thing)

                yield return new CodeInstruction(OpCodes.Ldarg_S, 5);//thingToIgnore
                yield return new CodeInstruction(OpCodes.Beq, continueLabel);//if( ... == ... ) continue;
            }
            yield return i;
        }
    }

    public static bool IsConduit(Thing t)
    {
        return (t.def.building?.isPowerConduit ?? false);
    }
}