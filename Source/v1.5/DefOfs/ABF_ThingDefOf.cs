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
        public static ThingDef ABF_CoherenceSpot;

        [MayRequireSynstructsCore]
        public static ThingDef ABF_BedsideChargerFacility;

        [MayRequireMartialSynstructs]
        public static ThingDef ABF_Thing_Synstruct_Juggernaut_DescentBeam;

        [MayRequireMechcloudChemwalkers]
        public static ThingDef ABF_MechcloudInscribedChemwalkerShell;

        [MayRequireMechcloudChemwalkers]
        public static ThingDef ABF_LumberingInscribedChemwalkerShell;
    }
}
