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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse;
using Verse.AI;

namespace Replace_Stuff.OverMineable.Patches;
//Include blueprints and frames in IsCornerTouchAllowed
//Make sure mineable drop is in miner's region so it's not blocked off 
[HarmonyPatch(typeof(Mineable), "TrySpawnYield", [typeof(Map), typeof(bool), typeof(Pawn)])]
public static class DropOnPawn
{
    //private void TrySpawnYield(Map map, float yieldChance, bool moteOnWaste, Pawn pawn)
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        //There's two overloads for TryPlaceThing, one just literally calls the other and tosses out the (out Thing)..
        //I don't want to write out all the generic params here...
        //so just find the one with less parameters.
        MethodInfo TryPlaceThingInfo = typeof(GenPlace).GetMethods(AccessTools.all)
            .Where(mi => mi.Name == "TryPlaceThing")
            .MinBy(mi => mi.GetParameters().Length);

        //Same thing put takes a pawn to validate room.
        MethodInfo TryPlaceThingInSameRoomInfo = AccessTools.Method(typeof(DropOnPawn), nameof(TryPlaceThingInSameRoom));

        foreach (var inst in instructions)
        {
            if (inst.Calls(TryPlaceThingInfo))
            {
                yield return new CodeInstruction(OpCodes.Ldarg_S, 3);//Pawn pawn
                yield return new CodeInstruction(OpCodes.Call, TryPlaceThingInSameRoomInfo);
            }
            else yield return inst;
        }
    }


public static bool TryPlaceThingInSameRoom(Thing thing, IntVec3 center, Map map, ThingPlaceMode mode, Action<Thing, int> placedAction = null, Predicate<IntVec3> extraValidator = null, Rot4? rot = null, int squareRadius = 1, Pawn miner = null)
{
    if (miner != null)
    {
        // Cache these lookups out here so they only run once per item drop
        var minerRoom = miner.GetRoom();
        var minerMap = miner.Map;

        if (minerRoom != null && minerMap != null)
        {
            Predicate<IntVec3> roomValidator = (IntVec3 pos) => 
                pos.GetRoom(minerMap) == minerRoom;
            extraValidator = extraValidator == null ? roomValidator : pos => extraValidator(pos) && roomValidator(pos);
        }
    }

    return GenPlace.TryPlaceThing(thing, center, map, mode, placedAction, extraValidator, rot);
}

    //public static bool TryPlaceThing(Thing thing, IntVec3 center, Map map, ThingPlaceMode mode, Action<Thing, int> placedAction = null, Predicate<IntVec3> extraValidator = null, Rot4? rot = null, int squareRadius = 1)
    // public static bool TryPlaceThingInSameRoom(Thing thing, IntVec3 center, Map map, ThingPlaceMode mode, Action<Thing, int> placedAction = null, Predicate<IntVec3> extraValidator = null, Rot4? rot = null, int squareRadius = 1, Pawn miner = null)
    // {
    //     //For godmode mining there is no pawn
    //     if (miner != null)
    //     {
    //         Predicate<IntVec3> newValidator = (IntVec3 pos) => pos.GetRoom(miner.Map) == miner.GetRoom();
    //         //(Good luck setting this up in ILCode so I'll do it here)
    //         extraValidator = extraValidator == null ? newValidator : pos => extraValidator(pos) && newValidator(pos);
    //     }

    //     return GenPlace.TryPlaceThing(thing, center, map, mode, placedAction, extraValidator, rot);
    // }
}
