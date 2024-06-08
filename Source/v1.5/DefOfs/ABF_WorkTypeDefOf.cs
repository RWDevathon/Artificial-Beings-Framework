using Verse;
using RimWorld;

namespace ArtificialBeings
{
    [DefOf]
    public static class ABF_WorkTypeDefOf
    {
        static ABF_WorkTypeDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ABF_WorkTypeDefOf));
        }

        public static WorkTypeDef ABF_WorkType_Artificial_Artificer;
    }
}