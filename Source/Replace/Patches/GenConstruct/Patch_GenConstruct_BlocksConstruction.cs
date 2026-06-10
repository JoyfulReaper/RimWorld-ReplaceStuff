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
using Verse;

namespace Replace_Stuff.Replace.Patches.GenConstruct;

/// <summary>
/// Redirects reservation requests from replacement frames back to the
/// original structures they represent.
/// </summary>
/// <remarks>
/// <para>
/// Vanilla RimWorld uses <see cref="GenConstruct.BlocksConstruction"/> to determine if a building 
/// project is obstructed by other entities, which would normally prevent the placement or 
/// construction of an item.
/// </para> 
/// <para>
/// Since <see cref="ReplacementFrame"/> objects are designed to be placed directly atop existing, 
/// functional structures, they would otherwise trigger this obstruction logic and prevent 
/// the player from queuing replacements.
/// </para>
/// <para>
/// This patch acts as a <see cref="HarmonyLib.HarmonyPatchType.Postfix"/> on the blocking logic, 
/// forcing the result to <c>false</c> whenever the object being constructed is a 
/// <see cref="ReplacementFrame"/>, effectively "ghosting" the frame so it does not collide with 
/// its own placement validation.
/// </para>
/// </remarks>
[HarmonyPatch(typeof(RimWorld.GenConstruct), "BlocksConstruction")]
class Patch_GenConstruct_BlocksConstruction
{
    //public static bool BlocksConstruction(Thing constructible, Thing t)
    public static void Postfix(Thing constructible, Thing t, ref bool __result)
    {
        if (!__result || constructible is Blueprint_Install)
            return;

        if (constructible is ReplacementFrame)
        {
            __result = false;
            return;
        }

        if (constructible is Blueprint_Build blueprint)
        {
            BuildableDef entDef = blueprint.def.entityDefToBuild;
            if (entDef is not null && RimWorld.GenConstruct.CanReplace(entDef, t.def, blueprint.stuffToUse, t.Stuff))
            {
                __result = false;
            }
        }
    }
}