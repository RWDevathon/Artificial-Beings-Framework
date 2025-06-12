using Verse;
using HarmonyLib;
using RimWorld;

namespace ArtificialBeings
{
    public class BedUtility_Patch
    {
        // Drones can share beds with anyone, even if it unsettles their bedmate.
        [HarmonyPatch(typeof(BedUtility), "WillingToShareBed")]
        public class WillingToShareBed_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(ref bool __result, Pawn pawn1, Pawn pawn2)
            {
                __result = ABF_Utils.IsConsideredNonHumanlike(pawn1) || ABF_Utils.IsConsideredNonHumanlike(pawn2);
                return !__result;
            }
        }
    }
}