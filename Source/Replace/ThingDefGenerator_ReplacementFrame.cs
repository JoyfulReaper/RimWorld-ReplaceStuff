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
using UnityEngine;
using Verse;

namespace Replace_Stuff.Replace;

/// <summary>
/// Handles the dynamic generation of "Replacement Frames" that act as placeholders during building swaps.
/// </summary>
//[HarmonyPatch(typeof(DefGenerator), "GenerateImpliedDefs_PreResolve")]
[StaticConstructorOnStartup]
public static class ThingDefGenerator_ReplacementFrame
{
    /// <summary>Delegate for accessing the private ShortHashGiver.GiveShortHash method.</summary>
    public delegate void GiveShortHashDelegate(Def d, Type t, HashSet<ushort> h);

    /// <summary>Bridge to the game's internal method for assigning short hashes to dynamic Defs.</summary>
    public static readonly GiveShortHashDelegate GiveShortHash =
        AccessTools.MethodDelegate<GiveShortHashDelegate>(AccessTools.Method(typeof(ShortHashGiver), "GiveShortHash"));

    /// <summary>
    /// Registers newly generated replacement frame Defs into the game's DefDatabase and tracking systems.
    /// </summary>
    /// <param name="addShortHash">When set to <c>true</c>, registers unique identity fingerprint keys inside short reference maps.</param>
    public static void AddReplacementFrames(bool addShortHash = true)
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

    public static Dictionary<ThingDef, ThingDef> BuildingToFrameMap;

    /// <summary>
    /// Retrieves the replacement frame Def associated with the given building Def.
    /// </summary>
    public static ThingDef ReplacementFrameDefFor(ThingDef buildingDef)
    {
        if (BuildingToFrameMap != null && BuildingToFrameMap.TryGetValue(buildingDef, out ThingDef replaceFrame))
        {
            return replaceFrame;
        }
        return null;
    }

    /// <summary>Checks if the provided building Def has a registered replacement frame.</summary>
    public static bool HasReplacementFrame(this ThingDef def)
    {
        if (def is null || BuildingToFrameMap is null)
            return false;

        return BuildingToFrameMap.ContainsKey(def);
    }

    /// <summary>
    /// Scans for all artificial buildings that support "MadeFromStuff" and generates corresponding replacement frame Defs.
    /// </summary>
    public static IEnumerable<ThingDef> GetImpliedReplacementFrameDefs()
    {
        BuildingToFrameMap = [];

        var allDefs = DefDatabase<ThingDef>.AllDefsListForReading;

        for (int i = 0; i < allDefs.Count; i++)
        {
            ThingDef candidateDef = allDefs[i];

            if (candidateDef.designationCategory != null && candidateDef.IsBuildingArtificial && !candidateDef.IsFrame && candidateDef.MadeFromStuff)
            {
                ThingDef replaceFrameDef = CreateReplacementFrameDef(candidateDef);
                BuildingToFrameMap[candidateDef] = replaceFrameDef;
                yield return replaceFrameDef;
            }
        }
    }

    /// <summary>
    /// Maps custom building profiles to determine fallback tint colors inside display modules.
    /// </summary>
    private static Color DrawColor(ThingDef frameDef)
    {
        if (frameDef.MadeFromStuff)
            return Color.white;

        var costList = frameDef.entityDefToBuild.CostList;
        if (costList is null)
            return new Color(0.6f, 0.6f, 0.6f);

        for (int i = 0; i < costList.Count; i++)
        {
            var costItem = costList[i];
            var costDef = costItem.thingDef;
            if (costDef.IsStuff && costDef.stuffProps.color != Color.white)
                return frameDef.GetColorForStuff(costDef);
        }

        return new Color(0.6f, 0.6f, 0.6f);
    }

    /// <summary>
    /// Creates and configures a new <see cref="ThingDef"/> to serve as a replacement frame for the specified building.
    /// </summary>
    /// <param name="def">The source building Def to create a frame for.</param>
    public static ThingDef CreateReplacementFrameDef(ThingDef def)
    {
        var thingDef = CreateBaseReplacementFrameDef();
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
    /// Returns a base <see cref="ThingDef"/> initialized with standard structural defaults for a frame.
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

    /// <summary>Checks if a Def is a replacement frame.</summary>
    public static bool IsReplacementFrame(this ThingDef def) =>
        def.thingClass == typeof(ReplacementFrame);
}