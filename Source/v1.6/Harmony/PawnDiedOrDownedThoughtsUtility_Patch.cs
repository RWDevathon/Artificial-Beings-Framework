﻿using Verse;
using HarmonyLib;
using RimWorld;
using System;

namespace ArtificialBeings
{
    public class PawnDiedOrDownedThoughtsUtility_Patch
    {
        // Do not give mood thoughts about non-humanlike intelligences dying.
        [HarmonyPatch(typeof(PawnDiedOrDownedThoughtsUtility), "TryGiveThoughts")]
        [HarmonyPatch(new Type[] {typeof(Pawn), typeof(DamageInfo?), typeof(PawnDiedOrDownedThoughtsKind)})]
        public class TryGiveThoughts_Patch
        {
            [HarmonyPrefix]
            public static bool Listener(ref Pawn victim)
            { 
                return !ABF_Utils.IsConsideredNonHumanlike(victim);
            }
        }
    }
}