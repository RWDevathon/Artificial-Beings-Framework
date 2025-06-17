using RimWorld;

namespace ArtificialBeings
{
    [DefOf]
    public static class ABF_FleshTypeDefOf
    {
        static ABF_FleshTypeDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ABF_FleshTypeDefOf));
        }

        [MayRequireMechcloudChemwalkers]
        public static FleshTypeDef ABF_FleshType_Chemwalker_Base;
    }
}