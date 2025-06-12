using Verse;
using RimWorld;

namespace ArtificialBeings
{
    [DefOf]
    public static class ABF_LetterDefOf
    {
        static ABF_LetterDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ABF_LetterDefOf));
        }

        public static LetterDef ABF_Letter_Artificial_ReprogramDrone;

        [MayRequireSynstructsCore]
        public static LetterDef ABF_Letter_Synstruct_PersonalityShift;

        [MayRequireSynstructsCore]
        public static LetterDef ABF_Letter_Synstruct_PersonalityShiftRequest;
    }
}