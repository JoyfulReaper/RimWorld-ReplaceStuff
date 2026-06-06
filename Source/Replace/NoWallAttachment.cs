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

using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;
using HarmonyLib;
using Replace_Stuff.NewThing;
using Verse;

namespace Replace_Stuff.Replace
{
	[HarmonyPatch(typeof(GenConstruct), nameof(GenConstruct.GetWallAttachedTo), [typeof(IntVec3), typeof(Rot4), typeof(Map)])]
	public static class NoWallAttachment
	{
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			// In the loop: foreach (Thing thing in c.GetThingList(map))
			// insert:
			// if(thing is ReplaceFrame) continue;

			// (This is probably redundant because the wall under the replace frame will probably always be checked first and returned.)


			// The first br should branch to the entry point for the loop, keep that label to continue to
			Label continueLabel = (Label)instructions.First(ci => ci.opcode == OpCodes.Br_S).operand;

			FieldInfo defInfo = AccessTools.Field(typeof(Thing), nameof(Thing.def));

			List<CodeInstruction> insts = instructions.ToList();
			for (int i = 0; i < insts.Count; i++)
			{
				CodeInstruction inst = insts[i];

				yield return inst;

				// Before we get the thing.def, use the thing:
				if (i + 1 < insts.Count && insts[i + 1].LoadsField(defInfo))
				{
					// stack has: Thing thing from the list
					yield return new CodeInstruction(OpCodes.Isinst, typeof(ReplaceFrame));// thing == typeof(ReplaceFrame)
					yield return new CodeInstruction(OpCodes.Brtrue_S, continueLabel);// if(thing == typeof(ReplaceFrame)) continue;


					// Call ldlocal for Thing again to replace what was there (with, maybe, no labels...)
					yield return new CodeInstruction(inst.opcode, inst.operand);
				}
			}
		}
	}


	[HarmonyPatch(typeof(GenConstruct), nameof(GenConstruct.GetAttachedBuildings))]
	public static class NoAttachedBuildings
	{
		private static List<Thing> emptyList = [];

		public static bool Prefix(Thing thing, ref List<Thing> __result)
		{
			if (thing is ReplaceFrame)
			{
				__result = emptyList;
				return false;
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(Placeworker_AttachedToWall), nameof(Placeworker_AttachedToWall.AllowsPlacing))]
	public static class AllowAttachedWallReplacement
	{
		public static void Postfix(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, ref AcceptanceReport __result)
		{
			if (__result.Accepted || checkingDef is not ThingDef newDef || map == null)
				return;

			List<Thing> thingList = loc.GetThingList(map);
			for (int i = 0; i < thingList.Count; i++)
			{
				Thing oldThing = thingList[i];
				if (oldThing.Rotation != rot)
					continue;

				ThingDef oldBuiltDef = GenConstruct.BuiltDefOf(oldThing.def) as ThingDef;
				if (oldBuiltDef?.building?.isAttachment != true)
					continue;

				if (newDef.CanReplace(oldThing.def))
				{
					__result = AcceptanceReport.WasAccepted;
					return;
				}
			}
		}
	}

	[HarmonyPatch(typeof(GenSpawn), nameof(GenSpawn.SpawningWipes))]
	public static class AttachmentsCanWipeReplacementTargets
	{
		public static void Postfix(BuildableDef newEntDef, BuildableDef oldEntDef, ref bool __result)
		{
			if (__result || newEntDef is not ThingDef newDef || oldEntDef is not ThingDef oldDef)
				return;

			if ((!newDef.building?.isAttachment ?? true) || (!oldDef.building?.isAttachment ?? true))
				return;

			if (newDef.CanReplace(oldDef))
				__result = true;
		}
	}
}

