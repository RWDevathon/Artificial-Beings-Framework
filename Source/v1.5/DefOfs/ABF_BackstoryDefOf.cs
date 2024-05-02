﻿using RimWorld;

namespace ArtificialBeings
{
    [DefOf]
    public static class ABF_BackstoryDefOf
    {
        static ABF_BackstoryDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ABF_BackstoryDefOf));
        }

        [MayRequireSynstructsCore]
        public static BackstoryDef ABF_MechChildhoodFreshBlank;

        [MayRequireSynstructsCore]
        public static BackstoryDef ABF_MechAdulthoodBlank;

        [MayRequireSynstructsCore]
        public static BackstoryDef ABF_NewbootChildhood;

        [MayRequireSynstructsCore]
        public static BackstoryDef ABF_NewbootAdulthood;
    }
}