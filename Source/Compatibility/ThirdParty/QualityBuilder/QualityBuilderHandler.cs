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
using Replace_Stuff.Data;
using Replace_Stuff.Interfaces;
using Replace_Stuff.Replace;
using Replace_Stuff.Utilities;
using RimWorld;
using System;
using Verse;

namespace Replace_Stuff.Compatibility.ThirdParty.QualityBuilder;

[ReplacementHandler("CompQualityBuilder")]
public class QualityBuilderHandler : IReplacementHandler
{
    private static readonly DesignationDef _designationDef;
    private const string DesignationKey = "QB_HasDesignation";
    private const string SkilledKey = "QB_IsSkilled";
    private const string QualityKey = "QB_Quality";

    static QualityBuilderHandler()
    {
        try
        {
            // NOTE: This is NOT a typo this is the actual class name:
            var propsType = AccessTools.TypeByName("CompProperties_QualityBuilderr"); // Not a typo
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

    public void PreAction(ReplacementData data, Thing oldThing, Thing newThing)
    {
        if (_designationDef is not null && oldThing.Map?.designationManager?.DesignationOn(oldThing, _designationDef) != null)
        {
            data.modData[DesignationKey] = "true";
        }

        if (oldThing is ThingWithComps twc)
        {
            var qbType = AccessTools.TypeByName("QualityBuilder.CompQualityBuilder");

            ThingComp qbComp = null;
            if (qbType is not null)
            {
                foreach (var comp in twc.AllComps)
                {
                    if (comp.GetType() == qbType)
                    {
                        qbComp = comp;
                        break;
                    }
                }
            }

            if (qbComp is not null)
            {
                var skilledField = AccessTools.Field(qbComp.GetType(), "skilled");
                var qualityField = AccessTools.Field(qbComp.GetType(), "desiredMinQualityRef");

                if (skilledField is not null && qualityField is not null)
                {
                    data.modData[SkilledKey] = ((bool)skilledField.GetValue(qbComp)).ToString();
                    data.modData[QualityKey] = ((QualityCategory)qualityField.GetValue(qbComp)).ToString();
                }
            }
        }
    }

    public void PostAction(ReplacementData data, Thing oldThing, Thing newThing)
    {
        if (data.modData.TryGetValue(DesignationKey, out string hasDes) && hasDes == "true")
        {
            newThing.Map?.designationManager?.AddDesignation(new Designation(newThing, _designationDef));
        }

        if (newThing is ThingWithComps twc)
        {
            var qbType = AccessTools.TypeByName("QualityBuilder.CompQualityBuilder");

            ThingComp qbComp = null;
            if (qbType != null)
            {
                foreach (var comp in twc.AllComps)
                {
                    if (comp.GetType() == qbType)
                    {
                        qbComp = comp;
                        break;
                    }
                }
            }

            if (qbComp != null)
            {
                var skilledField = AccessTools.Field(qbComp.GetType(), "skilled");
                var qualityField = AccessTools.Field(qbComp.GetType(), "desiredMinQualityRef");

                if (data.modData.TryGetValue(SkilledKey, out string sk) && bool.TryParse(sk, out bool isSkilled))
                    skilledField?.SetValue(qbComp, isSkilled);

                if (data.modData.TryGetValue(QualityKey, out string q) && Enum.TryParse(q, out QualityCategory qual))
                    qualityField?.SetValue(qbComp, qual);
            }
        }
    }
}