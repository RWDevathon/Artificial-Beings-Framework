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
        public static HediffDef ABF_Incapacitated;

        public static HediffDef ABF_Disabled;

        public static HediffDef ABF_ComplexityRelation;

        [MayRequireSynstructsCore]
        public static HediffDef ABF_StasisPill;

        [MayRequireSynstructsCore]
        public static HediffDef ABF_RemainingCharge;

        [MayRequireSynstructsCore]
        public static HediffDef ABF_AutomodulatedVoiceSynthesizer;

        [MayRequireSynstructsCore]
        public static HediffDef ABF_CoerciveVoiceSynthesizer;

        // Coherence Part failures

        [MayRequireSynstructsCore]
        public static HediffDef ABF_RustedPart;

        // Coherence effects

        [MayRequireSynstructsCore]
        public static HediffDef ABF_CoherenceCritical;

        [MayRequireSynstructsCore]
        public static HediffDef ABF_CoherencePoor;

        [MayRequireSynstructsCore]
        public static HediffDef ABF_CoherenceSatisfactory;
    }
}