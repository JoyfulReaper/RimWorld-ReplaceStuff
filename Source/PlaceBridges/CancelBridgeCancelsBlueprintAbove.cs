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
using Verse;

namespace Replace_Stuff.PlaceBridges
{
    public static class CancelAboveBridges
    {
        public static void CancelAbove(BuildableDef defToBuild, DestroyMode mode, Map map, IntVec3 pos)
        {
            if (defToBuild.IsBridgelike()
                && mode != DestroyMode.Vanish && mode != DestroyMode.FailConstruction)
            {
                List<Thing> toKill = new List<Thing>();
                foreach (Thing thing in map.thingGrid.ThingsListAtFast(pos))
                {
                    //this sorta assumes the thing is not actually built, vanilla would handle that.
                    if (thing is Blueprint bp && !bp.def.entityDefToBuild.IsBridgelike())
                        toKill.Add(thing);
                    if (thing is Frame fr && !fr.def.entityDefToBuild.IsBridgelike())
                        toKill.Add(thing);
                }
                //Kill unless it's already killed or it's IsSelected.
                //IsSelected probably not the best since possible non-cancel destruction would keep the thing above, but what are the chances of that?
                toKill.Do(t =>
                    {
                        if (!t.Destroyed && !Find.Selector.IsSelected(t))
                            t.Destroy(DestroyMode.Refund);
                    });
            }
        }
    }
}