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

using Replace_Stuff.Utilities;
using RimWorld;
using UnityEngine;
using Verse;

namespace Replace_Stuff.Replace
{
    internal class ReplaceHandler
    {
        // TODO Start refactoring from here -
        public static void ExecuteReplacement(Thing thing, ThingDef stuffDef)
        {
            var pos = thing.Position;
            var rot = thing.Rotation;
            var map = thing.Map;

            //In case you're replacing with a stuff that needs a higher affordance that bridges can handle.
            PlaceBridges.EnsureBridge.PlaceBridgeIfNeeded(thing.def, pos, map, rot, Faction.OfPlayer, stuffDef);

            //CanReplaceStuffFor has verified this is different stuff
            //so the task here is: place new replacements, kill old replacement
            //Too finicky to change stuff of current replacement - canceling jobs and such.
            if (thing is Blueprint_Build oldBP)
            {
                oldBP.Destroy(DestroyMode.Cancel);
                //Destroy before Place beacause GenSpawn.Spawn will wipe it

                GenConstruct.PlaceBlueprintForBuild(oldBP.def.entityDefToBuild, pos, map, rot, Faction.OfPlayer, stuffDef);
            }
            else if (thing is ReplaceFrame oldRF)
            {
                if (DebugSettings.godMode)
                {
                    ReplaceUtility.InstantReplace(oldRF.oldThing, stuffDef);
                    oldRF.Destroy(DestroyMode.Cancel);
                    return;
                }
                if (oldRF.oldStuff != stuffDef)
                {
                    //replacement frame should keep deconstruction work mount
                    ReplaceFrame newFrame = GenReplace.PlaceReplaceFrame(oldRF.oldThing, stuffDef);
                    if (newFrame != null)
                    {
                        newFrame.workDone = Mathf.Min(oldRF.workDone, oldRF.WorkToDeconstruct);
                    }
                }
                //else, if same stuff as old stuff, we just chose replace with original stuff, so we're already done - just destroy the frame.
                //upgrade frames/blueprints

                oldRF.Destroy(DestroyMode.Cancel);
            }
            else if (thing is Frame oldFrame)
            {
                oldFrame.Destroy(DestroyMode.Cancel);

                GenConstruct.PlaceBlueprintForBuild(oldFrame.def.entityDefToBuild, pos, map, rot, Faction.OfPlayer, stuffDef);
            }
            else if (DebugSettings.godMode)
            {
                ReplaceUtility.InstantReplace(thing, stuffDef);
            }
            else
            {
                //Oh of course the standard case is, just place a replace frame! I almost forgot about that.
                GenReplace.PlaceReplaceFrame(thing, stuffDef);
            }

            FleckMaker.ThrowMetaPuffs(GenAdj.OccupiedRect(pos, rot, thing.def.size), map);
        }
    }
}
