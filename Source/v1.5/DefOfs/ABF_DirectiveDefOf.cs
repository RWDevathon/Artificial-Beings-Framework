using RimWorld;

namespace ArtificialBeings
{
    [DefOf]
    public static class ABF_DirectiveDefOf
    {
        static ABF_DirectiveDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ABF_DirectiveDefOf));
        }

        [MayRequireSynstructsCore]
        public static DirectiveDef ABF_DirectiveArtisan;

        [MayRequireSynstructsCore]
        public static DirectiveDef ABF_DirectiveMartyrdom;

        [MayRequireSynstructsCore]
        public static DirectiveDef ABF_DirectiveBerserker;

        [MayRequireSynstructsCore]
        public static DirectiveDef ABF_DirectiveAmicability;
    }
}
