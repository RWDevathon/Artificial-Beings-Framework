using RimWorld;

namespace ArtificialBeings
{
    [DefOf]
    public static class ABF_TraitDefOf
    {
        static ABF_TraitDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ABF_TraitDefOf));
        }

        [MayRequireSynstructsCore]
        public static TraitDef ABF_Trait_Synstruct_BiologicalPreference;
    }
}