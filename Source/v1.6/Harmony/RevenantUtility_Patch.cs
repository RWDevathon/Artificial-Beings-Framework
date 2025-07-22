using Verse;
using HarmonyLib;
using RimWorld;

namespace ArtificialBeings
{
    public class RevenantUtility_Patch
    {
        // Non-sapient artificial beings are immune to revenant hypnosis.
        [HarmonyPatch(typeof(RevenantUtility), nameof(RevenantUtility.ValidTarget))]
        public class RevenantUtility_ValidTarget_Patch
        {
            [HarmonyPrefix]
            public static bool Listener(ref bool __result, Pawn pawn)
            {
                if (ABF_Utils.IsConsideredNonHumanlike(pawn))
                {
                    __result = false;
                    return false;
                }
                return true;
            }
        }
    }
}