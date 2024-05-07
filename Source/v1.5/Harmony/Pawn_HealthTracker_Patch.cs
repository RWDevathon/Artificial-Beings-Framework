using Verse;
using HarmonyLib;
using RimWorld.Planet;

namespace ArtificialBeings
{
    public class Pawn_HealthTracker_Patch
    {
        // Pawns with a non-humanlike intelligence should not send letters to the player about their deaths.
        [HarmonyPatch(typeof(Pawn_HealthTracker), "NotifyPlayerOfKilled")]
        public class NotifyPlayerOfKilled_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(ref DamageInfo? dinfo, ref Hediff hediff, ref Caravan caravan, Pawn ___pawn)
            {
                // If the pawn is a surrogate and wasn't just turned into one, then abort.
                if (ABF_Utils.IsConsideredNonHumanlike(___pawn))
                {
                    return false;
                }

                return true;
            }
        }

        // Artificial pawns do not die from the lethal damage threshold mechanic.
        [HarmonyPatch(typeof(Pawn_HealthTracker), "ShouldBeDeadFromLethalDamageThreshold")]
        public class ShouldBeDeadFromLethalDamageThreshold_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(ref bool __result, Pawn ___pawn)
            {
                if (ABF_Utils.IsArtificial(___pawn))
                {
                    __result = false;
                    return false;
                }
                return true;
            }

        }
    }
}