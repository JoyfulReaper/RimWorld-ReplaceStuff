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

using Replace_Stuff.Compatibility;
using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Replace_Stuff.Replace;

/// <summary>
/// Generates replacement frame ThingDefs and maintains
/// the mapping between buildings and their replacement frames.
/// </summary>
internal static class ReplacementFrameDefGenerator
{
    private static readonly Dictionary<ThingDef, ThingDef> _buildingToFrameMap = [];

    public static IReadOnlyDictionary<ThingDef, ThingDef> BuildingToFrameMap
     => _buildingToFrameMap;

    /// <summary>
    /// Retrieves the replacement frame Def associated with the given building Def.
    /// </summary>
    public static ThingDef GetReplacementFrameDef(ThingDef buildingDef)
    {
        _buildingToFrameMap.TryGetValue(buildingDef, out var result);
        return result;
    }

    /// <summary>
    /// Scans for all artificial buildings that support "MadeFromStuff" and generates corresponding replacement frame Defs.
    /// </summary>
    public static IEnumerable<ThingDef> GenerateReplacementFrameDefs()
    {
        var allDefs = DefDatabase<ThingDef>.AllDefsListForReading;

        for (int i = 0; i < allDefs.Count; i++)
        {
            ThingDef candidateDef = allDefs[i];

            if (candidateDef.designationCategory != null && candidateDef.IsBuildingArtificial && !candidateDef.IsFrame && candidateDef.MadeFromStuff)
            {
                ThingDef replaceFrameDef = CreateForBuilding(candidateDef);
                _buildingToFrameMap[candidateDef] = replaceFrameDef;
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
    /// 
    public static ThingDef CreateForBuilding(ThingDef def)
    {
        var thingDef = CreateBaseFrameDef();

        // Identity
        thingDef.defName = def.defName + "_ReplaceStuff";
        thingDef.label = def.label + "TD.ReplacingTag".Translate();

        // Interaction: Zero out the offset to ensure we aren't inheriting 
        // a valid cell from the building we are replacing.
        thingDef.interactionCellOffset = IntVec3.Zero;

        // Standard Properties
        thingDef.size = def.size;
        thingDef.SetStatBaseValue(StatDefOf.MaxHitPoints, (float)def.BaseMaxHitPoints * 0.25f);
        thingDef.SetStatBaseValue(StatDefOf.Beauty, -8f);
        thingDef.fillPercent = 0.2f;
        thingDef.pathCost = 10;
        thingDef.description = def.description;
        thingDef.passability = def.passability;
        thingDef.selectable = def.selectable;
        thingDef.constructEffect = def.constructEffect;

        // Building Properties
        thingDef.building.isEdifice = false;

        thingDef.constructionSkillPrerequisite = def.constructionSkillPrerequisite;
        thingDef.clearBuildingArea = false;
        thingDef.drawPlaceWorkersWhileSelected = def.drawPlaceWorkersWhileSelected;
        thingDef.stuffCategories = def.stuffCategories;

        // Graphic Setup
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

        // Support QualityBuilder
        if (QualityBuilderCompat.qualityBuilderPropsType is not null)
        {
            if (def.HasComp(typeof(CompQuality)) && def.building != null)
                thingDef.comps.Add((CompProperties)Activator.CreateInstance(QualityBuilderCompat.qualityBuilderPropsType));
        }

        thingDef.entityDefToBuild = def;
        thingDef.modContentPack = LoadedModManager.GetMod<ReplaceStuffPerformance>().Content;

        return thingDef;
    }

    /// <summary>
    /// Returns a base <see cref="ThingDef"/> initialized with standard structural defaults for a frame.
    /// </summary>
    static ThingDef CreateBaseFrameDef()
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
}
