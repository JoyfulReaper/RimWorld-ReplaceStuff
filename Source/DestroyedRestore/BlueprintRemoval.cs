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
				DestroyedBuildingStore.RemoveAt(__instance.Position, __instance.Map);
		}
	}

	[HarmonyPatch(typeof(Frame), nameof(Frame.Destroy))]
	class FrameRemoval
	{
		//public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
		public static void Prefix(Frame __instance, DestroyMode mode)
		{
			if (mode != DestroyMode.Vanish && mode != DestroyMode.FailConstruction && mode != DestroyMode.KillFinalize)
				DestroyedBuildingStore.RemoveAt(__instance.Position, __instance.Map);
		}
	}
}
