using HarmonyLib;
using RimWorld;
using Verse;

namespace ArtificialBeings
{
    public class GatheringsUtility_Patch
    {
        // Drones and blanks cannot attend or organize gatherings.
        [HarmonyPatch(typeof(GatheringsUtility), nameof(GatheringsUtility.ShouldGuestKeepAttendingGathering))]
        public class GatheringsUtility_ShouldGuestKeepAttendingGathering_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(ref bool __result, Pawn p)
            {
                __result = !ABF_Utils.IsConsideredNonHumanlike(p);
                return __result;
            }
        }
    }
}