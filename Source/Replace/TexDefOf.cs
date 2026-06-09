/*
 * REPLACE STUFF: Perfomance Edition
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

using UnityEngine;
using Verse;

namespace Replace_Stuff.Replace
{
    [StaticConstructorOnStartup]
    public static class TexDefOf
    {
        public static Texture2D replaceIcon = ContentFinder<Texture2D>.Get("ReplaceStuff", true);
    }
}