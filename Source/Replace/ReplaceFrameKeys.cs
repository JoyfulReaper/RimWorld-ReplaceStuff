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
using Verse;

namespace Replace_Stuff.Replace;

// Define our own struct to act as the Dictionary key
public readonly struct ReplaceFrameKey(BuildableDef buildable, ThingDef stuff) : IEquatable<ReplaceFrameKey>
{
    public readonly BuildableDef Buildable = buildable;
    public readonly ThingDef Stuff = stuff;

    public bool Equals(ReplaceFrameKey other) =>
        Buildable == other.Buildable && Stuff == other.Stuff;

    public override bool Equals(object obj) =>
        obj is ReplaceFrameKey other && Equals(other);

    public override int GetHashCode() =>
        Gen.HashCombine(Buildable?.GetHashCode() ?? 0, Stuff?.GetHashCode() ?? 0);
}