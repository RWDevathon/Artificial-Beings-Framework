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

        public static StatDef ABF_Stat_Artificial_SurgerySuccessChance;

        public static StatDef ABF_Stat_Artificial_TendQuality;

        public static StatDef ABF_Stat_Artificial_TendSpeed;

        public static StatDef ABF_Stat_Artificial_TendQualityOffset;

        public static StatDef ABF_Stat_Artificial_SurgerySuccessChanceFactor;

        public static StatDef ABF_Stat_Artificial_ComplexityLimit;

        public static StatDef ABF_Stat_Artificial_SkillLimit;

        [MayRequireSynstructsCore]
        public static StatDef ABF_Stat_Synstruct_CoherenceRetention;

        [MayRequireSynstructsCore]
        public static StatDef ABF_Stat_Synstruct_MaxEnergy;

        [MayRequireSynstructsCore]
        public static StatDef ABF_Stat_Synstruct_EnergyConsumption;
    }
}