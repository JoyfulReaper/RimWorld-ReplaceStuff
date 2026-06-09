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
using Replace_Stuff.DestroyedRestore;
using Replace_Stuff.Utilities;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Replace_Stuff.Replace;

static class GenReplace
{
    public static ReplacementFrame PlaceReplaceFrame(Thing oldThing, ThingDef stuff)
    {
        ThingDef replaceFrameDef =
            ThingDefGenerator_ReplaceFrame.ReplaceFrameDefFor(oldThing.def);

        if (replaceFrameDef == null)
        {
            RSLog.Debug($"No replace frame def found for {oldThing.def.defName}");
            return null;
        }

        var replaceFrame = (ReplacementFrame)ThingMaker.MakeThing(replaceFrameDef, stuff);
        replaceFrame.replaceData = BuildingStateTransfer.Capture(oldThing, new HashSet<int>());

        replaceFrame.SetFactionDirect(Faction.OfPlayer);
        oldThing.SetFactionDirect(Faction.OfPlayer);

        replaceFrame.targetThing = oldThing;
        replaceFrame.targetStuff = oldThing.Stuff;


        RSLog.Debug(
            $"GenReplace.PlaceReplaceFrame(): BEFORE SPAWN: OldRot={(oldThing is null ? "null" : oldThing.Rotation.ToString())} "
            + $"NewRot=New thing not spawned yet");

        GenSpawn.Spawn(replaceFrame, oldThing.Position, oldThing.Map, oldThing.Rotation);
        // TODO Apply here


        RSLog.Debug(
            $"GenReplace.PlaceReplaceFrame(): AFTER SPAWN: OldRot={(oldThing is null ? "null" : oldThing.Rotation.ToString())} "
            + $"NewRot={(replaceFrame is null ? "null" : replaceFrame.Rotation.ToString())} {replaceFrame.Rotation}");

        return replaceFrame;
    }

    private static ThingPlaceMode GetRestoreMode(
        Building_Storage storage,
        Thing thing)
    {
        if (storage.GetSlotGroup().HeldThings.Count() <
            storage.GetSlotGroup().CellsList.Sum(
                c => c.GetMaxItemsAllowedInCell(storage.Map)))
        {
            return ThingPlaceMode.Direct;
        }

        return ThingPlaceMode.Near;
    }

    /// <summary>
    /// Prepares a replacement building by transferring essential runtime
    /// state from the original structure, generating deconstruction
    /// resources, assigning faction and quality data, and retiring the
    /// original building.
    /// </summary>
    /// <param name="oldThing">
    /// The building being replaced.
    /// </param>
    /// <param name="newThing">
    /// The replacement building instance.
    /// </param>
    /// <param name="worker">
    /// The pawn performing the replacement, if applicable.
    /// </param>
    /// <param name="faction">
    /// Optional faction override for the replacement building.
    /// </param>
    public static Thing ApplyReplacementState(Thing oldThing, Thing newThing, ReplaceData replaceData, Pawn worker = null, Faction faction = null)
    {
        RSLog.Debug($"ApplyReplacementState() START: Old Rot={oldThing.Rotation} New Rot={newThing.Rotation}");
        ReplacementFrame.PrepareReplacementBuilding(oldThing, newThing, worker, faction);

        //GenSpawn.Spawn(newThing, oldThing.Position, oldThing.Map, oldThing.Rotation, WipeMode.Vanish);
        //BuildingStateTransfer.Apply(replaceData, newThing);
        RSLog.Debug($"ApplyReplacementState() END: Old Rot={oldThing.Rotation} New Rot={newThing.Rotation}");
        return newThing;
    }

    private static void DebugStorage(Building_Storage storage, string stage)
    {
        if (storage is not null)
        {
            string posStr = storage.Spawned ? storage.Position.ToString() : "UNSPAWNED";
            bool hasMap = storage.Map != null;

            RSLog.Debug("DebugStorage(): " +
                $"{stage}: " +
                $"Quantity={storage.GetSlotGroup().HeldThings.Count()} " +
                $"Spawned={storage.Spawned} " +
                $"Map={hasMap} " +
                $"Pos={posStr}");

            if (storage?.GetSlotGroup() == null)
            {
                RSLog.Debug("<no slot group>");
                return;

            }
            else
            {
                RSLog.Debug($"DebugStorage(): {stage}: Held things:");
                RSLog.Debug(String.Join(", ", storage.GetSlotGroup().HeldThings
                    .Select(t => $"{t.def.defName} x{t.stackCount} ({t.thingIDNumber})")));
            }
        }
        else
            RSLog.Debug($"{stage}: Storage is null");
    }

    public static List<Thing> ExtractStoredThings(Building_Storage storage)
    {
        DebugStorage(storage, "Before Extract");
        List<Thing> result = new();

        foreach (Thing thing in storage.GetSlotGroup().HeldThings.ToList())
        {
            result.Add(thing);
            thing.DeSpawn();
        }

        return result;
    }

    public static void RestoreStoredThings(Building_Storage storage, List<Thing> things)
    {
        foreach (Thing thing in things)
        {
            bool success = GenPlace.TryPlaceThing(
                thing,
                storage.Position,
                storage.Map,
                ThingPlaceMode.Direct); // Changed from Near for testing TODO

            if (!success)
            {
                success = GenPlace.TryPlaceThing(thing, storage.Position, storage.Map, ThingPlaceMode.Near);
                RSLog.Debug(
                    $"Overflow drop {thing.def.defName} Success={success}");
            }

            RSLog.Debug(
                $"Restore {thing.def.defName} "
                + $"ID={thing.thingIDNumber} "
                + $"Success={success}");

            RSLog.Debug(
                $"{thing.thingIDNumber} "
                + $"Spawned={thing.Spawned} "
                + $"Pos={(thing.Spawned ? thing.Position.ToString() : "UNSPAWNED")}");

            DebugStorage(storage, $"After placing {thing.def.defName}");
        }

        DebugStorage(storage, "After Restore");
    }
}

//[HarmonyPatch(typeof(DefGenerator), "GenerateImpliedDefs_PreResolve")]
[StaticConstructorOnStartup]
public static class ThingDefGenerator_ReplaceFrame
{
    /*
    public static void Postfix()
    {
        // would be nice but mods don't add defs early enough.
        // I mean they assume blueprints/frames don't need to be implied from them
        // But replace frames sure can!
        AddReplaceFrames(false);
    }
    */
    public delegate void GiveShortHashDel(Def d, Type t, HashSet<ushort> h);
    public static GiveShortHashDel GiveShortHash = AccessTools.MethodDelegate<GiveShortHashDel>(AccessTools.Method(typeof(ShortHashGiver), "GiveShortHash"));
    public static void AddReplaceFrames(bool addShortHash = true)
    {
        Type type = typeof(ThingDef);

        // Slow reflection since this is only once:
        HashSet<ushort> takenHashes = ((Dictionary<Type, HashSet<ushort>>)AccessTools.Field(typeof(ShortHashGiver), "takenHashesPerDeftype").GetValue(null))[type];

        foreach (ThingDef current in ThingDefGenerator_ReplaceFrame.ImpliedReplaceFrameDefs())
        {
            if (addShortHash)  //Wouldn't need this if other mods added defs earlier. Oh well.
                GiveShortHash(current, type, takenHashes);
            current.PostLoad();
            DefDatabase<ThingDef>.Add(current);
        }
    }

    public static Dictionary<ThingDef, ThingDef> replaceFrameDefs;
    public static ThingDef ReplaceFrameDefFor(ThingDef def)
    {
        if (replaceFrameDefs == null)
        {
            return null;
        }

        if (replaceFrameDefs.TryGetValue(def, out ThingDef replaceFrame))
            return replaceFrame;

        return null;
    }

    public static bool HasReplaceFrame(this ThingDef def)
    {
        if (def == null)
            return false;

        if (replaceFrameDefs == null)
        {
            return false;
        }

        return replaceFrameDefs.ContainsKey(def);
    }

    public static IEnumerable<ThingDef> ImpliedReplaceFrameDefs()
    {
        replaceFrameDefs = new Dictionary<ThingDef, ThingDef>();
        foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs.ToList<ThingDef>())
        {
            if (def.designationCategory != null && def.IsBuildingArtificial && !def.IsFrame && def.MadeFromStuff)
            {
                ThingDef replaceFrameDef = NewReplaceFrameDef_Thing(def);
                replaceFrameDefs[def] = replaceFrameDef;
                yield return replaceFrameDef;
            }
        }
    }

    private static Color DrawColor(ThingDef def)
    {
        if (def.MadeFromStuff)
            return Color.white;

        var costList = def.entityDefToBuild.CostList;
        if (costList == null) return new Color(0.6f, 0.6f, 0.6f);

        foreach (var costItem in costList)
        {
            var costDef = costItem.thingDef;
            if (costDef.IsStuff && costDef.stuffProps.color != Color.white)
                return def.GetColorForStuff(costDef);
        }

        return new Color(0.6f, 0.6f, 0.6f);
    }

    public static ThingDef NewReplaceFrameDef_Thing(ThingDef def)
    {
        ThingDef thingDef = BaseReplaceFrameDef();
        thingDef.defName = def.defName + "_ReplaceStuff";
        thingDef.label = def.label + "TD.ReplacingTag".Translate();//Not entirely sure if this is needed since ReplaceFrame.Label doesn't use it, but, this is vanilla Frame code.
        thingDef.size = def.size;
        thingDef.SetStatBaseValue(StatDefOf.MaxHitPoints, (float)def.BaseMaxHitPoints * 0.25f);
        thingDef.SetStatBaseValue(StatDefOf.Beauty, -8f);
        thingDef.fillPercent = 0.2f;
        thingDef.pathCost = 10;
        thingDef.description = def.description;
        thingDef.passability = def.passability;
        thingDef.selectable = def.selectable;
        thingDef.constructEffect = def.constructEffect;
        thingDef.building.isEdifice = false;
        thingDef.constructionSkillPrerequisite = def.constructionSkillPrerequisite;
        thingDef.clearBuildingArea = false;
        thingDef.drawPlaceWorkersWhileSelected = def.drawPlaceWorkersWhileSelected;
        thingDef.stuffCategories = def.stuffCategories;

        if (def.size.x <= 4 && def.size.z <= 4)
        {
            thingDef.drawerType = DrawerType.RealtimeOnly;
            thingDef.graphicData = new GraphicData();
            thingDef.graphicData.graphicClass = typeof(Graphic_Single);
            thingDef.graphicData.texPath = $"ReplaceStuffFrame/{def.size.x}x{def.size.z}";
            thingDef.graphicData.drawSize = new Vector2(def.size.x, def.size.z);
            thingDef.graphicData.drawOffset = def.graphicData.drawOffset;
            thingDef.graphicData.shaderType = ShaderTypeDefOf.Transparent;
            thingDef.graphicData.color = DrawColor(thingDef);
        }

        //Support QualityBuilder
        if (QualityBuilderCompat.z != null)
            if (def.HasComp(typeof(CompQuality)) && def.building != null)
                thingDef.comps.Add((CompProperties)Activator.CreateInstance(QualityBuilderCompat.z));

        thingDef.entityDefToBuild = def;
        //def.replaceFrameDef = thingDef;	//Dictionary instead

        thingDef.modContentPack = LoadedModManager.GetMod<ReplaceStuffPerformance>().Content;
        return thingDef;
    }

    static ThingDef BaseReplaceFrameDef()
    {
        return new ThingDef
        {
            isFrameInt = true,
            category = ThingCategory.Building,
            label = "Unspecified stuff replacement frame",
            thingClass = typeof(ReplacementFrame),
            altitudeLayer = AltitudeLayer.BuildingOnTop,
            useHitPoints = true,
            selectable = true,
            building = new BuildingProperties(),
            comps =
                {
                    new CompProperties_Forbiddable()
                },
            scatterableOnMapGen = false,
            leaveResourcesWhenKilled = true
        };
    }

    public static bool IsReplaceFrame(this ThingDef def) =>
        def.thingClass == typeof(ReplacementFrame);

}