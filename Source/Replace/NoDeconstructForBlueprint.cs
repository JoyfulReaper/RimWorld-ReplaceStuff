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
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Replace_Stuff.Replace;

// 1.6 made a new job to specifically deconstruct for blueprints. Noooo
// WorkGiver_ConstructDeliverResourcesToBlueprints will do it anyway! The new job, what, only is higher priority?
[HarmonyPatch(typeof(WorkGiver_DeconstructForBlueprint), nameof(WorkGiver_DeconstructForBlueprint.PotentialWorkThingsGlobal))]
public static class NoDeconstructForBlueprint
{
	// public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
	public static bool Prefix(ref IEnumerable<Thing> __result)
	{
		__result = Enumerable.Empty<Thing>();
		return false;
	}
}