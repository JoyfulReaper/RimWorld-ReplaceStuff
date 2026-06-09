// TODO: Lets finish some other refactoring before attempting this

//using HarmonyLib;
//using RimWorld;
//using System.Reflection;
//using Verse;
//using static UnityEngine.Networking.UnityWebRequest;

//// Patch for compatability with SBZ Workbench Shelf
//// https://steamcommunity.com/sharedfiles/filedetails/?id=3484671309

//[HarmonyPatch]
//public static class Patch_NeatStorage_Compatibility
//{
//	// Check if the type exists before patching
//	static bool Prepare()
//	{
//		var result = AccessTools.TypeByName("NeatStorageFix.Placeworker_StoragesNoOverlapEachOther") != null;
//		if (Prefs.DevMode)
//		{
//			if (result)
//			{
//				Log.Message("[ReplaceStuffPerformance Compatability Patch:] FOUND SBZ Workbench shelf");
//			}
//			else
//			{
//				Log.Message("[ReplaceStuffPerformance Compatability Patch:] MISSING SBZ Workbench shelf");
//			}
//		}

//		return result;
//	}

//	static MethodBase TargetMethod()
//	{
//		return AccessTools.Method("NeatStorageFix.Placeworker_StoragesNoOverlapEachOther:AllowsPlacing");
//	}

//	[HarmonyPostfix]
//	public static void Postfix(ref AcceptanceReport __result, BuildableDef checkingDef)
//	{
//		if (!__result.Accepted && checkingDef is ThingDef thingDef && typeof(Building_Storage).IsAssignableFrom(thingDef.thingClass))
//		{
//			__result = AcceptanceReport.WasAccepted;
//		}
//	}
//}

//[HarmonyPatch(typeof(Building_Storage), nameof(Building_Storage.SpawnSetup))]
//public static class Patch_ClearOldStorageRegistration
//{
//	static bool Prepare()
//	{
//		return AccessTools.TypeByName("NeatStorageFix.Placeworker_StoragesNoOverlapEachOther") != null;
//	}

//	[HarmonyPrefix]
//	public static void Prefix(Building_Storage __instance, Map map)
//	{
//		SlotGroup group = __instance.GetSlotGroup();

//		if (map == null)
//			return;

//		var cells = GenAdj.OccupiedRect(__instance.Position, __instance.Rotation, __instance.def.Size);

//		foreach (var cell in cells)
//		{
//			map.haulDestinationManager.SetCellFor(cell, null);
//		}
//	}
//}