using Verse;
using RimWorld;

namespace ArtificialBeings
{
    [DefOf]
    public static class ABF_HediffDefOf
    {
        static ABF_HediffDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ABF_HediffDefOf));
        }
        public static HediffDef ABF_Hediff_Artificial_Incapacitated;

        public static HediffDef ABF_Hediff_Artificial_Disabled;

        public static HediffDef ABF_Hediff_Artificial_ComplexityRelation;

        [MayRequireSynstructsCore]
        public static HediffDef ABF_Hediff_Synstruct_RemainingCharge;

        [MayRequireSynstructsCore]
        public static HediffDef ABF_Hediff_Synstruct_Utility_InduciveVocalizer;

        [MayRequireSynstructsCore]
        public static HediffDef ABF_Hediff_Synstruct_Utility_CoerciveVocalizer;

        // Coherence Part failures

        [MayRequireSynstructsCore]
        public static HediffDef ABF_Hediff_Synstruct_Coherence_RustedPart;

        // Coherence effects

        [MayRequireSynstructsCore]
        public static HediffDef ABF_Hediff_Synstruct_Coherence_CriticalStage;

        [MayRequireSynstructsCore]
        public static HediffDef ABF_Hediff_Synstruct_Coherence_PoorStage;

        [MayRequireSynstructsCore]
        public static HediffDef ABF_Hediff_Synstruct_Coherence_SatisfactoryStage;
    }
}