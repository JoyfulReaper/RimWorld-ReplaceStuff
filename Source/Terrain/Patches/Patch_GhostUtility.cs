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
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;

namespace Replace_Stuff.Terrain.Patches;

//Cursor
[HarmonyPatch(typeof(GhostUtility), "GhostGraphicFor")]
public static class ShowGhostOverFog
{
    public const int queueOverFog = 3176;

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        // Overload 1: 4-parameters (Linked items / Doors)
        MethodInfo Get4Info = AccessTools.Method(typeof(GraphicDatabase), nameof(GraphicDatabase.Get),
            new Type[] { typeof(string), typeof(Shader), typeof(Vector2), typeof(Color) },
            new Type[] { typeof(Graphic_Single) });
        MethodInfo Get4Replacement = AccessTools.Method(typeof(ShowGhostOverFog), nameof(GetRenderHigh4));

        // Overload 2: 6-parameters (Standard items branch B)
        MethodInfo Get6Info = AccessTools.Method(typeof(GraphicDatabase), nameof(GraphicDatabase.Get),
            new Type[] { typeof(string), typeof(Shader), typeof(Vector2), typeof(Color), typeof(Color), typeof(GraphicData) },
            new Type[] { typeof(Graphic_Single) });
        MethodInfo Get6Replacement = AccessTools.Method(typeof(ShowGhostOverFog), nameof(GetRenderHigh6));

        // Overload 3: 8-parameters non-generic (Standard items branch A)
        MethodInfo Get8Info = AccessTools.Method(typeof(GraphicDatabase), nameof(GraphicDatabase.Get),
            new Type[] { typeof(Type), typeof(string), typeof(Shader), typeof(Vector2), typeof(Color), typeof(Color), typeof(GraphicData), typeof(List<ShaderParameter>) });
        MethodInfo Get8Replacement = AccessTools.Method(typeof(ShowGhostOverFog), nameof(GetRenderHigh8));

        // Chain the replacements together cleanly
        var wrappedInstructions = HarmonyLib.Transpilers.MethodReplacer(instructions, Get4Info, Get4Replacement);
        wrappedInstructions = HarmonyLib.Transpilers.MethodReplacer(wrappedInstructions, Get6Info, Get6Replacement);
        return HarmonyLib.Transpilers.MethodReplacer(wrappedInstructions, Get8Info, Get8Replacement);
    }

    // Replacement for 4-param call
    public static Graphic GetRenderHigh4(string path, Shader shader, Vector2 drawSize, Color color)
    {
        return GraphicDatabase.Get<Graphic_Single>(path, shader, drawSize, color, queueOverFog);
    }

    // Replacement for 6-param call (Ensures GraphicData isn't null and applies renderQueue)
    public static Graphic GetRenderHigh6(string path, Shader shader, Vector2 drawSize, Color color, Color colorTwo, GraphicData data)
    {
        data ??= new GraphicData();
        data.renderQueue = queueOverFog;
        return GraphicDatabase.Get<Graphic_Single>(path, shader, drawSize, color, colorTwo, data);
    }

    // Replacement for 8-param call (Ensures GraphicData isn't null and applies renderQueue)
    public static Graphic GetRenderHigh8(Type graphicType, string path, Shader shader, Vector2 drawSize, Color color, Color colorTwo, GraphicData data, List<ShaderParameter> shaderParameters)
    {
        data ??= new GraphicData();
        data.renderQueue = queueOverFog;
        return GraphicDatabase.Get(graphicType, path, shader, drawSize, color, colorTwo, data, shaderParameters);
    }
}