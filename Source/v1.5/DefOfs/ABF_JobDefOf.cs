using Verse;
using RimWorld;

namespace ArtificialBeings
{
    [DefOf]
    public static class ABF_JobDefOf
    {
        static ABF_JobDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ABF_JobDefOf));
        }

        public static JobDef ABF_FulfillArtificialNeed;

        public static JobDef ABF_TendArtificial;

        [MayRequireSynstructsCore]
        public static JobDef ABF_GetRecharge;

        [MayRequireSynstructsCore]
        public static JobDef ABF_BuildCoherenceUrgent;

        [MayRequireSynstructsCore]
        public static JobDef ABF_BuildCoherenceIdle;

        [MayRequireSynstructsCore]
        public static JobDef ABF_ResurrectArtificial;

        [MayRequireMechcloudChemwalkers]
        public static JobDef ABF_AttuneToArmor;

        [MayRequireMechcloudChemwalkers]
        public static JobDef ABF_PackUpArmor;
    }
}