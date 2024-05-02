using RimWorld;
using Verse;

namespace ArtificialBeings
{
    public class CompAbilityEffect_GiveHediffsUserTarget : CompAbilityEffect_WithDuration
    {
        public new CompProperties_AbilityGiveHediffsUserTarget Props => (CompProperties_AbilityGiveHediffsUserTarget)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            if (Props.hediffDefTarget != null && target.Pawn != null && target.Pawn != parent.pawn)
            {
                ApplyHediffTarget(target.Pawn);
            }
            if (Props.hediffDefSelf != null)
            {
                ApplyHediffSelf(parent.pawn);
            }
        }

        private void ApplyHediffSelf(Pawn pawn)
        {
            if (Props.hediffDefSelf != null)
            {
                Hediff hediffSelf = HediffMaker.MakeHediff(Props.hediffDefSelf, pawn);
                HediffComp_Disappears hediffComp_Disappears = hediffSelf.TryGetComp<HediffComp_Disappears>();
                if (hediffComp_Disappears != null)
                {
                    hediffComp_Disappears.ticksToDisappear = GetDurationSeconds(pawn).SecondsToTicks();
                }
                if (Props.severitySelf >= 0f)
                {
                    hediffSelf.Severity = Props.severitySelf;
                }
                pawn.health.AddHediff(hediffSelf);
            }
        }

        private void ApplyHediffTarget(Pawn pawn)
        {
            if (Props.hediffDefTarget != null)
            {
                Hediff hediffTarget = HediffMaker.MakeHediff(Props.hediffDefTarget, pawn);
                HediffComp_Disappears hediffComp_Disappears = hediffTarget.TryGetComp<HediffComp_Disappears>();
                if (hediffComp_Disappears != null)
                {
                    hediffComp_Disappears.ticksToDisappear = GetDurationSeconds(pawn).SecondsToTicks();
                }
                if (Props.severityTarget >= 0f)
                {
                    hediffTarget.Severity = Props.severitySelf;
                }
                pawn.health.AddHediff(hediffTarget);
            }
        }
    }
}
