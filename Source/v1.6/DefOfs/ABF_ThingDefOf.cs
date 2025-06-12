using RimWorld;
using Verse;

namespace ArtificialBeings
{
    [DefOf]
    public static class ABF_ThingDefOf
    {
        static ABF_ThingDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ABF_ThingDefOf));
        }

        [MayRequireSynstructsCore]
        public static ThingDef ABF_Thing_Synstruct_CoherenceSpot;

        [MayRequireMartialSynstructs]
        public static ThingDef ABF_Thing_Synstruct_Juggernaut_DescentBeam;

        [MayRequireMechcloudChemwalkers]
        public static ThingDef ABF_Thing_Chemwalker_Shell_MechcloudInscribed;

        [MayRequireMechcloudChemwalkers]
        public static ThingDef ABF_Thing_Chemwalker_Shell_LumberingInscribed;

        [MayRequireMechcloudChemwalkers]
        public static ThingDef ABF_Thing_Chemwalker_Shell_FlakkerInscribed;

        [MayRequireMechcloudChemwalkers]
        public static ThingDef ABF_Thing_Chemwalker_Shell_MarineInscribed;

        [MayRequireMechcloudChemwalkers]
        public static ThingDef ABF_Thing_Chemwalker_Shell_CataphractInscribed;
    }
}
