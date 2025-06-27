using RimWorld;
using Verse;

namespace ArtificialBeings
{
    [DefOf]
    public static class ABF_NeedDefOf
    {
        static ABF_NeedDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ABF_NeedDefOf));
        }

        [MayRequireSynstructsCore]
        public static NeedDef ABF_Need_Synstruct_Energy;
    }
}