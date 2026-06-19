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
using Replace_Stuff.Replace.Patches;
using Replace_Stuff.Terrain.Extensions;
using RimWorld;
using System.Linq;
using Verse;

namespace Replace_Stuff.Terrain.Patches;

[HarmonyPatch(typeof(FogGrid), "UnfogWorker")]
public static class Patch_FogGrid
{
    //private void UnfogWorker(IntVec3 c)
    public static void Postfix(FogGrid __instance, IntVec3 c, Map ___map)
    {
        Map map = ___map;
        if (c.GetThingList(map).FirstOrDefault(t => t.def.IsBlueprint) is Thing blueprint && !blueprint.IsUnderFog())
        {
            DesignatorContext.Enter(); // as good as designating.

            if (!GenConstruct.CanPlaceBlueprintAt(blueprint.def.entityDefToBuild, blueprint.Position, blueprint.Rotation, map, false, blueprint).Accepted)
                blueprint.Destroy();
            else
                blueprint.Notify_ColorChanged();//does the job, haha.

            DesignatorContext.Exit();
        }
    }
}

