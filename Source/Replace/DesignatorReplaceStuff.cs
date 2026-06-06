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
using UnityEngine;
using Verse;
using RimWorld;
using Replace_Stuff.NewThing;

namespace Replace_Stuff.Replace
{
	[StaticConstructorOnStartup]
	public static class TexDefOf
	{
		public static Texture2D replaceIcon = ContentFinder<Texture2D>.Get("ReplaceStuff", true);
	}

	public class Designator_ReplaceStuff : Designator
	{
		private ThingDef stuffDef;

		public override DrawStyleCategoryDef DrawStyleCategory => DrawStyleCategoryDefOf.Orders;

		private static readonly Vector2 DragPriceDrawOffset = new Vector2(19f, 17f);
		private static readonly Dictionary<BuildableDef, HashSet<ThingDef>> _allowedStuffCache = new();

		public Designator_ReplaceStuff()
		{
			this.soundDragSustain = SoundDefOf.Designate_DragBuilding;
			this.soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
			this.soundSucceeded = SoundDefOf.Designate_PlaceBuilding;

			this.defaultLabel = "TD.Replace".Translate();
			this.defaultDesc = "TD.ReplaceDesc".Translate();
			this.icon = TexDefOf.replaceIcon;
			this.iconProportions = new Vector2(1f, 1f);
			this.iconDrawScale = 1f;
			this.ResetStuffToDefault();

			this.hotKey = KeyBindingDefOf.Command_ColonistDraft;
		}

		public void ResetStuffToDefault()
		{
			stuffDef = ThingDefOf.WoodLog;
		}

		public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
		{
			GizmoResult result = base.GizmoOnGUI(topLeft, maxWidth, parms);

			float w = GetWidth(maxWidth);
			Rect rect = new Rect(topLeft.x + w / 2, topLeft.y, w / 2, Height / 2);
			Widgets.ThingIcon(rect, stuffDef);

			return result;
		}

		public override void DrawMouseAttachments()
		{
			base.DrawMouseAttachments();
			if (!ArchitectCategoryTab.InfoRect.Contains(UI.MousePositionOnUIInverted))
			{
				int cost = 0;
				List<IntVec3> dragCells = Find.DesignatorManager.Dragger.DragCells;

				// Loop over the cells being dragged
				for (int c = 0; c < dragCells.Count; c++)
				{
					IntVec3 cell = dragCells[c];
					List<Thing> thingsInCell = cell.GetThingList(Map);

					for (int t = 0; t < thingsInCell.Count; t++)
					{
						Thing thing = thingsInCell[t];

						if (thing is not ReplaceFrame && CanReplaceStuffFor(stuffDef, thing))
						{
							if (GenConstruct.BuiltDefOf(thing.def) is ThingDef builtDef)
							{
								cost += Mathf.RoundToInt((float)builtDef.costStuffCount / stuffDef.VolumePerUnit);
							}
						}
					}
				}

				Vector2 drawPoint = Event.current.mousePosition + DragPriceDrawOffset;
				Rect iconRect = new Rect(drawPoint.x, drawPoint.y, 27f, 27f);
				GUI.color = stuffDef.uiIconColor;
				GUI.DrawTexture(iconRect, stuffDef.uiIcon);

				Rect textRect = new Rect(drawPoint.x + 29f, drawPoint.y, 999f, 29f);
				string text = cost.ToString();
				if (base.Map.resourceCounter.GetCount(stuffDef) < cost)
				{
					GUI.color = Color.red;
					text = text + " (" + "NotEnoughStoredLower".Translate() + ")";
				}
				else
					GUI.color = Color.white;
				Text.Font = GameFont.Small;
				Text.Anchor = TextAnchor.MiddleLeft;
				Widgets.Label(textRect, text);
				Text.Anchor = TextAnchor.UpperLeft;
				GUI.color = Color.white;
			}
		}

		public override void ProcessInput(Event ev)
		{
			if (!CheckCanInteract()) return;

			var amounts = Map.resourceCounter.AllCountedAmounts;
			List<FloatMenuOption> list = new List<FloatMenuOption>(amounts.Count);

			bool godMode = DebugSettings.godMode;

			foreach (var kvp in amounts)
			{
				ThingDef def = kvp.Key;
				if (!def.IsStuff || (!godMode && kvp.Value <= 0)) continue;

				list.Add(new FloatMenuOption(def.LabelCap, () =>
				{
					base.ProcessInput(ev);
					Find.DesignatorManager.Select(this);
					stuffDef = def;
				}, def));
			}

			if (list.Count == 0)
			{
				Messages.Message("NoStuffsToBuildWith".Translate(), MessageTypeDefOf.RejectInput);
			}
			else
			{
				Find.WindowStack.Add(new FloatMenu(list) { vanishIfMouseDistant = true });
				Find.DesignatorManager.Select(this);
			}
		}

		public override void SelectedUpdate()
		{
			GenDraw.DrawNoBuildEdgeLines();
			if (!ArchitectCategoryTab.InfoRect.Contains(UI.MousePositionOnUIInverted))
			{
				IntVec3 mousePos = UI.MouseCell();
				if (mousePos.InBounds(Map))
					DrawGhost(CanDesignateCell(mousePos).Accepted ? new Color(0.5f, 1f, 0.6f, 0.4f) : new Color(1f, 0f, 0f, 0.4f));
			}
		}

		protected virtual void DrawGhost(Color ghostCol)
		{
			GhostDrawer.DrawGhostThing(UI.MouseCell(), Rot4.North, stuffDef, null, ghostCol, AltitudeLayer.Blueprint);
		}

		public override AcceptanceReport CanDesignateCell(IntVec3 cell)
		{
			DesignatorContext.designating = true;
			try
			{
				if (!CanReplaceStuffAt(stuffDef, cell, Map))
					return false;

				List<Thing> things = cell.GetThingList(Map);
				for (int i = 0; i < things.Count; i++)
				{
					if (things[i] is ReplaceFrame rf && rf.EntityToBuildStuff() == stuffDef)
						return false;
				}

				return true;
			}
			finally
			{
				DesignatorContext.designating = false;
			}
		}

		public static bool CanReplaceStuffAt(ThingDef stuff, IntVec3 cell, Map map)
		{
			List<Thing> things = cell.GetThingList(map);
			for (int i = 0; i < things.Count; i++)
			{
				if (CanReplaceStuffFor(stuff, things[i]))
				{
					return true;
				}
			}
			return false;
		}


		public static bool CanReplaceStuffFor(ThingDef stuff, Thing thing, ThingDef matchDef = null)
		{
			// Can't replace enemy items
			if (thing.Faction != Faction.OfPlayer && thing.Faction != null)
				return false;

			if (thing is Blueprint bp)
			{
				if (bp.EntityToBuildStuff() == stuff)
					return false;
			}
			else if (thing is Frame frame)
			{
				if (frame.EntityToBuildStuff() == stuff)
					return false;
			}
			else if (thing.def.HasReplaceFrame())
			{
				if (thing.Stuff == stuff) 
					return false;
			}
			else
			{
				return false; // Not a replaceable structure type
			}

			BuildableDef builtDef = GenConstruct.BuiltDefOf(thing.def);
			if (matchDef != null && builtDef != matchDef)
				return false;

			if (!GenConstruct.CanBuildOnTerrain(builtDef, thing.Position, thing.Map, thing.Rotation, thing, stuff))
				return false;

			if (thing.BeingReplacedByNewThing() != null)
				return false;

			if (!_allowedStuffCache.TryGetValue(builtDef, out var allowedSet))
			{
				allowedSet = new HashSet<ThingDef>(GenStuff.AllowedStuffsFor(builtDef));
				_allowedStuffCache[builtDef] = allowedSet;
			}

			return allowedSet.Contains(stuff);
		}

		public override void DesignateSingleCell(IntVec3 cell)
		{
			FindReplace(Map, cell, stuffDef);
		}

		// Credit: https://github.com/MemeGoddess/RimWorld-ReplaceStuff/pull/10
		public static void FindReplace(Map map, IntVec3 cell, ThingDef stuffDef)
		{
			Thing firstReplaceable = null;
			Thing firstBlueprintOrFrame = null;

			List<Thing> replaceables = cell.GetThingList(map);

			for (int i = 0; i < replaceables.Count; i++)
			{
				Thing replaceable = replaceables[i];

				if (!CanReplaceStuffFor(stuffDef, replaceable)) continue;

				firstReplaceable ??= replaceable;

				if (replaceable is Blueprint_Build || replaceable is Frame)
				{
					firstBlueprintOrFrame = replaceable;
					break;
				}
			}

			Thing thingToReplace = firstBlueprintOrFrame ?? firstReplaceable;

			if (thingToReplace != null)
			{
				DoReplace(thingToReplace, stuffDef);
			}
		}

		public static void DoReplace(Thing thing, ThingDef stuffDef)
		{
			var pos = thing.Position;
			var rot = thing.Rotation;
			var map = thing.Map;

			//In case you're replacing with a stuff that needs a higher affordance that bridges can handle.
			PlaceBridges.EnsureBridge.PlaceBridgeIfNeeded(thing.def, pos, map, rot, Faction.OfPlayer, stuffDef);

			//CanReplaceStuffFor has verified this is different stuff
			//so the task here is: place new replacements, kill old replacement
			//Too finicky to change stuff of current replacement - canceling jobs and such.
			if (thing is Blueprint_Build oldBP)
			{
				oldBP.Destroy(DestroyMode.Cancel);
				//Destroy before Place beacause GenSpawn.Spawn will wipe it

				GenConstruct.PlaceBlueprintForBuild(oldBP.def.entityDefToBuild, pos, map, rot, Faction.OfPlayer, stuffDef);
			}
			else if (thing is ReplaceFrame oldRF)
			{
				if (DebugSettings.godMode)
				{
					ReplaceFrame.FinalizeReplace(thing, stuffDef);
				}
				if (oldRF.oldStuff != stuffDef)
				{
					//replacement frame should keep deconstruction work mount
					ReplaceFrame newFrame = GenReplace.PlaceReplaceFrame(oldRF.oldThing, stuffDef);
					if (newFrame != null)
					{
						newFrame.workDone = Mathf.Min(oldRF.workDone, oldRF.WorkToDeconstruct);
					}
				}
				//else, if same stuff as old stuff, we just chose replace with original stuff, so we're already done - just destroy the frame.
				//upgrade frames/blueprints

				oldRF.Destroy(DestroyMode.Cancel);
			}
			else if (thing is Frame oldFrame)
			{
				oldFrame.Destroy(DestroyMode.Cancel);

				GenConstruct.PlaceBlueprintForBuild(oldFrame.def.entityDefToBuild, pos, map, rot, Faction.OfPlayer, stuffDef);
			}
			else if (DebugSettings.godMode)
			{
				ReplaceFrame.FinalizeReplace(thing, stuffDef);
			}
			else
			{
				//Oh of course the standard case is, just place a replace frame! I almost forgot about that.
				GenReplace.PlaceReplaceFrame(thing, stuffDef);
			}

			FleckMaker.ThrowMetaPuffs(GenAdj.OccupiedRect(pos, rot, thing.def.size), map);
		}

		public override void DrawPanelReadout(ref float curY, float width)
		{
			Widgets.InfoCardButton(width - 24f - 6f, 6f, stuffDef);
			Text.Font = GameFont.Tiny;
		}

		public override void RenderHighlight(List<IntVec3> dragCells)
		{
			DesignatorUtility.RenderHighlightOverSelectableCells(this, dragCells);
		}
	}
}
