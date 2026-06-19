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
using Replace_Stuff.Core;
using RimWorld;
using System;
using System.Linq;
using System.Reflection;
using Verse;
using Verse.AI;

namespace Replace_Stuff.Replace.Patches;

/// <summary>
/// Patches <see cref="ReservationManager"/> so replacement frames and
/// the structures they replace are treated as the same reservation target.
/// </summary>
/// <remarks>
/// Replace Stuff allows a replacement frame to coexist with the original
/// building during construction. Since RimWorld reserves individual
/// <see cref="Thing"/> instances, pawns could otherwise reserve the frame
/// and building independently, resulting in conflicting jobs.
///
/// This patch redirects reservation requests targeting replacement
/// frames back to the original structure before vanilla reservation
/// logic executes.
/// </remarks>

public static class Patch_ReservationManager
{
    /// <summary>
    /// Applies reservation-target redirection patches to
    /// <see cref="ReservationManager"/> methods that operate on
    /// <see cref="LocalTargetInfo"/>.
    /// </summary>
    /// <param name="harmony">
    /// The Harmony instance used to apply the patches.
    /// </param>
    /// <remarks>
    /// Generic methods are patched using a concrete
    /// <see cref="JobDriver"/> specialization because Harmony cannot
    /// patch open generic methods directly.
    /// </remarks>
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
    /// Redirects replacement-frame reservation targets to the structure
    /// being replaced.
    /// </summary>
    /// <param name="target">
    /// The reservation target being evaluated.
    /// </param>
    /// <remarks>
    /// If the target is a <see cref="ReplacementFrame"/> or a replacement
    /// frame created through the new-thing replacement system, the target
    /// is replaced with the original structure before reservation checks
    /// occur.
    /// </remarks>
    // public bool CanReserve(Pawn claimant, LocalTargetInfo target, int maxPawns = 1, int stackCount = -1, ReservationLayerDef layer = null, bool ignoreOtherReservations = false)
    public static void Prefix(ref LocalTargetInfo target)
    {
        if (!target.IsValid || !target.HasThing)
            return;

        if (target.Thing is ReplacementFrame replaceFrame)
            target = replaceFrame.TargetThing;
        else if (target.Thing is Frame frame && frame.TryFindTarget(out Thing oldThing))
            target = oldThing;
    }
}