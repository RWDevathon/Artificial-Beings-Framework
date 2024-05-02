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

        public static LetterDef ABF_ReprogramDroneLetter;

        [MayRequireSynstructsCore]
        public static LetterDef ABF_PersonalityShiftLetter;

        [MayRequireSynstructsCore]
        public static LetterDef ABF_PersonalityShiftRequestLetter;
    }
}