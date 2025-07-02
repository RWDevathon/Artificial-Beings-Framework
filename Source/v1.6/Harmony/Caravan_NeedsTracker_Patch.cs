using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace ArtificialBeings
{
    // Pawns in caravans should attempt to refill artificial needs.
    [HarmonyPatch(typeof(Caravan_NeedsTracker), "TrySatisfyPawnNeeds")]
    public static class Caravan_NeedsTracker_TrySatisfyPawnNeeds_Patch
    {
        [HarmonyPostfix]
        public static void Listener(Caravan ___caravan, Pawn pawn)
        {
            if (ABF_Utils.IsArtificial(pawn) && pawn.needs is Pawn_NeedsTracker needTracker)
            {
                foreach (Need need in needTracker.AllNeeds)
                {
                    if (need is Need_Artificial needArtificial && needArtificial.ShouldReplenishNow())
                    {
                        needArtificial.TryReplenishNeedInCaravan(___caravan);
                    }
                }
            }
        }
    }
}
