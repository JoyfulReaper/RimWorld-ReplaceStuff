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
using Replace_Stuff.NewThing;
using Verse;

namespace Replace_Stuff.Replace.WallAttachments;

/// <summary>
/// Allows compatible wall attachments to wipe their replacement targets.
/// </summary>
/// <remarks>
/// RimWorld's default wipe logic does not consider attachment-to-attachment
/// replacement. This patch enables one attached building to overwrite
/// another when the replacement rules allow it.
/// </remarks>
[HarmonyPatch(typeof(GenSpawn), nameof(GenSpawn.SpawningWipes))]
public static class AttachmentsCanWipeReplacementTargets
{
    /// <summary>
    /// Marks compatible attachment replacements as valid wipe operations.
    /// </summary>
    public static void Postfix(BuildableDef newEntDef, BuildableDef oldEntDef, ref bool __result)
    {
        if (__result || newEntDef is not ThingDef newDef || oldEntDef is not ThingDef oldDef)
            return;

        if ((!newDef.building?.isAttachment ?? true) || (!oldDef.building?.isAttachment ?? true))
            return;

        if (newDef.CanReplace(oldDef))
            __result = true;
    }
}
