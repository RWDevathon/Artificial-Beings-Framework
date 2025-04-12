using RimWorld;

namespace ArtificialBeings
{
    [DefOf]
    public static class ABF_AbilityDefOf
    {
        static ABF_AbilityDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ABF_AbilityDefOf));
        }

        // Used in Mechcloud Chemwalkers, but is from Royalty.
        [MayRequireRoyalty]
        public static AbilityDef Stun;
    }
}
