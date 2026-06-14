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
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace Replace_Stuff.NewThing;

[HarmonyPatch(typeof(Frame), "CompleteConstruction")]
//public void CompleteConstruction(Pawn worker)
public static class RememberWasNewThing
{
    public static void Prefix(Frame __instance)
    {
        RefundDeconstruct.IsReplacementInProgress = __instance.IsNewThingReplacement(out Thing replacement);
    }
}

[HarmonyPatch(typeof(GenSpawn), nameof(GenSpawn.Refund))]
//public static void Refund(Thing thing, Map map, CellRect avoidThisRect)
public static class RefundDeconstruct
{
    public static bool IsReplacementInProgress = false;

    public static void Postfix()
    {
        IsReplacementInProgress = false;
    }

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        MethodInfo MinifiableInfo = AccessTools.Property(typeof(ThingDef), "Minifiable").GetGetMethod();

        MethodInfo DecideDestroyModeInfo = AccessTools.Method(typeof(RefundDeconstruct), nameof(RefundDeconstruct.DecideDestroyMode));
        MethodInfo NevermindAboutMinifiableInfo = AccessTools.Method(typeof(RefundDeconstruct), nameof(RefundDeconstruct.NevermindAboutMinifiable));

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
