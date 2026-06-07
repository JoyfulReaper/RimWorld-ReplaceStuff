/*
 * REPLACE STUFF: Perfomance Edition
 * 
 * 
 * Part of this code is based on Replace Stuff
 * Copyright (c) 2024 Alex Tearse-Doyle
 * Licensed under the MIT License.
 *
 * Modified by Kyle Givler
 * Copyright (c) 2026 Kyle Givler
 * Licensed under the MIT License.
 */

using HarmonyLib;
using Replace_Stuff.Utilities;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace Replace_Stuff.DestroyedRestore
{
    [HarmonyPatch(typeof(ThingUtility), nameof(ThingUtility.CheckAutoRebuildOnDestroyed))]
    static class SaveDestroyedBuildings
    {
        //public static void CheckAutoRebuildOnDestroyed(Thing thing, DestroyMode mode, Map map, BuildableDef buildingDef)
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo PlaceBlueprintForBuildInfo = AccessTools.Method(typeof(GenConstruct), nameof(GenConstruct.PlaceBlueprintForBuild));

            foreach (CodeInstruction i in instructions)
            {
                yield return i;
                if (i.Calls(PlaceBlueprintForBuildInfo))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);//Thing thing
                    yield return new CodeInstruction(OpCodes.Ldarg_2);//Map map
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DestroyedBuildings), nameof(DestroyedBuildings.SaveBuilding)));//SaveBuilding(thing)
                }
            }
        }
    }

    public class DestroyedBuildings : MapComponent
    {
        public Dictionary<IntVec3, ReplaceData> destroyedBuildings;
        //Actually want this to be deep-ref since it's despawned!

        public DestroyedBuildings(Map map) : base(map)
        {
            destroyedBuildings = new Dictionary<IntVec3, ReplaceData>();
        }

        public override void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                foreach (IntVec3 pos in destroyedBuildings.Keys.ToList())
                {
                    if (!pos.GetThingList(map).Any(t => t.def.IsFrame || t.def.IsBlueprint))
                    {
                        Verse.Log.Warning(
                            $"[ReplaceStuffPerfomance] - Forgetting orphaned building state at {pos}");
                        destroyedBuildings.Remove(pos);
                    }
                }
            }

            Scribe_Collections.Look(ref destroyedBuildings, "destroyedBuildings", LookMode.Value, LookMode.Deep);
        }

        public static void SaveBuilding(Thing thing, Map map)
        {
            ReplaceData data = BuildingStateTransfer.Capture(thing);
            DestroyedBuildings comp = map.GetComponent<DestroyedBuildings>();
            comp.destroyedBuildings[thing.Position] = data;
            thing.ForceSetStateToUnspawned();
        }

        public static void ReviveBuilding(Thing newBuilding, IntVec3 pos, Map map)
        {
            DestroyedBuildings comp = map.GetComponent<DestroyedBuildings>();

            if (comp.destroyedBuildings.TryGetValue(pos, out ReplaceData data))
            {
                BuildingStateTransfer.Apply(data, newBuilding);
                comp.destroyedBuildings.Remove(pos);
            }
        }

        public static void RemoveAt(IntVec3 pos, Map map)
        {
            DestroyedBuildings comp = map.GetComponent<DestroyedBuildings>();

            if (comp.destroyedBuildings.Remove(pos))
            {
                RSLog.Debug($"Forgetting destroyed building state at {pos}");
            }
        }
    }
}
