using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ArtificialBeings
{
    // Hediff for the complexity malus/bonus given by the relation between a pawn's current and maximum complexity.
    public class Hediff_Complexity : Hediff
    {
        bool maintenanceExists = false;

        private HediffStage hediffStage = new HediffStage()
        {
            label = "ABF_NormalComplexityHediffLabel".Translate(),
            becomeVisible = false,
            hungerRateFactor = 1,
            statFactors = new List<StatModifier>(),
        };

        private CompArtificialPawn programmingComp;

        private float cachedSeverity = 0;

        public override bool ShouldRemove => false;

        public override bool Visible => Severity != 0;

        public override float PainOffset => 0;

        public override float PainFactor => 1;

        public override float TendPriority => 0;

        public override float Severity
        {
            get
            {
                return cachedSeverity;
            }
        }

        public override HediffStage CurStage
        {
            get
            {
                return hediffStage;
            }
        }

        public override bool TendableNow(bool ignoreTimer = false)
        {
            return false;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                programmingComp = pawn.GetComp<CompArtificialPawn>();
            }
        }

        public override void Tick()
        {
        }

        public override void PostMake()
        {
            base.PostMake();
            programmingComp = pawn.GetComp<CompArtificialPawn>();

            StatDef coherenceStat = ABF_StatDefOf.ABF_CoherenceRetention;
            if (coherenceStat != null)
            {
                maintenanceExists = true;
                hediffStage.statFactors.Add(new StatModifier()
                {
                    stat = coherenceStat,
                    value = 1
                });
            }
        }

        public override void Heal(float amount)
        {
            return;
        }

        public override bool TryMergeWith(Hediff other)
        {
            return def == other.def;
        }

        public void CalculateSeverity()
        {
            float currentComplexity = programmingComp.Complexity;
            float maxComplexity = Mathf.Max(1, programmingComp.MaxComplexity);
            float baselineComplexity = Mathf.Max(1, programmingComp.BaselineComplexity);
            // Complexity usage is below or equal to baseline standards and under the limit.
            // Severity is how much complexity is left unused as a percentage, and is negative.
            if (currentComplexity <= baselineComplexity && currentComplexity <= maxComplexity)
            {
                cachedSeverity = currentComplexity / baselineComplexity - 1;
            }
            // Complexity is above baseline standards but under the limit.
            // Severity is the percentage over the baseline complexity level, to a max of 100%.
            else if (currentComplexity <= maxComplexity)
            {
                cachedSeverity = Mathf.Clamp((currentComplexity - baselineComplexity) / baselineComplexity, 0, 1);
            }
            // Complexity limit has been exceeded.
            // Severity is how far the current complexity is over the limit is as a percentage, squared.
            else
            {
                cachedSeverity = Mathf.Pow(1 + ((currentComplexity - maxComplexity) / maxComplexity), 2);
            }
        }

        public void UpdateHediffStage()
        {
            CalculateSeverity();
            // If severity is negative, complexity is below standard. Apply small buffs to hunger rate (energy fall rate) and maintenance retention.
            if (Severity < 0)
            {
                hediffStage.label = "ABF_LowComplexityHediffLabel".Translate();
                hediffStage.hungerRateFactor = 1 + (Severity / 2);
                if (maintenanceExists)
                {
                    hediffStage.statFactors[0].value = 1 - Severity;
                }
            }
            // If severity is less than or equal to 1, complexity is within limit. Apply small debuffs.
            else if (Severity <= 1)
            {
                hediffStage.label = "ABF_NormalComplexityHediffLabel".Translate();
                hediffStage.hungerRateFactor = 1 + (Severity / 5);
                if (maintenanceExists)
                {
                    hediffStage.statFactors[0].value = 1 - (Severity / 5);
                }
            }
            // Otherwise, complexity has exceeded limits. Apply major debuffs.
            else
            {
                hediffStage.label = "ABF_HighComplexityHediffLabel".Translate();
                hediffStage.hungerRateFactor = Mathf.Clamp(1 + (Severity / 2.5f), 1.2f, 4f);
                if (maintenanceExists)
                {
                    hediffStage.statFactors[0].value = Mathf.Clamp(1 - (Severity / 20f), 0.5f, 0.8f);
                }
            }
        }
    }
}
