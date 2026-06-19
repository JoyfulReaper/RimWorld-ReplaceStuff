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
using System.Reflection.Emit;
using UnityEngine;
using Verse;

namespace Replace_Stuff.Terrain.Patches;

//Graphic_Appearances doesn't pass renderQueue along.
//Fences blueprints uses Graphic_Appearances and nothing else does.
//Fix that bug so fences blueprints can render over fog

[HarmonyPatch(typeof(Graphic_Appearances), nameof(Graphic_Appearances.Init))]
public static class GraphicAppearancesPassData
{
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        MethodInfo GetInfo = AccessTools.Method(typeof(GraphicDatabase), nameof(GraphicDatabase.Get),
                parameters: new Type[] { typeof(string), typeof(Shader), typeof(Vector2), typeof(Color) },
                generics: new Type[] { typeof(Graphic_Single) });

        MethodInfo GetWithDataInfo = AccessTools.Method(typeof(GraphicDatabase), nameof(GraphicDatabase.Get),
                parameters: new Type[] { typeof(string), typeof(Shader), typeof(Vector2), typeof(Color), typeof(Color), typeof(GraphicData), typeof(string) },
                generics: new Type[] { typeof(Graphic_Single) });

        MethodInfo ColorWhiteInfo = AccessTools.Property(typeof(Color), nameof(Color.white)).GetGetMethod();
        FieldInfo dataInfo = AccessTools.Field(typeof(Graphic), nameof(Graphic.data));

        foreach (var inst in instructions)
        {
            if (inst.Calls(GetInfo))
            {
                // Create the first instruction of our injection sequence
                CodeInstruction firstInjected = new CodeInstruction(OpCodes.Call, ColorWhiteInfo);

                // migrate compiler labels to prevent stack corruption on jump branches
                if (inst.labels.Count > 0)
                {
                    firstInjected.labels.AddRange(inst.labels);
                    inst.labels.Clear();
                }

                yield return firstInjected;
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Ldfld, dataInfo);
                yield return new CodeInstruction(OpCodes.Ldnull);

                inst.operand = GetWithDataInfo;
                yield return inst;
            }
            else
            {
                yield return inst;
            }
        }
    }
}


// [HarmonyPatch(typeof(Graphic_Appearances), nameof(Graphic_Appearances.Init))]
// public static class GraphicAppearancesPassData
// {
//     //public override void Init(GraphicRequest req)
//     public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
//     {
//         //public static Graphic Get<T>(string path, Shader shader, Vector2 drawSize, Color color) where T : Graphic, new()
//         //public static Graphic Get<T>(string path, Shader shader, Vector2 drawSize, Color color, Color colorTwo, GraphicData data, string maskPath = null) where T : Graphic, new()

//         MethodInfo GetInfo = AccessTools.Method(typeof(GraphicDatabase), nameof(GraphicDatabase.Get),
//                 parameters: new Type[] { typeof(string), typeof(Shader), typeof(Vector2), typeof(Color) },
//                 generics: new Type[] { typeof(Graphic_Single) });
//         MethodInfo GetWithDataInfo = AccessTools.Method(typeof(GraphicDatabase), nameof(GraphicDatabase.Get),
//                 parameters: new Type[] { typeof(string), typeof(Shader), typeof(Vector2), typeof(Color), typeof(Color), typeof(GraphicData), typeof(string) },
//                 generics: new Type[] { typeof(Graphic_Single) });

//         //Color.white
//         MethodInfo ColorWhiteInfo = AccessTools.Property(typeof(Color), nameof(Color.white)).GetGetMethod();

//         //Graphic.data
//         FieldInfo dataInfo = AccessTools.Field(typeof(Graphic), nameof(Graphic.data));

//         foreach (var inst in instructions)
//         {
//             if(inst.Calls(GetInfo))
//             {
//                 //From:	GraphicDatabase.Get<Graphic_Single>(text + "/" + texture2D.name, req.shader, drawSize, color);
//                 //To:		GraphicDatabase.Get<Graphic_Single>(text + "/" + texture2D.name, req.shader, drawSize, color, Color.white, this.data, null);
//                 yield return new CodeInstruction(OpCodes.Call, ColorWhiteInfo);//Color.white (property getter)
//                 yield return new CodeInstruction(OpCodes.Ldarg_0);//this(Graphic_Appearances)
//                 yield return new CodeInstruction(OpCodes.Ldfld, dataInfo);//this.data(GraphicData)
//                 yield return new CodeInstruction(OpCodes.Ldnull);//null (string)
//                 inst.operand = GetWithDataInfo;//Override method...
//                 yield return inst;//Get(path, shader, drawSize, color, Color.white, this.data, null);
//             }
//             else
//                 yield return inst;
//         }
//     }
// }