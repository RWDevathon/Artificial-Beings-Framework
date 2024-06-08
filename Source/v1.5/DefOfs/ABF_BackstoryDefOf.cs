using RimWorld;

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
        public static BackstoryDef ABF_Backstory_Synstruct_Childhood_Blank;

        [MayRequireSynstructsCore]
        public static BackstoryDef ABF_Backstory_Synstruct_Adulthood_Blank;
    }
}