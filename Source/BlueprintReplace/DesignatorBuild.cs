using Verse;
using HarmonyLib;

namespace Replace_Stuff.BlueprintReplace
{
	[HarmonyPatch(typeof(GenSpawn), "SpawningWipes")]
	static class WipeBlueprints
	{
		//public static bool SpawningWipes(BuildableDef newEntDef, BuildableDef oldEntDef)
		public static void Postfix(BuildableDef newEntDef, BuildableDef oldEntDef, ref bool __result)
		{
			if (__result || newEntDef != oldEntDef) return;

			if (newEntDef is ThingDef newD && newD.IsBlueprint &&
					oldEntDef is ThingDef oldD && oldD.IsBlueprint)
				__result = true;
		}
	}
}
