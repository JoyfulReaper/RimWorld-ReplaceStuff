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
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace Replace_Stuff.Core.Patches;

[HarmonyPatch(typeof(GenSpawn), nameof(GenSpawn.Refund))]
//public static void Refund(Thing thing, Map map, CellRect avoidThisRect)
public static class Patch_GenSpawn
{
    public static bool IsReplacementInProgress = false;

    public static void Postfix()
    {
        IsReplacementInProgress = false;
    }

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        MethodInfo MinifiableInfo = AccessTools.Property(typeof(ThingDef), "Minifiable").GetGetMethod();
        MethodInfo DecideDestroyModeInfo = AccessTools.Method(typeof(Patch_GenSpawn), nameof(Patch_GenSpawn.DecideDestroyMode));
        MethodInfo NevermindAboutMinifiableInfo = AccessTools.Method(typeof(Patch_GenSpawn), nameof(Patch_GenSpawn.NevermindAboutMinifiable));

        foreach (CodeInstruction i in instructions)
        {
            if (i.LoadsConstant(DestroyMode.Refund))//DestroyMode.Refund
                yield return new CodeInstruction(OpCodes.Call, DecideDestroyModeInfo);
            else
                yield return i;

            if (i.Calls(MinifiableInfo))
                yield return new CodeInstruction(OpCodes.Call, NevermindAboutMinifiableInfo);
        }
    }

    public static DestroyMode DecideDestroyMode()
    {
        return IsReplacementInProgress ? DestroyMode.Deconstruct : DestroyMode.Refund;
    }

    public static bool NevermindAboutMinifiable(bool minifiable)
    {
        return IsReplacementInProgress ? false : minifiable;
    }
}