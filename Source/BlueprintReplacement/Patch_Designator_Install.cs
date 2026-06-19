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

// NOTE:
// I don't think that this patch is needed anymore
// If we start to get Identical Thing Exists Erorrs
// its possible that its due to removing this patch

// using HarmonyLib;
// using Replace_Stuff.Replace.Patches;
// using Replace_Stuff.Utilities;
// using RimWorld;
// using System.Collections.Generic;
// using System.Reflection.Emit;

// namespace Replace_Stuff.BlueprintReplace
// {
//     [HarmonyPatch(typeof(Designator_Install), "CanDesignateCell")]
//     //public override AcceptanceReport CanDesignateCell(IntVec3 c)
//     static class DesignatorInstall
//     {
//         public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
//         {
//             RSLog.Debug("Designator_Install transpiler active");
//             RSLog.Debug($"IsInBuildDesignation = {DesignatorContext.IsInBuildDesignation}");
//             bool next = false;
//             foreach (CodeInstruction i in instructions)
//             {
//                 if (next)
//                 {
//                     next = false;
//                     yield return new CodeInstruction(OpCodes.Pop);
//                 }
//                 else
//                     yield return i;

//                 //Code checks !this.MiniToInstallOrBuildingToReinstall is MinifiedThing to set IdenticalThingExists, so let's ignore taht
//                 if (i.opcode == OpCodes.Isinst)
//                     next = true;
//             }
//         }
//     }
// }
