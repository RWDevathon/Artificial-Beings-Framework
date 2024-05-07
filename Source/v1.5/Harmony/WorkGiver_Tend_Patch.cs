using Verse;
using HarmonyLib;
using RimWorld;

namespace ArtificialBeings
{
    public class WorkGiver_Tend_Patch
    {
        // Patch the medical tend WorkGiver to not give doctoring jobs on artificial pawns. WorkGiver_TendArtificial handles artificial tending.
        [HarmonyPatch(typeof(WorkGiver_Tend), "HasJobOnThing")]
        public class PotentialWorkThingsGlobal_Patch
        {
            [HarmonyPostfix]
            public static void Listener(Pawn pawn, Thing t, bool forced, ref bool __result)
            {
                if (!__result)
                {
                    return;
                }

                if (t is Pawn target && ABF_Utils.IsArtificial(target))
                {
                    __result = false;
                }
            }
        }
    }
}