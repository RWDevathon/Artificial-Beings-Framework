using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace ArtificialBeings
{
    // Simple ThingComp that allows pawns to consume it directly via an order to fulfill an artificial need.
    public class CompArtificialNeedFulfiller : ThingComp
    {
        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            base.CompFloatMenuOptions(selPawn);
            // No reason to force organics to consume an item in this way.
            if (!ABF_Utils.IsArtificial(selPawn))
            {
                yield break;
            }

            // Force consuming one of this item.
            yield return new FloatMenuOption("ABF_ForceConsumption".Translate(parent.LabelNoCount), delegate () {
                Job job = JobMaker.MakeJob(ABF_JobDefOf.ABF_FulfillArtificialNeed, parent);
                job.count = 1;
                selPawn.jobs.TryTakeOrderedJob(job, JobTag.SatisfyingNeeds);
            });

            // Force consuming multiple of this item - a dialog slider will appear to select how much to consume.
            yield return new FloatMenuOption("ABF_ForceConsumptionMultiple".Translate(parent.LabelNoCount), delegate ()
            {
                Dialog_Slider selectorWindow = new Dialog_Slider("ABF_ConsumeCount".Translate(parent.LabelNoCount, parent), 1, parent.stackCount, delegate (int count)
                {
                    Job job = JobMaker.MakeJob(ABF_JobDefOf.ABF_FulfillArtificialNeed, parent);
                    job.count = count;
                    selPawn.jobs.TryTakeOrderedJob(job, JobTag.SatisfyingNeeds);
                });
                Find.WindowStack.Add(selectorWindow);
            });
        }
    }
}