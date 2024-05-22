using RimWorld;

namespace ArtificialBeings
{
    [DefOf]
    public static class ABF_StatDefOf
    {
        static ABF_StatDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ABF_StatDefOf));
        }

        public static StatDef ABF_ArtificialSurgerySuccessChance;

        public static StatDef ABF_ArtificialTendQuality;

        public static StatDef ABF_ArtificialTendSpeed;

        public static StatDef ABF_ArtificialTendQualityOffset;

        public static StatDef ABF_ArtificialSurgerySuccessChanceFactor;

        public static StatDef ABF_ComplexityLimit;

        public static StatDef ABF_SkillLimit;

        [MayRequireSynstructsCore]
        public static StatDef ABF_CoherenceRetention;

        [MayRequireSynstructsCore]
        public static StatDef ABF_ChargingSpeed;

        [MayRequireSynstructsCore]
        public static StatDef ABF_NutritionalIntakeEfficiency;
    }
}