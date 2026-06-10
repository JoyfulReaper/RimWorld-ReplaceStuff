using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace Replace_Stuff.Replace.Patches;

/// <summary>
/// Prevents pawns from using beds that are actively being replaced.
///
/// Rather than modifying RestUtility.FindBedFor directly,
/// the returned bed is nullified after selection to avoid
/// colonists claiming alternative beds.
/// </summary>
[HarmonyPatch(typeof(JobGiver_GetRest), "TryGiveJob")]
public static class Patch_JobGiver_GetRest
{
    private static MethodInfo FindBedForInfo = AccessTools.Method(typeof(RestUtility), "FindBedFor", new Type[] { typeof(Pawn) });
    private static MethodInfo NullifyReplacingBedInfo = AccessTools.Method(typeof(Patch_JobGiver_GetRest), nameof(Patch_JobGiver_GetRest.NullifyBed));

    //protected override Job TryGiveJob(Pawn pawn)
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        foreach (CodeInstruction i in instructions)
        {
            yield return i;

            if (i.Calls(FindBedForInfo))
            {
                //Ideally filter out the bed in IsValidBedFor,
                //but then FindBedFor would skip your owned bed, find another bed and claim it
                //so this is simplest, just sleep on the ground for tonight if your bed is being worked on
                yield return new CodeInstruction(OpCodes.Call, NullifyReplacingBedInfo);
            }
        }
    }

    /// <summary>
    /// Returns null if the specified bed is currently being
    /// replaced; otherwise returns the original bed.
    /// </summary>
    /// <param name="bed">
    /// The bed selected by RestUtility.FindBedFor.
    /// </param>
    /// <returns>
    /// The original bed or null if replacement is active.
    /// </returns>
    public static Building_Bed NullifyBed(Building_Bed bed)
    {
        return ReplacementUtility.IsBeingReplaced(bed) ? null : bed;
    }
}