using Verse;
using RimWorld;
using HarmonyLib;

namespace Replace_Stuff.NewThing
{
	/// <summary>
	/// Extends vanilla replacement logic to allow custom building replacements.
	/// This patch intercepts GenConstruct.CanReplace to evaluate if a new ThingDef 
	/// is authorized to replace an existing one. It includes safety checks to 
	/// prevent replacing indestructible structures and only executes during active 
	/// blueprint designation to ensure performance.
	/// </summary>
	[HarmonyPatch(typeof(GenConstruct), nameof(GenConstruct.CanReplace))]
	class CanReplaceNewThingOverOldThing
	{
		public static void Postfix(ref bool __result, BuildableDef placing, BuildableDef existing)
		{
			// The player isn't actively placing a blueprint
			if (!DesignatorContext.designating) 
				return;

			if (((placing as ThingDef)?.IsNonDeconstructibleAttackableBuilding ?? false) ||
				((existing as ThingDef)?.IsNonDeconstructibleAttackableBuilding ?? false))
			{
				__result = false;
				return;
			}

			if (__result) return;

			if (placing is ThingDef newD && existing is ThingDef oldD && newD.CanReplace(oldD))
				__result = true;
		}
	}

	/// <summary>
	/// Adjusts the rendering altitude of construction frames.
	/// By shifting frames to the 'BuildingOnTop' altitude layer, this patch ensures 
	/// that construction frames for replacement structures (like doors or walls) 
	/// render correctly over the existing building, preventing visual Z-fighting 
	/// or flickering while the replacement is being constructed.
	/// </summary>
	[HarmonyPatch(typeof(ThingDefGenerator_Buildings), "NewFrameDef_Thing")]
	public static class FramesDrawOverBuildingsEvenTheDoors
	{
		//		private static ThingDef NewFrameDef_Thing(ThingDef def)
		public static void Postfix(ThingDef __result)
		{
			__result.altitudeLayer = AltitudeLayer.BuildingOnTop;
		}
	}
}
