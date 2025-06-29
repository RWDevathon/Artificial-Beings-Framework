using RimWorld;
using Verse;
using Verse.AI;

namespace ArtificialBeings
{
    public class JobGiver_FillArtificialNeed : ThinkNode_JobGiver
    {
        public override float GetPriority(Pawn pawn)
        {
            if (!ABF_Utils.IsArtificial(pawn))
            {
                return 0f;
            }
            return 9.5f;
        }

        // Try to find an artificial need that needs to be satisfied, and return a job to satisfy it.
        protected override Job TryGiveJob(Pawn pawn)
        {
            foreach (Need need in pawn.needs.AllNeeds)
            {
                if (!(need is Need_Artificial artificalNeed))
                {
                    continue;
                }

                if (!artificalNeed.ShouldReplenishNow())
                {
                    continue;
                }

                // Try to get a job to replenish this need. If it's successful, return it. If we didn't find one for this need, keep looking through the others.
                Job job = artificalNeed.GetReplenishJob();
                if (job != null)
                {
                    return job;
                }
            }
            return null;
        }
    }
}
