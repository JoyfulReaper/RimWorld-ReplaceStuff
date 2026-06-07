/*
 Copyright (c) [2025] [Alex Tearse-Doyle]
Contributions for Performance Edtion: Kyle Givler
Other known Contribotors: MemeGoddess, Hexnet111, 

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
using Replace_Stuff;
using RimWorld;

// Controls if the OverTheWall coolers and vents should be visible
[HarmonyPatch(typeof(Designator_Build), "Visible", MethodType.Getter)]
public static class HideCoolerBuild
{
	public static void Postfix(Designator_Build __instance, ref bool __result)
	{
		if (!__result) return;

		var def = __instance.PlacingDef;

		if (ReplaceStuffPrefomanceMod.settings.hideOverwallCoolers &&
			(def == OverWallDef.Cooler_Over ||
			def == OverWallDef.Cooler_Over2W ||
			def == OverWallDef.Vent_Over ||
			def == OverWallDef.Vent_Over2W))
		{
			__result = false;
			return;
		}

		if (ReplaceStuffPrefomanceMod.settings.hideNormalCoolers &&
			(__instance.PlacingDef == ThingDefOf.Cooler ||
			__instance.PlacingDef == OverWallDef.Vent))
		{
			__result = false;
			return;
		}
	}
}