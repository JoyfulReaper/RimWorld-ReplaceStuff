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

using HarmonyLib;
using RimWorld;
using Verse;

namespace Replace_Stuff
{
    /// <summary>
    /// Harmony patch that overrides construction obstruction checks for <see cref="ReplaceFrame"/> instances.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Vanilla RimWorld uses <see cref="GenConstruct.BlocksConstruction"/> to determine if a building 
    /// project is obstructed by other entities, which would normally prevent the placement or 
    /// construction of an item.
    /// </para>
    /// <para>
    /// Since <see cref="ReplaceFrame"/> objects are designed to be placed directly atop existing, 
    /// functional structures, they would otherwise trigger this obstruction logic and prevent 
    /// the player from queuing replacements.
    /// </para>
    /// <para>
    /// This patch acts as a <see cref="HarmonyLib.HarmonyPatchType.Postfix"/> on the blocking logic, 
    /// forcing the result to <c>false</c> whenever the object being constructed is a 
    /// <see cref="ReplaceFrame"/>, effectively "ghosting" the frame so it does not collide with 
    /// its own placement validation.
    /// </para>
    /// </remarks>
    [HarmonyPatch(typeof(GenConstruct), "BlocksConstruction")]
	class ReplaceFrameNoBlock
	{
		//public static bool BlocksConstruction(Thing constructible, Thing t)
		public static void Postfix(Thing constructible, Thing t, ref bool __result)
		{
			if (!__result)
                return;

			if (constructible is ReplaceFrame frame)
                __result = false;
		}
	}
}
