using Verse;
using HarmonyLib;
using RimWorld;

namespace ArtificialBeings
{
    public class JobDriver_Vomit_Patch
    {
        // Artificial pawns do not vomit. Do not transpile sources as it involves Hediff.Tick
        [HarmonyPatch(typeof(JobDriver_Vomit), "TryMakePreToilReservations")]
        public class TryMakePreToilReservations_Patch
        {
            [HarmonyPostfix]
            public static void Listener(ref Pawn ___pawn, ref bool __result)
            {
                __result = __result && !ABF_Utils.IsArtificial(___pawn);
            }
        }
    }
}