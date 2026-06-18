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

using Verse;
using RimWorld;
using HarmonyLib;

namespace Replace_Stuff.DestroyedRestore.Patches;

[HarmonyPatch(typeof(Frame), nameof(Frame.Destroy))]
class FrameRemoval
{
    //public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
    public static void Prefix(Frame __instance, DestroyMode mode)
    {
        if (mode != DestroyMode.Vanish && mode != DestroyMode.FailConstruction && mode != DestroyMode.KillFinalize)
            ReplacementStateStore.RemoveAt(__instance.Position, __instance.Map);
    }
}