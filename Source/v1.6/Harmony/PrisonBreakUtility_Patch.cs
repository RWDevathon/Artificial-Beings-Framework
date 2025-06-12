using Verse;
using HarmonyLib;
using RimWorld;

namespace ArtificialBeings
{
    public class PrisonBreakUtility_Patch
    {
        // Non-humanlike intelligences can not participate in prison breaks.
        [HarmonyPatch(typeof(PrisonBreakUtility), "CanParticipateInPrisonBreak")]
        public class CanParticipateInPrisonBreak_Patch
        {
            [HarmonyPostfix]
            public static bool Listener(bool __result, Pawn pawn)
            {
                return __result && !ABF_Utils.IsConsideredNonHumanlike(pawn);
            }
        }
    }
}