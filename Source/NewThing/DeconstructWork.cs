using Verse;
using RimWorld;
using HarmonyLib;

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
