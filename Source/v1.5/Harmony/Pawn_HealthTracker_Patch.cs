using Verse;
using HarmonyLib;

namespace ArtificialBeings
{
    public class Pawn_HealthTracker_Patch
    {
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