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

using Replace_Stuff.Interfaces;
using Replace_Stuff.Replace;
using Replace_Stuff.Utilities;
using System;
using Verse;

namespace Replace_Stuff.Compatibility.ThirdParty;

[ReplacementHandler("RimFridge.RimFridge_Building")]
public class FridgeHandler : IReplacementHandler
{
    public void PreAction(ReplaceData data, Thing oldThing, Thing newThing)
    {
        // No pre-action needed, but you can capture data if you want
        // RimFridge usually just needs the temp copied over.
    }

    public void PostAction(ReplaceData data, Thing oldThing, Thing newThing)
    {
        if (RimFridgeCompat.DesiredTempInfo != null)
        {
            try
            {
                float temp = (float)RimFridgeCompat.DesiredTempInfo.GetValue(oldThing);
                RimFridgeCompat.DesiredTempInfo.SetValue(newThing, temp);
            }
            catch (Exception ex)
            {
                RSLog.Error($"Failed to sync fridge temp: {ex.Message}");
            }
        }
    }
}