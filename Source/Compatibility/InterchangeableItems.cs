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

using Replace_Stuff.Utilities;
using System.Collections.Generic;
using Verse;

/*
 * Example Def XML
<Defs>
    <Replace_Stuff.InterchangeableItems MayRequire="Ludeon.RimWorld.Ideology">
        <defName>ReplaceStuffModCompat_Ideology</defName>
        <replaceLists>
            <li>
                <comps>
                    <li>MyMod.Namespace.CompQualityBuilder</li>
                    <li>MyMod.Namespace.CompTemperatureSync</li>
                </comps>
            </li>
        </replaceLists>
    </Replace_Stuff.InterchangeableItems>
</Defs>
 */

// Do not change this namespace needed for compatibility: namespace Replace_Stuff;
namespace Replace_Stuff;

/// <summary>
/// Custom Def used to register data groups of cross-compatible items eligible for in-place replacement.
/// </summary>
public class InterchangeableItems : Def
{
    public List<ReplaceList> replaceLists = new();

    public override void ResolveReferences()
    {
        base.ResolveReferences();

        // Trigger the resolution for every list in this Def
        foreach (var list in replaceLists)
        {
            list.ResolveComps();
        }
    }
}

/// <summary>
/// Represents a specific category of interchangeable items and their associated behavioral overrides.
/// </summary>
public class ReplaceList
{
    public string category = "";

    public List<ThingDef> items = new();

    // Raw strings from XML
    public List<string> comps = new();

    // Performance Cache: Parsed types
    [Unsaved]
    public List<System.Type> compTypes = new();

    // Call this once during Def initialization
    public void ResolveComps()
    {
        if (comps.NullOrEmpty())
            return;

        foreach (string compName in comps)
        {
            var type = GenTypes.GetTypeInAnyAssembly(compName);
            if (type != null)
                compTypes.Add(type);
            else
                RSLog.Error($"Could not find component type: {compName}");
        }
    }
}