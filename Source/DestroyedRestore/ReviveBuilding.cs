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
using Replace_Stuff.DestroyedRestore;
using Replace_Stuff.Replace;
using RimWorld;
using Verse;

[HarmonyPatch(typeof(Frame), nameof(Frame.CompleteConstruction))]
public static class Frame_CompleteConstruction_Patch
{
	public static bool Prefix(Frame __instance, Pawn worker)
	{
		if (__instance is not ReplaceFrame rf)
			return true;

		Map map = __instance.Map;
		IntVec3 pos = __instance.Position;
		Rot4 rot = __instance.Rotation;

		ThingDef thingDef = rf.def.entityDefToBuild as ThingDef;
		ThingDef stuff = rf.Stuff;

		Thing oldThing = rf.oldThing;

		// Hard remove old thing first
		if (oldThing != null && oldThing.Spawned)
		{
			oldThing.Destroy(DestroyMode.KillFinalize);
		}

        Thing newThing = ThingMaker.MakeThing(thingDef, stuff);
		GenSpawn.Spawn(newThing, pos, map, rot);

		ReplaceFrame.FinalizeReplace(newThing, stuff, worker);
        DestroyedBuildings.ReviveBuilding(newThing, pos, map);

        worker?.records?.Increment(RecordDefOf.ThingsConstructed);

		return false; // block vanilla entirely
	}
}


/*
 * Previously a Transpiler was user here is the old code in case there are any issues
 * 
 * This may be the most fragile part of this mod and I'm not sure which apporach to use
 * 
 * namespace Replace_Stuff.DestroyedRestore
{
	[HarmonyPatch(typeof(Frame), nameof(Frame.CompleteConstruction))]
	static class ReviveBuilding
	{
		//public void CompleteConstruction(Pawn worker)
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase mb, ILGenerator ilg)
		{
			//ThingMaker.MakeThing(thingDef, base.Stuff);
			MethodInfo SpawnInfo = AccessTools.Method(typeof(GenSpawn), nameof(GenSpawn.Spawn), 
				new Type[] { typeof(Thing), typeof(IntVec3), typeof(Map), typeof(Rot4), typeof(WipeMode), typeof(bool), typeof(bool)});
			MethodInfo CheckForRevivalInfo = AccessTools.Method(typeof(ReviveBuilding), nameof(CheckForRevival));

			int localMapIndex = mb.GetMethodBody().LocalVariables.FirstOrDefault(lv => lv.LocalType == typeof(Map)).LocalIndex;

			foreach(CodeInstruction i in instructions)
			{
				if(i.Calls(SpawnInfo))
				{
					yield return new CodeInstruction(OpCodes.Call, CheckForRevivalInfo);//CheckForRevival(thingDef, base.Stuff, Frame, map)
				}
				else 
					yield return i;
			}
		}

		//public static Thing Spawn(Thing newThing, IntVec3 loc, Map map, Rot4 rot, WipeMode wipeMode = WipeMode.Vanish, bool respawningAfterLoad = false, bool forbidLeavings = false)
		public static Thing CheckForRevival(Thing newThing, IntVec3 loc, Map map, Rot4 rot, WipeMode wipeMode = WipeMode.Vanish, bool respawningAfterLoad = false, bool forbidLeavings = false)
		{
			Log.Message($"Checking {newThing} for revival");
			Thing thing = GenSpawn.Spawn(newThing, loc, map, rot, wipeMode, respawningAfterLoad, forbidLeavings);
			DestroyedBuildings.ReviveBuilding(thing, loc, map);	//After spawn so SpawnSetup is called, comps created.
			return thing;
		}
	}
}
*/