using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace ArtificialBeings
{
    public class JobGiver_GetMechNeed : ThinkNode_JobGiver
    {
        public override float GetPriority(Pawn pawn)
        {
            if (!ABF_Utils.IsArtificial(pawn))
            {
                return 0f;
            }
            return 9.5f;
        }

        // Try to find an artificial need that needs to be satisfied, and return a job to satisfy it if it can find an item to ingest for it.
        protected override Job TryGiveJob(Pawn pawn)
        {
            foreach (NeedDef needDef in ABF_Utils.cachedArtificialNeedFulfillments.Keys)
            {
                Need need = pawn.needs.TryGetNeed(needDef);
                if (need == null)
                {
                    continue;
                }

                // Only seek to fulfill the need if you are under the critical threshold.
                if (need.CurLevelPercentage > need.def.GetModExtension<ABF_ArtificialNeedExtension>().criticalThreshold)
                {
                    continue;
                }

                Thing item = GetNeedSatisfyingItem(pawn, needDef);
                if (item != null)
                {
                    ABF_NeedFulfillerExtension needFulfiller = item.def.GetModExtension<ABF_NeedFulfillerExtension>();
                    int desiredCount = Mathf.Max(1, Mathf.FloorToInt((need.MaxLevel - need.CurLevel) / needFulfiller.needOffsetRelations[needDef]));
                    Job job = JobMaker.MakeJob(ABF_JobDefOf.ABF_FulfillArtificialNeed, item);
                    job.count = Mathf.Min(item.stackCount, desiredCount);
                    return job;
                }
            }
            return null;
        }

        public static Thing GetNeedSatisfyingItem(Pawn pawn, NeedDef need)
        {
            Thing carriedThing = pawn.carryTracker.CarriedThing;
            if (!(ABF_Utils.cachedArtificialNeedFulfillments[need] is List<ThingDef> fulfillingThingDefs))
            {
                return null;
            }

            if (carriedThing != null && fulfillingThingDefs.Contains(carriedThing.def) == true)
            {
                return carriedThing;
            }
            for (int i = 0; i < pawn.inventory.innerContainer.Count; i++)
            {
                if (fulfillingThingDefs.Contains(pawn.inventory.innerContainer[i].def) == true)
                {
                    return pawn.inventory.innerContainer[i];
                }
            }
            if (pawn.Map == null)
            {
                return null;
            }
            List<Thing> searchList = new List<Thing>();
            foreach (ThingDef thingDef in fulfillingThingDefs)
            {
                searchList.AddRange(pawn.Map.listerThings.ThingsOfDef(thingDef));
            }
            return GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map, searchList, PathEndMode.OnCell, TraverseParms.For(pawn), 9999f, (Thing t) => pawn.CanReserve(t) && !t.IsForbidden(pawn));
        }
    }
}
