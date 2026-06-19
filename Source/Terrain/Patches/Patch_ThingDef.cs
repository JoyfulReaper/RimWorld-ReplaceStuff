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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace Replace_Stuff.Terrain.Patches;

//remove rock from rejection of CanInteractThroughCorners
[HarmonyPatch(typeof(ThingDef), nameof(ThingDef.CanInteractThroughCorners), MethodType.Getter)]
public static class Patch_ThingDef
{
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        FieldInfo buildingInfo = AccessTools.Field(typeof(ThingDef), nameof(ThingDef.building));

        List<CodeInstruction> instList = instructions.ToList();
        for (int i = 0; i < instList.Count(); i++)
        {
            CodeInstruction inst = instList[i];

            if (inst.LoadsField(buildingInfo))
            {
                //IL_0015: ldarg.0      // this
                //IL_0016: ldfld        class RimWorld.BuildingProperties Verse.ThingDef::building

                //replace the this.building code with the end return true:

                instList[i - 1].opcode = OpCodes.Ldc_I4_1;//preserve label here
                instList[i] = new CodeInstruction(OpCodes.Ret);

                //chop off rest of the code that checks rock and smooth ezpz, we can corner touch rocks now!
                return instList.Take(i + 1);
            }
        }
        Verse.Log.Warning("Replace Stuff failed to patch CanInteractThroughCorners");
        return instructions;
    }
}