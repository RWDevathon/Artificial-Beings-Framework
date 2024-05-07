using RimWorld;
using Verse;
using Verse.AI;

namespace ArtificialBeings
{
    public class JobGiver_SelfTendArtificial : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (!pawn.RaceProps.Humanlike || !pawn.health.HasHediffsNeedingTend() || !pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) || pawn.InAggroMentalState)
            {
                return null;
            }

            if (pawn.IsColonist && pawn.WorkTypeIsDisabled(ABF_WorkTypeDefOf.ABF_Artificer))
            {
                return null;
            }

            Job job = JobMaker.MakeJob(ABF_JobDefOf.ABF_TendArtificial, pawn);
            job.endAfterTendedOnce = true;
            return job;
        }
    }
}
