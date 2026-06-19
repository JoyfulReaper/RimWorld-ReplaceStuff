/*
 * REPLACE STUFF: Performance Edition
 * 
 * 
 * Part of this code is based on Replace Stuff
 * Copyright (c) 2025 Alex Tearse-Doyle
 * Licensed under the MIT License.
 *
 * Modified by Kyle Givler
 * Copyright (c) 2026 Kyle Givler
 * Licensed under the MIT License.
 */

using HarmonyLib;
using Replace_Stuff.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;

namespace Replace_Stuff.Terrain.Patches;

[HarmonyPatch(typeof(GhostUtility), "GhostGraphicFor")]
public static class ShowGhostOverFog
{
    public const int queueOverFog = 3176;

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        // Overload 1: 5-parameters (Added int renderQueue in recent RW updates)
        MethodInfo Get5Info = AccessTools.Method(typeof(GraphicDatabase), nameof(GraphicDatabase.Get),
            new Type[] { typeof(string), typeof(Shader), typeof(Vector2), typeof(Color), typeof(int) },
            new Type[] { typeof(Graphic_Single) });
        MethodInfo Get5Replacement = AccessTools.Method(typeof(ShowGhostOverFog), nameof(GetRenderHigh5));

        // Overload 2: 7-parameters (Added string maskingPath)
        MethodInfo Get7Info = AccessTools.Method(typeof(GraphicDatabase), nameof(GraphicDatabase.Get),
            new Type[] { typeof(string), typeof(Shader), typeof(Vector2), typeof(Color), typeof(Color), typeof(GraphicData), typeof(string) },
            new Type[] { typeof(Graphic_Single) });
        MethodInfo Get7Replacement = AccessTools.Method(typeof(ShowGhostOverFog), nameof(GetRenderHigh7));

        // Overload 3: 9-parameters non-generic (Added string maskingPath)
        MethodInfo Get9Info = AccessTools.Method(typeof(GraphicDatabase), nameof(GraphicDatabase.Get),
            new Type[] { typeof(Type), typeof(string), typeof(Shader), typeof(Vector2), typeof(Color), typeof(Color), typeof(GraphicData), typeof(List<ShaderParameter>), typeof(string) });
        MethodInfo Get9Replacement = AccessTools.Method(typeof(ShowGhostOverFog), nameof(GetRenderHigh9));

        // Safety Catch: Prevent hard crashes if RimWorld updates signatures again
        if (Get5Info == null || Get7Info == null || Get9Info == null)
        {
            RSLog.Error("ShowGhostOverFog Transpiler failed to find GraphicDatabase.Get targets. Signatures may have changed!");
            return instructions;
        }

        // Chain the replacements together cleanly
        var wrappedInstructions = HarmonyLib.Transpilers.MethodReplacer(instructions, Get5Info, Get5Replacement);
        wrappedInstructions = HarmonyLib.Transpilers.MethodReplacer(wrappedInstructions, Get7Info, Get7Replacement);
        return HarmonyLib.Transpilers.MethodReplacer(wrappedInstructions, Get9Info, Get9Replacement);
    }

    public static Graphic GetRenderHigh5(string path, Shader shader, Vector2 drawSize, Color color, int renderQueue)
    {
        return GraphicDatabase.Get<Graphic_Single>(path, shader, drawSize, color, queueOverFog);
    }

    public static Graphic GetRenderHigh7(string path, Shader shader, Vector2 drawSize, Color color, Color colorTwo, GraphicData data, string maskingPath)
    {
        data ??= new GraphicData();
        data.renderQueue = queueOverFog;
        return GraphicDatabase.Get<Graphic_Single>(path, shader, drawSize, color, colorTwo, data, maskingPath);
    }

    public static Graphic GetRenderHigh9(Type graphicType, string path, Shader shader, Vector2 drawSize, Color color, Color colorTwo, GraphicData data, List<ShaderParameter> shaderParameters, string maskingPath)
    {
        data ??= new GraphicData();
        data.renderQueue = queueOverFog;
        return GraphicDatabase.Get(graphicType, path, shader, drawSize, color, colorTwo, data, shaderParameters, maskingPath);
    }
}