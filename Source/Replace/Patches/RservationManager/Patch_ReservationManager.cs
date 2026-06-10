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
using Replace_Stuff.NewThing;
using RimWorld;
using System;
using System.Linq;
using System.Reflection;
using Verse;
using Verse.AI;

namespace Replace_Stuff.Replace.Patches.RservationManager;

/// <summary>
/// Patches <see cref="ReservationManager"/> so replacement frames and their
/// underlying buildings share a single reservation target.
/// </summary>
/// <remarks>
/// <para>
/// Replace Stuff allows a building to remain in place while a
/// <see cref="ReplacementFrame"/> occupies the same location during
/// construction. From the game's perspective these are two separate
/// <see cref="Thing"/> instances.
/// </para>
/// <para>
/// RimWorld's reservation system operates on individual Things. Without
/// intervention, one pawn could reserve the replacement frame while
/// another reserves the original building, causing inconsistent AI
/// behaviour and job conflicts.
/// </para>
/// <para>
/// Rather than patching individual reservation methods, this class
/// automatically patches every <see cref="ReservationManager"/> method
/// that accepts a <see cref="LocalTargetInfo"/> parameter. Whenever a
/// reservation request targets a replacement frame, the target is
/// redirected to the original building before vanilla reservation logic
/// executes.
/// </para>
/// <para>
/// This effectively makes replacement frames transparent to the
/// reservation system while preserving vanilla reservation behaviour.
/// </para>
/// </remarks>

public static class Patch_ReservationManager
{
    /// <summary>
    /// Registers prefix patches for every ReservationManager method that
    /// operates on a <see cref="LocalTargetInfo"/>.
    /// </summary>
    /// <remarks>
    /// Generic ReservationManager methods cannot be patched directly.
    /// Harmony requires a concrete generic instantiation, so
    /// <see cref="JobDriver_TakeToBed"/> is used as an arbitrary
    /// <see cref="JobDriver"/> specialization to construct a valid
    /// runtime method for patching.
    /// </remarks>
    /// <param name="harmony">
    /// The Harmony instance responsible for applying the patches.
    /// </param>
    public static void Initialize(Harmony harmony)
    {
        // Generic ReservationManager methods cannot be patched directly.
        // Harmony requires a concrete generic instantiation, so
        // JobDriver_TakeToBed is used as a valid JobDriver specialization
        // when creating the runtime method to patch.
        var prefix = new HarmonyMethod(typeof(Patch_ReservationManager), nameof(Patch_ReservationManager.Prefix));

        foreach (MethodInfo method in AccessTools.GetDeclaredMethods(typeof(ReservationManager)))
        {
            // Patch every ReservationManager overload that takes a LocalTargetInfo.
            // The Prefix only declares a ref LocalTargetInfo target parameter;
            // Harmony automatically injects it into compatible methods.
            if (method.GetParameters().Any(t => t.ParameterType == typeof(LocalTargetInfo)))
            {
                if (method.IsGenericMethod)
                    harmony.Patch(method.MakeGenericMethod(new Type[] { typeof(JobDriver_TakeToBed) }), prefix, null);
                else
                    harmony.Patch(method, prefix, null);
            }
        }
    }


    /// <summary>
    /// Redirects reservation targets from replacement frames back to the
    /// original building before ReservationManager performs its checks.
    /// </summary>
    /// <remarks>
    /// ReservationManager consistently passes the target through a
    /// <see cref="LocalTargetInfo"/> parameter. By modifying that
    /// parameter by reference, every patched reservation method
    /// transparently operates on the original building instead of the
    /// temporary replacement frame.
    /// </remarks>
    /// <param name="target">
    /// The reservation target being evaluated.
    /// </param>

    // public bool CanReserve(Pawn claimant, LocalTargetInfo target, int maxPawns = 1, int stackCount = -1, ReservationLayerDef layer = null, bool ignoreOtherReservations = false)
    public static void Prefix(ref LocalTargetInfo target)
    {
        if (!target.IsValid || !target.HasThing)
            return;

        if (target.Thing is ReplacementFrame replaceFrame)
            target = replaceFrame.targetThing;
        else if (target.Thing is Frame frame && frame.IsNewThingReplacement(out Thing oldThing))
            target = oldThing;
    }
}