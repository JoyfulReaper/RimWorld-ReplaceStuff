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

using Verse;
using RimWorld;
using HarmonyLib;
using Replace_Stuff.Replace;

namespace Replace_Stuff.NewThing

/// <summary>
/// Adjusts the construction work amount required for replacement frames.
/// This patch intercepts the WorkToBuild getter for Frames to ensure that 
/// the total effort includes the labor required to deconstruct the existing 
/// structure being replaced
/// </summary>
{
	[HarmonyPatch(typeof(Frame), "WorkToBuild", MethodType.Getter)]
	public static class NewThingDeconstructWork
	{
		//public float WorkToBuild
		public static void Postfix(Frame __instance, ref float __result)
		{
			if (__instance is ReplaceFrame)
				return; //ReplaceFrame already has its WorkToBuild set to the correct methods.

			if (__instance.IsNewThingReplacement(out Thing oldThing))
				__result += ReplaceFrame.WorkToDeconstructDef(oldThing.def, oldThing.Stuff);
		}
	}

	/// <summary>
	/// Extends the work calculation for build blueprints to include deconstruction effort.
	/// This patch modifies the WorkTotal getter for Blueprints to account for 
	/// the deconstruction cost of the target object, ensuring pawns correctly 
	/// calculate the total labor needed to replace an existing building with 
	/// the new structure specified in the blueprint.
	/// </summary>
	[HarmonyPatch(typeof(Blueprint_Build), "WorkTotal", MethodType.Getter)]
	public static class NewThingDeconstructWork_Blueprint
	{
		//public float WorkToBuild
		public static void Postfix(Frame __instance, ref float __result)
		{
			if (__instance.IsNewThingReplacement(out Thing oldThing))
				__result += ReplaceFrame.WorkToDeconstructDef(oldThing.def, oldThing.Stuff);
		}
	}
}
