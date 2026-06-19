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
using Replace_Stuff.Interfaces;
using Replace_Stuff.Replace;
using Replace_Stuff.Utilities;
using RimWorld;
using System;
using Verse;

namespace Replace_Stuff.Compatibility.ThirdParty;

public class QualityBuilderHandler : IReplacementHandler
{
    private static readonly Type _compType;
    private static readonly DesignationDef _designationDef;

    // Define a unique key for this handler to prevent collisions
    private const string DesignationKey = "QualityBuilder_HasDesignation";

    static QualityBuilderHandler()
    {
        try
        {
            _compType = AccessTools.TypeByName("CompQualityBuilder");

            if (_compType != null)
            {
                // Register using the Comp's FullName
                ReplacementRegistry.RegisterHandler(_compType.FullName, new QualityBuilderHandler());
            }

            // TODO: Assuming the typo is on purpose, verify
            var propsType = AccessTools.TypeByName("CompProperties_QualityBuilderr");
            _designationDef = DefDatabase<DesignationDef>.GetNamed("SkilledBuilder", false);

            if (propsType != null)
            {
                ReplacementFrameDefGenerator.OnFrameCreated += (buildingDef, frameDef) =>
                {
                    if (buildingDef.HasComp(typeof(CompQuality)) && buildingDef.building is not null)
                    {
                        frameDef.comps.Add((CompProperties)Activator.CreateInstance(propsType));
                    }
                };
            }
        }
        catch (Exception e)
        {
            RSLog.Warning($"Failed to resolve QualityBuilder types: {e.Message}");
        }
    }

    public void PreAction(ReplaceData data, Thing oldThing, Thing newThing)
    {
        if (_designationDef is null)
            return;

        var des = oldThing.Map?.designationManager?.DesignationOn(oldThing, _designationDef);
        if (des != null)
        {
            data.modData[DesignationKey] = "true";
        }
    }

    public void PostAction(ReplaceData data, Thing oldThing, Thing newThing)
    {
        if (_designationDef is null)
            return;

        if (data.modData.TryGetValue(DesignationKey, out string val) && val == "true")
        {
            newThing.Map?.designationManager?.AddDesignation(new Designation(newThing, _designationDef));
        }
    }
}