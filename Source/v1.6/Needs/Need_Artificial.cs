﻿using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace ArtificialBeings
{
    public class Need_Artificial : Need
    {
        private float maxLevel = 1f;

        protected ABF_ArtificialNeedExtension needExtension;

        protected ArtificialNeedDetails needDetails;

        public float AmountDesired => MaxLevel - CurLevel;

        protected ABF_ArtificialNeedExtension NeedExtension
        {
            get
            {
                if (needExtension == null)
                {
                    needExtension = def.GetModExtension<ABF_ArtificialNeedExtension>();
                }
                return needExtension;
            }
        }

        protected ArtificialNeedDetails NeedDetails
        {
            get
            {
                if (needDetails == null)
                {
                    needDetails = pawn.def.GetModExtension<ABF_ArtificialPawnExtension>().artificialNeeds.TryGetValue(def);
                }
                return needDetails;
            }
        }

        public virtual float FallRatePerDay
        {
            get
            {
                return NeedDetails?.fallRatePerDay ?? def.fallPerDay;
            }
        }

        protected float FallRatePerTick
        {
            get
            {
                return FallRatePerDay / GenDate.TicksPerDay;
            }
        }

        public Need_Artificial(Pawn pawn) : base(pawn)
        {
        }

        public override void SetInitialLevel()
        {
            maxLevel = NeedDetails?.maximumCapacity ?? 1f;
            CurLevelPercentage = 1.0f;
        }

        public override float MaxLevel
        {
            get
            {
                return maxLevel;
            }
        }

        public override void NeedInterval()
        {
            if (!IsFrozen)
            {
                HandleTicks(NeedTunings.NeedUpdateInterval);
            }

            // If the need is depleted, and a hediff on empty is expected, then it should be updated. It won't update itself.
            if (NeedExtension.hediffToApplyOnEmpty != null)
            {
                Hediff needHediff = pawn.health.hediffSet.GetFirstHediffOfDef(NeedExtension.hediffToApplyOnEmpty);
                if (CurLevel <= 0.001)
                {
                    if (needHediff == null)
                    {
                        needHediff = HediffMaker.MakeHediff(NeedExtension.hediffToApplyOnEmpty, pawn);
                        needHediff.Severity = 0f;
                        pawn.health.AddHediff(needHediff);
                    }
                    needHediff.Severity += NeedTunings.NeedUpdateInterval * (NeedExtension.hediffRisePerDay / GenDate.TicksPerDay);
                }
                else if (needHediff != null)
                {
                    needHediff.Severity -= NeedTunings.NeedUpdateInterval * (NeedExtension.hediffFallPerDay / GenDate.TicksPerDay);
                }
            }
        }

        public override int GUIChangeArrow
        {
            get
            {
                if (IsFrozen)
                {
                    return 0;
                }
                return -1;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                maxLevel = NeedDetails?.maximumCapacity ?? 1f;
            }
        }

        public override string GetTipString()
        {
            return (LabelCap + ": " + CurLevelPercentage.ToStringPercent()).Colorize(ColoredText.TipSectionTitleColor) + " (" + CurLevel.ToString("0.##") + " / " + MaxLevel.ToString("0.##") + ")\n" + def.description + "\n\n" + "ABF_ArtificialNeedFallRate".Translate(FallRatePerDay.ToString("0.##"), NeedExtension?.unitsLabel).Resolve();
        }

        // This method should be where all handling of the level changing should be done.
        public virtual void HandleTicks(int delta)
        {
            if (NeedDetails?.fallRateIsPercentage == true)
            {
                CurLevelPercentage -= delta * FallRatePerTick;
            }
            else
            {
                CurLevel -= delta * FallRatePerTick;
            }
        }

        // This method should be called to check if a pawn should automatically replenish this need.
        public virtual bool ShouldReplenishNow()
        {
            return CurLevelPercentage < NeedExtension.criticalThreshold;
        }

        // This method is responsible for returning a job to replenish this need. Subclasses may desire to have specific ways of satisfying it.
        public virtual Job GetReplenishJob()
        {
            Thing item = ABF_Utils.GetNeedSatisfyingItem(pawn, def);
            if (item != null)
            {
                ABF_NeedFulfillerExtension needFulfiller = item.def.GetModExtension<ABF_NeedFulfillerExtension>();
                int desiredCount = Mathf.Max(1, Mathf.FloorToInt(AmountDesired / needFulfiller.needOffsetRelations[def]));
                Job job = JobMaker.MakeJob(ABF_JobDefOf.ABF_Job_Artificial_FulfillNeed, item);
                job.count = Mathf.Min(item.stackCount, desiredCount);
                return job;
            }
            return null;
        }

        // This method is responsible for handling the pawn replenishing this need while in a caravan (if it should at all). Subclasses may desire to override this.
        public virtual void TryReplenishNeedInCaravan(Caravan caravan)
        {
            List<ThingDef> fulfillingThingDefs = ABF_Utils.cachedArtificialNeedFulfillments[def];
            if (fulfillingThingDefs != null && fulfillingThingDefs.Count > 0)
            {
                foreach (Thing thing in CaravanInventoryUtility.AllInventoryItems(caravan))
                {
                    if (fulfillingThingDefs.Contains(thing.def))
                    {
                        ABF_NeedFulfillerExtension needFulfiller = thing.def.GetModExtension<ABF_NeedFulfillerExtension>();
                        float fulfillmentAmount = needFulfiller.needOffsetRelations[def];
                        int consumptionCount = Mathf.Min(thing.stackCount, Mathf.Max(1, Mathf.FloorToInt(AmountDesired / fulfillmentAmount)));
                        CurLevel += fulfillmentAmount * consumptionCount;
                        if (thing.stackCount == consumptionCount)
                        {
                            thing.Destroy();
                        }
                        else
                        {
                            thing.stackCount -= consumptionCount;
                        }
                    }
                }
            }
        }
    }
}
