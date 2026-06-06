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
using Replace_Stuff.Replace;
using System.Collections.Generic;
using Verse;
using RimWorld;

namespace Replace_Stuff.Patches
{
    /// <summary>
    /// Provides Harmony patches for the base <see cref="Frame"/> class to inject 
    /// custom behavior for <see cref="ReplaceFrame"/> instances.
    /// </summary>
    [HarmonyPatch(typeof(Frame))]
    internal static class FramePatches
    {
        /// <summary>
        /// Intercepts requests for material costs to ensure the game uses 
        /// <see cref="ReplaceFrame.TotalMaterialCost"/> for replacement tasks.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Frame.TotalMaterialCost))]
        public static bool Prefix_TotalMaterialCost(Frame __instance, ref List<ThingDefCountClass> __result)
        {
            if (__instance is ReplaceFrame rf)
            {
                __result = rf.TotalMaterialCost();
                return false;
            }
            return true;
        }

        /// <summary>
        /// Redirects construction completion to <see cref="ReplaceFrame.CompleteConstruction"/>,
        /// handling the destruction of the old object and spawning of the new one.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Frame.CompleteConstruction))]
        public static bool Prefix_CompleteConstruction(Frame __instance, Pawn worker)
        {
            if (__instance is ReplaceFrame rf)
            {
                rf.CompleteConstruction(worker);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Redirects construction failure to <see cref="ReplaceFrame.FailConstruction"/>,
        /// ensuring partial material refunds for failed replacements.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Frame.FailConstruction))]
        public static bool Prefix_FailConstruction(Frame __instance, Pawn worker)
        {
            if (__instance is ReplaceFrame rf)
            {
                rf.FailConstruction(worker);
                return false;
            }
            return true;
        }


        /// <summary>
        /// Updates the work progress bar by intercepting the getter for <see cref="Frame.WorkToBuild"/>,
        /// returning the combined deconstruction and construction labor required for replacements.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Frame.WorkToBuild), MethodType.Getter)]
        public static bool Prefix_WorkToBuild(Frame __instance, ref float __result)
        {
            if (__instance is ReplaceFrame rf)
            {
                __result = rf.WorkToBuild;
                return false;
            }
            return true;
        }
    }
}