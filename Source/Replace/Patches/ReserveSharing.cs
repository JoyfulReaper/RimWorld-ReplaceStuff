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
using Replace_Stuff.NewThing;
using RimWorld;
using System;
using System.Linq;
using System.Reflection;
using Verse;
using Verse.AI;

namespace Replace_Stuff.Replace.Patches;

/// <summary>
/// Provides a global reservation bypass to synchronize AI interactions between 
/// placeholder frames and the underlying structures they replace.
/// </summary>
/// <remarks>
/// <para>
/// In RimWorld's AI, a <see cref="ReservationManager"/> prevents multiple pawns 
/// from interacting with the same object simultaneously. When a player queues a 
/// replacement task, the game generates a new frame atop the old building. 
/// </para>
/// <para>
/// Without this class, pawns might reserve the new frame while others reserve 
/// the old building, leading to AI task cancellation, pathfinding loops, or 
/// structural deadlocks.
/// </para>
/// <para>
/// This class intercepts all <see cref="ReservationManager"/> methods that accept 
/// a <see cref="LocalTargetInfo"/> via a Harmony <see cref="HarmonyMethod.Prefix"/>. 
/// It performs a target redirection, effectively "tricking" the AI into 
/// interacting with the original <see cref="Thing"/> regardless of whether 
/// they are targeting the <see cref="ReplaceFrame"/> or the underlying 
/// construction <see cref="Frame"/>.
/// </para>
/// </remarks>

public static class ReserveSharing
{
    /// <summary>
    /// Dynamically registers Harmony patches for all methods in <see cref="ReservationManager"/> 
    /// that handle <see cref="LocalTargetInfo"/>.
    /// </summary>
    /// <param name="harmony">The active <see cref="Harmony"/> instance for this mod.</param>
    public static void Initialize(Harmony harmony)
    {
        HarmonyMethod prefix = new HarmonyMethod(typeof(ReserveSharing), nameof(ReserveSharing.Prefix_CanReserve));

        foreach (MethodInfo method in AccessTools.GetDeclaredMethods(typeof(ReservationManager)))
        {
            if (method.GetParameters().Any(t => t.ParameterType == typeof(LocalTargetInfo)))
            {
                if (method.IsGenericMethod)
                    harmony.Patch(method.MakeGenericMethod(new Type[] { typeof(JobDriver_TakeToBed) }), prefix, null);
                else
                    harmony.Patch(method, prefix, null);
            }
        }
    }

    //public bool CanReserve(Pawn claimant, LocalTargetInfo target, int maxPawns = 1, int stackCount = -1, ReservationLayerDef layer = null, bool ignoreOtherReservations = false)
    /// <summary>
    /// Intercepts and redirects reservation targets from frames back to their 
    /// source building.
    /// </summary>
    /// <param name="target">The <see cref="LocalTargetInfo"/> being requested for reservation.</param>
    public static void Prefix_CanReserve(ref LocalTargetInfo target)//Luckily each parameter is named 'target' or this wouldn't work
    {
        if (!target.IsValid || !target.HasThing)
            return;

        if (target.Thing is ReplaceFrame replaceFrame)
            target = replaceFrame.oldThing;
        else if (target.Thing is Frame frame && frame.IsNewThingReplacement(out Thing oldThing))
            target = oldThing;
    }
}
