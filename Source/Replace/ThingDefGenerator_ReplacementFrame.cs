/*
 * REPLACE STUFF: Performance  Edition
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
using Replace_Stuff.Compatibility;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Replace_Stuff.Replace;

/// <summary>
/// Dynamic framework engine mapping custom structural variants onto item blueprints to provide material swaps.
/// </summary>
//[HarmonyPatch(typeof(DefGenerator), "GenerateImpliedDefs_PreResolve")]
[StaticConstructorOnStartup]
public static class ThingDefGenerator_ReplacementFrame
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

    /// <summary>Defines parameters used to register dynamic entries inside the engine unique hash tracker system.</summary>
    public delegate void GiveShortHashDel(Def d, Type t, HashSet<ushort> h);

    /// <summary>The engine runtime method bridge execution address used to record identity parameters inside the core database maps.</summary>
    public static GiveShortHashDel GiveShortHash =
        AccessTools.MethodDelegate<GiveShortHashDel>(AccessTools.Method(typeof(ShortHashGiver), "GiveShortHash"));

    /// <summary>
    /// Iterates through target elements and loads newly computed implied blueprints directly into core tracking tables.
    /// </summary>
    /// <param name="addShortHash">When set to <c>true</c>, registers unique identity fingerprint keys inside short reference maps.</param>
    public static void AddReplaceFrames(bool addShortHash = true)
    {
        Type type = typeof(ThingDef);

        // Slow reflection since this is only once:
        var takenHashes = ((Dictionary<Type, HashSet<ushort>>)AccessTools.Field(typeof(ShortHashGiver), "takenHashesPerDeftype").GetValue(null))[type];

        foreach (ThingDef current in GetImpliedReplacementFrameDefs())
        {
            if (addShortHash)  //Wouldn't need this if other mods added defs earlier. Oh well.
                GiveShortHash(current, type, takenHashes);
            current.PostLoad();
            DefDatabase<ThingDef>.Add(current);
        }
    }

    public static Dictionary<ThingDef, ThingDef> ReplacementFrameDefs;

    /// <summary>
    /// Locates the dynamic tracking frame matching a specific world building structure specification.
    /// </summary>
    public static ThingDef ReplaceFrameDefFor(ThingDef def)
    {
        if (ReplacementFrameDefs is null)
        {
            return null;
        }

        if (ReplacementFrameDefs.TryGetValue(def, out ThingDef replaceFrame))
            return replaceFrame;

        return null;
    }

    public static bool HasReplacementFrrame(this ThingDef def)
    {
        if (def is null || ReplacementFrameDefs is null)
            return false;

        return ReplacementFrameDefs.ContainsKey(def);
    }

    /// <summary>
    /// Collects and processes artificial world structures valid for integration into substitution systems.
    /// </summary>
    public static IEnumerable<ThingDef> GetImpliedReplacementFrameDefs()
    {
        ReplacementFrameDefs = new Dictionary<ThingDef, ThingDef>();
        foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs.ToList())
        {
            if (def.designationCategory != null && def.IsBuildingArtificial && !def.IsFrame && def.MadeFromStuff)
            {
                ThingDef replaceFrameDef = CreateReplacementFrameDef(def);
                ReplacementFrameDefs[def] = replaceFrameDef;
                yield return replaceFrameDef;
            }
        }
    }

    /// <summary>
    /// Maps custom building profiles to determine fallback tint colors inside display modules.
    /// </summary>
    private static Color DrawColor(ThingDef def)
    {
        if (def.MadeFromStuff)
            return Color.white;

        var costList = def.entityDefToBuild.CostList;
        if (costList is null)
            return new Color(0.6f, 0.6f, 0.6f);

        foreach (var costItem in costList)
        {
            var costDef = costItem.thingDef;
            if (costDef.IsStuff && costDef.stuffProps.color != Color.white)
                return def.GetColorForStuff(costDef);
        }

        return new Color(0.6f, 0.6f, 0.6f);
    }

    /// <summary>
    /// Generates and assigns a standalone runtime <see cref="ThingDef"/> built to track replacements.
    /// </summary>
    /// <param name="def">The source building block schema used to shape parameters inside the newly formed tracker item layout.</param>
    public static ThingDef CreateReplacementFrameDef(ThingDef def)
    {
        ThingDef thingDef = CreateBaseReplacementFrameDef();
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
        if (QualityBuilderCompat.qualityBuilderPropsType is not null)
            if (def.HasComp(typeof(CompQuality)) && def.building != null)
                thingDef.comps.Add((CompProperties)Activator.CreateInstance(QualityBuilderCompat.qualityBuilderPropsType));

        thingDef.entityDefToBuild = def;
        thingDef.modContentPack = LoadedModManager.GetMod<ReplaceStuffPerformance>().Content;
        return thingDef;
    }

    /// <summary>
    /// Produces a baseline blueprint property layout containing structural defaults required by the game engine.
    /// </summary>
    static ThingDef CreateBaseReplacementFrameDef()
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

    /// <summary>
    /// Evaluates if an arbitrary object asset record tracks replacement frameworks.
    /// </summary>
    public static bool IsReplaceFrame(this ThingDef def) =>
        def.thingClass == typeof(ReplacementFrame);
}
