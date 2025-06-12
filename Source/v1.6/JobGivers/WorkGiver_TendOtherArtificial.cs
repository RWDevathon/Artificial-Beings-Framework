using Verse;

namespace ArtificialBeings
{
    public class WorkGiver_TendOtherArtificial : WorkGiver_TendArtificial
    {
        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (base.HasJobOnThing(pawn, t, forced))
            {
                return pawn != t;
            }

            return false;
        }
    }
}
