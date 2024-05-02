using Verse;

namespace ArtificialBeings
{
    // This worker prevents pawns that have insufficient maximum complexity from having this directive.
    public class DirectiveRequirementWorker_MinimumComplexity : DirectiveRequirementWorker
    {
        public int complexityRequirement = 0;

        public override AcceptanceReport ValidFor(Pawn pawn)
        {
            AcceptanceReport baseReport = base.ValidFor(pawn);
            if (!baseReport)
            {
                return baseReport.Reason;
            }

            int pawnMaxComplexity = pawn.GetComp<CompArtificialPawn>().MaxComplexity;
            if (pawnMaxComplexity < complexityRequirement)
            {
                return "ABF_ComplexityRequirementInsufficient".Translate(def.LabelCap, complexityRequirement, pawn.LabelShortCap, pawnMaxComplexity);
            }
            return true;
        }
    }
}
