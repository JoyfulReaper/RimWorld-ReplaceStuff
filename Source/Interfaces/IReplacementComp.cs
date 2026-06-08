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

using Replace_Stuff.Interfaces;
using System;

// Do not change this namespace needed for compatibility: namespace Replace_Stuff;
namespace Replace_Stuff;

/// <summary>
/// Allows other mods to implement IReplacementComp and run custom
/// actions during the replacement process, such as transferring
/// storage contents or preserving custom data.
/// </summary>

// Bad name, kept for compatability
[Obsolete("IReplacementComp is deprecated. Use IReplacementHandler instead.")]
public interface IReplacementComp : IReplacementHandler
{
    // By inheriting, anything that used to implement IReplacementComp 
    // still technically implements IReplacementHandler.
}