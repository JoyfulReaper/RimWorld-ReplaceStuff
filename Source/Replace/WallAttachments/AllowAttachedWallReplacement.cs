/*
 * REPLACE STUFF: Perfomance Edition
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
using RimWorld;
using Verse;

namespace Replace_Stuff.Replace.WallAttachments
{
    /// <summary>
    /// Allows wall-mounted buildings to be replaced in-place.
    /// </summary>
    /// <remarks>
    /// RimWorld normally rejects placement if an existing wall
    /// attachment occupies the target location. If the existing
    /// attachment is a valid replacement target, this patch treats
    /// the placement as valid.
    /// </remarks>
    [HarmonyPatch(typeof(Placeworker_AttachedToWall), nameof(Placeworker_AttachedToWall.AllowsPlacing))]
    public static class AllowAttachedWallReplacement
    {
        /// <summary>
        /// Accepts placement when the existing wall attachment can
        /// legally be replaced by the new building.
        /// </summary>
        public static void Postfix(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, ref AcceptanceReport __result)
        {
            if (__result.Accepted || checkingDef is not ThingDef newDef || map == null)
                return;

            var thingList = loc.GetThingList(map);
            for (int i = 0; i < thingList.Count; i++)
            {
                var oldThing = thingList[i];
                if (oldThing.Rotation != rot)
                    continue;

                var oldBuiltDef = GenConstruct.BuiltDefOf(oldThing.def) as ThingDef;
                if (oldBuiltDef?.building?.isAttachment != true)
                    continue;

                if (newDef.CanReplace(oldThing.def))
                {
                    __result = AcceptanceReport.WasAccepted;
                    return;
                }
            }
        }
    }
}
