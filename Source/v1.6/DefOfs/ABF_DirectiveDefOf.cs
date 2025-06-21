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
        public static DirectiveDef ABF_Directive_Synstruct_Martyr;

        [MayRequireSynstructsCore]
        public static DirectiveDef ABF_Directive_Synstruct_Berserker;

        [MayRequireSynstructsCore]
        public static DirectiveDef ABF_Directive_Synstruct_Friend;
    }
}
