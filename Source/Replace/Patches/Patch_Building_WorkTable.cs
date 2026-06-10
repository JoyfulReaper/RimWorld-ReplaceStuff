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
using Verse;
using Verse.AI;

namespace Replace_Stuff.Replace.Patches;

/// <summary>
/// Prevents worktables from accepting bills while replacement
/// construction is in progress.
/// </summary>
[HarmonyPatch(typeof(Building_WorkTable), "UsableForBillsAfterFueling")]
class Patch_Building_WorkTable
{
    /// <summary>
    /// Marks the workbench as unusable when replacing.
    /// </summary>
    //public virtual bool UsableNow
    public static void Postfix(ref bool __result, Building_WorkTable __instance)
    {
        if (!__result)
        {
            return;
        }

        if (ReplacementUtility.IsBeingReplaced(__instance))
        {
            __result = false;
            JobFailReason.Is("TD.FailedStuffBeingReplaced".Translate());
        }
    }
}