using RimWorld;
using Verse;

namespace ArtificialBeings
{
    [DefOf]
    public static class ABF_MentalStateDefOf
    {
        static ABF_MentalStateDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ABF_MentalStateDefOf));
        }

        [MayRequireSynstructsCore]
        public static MentalStateDef ABF_MentalState_Synstruct_Exterminator;
    }
}