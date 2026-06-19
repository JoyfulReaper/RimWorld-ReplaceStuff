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

namespace Replace_Stuff.Compatibility
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ReplacementRuleAttribute : Attribute
    {
        // Optionally add a Priority field if you want to control order
        public int Priority { get; }

        public ReplacementRuleAttribute(int priority = 0) =>
            Priority = priority;
    }
}
