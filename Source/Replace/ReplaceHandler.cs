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
    /// <summary>
    /// Handles the orchestration of replacing existing structures with new material definitions.
    /// </summary>
    /// <remarks>
    /// This class acts as the central logic gate for determining how to transition from an existing 
    /// <see cref="Thing"/> (or <see cref="Frame"/>/<see cref="Blueprint"/>) to a replacement structure.
    /// It manages bridge requirements, state preservation, and the spawning of new construction tasks.
    /// </remarks>
    internal class ReplaceHandler
    {
        /// <summary>
        /// Executes the replacement process for a specific structure using the provided <see cref="ThingDef"/>.
        /// </summary>
        /// <param name="thing">The existing structure, blueprint, or replacement frame to be replaced.</param>
        /// <param name="stuffDef">The new <see cref="ThingDef"/> (material) to use for the replacement.</param>
        /// <remarks>
        /// This method:
        /// <list type="bullet">
        /// <item><description>Ensures necessary bridges are placed if the site requires terrain support.</description></item>
        /// <item><description>Handles cleanup of existing blueprints, frames, or structures.</description></item>
        /// <item><description>Spawns the new <see cref="ReplacementFrame"/> or <see cref="Blueprint_Build"/>.</description></item>
        /// <item><description>Provides instant replacement feedback under God Mode.</description></item>
        /// </list>
        /// </remarks>
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
            else if (thing is ReplacementFrame oldRF)
            {
                if (DebugSettings.godMode)
                {
                    ReplaceUtility.InstantReplace(oldRF.targetThing, stuffDef);
                    oldRF.Destroy(DestroyMode.Cancel);
                    return;
                }
                if (oldRF.targetStuff != stuffDef)
                {
                    //replacement frame should keep deconstruction work mount
                    var newFrame = GenReplace.PlaceReplaceFrame(oldRF.targetThing, stuffDef);
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
                GenReplace.PlaceReplaceFrame(thing, stuffDef);
            }

            FleckMaker.ThrowMetaPuffs(GenAdj.OccupiedRect(pos, rot, thing.def.size), map);
        }
    }
}
