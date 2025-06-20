using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
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
            // No reason to show an option for a race which has no artificial need that is fulfilled by this.
            ABF_NeedFulfillerExtension fulfillerExtension = parent.def.GetModExtension<ABF_NeedFulfillerExtension>();
            if (fulfillerExtension == null || fulfillerExtension.needOffsetRelations == null)
            {
                yield break;
            }

            Dictionary<Need, float> fulfillsNeeds = new Dictionary<Need, float>();
            foreach (NeedDef needDef in fulfillerExtension.needOffsetRelations.Keys)
            {
                if (selPawn.needs.TryGetNeed(needDef) is Need need)
                {
                    fulfillsNeeds[need] = fulfillerExtension.needOffsetRelations[needDef];
                    break;
                }
            }
            if (fulfillsNeeds.Count <= 0)
            {
                yield break;
            }

            // Force consuming one of this item.
            yield return new FloatMenuOption("ABF_ForceConsumption".Translate(parent.LabelNoCount), delegate () {
                Job job = JobMaker.MakeJob(ABF_JobDefOf.ABF_Job_Artificial_FulfillNeed, parent);
                job.count = 1;
                selPawn.jobs.TryTakeOrderedJob(job, JobTag.SatisfyingNeeds);
            });

            // Force consuming multiple of this item - a dialog slider will appear to select how much to consume.
            // The maximum amount should be the amount of which is necessary to fully fill the need with the lowest requirement.
            yield return new FloatMenuOption("ABF_ForceConsumptionMultiple".Translate(parent.LabelNoCount), delegate ()
            {
                int maxToConsume = parent.stackCount;
                foreach (Need need in fulfillsNeeds.Keys)
                {
                    int amountNeededToFulfill = Mathf.FloorToInt((need.MaxLevel - need.CurLevel) / fulfillsNeeds[need]);
                    if (amountNeededToFulfill < maxToConsume)
                    {
                        maxToConsume = amountNeededToFulfill;
                    }
                }
                Dialog_Slider selectorWindow = new Dialog_Slider("ABF_ConsumeCount".Translate(parent.LabelNoCount, parent), 1, Math.Max(1, maxToConsume), delegate (int count)
                {
                    Job job = JobMaker.MakeJob(ABF_JobDefOf.ABF_Job_Artificial_FulfillNeed, parent);
                    job.count = count;
                    selPawn.jobs.TryTakeOrderedJob(job, JobTag.SatisfyingNeeds);
                });
                Find.WindowStack.Add(selectorWindow);
            });
        }
    }
}