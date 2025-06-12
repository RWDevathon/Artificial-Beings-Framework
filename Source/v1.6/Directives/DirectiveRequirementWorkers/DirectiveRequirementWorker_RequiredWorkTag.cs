using RimWorld;
using Verse;

namespace ArtificialBeings
{
    // This worker takes a list of required work tags and ensures the pawn has all
    // of the work tags enabled in order for them to have this directive.
    public class DirectiveRequirementWorker_RequiredWorkTag : DirectiveRequirementWorker
    {
        public WorkTags requiredWorkTags;

        public override AcceptanceReport ValidFor(Pawn pawn)
        {
            AcceptanceReport baseReport = base.ValidFor(pawn);
            if (!baseReport)
            {
                return baseReport.Reason;
            }

            // Compare the active work tags to the forbidden work tags. If it is not disabled, it is illegal.
            if ((pawn.CombinedDisabledWorkTags & requiredWorkTags) != WorkTags.None)
            {
                return "ABF_RequiredWorkTagAbsent".Translate(requiredWorkTags.LabelTranslated());
            }
            // If we scanned all work types and no forbidden work types were active, this is valid.
            return true;
        }
    }
}
