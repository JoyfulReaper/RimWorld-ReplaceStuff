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

using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Replace_Stuff.OverWallCoolers;

/// <summary>
/// Fixes issues where Designator_Builds (specifically those made from stuff) are 
/// trapped in dropdowns that shouldn't contain them.
/// </summary>
public static class DesignationMenuFixer
{
    public static void FlattenDesignationMenus()
    {
        foreach (var category in DefDatabase<DesignationCategoryDef>.AllDefsListForReading)
        {
            // Use a temporary list to rebuild the category's designators
            List<Designator> updatedDesignators = new List<Designator>();

            foreach (var designator in category.AllResolvedDesignators)
            {
                if (designator is Designator_Dropdown dropdown && ShouldUnpackDropdown(dropdown))
                {
                    // If it matches, unpack the contents into the new list instead of the dropdown
                    updatedDesignators.AddRange(dropdown.Elements);
                }
                else
                {
                    // Otherwise, keep the original designator
                    updatedDesignators.Add(designator);
                }
            }

            // Replace the old list with our clean, unpacked version
            category.AllResolvedDesignators.Clear();
            category.AllResolvedDesignators.AddRange(updatedDesignators);
        }
    }

    /// <summary>
    /// Determines if a dropdown should be unpacked.
    /// Returns true if the dropdown contains any Build designators made from stuff.
    /// </summary>
    private static bool ShouldUnpackDropdown(Designator_Dropdown dropdown)
    {
        return dropdown.Elements.Any(element =>
            element is Designator_Build buildDesignator &&
            buildDesignator.PlacingDef.MadeFromStuff);
    }
}

// //Designator_Build and _Dropdown both create their own dropdown list but only one shows
// //That'll be _build designators with things made from stuff
// //So let's remove any dropdown list if it holds things made from stuff
// //(if a mod makes coolers made from stuff, overwall coolers and their 2-wide version fit this description)
// public static class DesignatorBuildDropdownStuffFix
// {
// 	public static void SanityCheck()
// 	{
// 		//Either patch the method that created the dropdown designator, or just undo it here:
// 		foreach (var catDef in DefDatabase<DesignationCategoryDef>.AllDefsListForReading)
// 			for (int i = 0; i < catDef.AllResolvedDesignators.Count; i++)
// 				if (catDef.AllResolvedDesignators[i] is Designator_Dropdown des
// 					&& des.Elements.Any(d => d is Designator_Build db && db.PlacingDef.MadeFromStuff))
// 				{
// 					catDef.AllResolvedDesignators.RemoveAt(i);
// 					foreach (var dropDes in des.Elements)
// 						catDef.AllResolvedDesignators.Insert(i, dropDes);
// 				}
// 	}
// }