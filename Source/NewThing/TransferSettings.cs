// We do this in the Replacement Pipeline now

///*
// * REPLACE STUFF: Performance Edition
// * 
// * 
// * Part of this code is based on Replace Stuff
// * Copyright (c) 2025 Alex Tearse-Doyle
// * Licensed under the MIT License.
// *
// * Modified by Kyle Givler
// * Copyright (c) 2026 Kyle Givler
// * Licensed under the MIT License.
// */

//using HarmonyLib;
//using System;
//using Verse;

//namespace Replace_Stuff.NewThing
//{
//    [HarmonyPatch(typeof(GenSpawn), "Spawn", new Type[]
//    {   typeof(Thing), typeof(IntVec3), typeof(Map), typeof(Rot4), typeof(WipeMode), typeof(bool), typeof(bool)})]
//    class TransferSettings
//    {
//        // public static Thing Spawn(Thing newThing, IntVec3 loc, Map map, Rot4 rot, WipeMode wipeMode = WipeMode.Vanish, bool respawningAfterLoad = false, bool forbidLeavings = false)
//        public static void Prefix(Thing newThing, IntVec3 loc, Map map, Rot4 rot, bool respawningAfterLoad, ref Thing __state)
//        {
//            __state = null;
//            if (respawningAfterLoad)
//                return;

//            if (newThing.def.category != ThingCategory.Building)
//                return;

//            if (newThing.def.IsNewThingReplacement(loc, rot, map, out Thing oldThing))
//            {
//                newThing.PreFinalizeNewThingReplace(oldThing);
//                __state = oldThing;
//            }
//        }

//        public static void Postfix(Thing __result, Thing __state)
//        {
//            if (__result == null || __state == null) return;

//            __result.FinalizeNewThingReplace(__state);
//        }
//    }
//}