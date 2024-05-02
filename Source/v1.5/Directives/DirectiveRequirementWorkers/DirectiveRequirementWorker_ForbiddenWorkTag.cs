using RimWorld;
using Verse;

namespace ArtificialBeings
{
    // This worker takes a list of forbidden work tags and ensures the pawn has none
    // of the work tags enabled in order for them to have this directive.
    public class DirectiveRequirementWorker_ForbiddenWorkTag : DirectiveRequirementWorker
    {
        public WorkTags forbiddenWorkTags;

        public override AcceptanceReport ValidFor(Pawn pawn)
        {
            AcceptanceReport baseReport = base.ValidFor(pawn);
            if (!baseReport)
            {
                return baseReport.Reason;
            }

            // Compare the active work tags to the forbidden work tags. If it is not disabled, it is illegal.
            if ((pawn.CombinedDisabledWorkTags & forbiddenWorkTags) != forbiddenWorkTags)
            {
                return "ABF_ForbiddenWorkTagsPresent".Translate(forbiddenWorkTags.LabelTranslated());
            }
            // If we scanned all work tags and no forbidden work tags were active, this is valid.
            return true;
        }
    }
}
