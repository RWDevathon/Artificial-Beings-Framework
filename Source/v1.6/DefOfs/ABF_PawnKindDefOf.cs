using Verse;
using RimWorld;

namespace ArtificialBeings
{
    [DefOf]
    public static class ABF_PawnKindDefOf
    {
        static ABF_PawnKindDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ABF_PawnKindDefOf));
        }

        [MayRequireMartialSynstructs]
        public static PawnKindDef ABF_PawnKind_Synstruct_Jellyman_QuestCrash;

        [MayRequireMartialSynstructs]
        public static PawnKindDef ABF_PawnKind_Synstruct_Juggernaut_Calldown;
    }
}