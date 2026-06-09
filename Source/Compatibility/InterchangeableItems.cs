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

using System.Collections.Generic;
using Verse;

/*
 * Example Def XML
   <Defs>
    <Replace_Stuff.InterchangeableItems>
        <defName>MyUpgradedCoolers</defName>
        <replaceLists>
            <li>
                <category>Coolers</category>
                <items>
                    <li>Cooler</li> <li>SuperCooler_Advanced</li> </items>
                <comps>
                    <li>Replace_Stuff.CoolerReplacementComp</li> </comps>
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
}

/// <summary>
/// Represents a specific category of interchangeable items and their associated behavioral overrides.
/// </summary>
public class ReplaceList
{
    public string category = "";
    public List<ThingDef> items = new(); // List of items that are allowed to replace eachother
    public List<string> comps = new(); // Strings for class name
}