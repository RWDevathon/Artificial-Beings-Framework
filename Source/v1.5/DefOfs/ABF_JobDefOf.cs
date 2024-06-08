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

        public static JobDef ABF_Job_Artificial_FulfillNeed;

        public static JobDef ABF_Job_Artificial_Tend;

        [MayRequireSynstructsCore]
        public static JobDef ABF_Job_Synstruct_ChargeSelf;

        [MayRequireSynstructsCore]
        public static JobDef ABF_Job_Synstruct_BuildCoherenceUrgently;

        [MayRequireSynstructsCore]
        public static JobDef ABF_Job_Synstruct_BuildCoherenceIdly;

        [MayRequireSynstructsCore]
        public static JobDef ABF_Job_Synstruct_ResurrectArtificial;

        [MayRequireMechcloudChemwalkers]
        public static JobDef ABF_Job_Chemwalker_AttuneToArmor;

        [MayRequireMechcloudChemwalkers]
        public static JobDef ABF_Job_Chemwalker_PackUpArmor;
    }
}