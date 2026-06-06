/*
 Copyright (c) [2025] [Alex Tearse-Doyle]
Contributions for Performance Edtion: Kyle Givler
Other known Contributors: MemeGoddess, Hexnet111, 

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
 */

using System;
using System.Linq;
using System.Reflection;
using RimWorld;
using Verse;
using Verse.AI;
using HarmonyLib;
using Replace_Stuff.NewThing;

namespace Replace_Stuff.Replace
{
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
            HarmonyMethod prefix = new HarmonyMethod(typeof(ReserveSharing), nameof(ReserveSharing.Prefix));

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
        public static void Prefix(ref LocalTargetInfo target)//Luckily each parameter is named 'target' or this wouldn't work
		{
			if (!target.IsValid || !target.HasThing)
				return;

			if (target.Thing is ReplaceFrame replaceFrame)
				target = replaceFrame.oldThing;
			else if (target.Thing is Frame frame && frame.IsNewThingReplacement(out Thing oldThing))
				target = oldThing;
		}
	}
}
