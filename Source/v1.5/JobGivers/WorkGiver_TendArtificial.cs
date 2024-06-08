using RimWorld;
using Verse.AI;
using Verse;

namespace ArtificialBeings
{
    // Create an alternate version of the Tend WorkGiver so that artificial pawns are only targetted by artificers, and the WorkGiver will give the artificer jobs instead of doctor jobs.
    public class WorkGiver_TendArtificial : WorkGiver_Tend
    {
        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!(t is Pawn target) || !ABF_Utils.IsArtificial(target) || pawn.WorkTypeIsDisabled(ABF_WorkTypeDefOf.ABF_WorkType_Artificial_Artificer) || !GoodLayingStatusForTend(target, pawn) || !HealthAIUtility.ShouldBeTendedNowByPlayer(target) || !pawn.CanReserve(target, 1, -1, null, forced) || (target.InAggroMentalState && !target.health.hediffSet.HasHediff(HediffDefOf.Scaria)))
            {
                return false;
            }

            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Pawn target = t as Pawn;
            Thing thing = HealthAIUtility.FindBestMedicine(pawn, target);
            if (thing != null && thing.SpawnedParentOrMe != thing)
            {
                return JobMaker.MakeJob(ABF_JobDefOf.ABF_Job_Artificial_Tend, target, thing, thing.SpawnedParentOrMe);
            }
            if (thing != null)
            {
                return JobMaker.MakeJob(ABF_JobDefOf.ABF_Job_Artificial_Tend, target, thing);
            }
            return JobMaker.MakeJob(ABF_JobDefOf.ABF_Job_Artificial_Tend, target);
        }
    }
}
