﻿using Verse;
using HarmonyLib;
using RimWorld;

namespace ArtificialBeings
{
    // Non-humanlike intelligences don't gain or forget skills.
    [HarmonyPatch(typeof(SkillRecord), "Learn")]
    public static class Learn_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(float xp, bool direct, ref Pawn ___pawn)
        {
            if (ABF_Utils.IsConsideredNonHumanlike(___pawn))
            {
                return false;
            }
            return true;
        }
    }
}