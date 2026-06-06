/*
 Copyright (c) [2025] [Alex Tearse-Doyle]
Contributions for Performance Edtion: Kyle Givler
Other known Contribotors: MemeGoddess, Hexnet111, 

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
using UnityEngine;
using HarmonyLib;
using RimWorld;
using Replace_Stuff.Comps;

namespace Replace_Stuff
{
	public class Mod : Verse.Mod
	{
		public static Settings settings;

		public Mod(ModContentPack content) : base(content)
		{
			settings = GetSettings<Settings>();

			var harmony = new Harmony("Uuugggg.rimworld.Replace_StuffPerfomance.main");
			harmony.PatchAll();

			Verse.Log.Message("[ReplaceStuffPerfomance] Version {settings.Version}");
		}


		[StaticConstructorOnStartup]
		public static class ModStartup
		{
			static ModStartup()
			{
				//Hugslibs-added defs will be queued up before this Static Constructor
				//So queue replace frame generation after that
				LongEventHandler.QueueLongEvent(() => ThingDefGenerator_ReplaceFrame.AddReplaceFrames(), null, true, null);
				LongEventHandler.QueueLongEvent(CoolersOverWalls.DesignatorBuildDropdownStuffFix.SanityCheck, null, true, null);
				LongEventHandler.QueueLongEvent(Compatibility.AddRulesFromXML, null, true, null);
			}
		}

		public override void DoSettingsWindowContents(Rect inRect)
		{
			base.DoSettingsWindowContents(inRect);
			settings.DoWindowContents(inRect);
		}

		public override string SettingsCategory()
		{
			// TODO: Do better, make translations
			var result = "TD.ReplaceStuff".Translate();
			return $"{result} Perfomance Version ({settings.Version})";
		}
	}
}