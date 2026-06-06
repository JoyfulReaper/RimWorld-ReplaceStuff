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

namespace Replace_Stuff.DestroyedRestore
{
	[HarmonyPatch(typeof(Blueprint), nameof(Blueprint.DeSpawn))]
	class BlueprintRemoval
	{
		//public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
		/// <summary>
		/// 
		/// </summary>
		/// <param name="__instance">BluePrint Instance</param>
		/// <param name="mode">Blueprint's DestoryMode</param>
		public static void Prefix(Blueprint __instance, DestroyMode mode)
		{
			if (mode != DestroyMode.Vanish)
				DestroyedBuildings.RemoveAt(__instance.Position, __instance.Map);
		}
	}

	[HarmonyPatch(typeof(Frame), nameof(Frame.Destroy))]
	class FrameRemoval
	{
		//public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
		public static void Prefix(Frame __instance, DestroyMode mode)
		{
			if (mode != DestroyMode.Vanish && mode != DestroyMode.FailConstruction && mode != DestroyMode.KillFinalize)
				DestroyedBuildings.RemoveAt(__instance.Position, __instance.Map);
		}
	}
}
