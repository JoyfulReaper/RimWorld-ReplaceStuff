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
    internal static class ReplacementHandler
    {
        /// <summary>
        /// Initiates the appropriate replacement workflow for the
        /// specified object.
        /// </summary>
        /// <param name="thing">
        /// The existing building, frame, or blueprint.
        /// </param>
        /// <param name="stuffDef">
        /// The material to use for the replacement.
        /// </param>
        /// <remarks>
        /// Depending on the target object, this method may:
        /// <list type="bullet">
        /// <item><description>Create required bridge support.</description></item>
        /// <item><description>Replace existing blueprints.</description></item>
        /// <item><description>Upgrade replacement frames.</description></item>
        /// <item><description>Perform instant replacement in God Mode.</description></item>
        /// <item><description>Create a new replacement frame.</description></item>
        /// </list>
        /// </remarks>
        public static void ExecuteReplacement(Thing thing, ThingDef stuffDef)
        {
            var pos = thing.Position;
            var rot = thing.Rotation;
            var map = thing.Map;

            //In case you're replacing with a stuff that needs a higher affordance that bridges can handle.
            PlaceBridges.EnsureBridge.PlaceBridgeIfNeeded(thing.def, pos, map, rot, Faction.OfPlayer, stuffDef);

            // Validation has already confirmed that the target
            // material differs from the current one. At this
            // stage we only need to transition existing
            // replacement objects to the new material.
            if (thing is Blueprint_Build oldBP)
            {
                // Destroy first because blueprint placement
                // will automatically wipe existing objects
                oldBP.Destroy(DestroyMode.Cancel);
                GenConstruct.PlaceBlueprintForBuild(oldBP.def.entityDefToBuild, pos, map, rot, Faction.OfPlayer, stuffDef);
            }
            else if (thing is ReplacementFrame oldRF)
            {
                if (DebugSettings.godMode)
                {
                    ReplacementUtility.InstantReplace(oldRF.targetThing, stuffDef);
                    oldRF.Destroy(DestroyMode.Cancel);
                    return;
                }
                if (oldRF.targetStuff != stuffDef)
                {
                    //replacement frame should keep deconstruction work amount
                    var newFrame = ReplacementUtility.SpawnReplacementFrame(oldRF.targetThing, stuffDef);
                    if (newFrame != null)
                    {
                        newFrame.workDone = Mathf.Min(oldRF.workDone, oldRF.WorkToDeconstruct);
                    }
                }
                //else, if same stuff as old stuff, we just chose replace with original stuff, so we're already done - just destroy the frame.
                oldRF.Destroy(DestroyMode.Cancel);
            }
            else if (thing is Frame oldFrame)
            {
                oldFrame.Destroy(DestroyMode.Cancel);
                GenConstruct.PlaceBlueprintForBuild(oldFrame.def.entityDefToBuild, pos, map, rot, Faction.OfPlayer, stuffDef);
            }
            else if (DebugSettings.godMode)
            {
                ReplacementUtility.InstantReplace(thing, stuffDef);
            }
            else
            {
                ReplacementUtility.SpawnReplacementFrame(thing, stuffDef);
            }

            FleckMaker.ThrowMetaPuffs(GenAdj.OccupiedRect(pos, rot, thing.def.size), map);
        }
    }
}