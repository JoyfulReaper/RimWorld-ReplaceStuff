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

using System;

namespace Replace_Stuff.Compatibility;

[AttributeUsage(AttributeTargets.Class)]
public class ReplacementHandlerAttribute : Attribute
{
    public string TargetCompName { get; }
    public int Priority { get; set; } // Higher number = higher priority

    public ReplacementHandlerAttribute(string targetCompName, int priority = 0)
    {
        TargetCompName = targetCompName;
        Priority = priority;
    }
}