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
using Replace_Stuff.Utilities;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace Replace_Stuff.OverWallCoolers
{
    [HarmonyPatch]
    static class Patch_DrawGhost
    {
        private static readonly MethodInfo ExpandCoordinatesInfo = AccessTools.Method(typeof(Patch_DrawGhost), nameof(ExpandCoordinates));

        static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(PlaceWorker_Cooler), nameof(PlaceWorker_Cooler.DrawGhost));
            yield return AccessTools.Method(typeof(PlaceWorker_Vent), nameof(PlaceWorker_Vent.DrawGhost));
        }

        public static IEnumerable<CodeInstruction> TranspileNorthWith(IEnumerable<CodeInstruction> instructions, OpCode paramCode)
        {
            var codes = instructions.ToList();
            FieldInfo NorthInfo = AccessTools.Field(typeof(IntVec3), nameof(IntVec3.North));

            var found = false;
            foreach (CodeInstruction i in codes)
            {
                if (i.LoadsField(NorthInfo))
                {
                    found = true;
                    yield return i; // Load North
                    yield return new CodeInstruction(paramCode); // Load def
                    yield return new CodeInstruction(OpCodes.Call, ExpandCoordinatesInfo);
                }
                else
                {
                    yield return i;
                }
            }
            if (!found)
            {
                RSLog.Error("Failed to patch DrawGhost: IntVec3.North not found!");
            }
        }

        //public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol)
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return TranspileNorthWith(instructions, OpCodes.Ldarg_1);
        }

        public static IntVec3 ExpandCoordinates(IntVec3 v, object o)
        {
            ThingDef thingDef = o as ThingDef ?? (o as Thing)?.def;


            return
                thingDef == OverWallDef.Cooler_Over2W ||
                thingDef.entityDefToBuild == OverWallDef.Cooler_Over2W ||
                thingDef == OverWallDef.Vent_Over2W ||
                thingDef.entityDefToBuild == OverWallDef.Vent_Over2W
                    ? v * 2 : v;
        }
    }

    [HarmonyPatch(typeof(Building_Cooler), "TickRare")]
    static class Patch_Building_Cooler
    {
        //public override void TickRare()
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return Patch_DrawGhost.TranspileNorthWith(instructions, OpCodes.Ldarg_0);
        }
    }
}