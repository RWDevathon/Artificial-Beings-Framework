using RimWorld;
using System.Collections.Generic;
using System;
using Verse;

namespace ArtificialBeings
{
    // StatWorker for calculating a programmable drone's maximum complexity. It will not display or be calculated otherwise.
    public class StatWorker_MaximumComplexity : StatWorker
    {
        public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
        {
            if (IsDisabledFor(req.Thing))
            {
                return 0f;
            }
            Pawn pawn = req.Thing as Pawn;
            List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
            float offsetValueResult = 0f;
            float factorValueResult = 1f;
            foreach (Hediff hediff in hediffs)
            {
                HediffStage curStage = hediff.CurStage;
                if (curStage != null)
                {
                    float offsetValue = curStage.statOffsets.GetStatOffsetFromList(stat);
                    float factorValue = curStage.statFactors.GetStatFactorFromList(stat);
                    if (offsetValue != 0f && curStage.statOffsetEffectMultiplier != null)
                    {
                        offsetValue *= pawn.GetStatValue(curStage.statOffsetEffectMultiplier);
                    }
                    if (offsetValue != 0f && curStage.multiplyStatChangesBySeverity)
                    {
                        offsetValue *= hediff.Severity;
                    }
                    if (Math.Abs(factorValue - 1f) > float.Epsilon && curStage.statFactorEffectMultiplier != null)
                    {
                        factorValue = ScaleFactor(factorValue, pawn.GetStatValue(curStage.statFactorEffectMultiplier));
                    }
                    if (curStage.multiplyStatChangesBySeverity)
                    {
                        factorValue = ScaleFactor(factorValue, hediff.Severity);
                    }
                    offsetValueResult += offsetValue;
                    factorValueResult *= factorValue;
                }
            }
            return (GetBaseValueFor(req) + offsetValueResult) * factorValueResult;

        }

        public override bool ShouldShowFor(StatRequest req)
        {
            if (!base.ShouldShowFor(req))
            {
                return false;
            }
            if (req.Thing != null && req.Thing is Pawn pawn)
            {
                return ABF_Utils.IsProgrammableDrone(pawn);
            }
            return false;
        }

        public override bool IsDisabledFor(Thing thing)
        {
            if (thing is Pawn pawn)
            {
                return !ABF_Utils.IsProgrammableDrone(pawn);
            }
            return true;
        }
    }
}