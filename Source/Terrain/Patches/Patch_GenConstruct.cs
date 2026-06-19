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
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse;
using Verse.AI;

namespace Replace_Stuff.Terrain.Patches;

//Smooth walls before replacing with other wall, don't mine them away and rebuild.
[HarmonyPatch(typeof(GenConstruct), "HandleBlockingThingJob")]
static class Patch_GenConstruct_HandleBlockingThingJob
{
    //public static Job HandleBlockingThingJob(Thing constructible, Pawn worker, bool forced = false)
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator iLGenerator)
    {
        FieldInfo mineableInfo = AccessTools.Field(typeof(ThingDef), "mineable");
        MethodInfo ToBeSmoothedInfo = AccessTools.Method(typeof(Patch_GenConstruct_HandleBlockingThingJob), nameof(Patch_GenConstruct_HandleBlockingThingJob.ToBeSmoothed),
            new Type[] { typeof(Thing), typeof(Thing) });
        MethodInfo SmoothItJobInfo = AccessTools.Method(typeof(Patch_GenConstruct_HandleBlockingThingJob), nameof(Patch_GenConstruct_HandleBlockingThingJob.SmoothItJob));

        List<CodeInstruction> list = instructions.ToList();
        yield return list[0];

        for (int i = 1; i < list.Count; i++)
        {
            yield return list[i];
            if (list[i - 1].LoadsField(mineableInfo))
            {
                Label otherwise = iLGenerator.DefineLabel();
                list[i + 1].labels.Add(otherwise);

                //Explicitly preserve the opcode and operand to safely load 'thing'
                yield return new CodeInstruction(list[i - 3].opcode, list[i - 3].operand);
                yield return new CodeInstruction(OpCodes.Ldarg_0); // Thing constructible
                yield return new CodeInstruction(OpCodes.Call, ToBeSmoothedInfo);
                yield return new CodeInstruction(OpCodes.Brfalse, otherwise);

                yield return new CodeInstruction(OpCodes.Ldarg_1); // worker
                                                                   // Explicitly preserve opcode and operand here too
                yield return new CodeInstruction(list[i - 3].opcode, list[i - 3].operand);
                yield return new CodeInstruction(OpCodes.Ldarg_2); // forced
                yield return new CodeInstruction(OpCodes.Call, SmoothItJobInfo);
                yield return new CodeInstruction(OpCodes.Ret);
            }
        }
    }

    public static bool ToBeSmoothed(Thing thing, Thing constructible) =>
        ToBeSmoothed(thing, constructible.def);

    public static bool ToBeSmoothed(Thing thing, ThingDef constructibleDef)
    {
        ThingDef smoothedThing = thing.def.building?.smoothedThing;
        return smoothedThing != null &&
            !GenSpawn.SpawningWipes(GenConstruct.BuiltDefOf(constructibleDef), smoothedThing) &&
            thing.Map.edificeGrid[thing.Position] == thing &&
            thing.Map.designationManager.DesignationAt(thing.Position, DesignationDefOf.SmoothWall) != null;
    }

    public static Job SmoothItJob(Pawn worker, Thing thing, bool forced)
    {
        if (worker.story != null && worker.WorkTypeIsDisabled(WorkTypeDefOf.Construction))
        {
            JobFailReason.Is("TD.IncapableOfSmoothing".Translate());
            return null;
        }
        if (worker.CanReserveAndReach(thing, PathEndMode.Touch, worker.NormalMaxDanger(), 1, -1, null, forced) &&
            worker.CanReserve(thing.Position, 1, -1, null, forced))
        {
            return new Job(JobDefOf.SmoothWall, thing)
            {
                ignoreDesignations = true
            };
        }
        return null;
    }
}

//It did create a problem! Frames counting as edifices meant they blocked blueprints
//So frames are edifices for blueprint consideration... that shouldn't create a problem, right?
[HarmonyPatch(typeof(GenConstruct), "CanPlaceBlueprintOver")]
public static class FramesAreEdificesInSomeCases
{
    //public static bool CanPlaceBlueprintOver(BuildableDef newDef, ThingDef oldDef)
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return Transpilers.MethodReplacer(instructions,
            AccessTools.Method(typeof(EdificeUtility), "IsEdifice"),
            AccessTools.Method(typeof(FramesAreEdificesInSomeCases), "IsEdificeOrFrame"));
    }

    public static bool IsEdificeOrFrame(BuildableDef def)
    {
        return def.IsEdifice() || (def is ThingDef thingDef && thingDef.IsFrame);
    }
}

[HarmonyPatch(typeof(GenConstruct))]//, "CanPlaceBlueprintOver.IsEdificeOverNonEdifice")]
public static class FramesAreEdificesInSomeCasesAndAlsoInTheCompilerGeneratedMethod
{
    public static MethodInfo TargetMethod() =>
        // "IsEdificeOverNonEdifice" Isn't compiled away? Okay I'll use that
        AccessTools.FirstMethod(typeof(GenConstruct), method => method.Name.Contains("IsEdificeOverNonEdifice"));

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) =>
        FramesAreEdificesInSomeCases.Transpiler(instructions);
}


// In CanConstruct, skip FirstBlockingThing if it's just a haul job
[HarmonyPatch(typeof(GenConstruct), nameof(GenConstruct.CanConstruct), [typeof(Thing), typeof(Pawn), typeof(bool), typeof(bool), typeof(JobDef)])]
public static class Patch_GenConstruct_CanConstruct
{
    //public static bool CanConstruct(Thing t, Pawn p, bool checkSkills = true, bool forced = false, JobDef jobForReservation = null)
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        //Replace
        MethodInfo FirstBlockingThingInfo = AccessTools.Method(typeof(GenConstruct), nameof(GenConstruct.FirstBlockingThing));
        //With
        MethodInfo FirstBlockingThingNotHaulInfo = AccessTools.Method(typeof(Patch_GenConstruct_CanConstruct), nameof(FirstBlockingThingNotHaul));

        foreach (var inst in instructions)
        {
            if (inst.Calls(FirstBlockingThingInfo))
            {
                //yield return new CodeInstruction(OpCodes.Ldarg_S, 4);//JobDef jobForReservation
                yield return CodeInstruction.LoadArgument(4);
                yield return new CodeInstruction(OpCodes.Call, FirstBlockingThingNotHaulInfo);//JobDef jobForReservation
            }
            else
                //JobDef jobForReservation
                yield return inst;
        }
    }

    //public static Thing FirstBlockingThing(Thing constructible, Pawn pawnToIgnore)
    public static Thing FirstBlockingThingNotHaul(Thing constructible, Pawn pawnToIgnore, JobDef jobForReservation)
    {
        if (jobForReservation == JobDefOf.HaulToContainer)
            return null;

        return GenConstruct.FirstBlockingThing(constructible, pawnToIgnore);
    }
}


//TODO: This should technically go inside Designator_Build.DesignateSingleCell, but this is easier.
[HarmonyPatch(typeof(GenConstruct), nameof(GenConstruct.PlaceBlueprintForBuild))]
class Patch_GenConstruct_PlaceBlueprintForBuild
{
    //public static Blueprint_Build PlaceBlueprintForBuild(BuildableDef sourceDef, IntVec3 center, Map map, Rot4 rotation, Faction faction, ThingDef stuff)
    public static void Prefix(BuildableDef sourceDef, IntVec3 center, Map map, Rot4 rotation, Faction faction)
    {
        if (faction != Faction.OfPlayer)
            return;

        if (sourceDef is not ThingDef thingDef)
            return;

        foreach (IntVec3 cell in GenAdj.CellsOccupiedBy(center, rotation, sourceDef.Size))
        {
            if (map.designationManager.DesignationAt(cell, DesignationDefOf.Mine) != null)
                continue;

            var thingsAtCell = map.thingGrid.ThingsAt(cell);
            foreach (Thing mineThing in thingsAtCell)
            {
                if (!mineThing.def.IsBlockingRock(sourceDef))
                    continue;
                if (Patch_GenConstruct_HandleBlockingThingJob.ToBeSmoothed(mineThing, thingDef))
                    continue;

                map.designationManager.AddDesignation(new Designation(mineThing, DesignationDefOf.Mine));

                if (mineThing.def.building?.mineableYieldWasteable ?? false)
                    TutorUtility.DoModalDialogIfNotKnown(ConceptDefOf.BuildersTryMine);
            }
        }
    }
}

[HarmonyPatch(typeof(GenConstruct), "CanPlaceBlueprintOver")]
class Patch_GenConstruct_CanPlaceBlueprintOver
{
    //public static bool CanPlaceBlueprintOver(BuildableDef newDef, ThingDef oldDef, ThingDef newStuff = null, ThingDef oldStuff = null)
    public static void Postfix(BuildableDef newDef, ThingDef oldDef, ref bool __result)
    {
        if (!BlueprintUtility.IsEnabledBlueprintOverRock)
            return;

        if (!DesignatorContext.IsInBuildDesignation) return;

        if (newDef.GetStatValueAbstract(StatDefOf.WorkToBuild) > 0f)
            __result |= oldDef.IsMineableRock();
    }
}

// At somepoint CanPlaceBlueprintAt turned into a hollow wrapper that just Calls
// CanPlaceBlueprintAt_NewTemp. Updated this to transpile that method instead.
[HarmonyPatch(typeof(GenConstruct), nameof(GenConstruct.CanPlaceBlueprintAt_NewTemp))]
public static class Patch_GenConstruct
{
    //public static AcceptanceReport CanPlaceBlueprintAt(BuildableDef entDef, IntVec3 center, Rot4 rot, Map map, bool godMode = false, Thing thingToIgnore = null)
    // ohheck this method has got a lot more
    //public static AcceptanceReport CanPlaceBlueprintAt(BuildableDef entDef, IntVec3 center, Rot4 rot, Map map, bool godMode = false, Thing thingToIgnore = null, Thing thing = null, ThingDef stuffDef = null, bool ignoreEdgeArea = false, bool ignoreInteractionSpots = false, bool ignoreClearableFreeBuildings = false)
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var FoggedInfo = AccessTools.Method(typeof(GridsUtility), "Fogged", new Type[] { typeof(IntVec3), typeof(Map) });
        var BlueprintAcceptedInfo = AccessTools.Method(typeof(Patch_GenConstruct), nameof(BlueprintOverFogAcceptance));

        bool foundFogged = false;
        foreach (CodeInstruction i in instructions)
        {
            yield return i;
            if (foundFogged)  //skip the brfalse after Fogged
            {
                //This should probably check for DesignatorContext.designating but then more of this code would need to change
                yield return new CodeInstruction(OpCodes.Ldarg_3);//map
                yield return new CodeInstruction(OpCodes.Ldarg_1);//center
                yield return new CodeInstruction(OpCodes.Ldarg_0);//entDef
                yield return new CodeInstruction(OpCodes.Call, BlueprintAcceptedInfo);
                yield return new CodeInstruction(OpCodes.Ret);
                foundFogged = false;
            }
            if (i.Calls(FoggedInfo))
                foundFogged = true;
        }
    }

    //if found fogged:
    public static AcceptanceReport BlueprintOverFogAcceptance(Map map, IntVec3 center, ThingDef entDef)
    {
        if (!BlueprintUtility.IsEnabledBlueprintOverRock)
            return new AcceptanceReport("CannotPlaceInUndiscovered".Translate());
        if (center.GetThingList(map).Any(t => t is Blueprint && t.def.entityDefToBuild == entDef))
            return new AcceptanceReport("IdenticalBlueprintExists".Translate());
        if (entDef.GetStatValueAbstract(StatDefOf.WorkToBuild) == 0f)
            return new AcceptanceReport("CannotPlaceInUndiscovered".Translate());
        return true;
    }
}