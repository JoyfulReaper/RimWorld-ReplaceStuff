using HarmonyLib;
using Replace_Stuff.Replace.Patches;
using RimWorld;
using Verse;

namespace Replace_Stuff.NewThing;

/// <summary>
/// Extends vanilla replacement logic to allow custom building replacements.
/// This patch intercepts GenConstruct.CanReplace to evaluate if a new ThingDef 
/// is authorized to replace an existing one. It includes safety checks to 
/// prevent replacing indestructible structures and only executes during active 
/// blueprint designation to ensure performance.
/// </summary>

[HarmonyPatch(typeof(GenConstruct), nameof(GenConstruct.CanReplace))]
internal class CanReplaceNewThingOverOldThing
{
    // Update: Include placingStuff and existingStuff to mirror the 1.6 vanilla signature
    public static void Postfix(ref bool __result, BuildableDef placing, BuildableDef existing, ThingDef placingStuff = null, ThingDef existingStuff = null)
    {
        if (!DesignatorContext.IsInBuildDesignation)
            return;

        if (((placing as ThingDef)?.IsNonDeconstructibleAttackableBuilding ?? false) ||
            ((existing as ThingDef)?.IsNonDeconstructibleAttackableBuilding ?? false))
        {
            __result = false;
            return;
        }

        if (placing == existing && placingStuff == existingStuff)
        {
            __result = false;
            return;
        }

        if (__result) return;

        if (placing is ThingDef newD && existing is ThingDef oldD && newD.CanReplace(oldD))
            __result = true;
    }
}

// [HarmonyPatch(typeof(GenConstruct), nameof(GenConstruct.CanReplace))]
// internal class CanReplaceNewThingOverOldThing
// {
//     public static void Postfix(ref bool __result, BuildableDef placing, BuildableDef existing)
//     {
//         // The player isn't actively placing a blueprint
//         if (!DesignatorContext.IsInBuildDesignation)
//             return;

//         if (((placing as ThingDef)?.IsNonDeconstructibleAttackableBuilding ?? false) ||
//             ((existing as ThingDef)?.IsNonDeconstructibleAttackableBuilding ?? false))
//         {
//             __result = false;
//             return;
//         }

//         if (__result) return;

//         if (placing is ThingDef newD && existing is ThingDef oldD && newD.CanReplace(oldD))
//             __result = true;
//     }
// }