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

using System.Collections.Generic;
using RimWorld;
using HarmonyLib;
using System.Reflection.Emit;
using Replace_Stuff.Replace;

namespace Replace_Stuff.BlueprintReplace
{
	[HarmonyPatch(typeof(Designator_Install), "CanDesignateCell")]
	//public override AcceptanceReport CanDesignateCell(IntVec3 c)
	static class DesignatorInstall
	{
		public static IEnumerable<CodeInstruction> Transpiler (IEnumerable<CodeInstruction> instructions)
		{
			bool next = false;
			foreach(CodeInstruction i in instructions)
			{
				if(next)
				{
					next = false;
					yield return new CodeInstruction(OpCodes.Pop);
				}
				else
					yield return i;

				//Code checks !this.MiniToInstallOrBuildingToReinstall is MinifiedThing to set IdenticalThingExists, so let's ignore taht
				if (i.opcode == OpCodes.Isinst)
					next = true;
			}
		}

		public static void Prefix()
		{
			DesignatorContext.designating = true;
		}
		public static void Postfix()
		{
			DesignatorContext.designating = false;
		}
	}
}
