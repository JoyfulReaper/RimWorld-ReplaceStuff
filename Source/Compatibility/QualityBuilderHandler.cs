using HarmonyLib;
using Replace_Stuff.Interfaces;
using Replace_Stuff.Replace;
using Replace_Stuff.Utilities;
using System;
using Verse;

namespace Replace_Stuff.Compatibility;

public class QualityBuilderHandler : IReplacementHandler
{
    private static Type compType;
    private static DesignationDef designationDef;

    // Define a unique key for this handler to prevent collisions
    private const string DesignationKey = "QualityBuilder_HasDesignation";

    // Static constructor ensures reflection runs only once
    static QualityBuilderHandler()
    {
        try
        {
            compType = AccessTools.TypeByName("CompQualityBuilder");
            // TODO: Assuming the typo is on purpose, verify
            AccessTools.TypeByName("CompProperties_QualityBuilderr");
            designationDef = DefDatabase<DesignationDef>.GetNamed("SkilledBuilder", false);
        }
        catch (Exception e)
        {
            RSLog.Warning($"Failed to resolve QualityBuilder types: {e.Message}");
        }
    }

    public void PreAction(ReplaceData data, Thing oldThing)
    {
        if (designationDef == null) return;

        var des = oldThing.Map?.designationManager?.DesignationOn(oldThing, designationDef);
        if (des != null)
        {
            data.modData[DesignationKey] = "true";
        }
    }

    public void PostAction(ReplaceData data, Thing newThing)
    {
        if (designationDef == null) return;
        if (data.modData.TryGetValue(DesignationKey, out string val) && val == "true")
        {
            newThing.Map?.designationManager?.AddDesignation(new Designation(newThing, designationDef));
        }
    }
}