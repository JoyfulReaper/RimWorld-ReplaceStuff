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

namespace Replace_Stuff.Replace.Patches;

/// <summary>
/// Prevents turrets from firing while they are being replaced.
/// </summary>
[HarmonyPatch(typeof(RimWorld.Building_TurretGun), "TryStartShootSomething")]
class Patch_Building_TurretGun
{
    /// <summary>
    /// Skips turret targeting if the turret is under replacement.
    /// </summary>
    //protected void TryStartShootSomething(bool canBeginBurstImmediately)
    public static bool Prefix(RimWorld.Building_TurretGun __instance)
    {
        return !ReplacementUtility.IsBeingReplaced(__instance);//__instance.ResetCurrentTarget();
    }
}