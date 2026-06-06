/*
 Copyright (c) [2025] [Alex Tearse-Doyle]
Contributions for Performance Edtion: Kyle Givler
Other known Contributors: MemeGoddess, Hexnet111, 

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
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


namespace Replace_Stuff
{
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
}