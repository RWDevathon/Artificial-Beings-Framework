using RimWorld;
using Verse;

namespace ArtificialBeings
{
    public class WorkGiver_TendOtherUrgentArtificial : WorkGiver_TendArtificial
    {
        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (base.HasJobOnThing(pawn, t, forced))
            {
                return HealthAIUtility.ShouldBeTendedNowByPlayerUrgent((Pawn)t);
            }

            return false;
        }
    }
}
