using HarmonyLib;
using Replace_Stuff.Replace;
using RimWorld;
using Verse;

namespace Replace_Stuff.Core.Patches;

/// <summary>
/// Extends the work calculation for build blueprints to include deconstruction effort.
/// This patch modifies the WorkTotal getter for Blueprints to account for 
/// the deconstruction cost of the target object, ensuring pawns correctly 
/// calculate the total labor needed to replace an existing building with 
/// the new structure specified in the blueprint.
/// </summary>
[HarmonyPatch(typeof(Blueprint_Build), "WorkTotal", MethodType.Getter)]
public static class NewThingDeconstructWork_Blueprint
{
    //public float WorkToBuild
    public static void Postfix(Frame __instance, ref float __result)
    {
        if (__instance.TryFindTarget(out Thing oldThing))
            __result += ReplacementFrame.WorkToDeconstructDef(oldThing.def, oldThing.Stuff);
    }
}