// Credit/Inspiration: https://github.com/Hexnet111/RimWorld-ReplaceStuff-Performance-Patch/blob/cf542b03449131de9393b6dc3e35229a292e38ed/Source/Replace/InterceptBlueprint.cs

using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Replace_Stuff.Replace
{
	[HarmonyPatch(typeof(Designator_Build), nameof(Designator_Build.DesignateSingleCell))]
	class InterceptDesignator_Build
	{
		public static bool Prefix(Designator_Build __instance, IntVec3 c, BuildableDef ___entDef, Rot4 ___placingRot)
		{
			// 1. Guard against nulls to prevent crashes
			if (__instance == null || ___entDef == null) return true;

			ThingDef thingDef = ___entDef as ThingDef;
			if (thingDef == null) return true;

			// 2. Safe check for stuff requirements
			if (thingDef.MadeFromStuff && __instance.StuffDef == null)
			{
				return true;
			}

			// 3. Skip if GodMode or no work needed
			if (DebugSettings.godMode || ___entDef.GetStatValueAbstract(StatDefOf.WorkToBuild, __instance.StuffDef) == 0f)
				return true;

			// 4. Handle door rotation
			if (typeof(Building_Door).IsAssignableFrom(thingDef.thingClass))
				___placingRot = DoorUtility.DoorRotationAt(c, __instance.Map, thingDef.building.preferConnectingToFences);

			// 5. Optimized search for replaceable items
			List<Thing> replaceables = c.GetThingList(__instance.Map);
			if (replaceables.Count == 0) return true;

			Thing thingToReplace = null;

			for (int i = 0; i < replaceables.Count; i++)
			{
				Thing replaceable = replaceables[i];

				if (replaceable.Rotation != ___placingRot) continue;
				if (!Designator_ReplaceStuff.CanReplaceStuffFor(__instance.StuffDef, replaceable, thingDef)) continue;

				// Priority: Blueprints and Frames
				if (replaceable is Blueprint_Build || replaceable is Frame)
				{
					thingToReplace = replaceable;
					break; // Found the best target, stop searching
				}

				// Fallback for regular buildings
				if (thingToReplace == null)
				{
					thingToReplace = replaceable;
				}
			}

			if (thingToReplace == null) return true;

			Designator_ReplaceStuff.DoReplace(thingToReplace, __instance.StuffDef);

			return false;
		}
	}
}